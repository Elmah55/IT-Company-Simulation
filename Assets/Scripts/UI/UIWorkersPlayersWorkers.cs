using ITCompanySimulation.Character;
using System.Collections.Generic;
using System.Linq;
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
    private Dictionary<Button, SharedWorker> ListViewPlayersWorkersButtonWorkerMap = new Dictionary<Button, SharedWorker>();
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
    private void AddPlayerWorkerButton(SharedWorker playerWorker)
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
        if (1 != PhotonNetwork.room.PlayerCount)
        {
            DropdownPlayersListSelectedPlayer = PhotonNetwork.playerList[index];
            ListViewPlayersWorkers.RemoveAllControls();
            AddListViewPlayersWorkersButtons(DropdownPlayersListSelectedPlayer);
        }
    }

    private void OnOtherPlayerWorkerAdded(SharedWorker worker, PhotonPlayer player)
    {
        if (DropdownPlayersListSelectedPlayer.ID == player.ID)
        {
            AddPlayerWorkerButton(worker);
        }
    }

    private void OnOtherPlayerWorkerRemoved(SharedWorker removedWorker, PhotonPlayer player)
    {
        if (DropdownPlayersListSelectedPlayer.ID == player.ID)
        {
            Button selectedWorkerButton = WorkersButtonSelector.GetSelectedButton();
            SharedWorker selectedWorker = ListViewPlayersWorkersButtonWorkerMap[selectedWorkerButton];

            List<SharedWorker> otherPlayerWorkers =
                SimulationManagerComponent.OtherPlayersWorkers[DropdownPlayersListSelectedPlayer];

            selectedWorker.Salary = selectedWorker.HireSalary;

            otherPlayerWorkers.Remove(selectedWorker);
            WorkersButtonSelector.RemoveButton(selectedWorkerButton);
            ListViewPlayersWorkers.RemoveControl(selectedWorkerButton.gameObject);
            ClearWorkerInfo();
        }
    }

    private void Start()
    {
        SimulationManagerComponent.OtherPlayerWorkerAdded += OnOtherPlayerWorkerAdded;
        SimulationManagerComponent.OtherPlayerWorkerRemoved += OnOtherPlayerWorkerRemoved;
        DropdownPlayersList.onValueChanged.AddListener(OnDropdownPlayersListValueChanged);
        WorkersButtonSelector.SelectedButtonChanged += OnListViewPlayersWorkersButtonClicked;
        InitDropdownPlayersList();
        //Initialize player's workers list at script start
        DropdownPlayersListSelectedPlayer = PhotonNetwork.playerList[DropdownPlayersList.value];
        OnDropdownPlayersListValueChanged(DropdownPlayersList.value);
    }

    private void AddListViewPlayersWorkersButtons(PhotonPlayer otherPlayer)
    {
        List<SharedWorker> playerWorkers = SimulationManagerComponent.OtherPlayersWorkers[otherPlayer];

        foreach (SharedWorker playerWorker in playerWorkers)
        {
            AddPlayerWorkerButton(playerWorker);
        }
    }

    private void OnListViewPlayersWorkersButtonClicked(Button selectedButton)
    {
        ButtonHireWorker.interactable = (selectedButton != null);

        if (null != selectedButton)
        {
            SharedWorker selectedWorker = ListViewPlayersWorkersButtonWorkerMap[selectedButton];
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
            foreach (KeyValuePair<PhotonPlayer, List<SharedWorker>> workersListPair in SimulationManagerComponent.OtherPlayersWorkers)
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

    protected override void UpdateWorkerInfo(SharedWorker selectedWorker)
    {
        base.UpdateWorkerInfo(selectedWorker);

        InputFieldSalary.text = string.Format("{0} $", selectedWorker.HireSalary);
    }

    /*Public methods*/

    public void OnButtonHireWorkerClick()
    {
        Button selectedWorkerButton = WorkersButtonSelector.GetSelectedButton();
        SharedWorker selectedWorker = ListViewPlayersWorkersButtonWorkerMap[selectedWorkerButton];
        SimulationManagerComponent.RemoveOtherPlayerControlledCompanyWorker(DropdownPlayersListSelectedPlayer, selectedWorker.ID);

        LocalWorker localSelectedWorker = selectedWorker as LocalWorker;

        if (null == localSelectedWorker)
        {
            localSelectedWorker = new LocalWorker(selectedWorker);
        }

        SimulationManagerComponent.ControlledCompany.AddWorker(localSelectedWorker);
    }

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
