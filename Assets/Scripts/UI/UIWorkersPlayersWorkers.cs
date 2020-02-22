using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles display of UI with workers of other
/// players in simulation
/// </summary>
public class UIWorkersPlayersWorkers : UIWorkers
{
    /*Private consts fields*/

    /*Private fields*/

    [SerializeField]
    private MainSimulationManager SimulationManagerComponent;
    [SerializeField]
    private ControlListView ListViewPlayersWorkers;
    [SerializeField]
    private Button ButtonHireWorker;
    /// <summary>
    /// Dropdown that holds list of other players in room
    /// </summary>
    [SerializeField]
    private Dropdown DropdownPlayersList;
    private PhotonPlayer DropdownPlayersListSelectedPlayer;
    /// <summary>
    /// Maps button in players list view to its coresponding worker
    /// </summary>
    private Dictionary<Button, Worker> ListViewPlayersWorkersButtonWorkerMap = new Dictionary<Button, Worker>();
    /// <summary>
    /// Maps photon player to its index in players dropdown
    /// </summary>
    private Dictionary<PhotonPlayer, int> PhotonPlayerDropdownMap = new Dictionary<PhotonPlayer, int>();
    private IButtonSelector WorkersButtonSelector = new ButtonSelector();

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    /// <summary>
    /// Adds buttons to list view with workers info from company of given
    /// players
    /// </summary>
    private void AddPlayerWorkerButton(Worker playerWorker)
    {
        Button playerWorkerButton = GameObject.Instantiate<Button>(ListViewButtonPrefab);

        Text buttonTextComponent = playerWorkerButton.GetComponentInChildren<Text>();
        string buttonText = string.Format("{0} {1} / {2} days / {3} $",
                                          playerWorker.Name,
                                          playerWorker.Surename,
                                          playerWorker.ExperienceTime,
                                          playerWorker.HireSalary);
        buttonTextComponent.text = buttonText;

        ListViewPlayersWorkersButtonWorkerMap.Add(playerWorkerButton, playerWorker);
        ListViewPlayersWorkers.AddControl(playerWorkerButton.gameObject);
        WorkersButtonSelector.AddButton(playerWorkerButton);
    }

    private void OnDropdownPlayersListValueChanged(int index)
    {
        DropdownPlayersListSelectedPlayer = PhotonNetwork.playerList[index];
        ListViewPlayersWorkers.RemoveAllControls();
        AddListViewPlayersWorkersButtons(DropdownPlayersListSelectedPlayer);
    }

    private void OnOtherPlayerWorkerAdded(Worker addedWorker, PhotonPlayer player)
    {
        if (DropdownPlayersListSelectedPlayer.ID == player.ID)
        {
            AddPlayerWorkerButton(addedWorker);
        }
    }

    private void Start()
    {
        SimulationManagerComponent.OtherPlayerWorkerAdded += OnOtherPlayerWorkerAdded;
        DropdownPlayersList.onValueChanged.AddListener(OnDropdownPlayersListValueChanged);
        WorkersButtonSelector.SelectedButtonChanged += OnListViewPlayersWorkersButtonClicked;
        InitDropdownPlayersList();
        //Initialize player's workers list at script start
        DropdownPlayersListSelectedPlayer = PhotonNetwork.playerList[DropdownPlayersList.value];
        OnDropdownPlayersListValueChanged(DropdownPlayersList.value);
    }

    private void AddListViewPlayersWorkersButtons(PhotonPlayer otherPlayer)
    {
        List<Worker> playerWorkers = SimulationManagerComponent.OtherPlayersWorkers[otherPlayer];

        foreach (Worker playerWorker in playerWorkers)
        {
            AddPlayerWorkerButton(playerWorker);
        }
    }

    private void OnListViewPlayersWorkersButtonClicked(Button selectedButton)
    {
        ButtonHireWorker.interactable = (selectedButton != null);

        if (null != selectedButton)
        {
            Worker selectedWorker = ListViewPlayersWorkersButtonWorkerMap[selectedButton];
            UpdateWorkerInfo(selectedWorker);
        }
        else
        {
            base.ClearWorkerInfo();
        }
    }

    private void InitDropdownPlayersList()
    {
        List<string> dropdownOptions = new List<string>();

        if (false == PhotonNetwork.offlineMode)
        {
            foreach (KeyValuePair<PhotonPlayer, List<Worker>> workersListPair in SimulationManagerComponent.OtherPlayersWorkers)
            {
                //No need to add local player
                if (PhotonNetwork.player.ID == workersListPair.Key.ID)
                {
                    continue;
                }

                string dropdownOptionText = string.Format("({0}) {1}",
                                                          workersListPair.Key.ID,
                                                          workersListPair.Key.NickName);

                PhotonPlayerDropdownMap.Add(workersListPair.Key, dropdownOptions.Count);
                dropdownOptions.Add(dropdownOptionText);
            }
        }

        DropdownPlayersList.AddOptions(dropdownOptions);
    }

    protected override void UpdateWorkerInfo(Worker selectedWorker)
    {
        base.UpdateWorkerInfo(selectedWorker);

        InputFieldSalary.text = string.Format("{0} $", selectedWorker.HireSalary);
    }

    /*Public methods*/

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        base.OnPhotonPlayerDisconnected(otherPlayer);

        int playerDropdownIndex = PhotonPlayerDropdownMap[otherPlayer];
        PhotonPlayerDropdownMap.Remove(otherPlayer);
        DropdownPlayersList.options.RemoveAt(playerDropdownIndex);

        if (DropdownPlayersListSelectedPlayer.ID == otherPlayer.ID)
        {
            ListViewPlayersWorkers.RemoveAllControls();
            WorkersButtonSelector.RemoveAllButtons();
        }
    }
}
