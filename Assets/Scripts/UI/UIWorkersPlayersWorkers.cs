using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This class handles display of UI with workers of other
/// players in simulation
/// </summary>
public class UIWorkersPlayersWorkers : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    [SerializeField]
    private MainSimulationManager SimulationManagerComponent;
    [SerializeField]
    private ControlListView ListViewPlayersWorkers;
    [SerializeField]
    private GameObject ListViewPlayersWorkersButtonPrefab;
    /// <summary>
    /// Dropdown that holds list of other players in room
    /// </summary>
    [SerializeField]
    private Dropdown DropdownPlayersList;
    [SerializeField]
    private InputField InputFieldWorkerAbilities;
    private PhotonPlayer DropdownPlayersListSelectedPlayer;
    /// <summary>
    /// Maps button in players list view to its coresponding worker
    /// </summary>
    private Dictionary<GameObject, Worker> ListViewPlayersWorkersButtonWorkerMap = new Dictionary<GameObject, Worker>();

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    /// <summary>
    /// Adds buttons to list view with workers info from company of given
    /// players
    /// </summary>
    private void AddPlayerWorkerButton(Worker playerWorker)
    {
        GameObject playerWorkerButton = GameObject.Instantiate(ListViewPlayersWorkersButtonPrefab);

        Button buttonComponent = playerWorkerButton.GetComponent<Button>();
        buttonComponent.onClick.AddListener(OnListViewPlayersWorkersButtonClicked);

        Text buttonTextComponent = playerWorkerButton.GetComponentInChildren<Text>();
        string buttonText = string.Format("{0} {1} / {2} days / {3} $",
                                          playerWorker.Name,
                                          playerWorker.Surename,
                                          playerWorker.ExperienceTime,
                                          playerWorker.HireSalary);
        buttonTextComponent.text = buttonText;

        ListViewPlayersWorkersButtonWorkerMap.Add(playerWorkerButton, playerWorker);
        ListViewPlayersWorkers.AddControl(playerWorkerButton);
    }

    private void OnDropdownPlayersListValueChanged(int index)
    {
        DropdownPlayersListSelectedPlayer = PhotonNetwork.playerList[index];
        ListViewPlayersWorkers.RemoveAllControls();
        AddListViewPlayersWorkersButtons(DropdownPlayersListSelectedPlayer);
    }

    private void OnOtherPlayerWorkerAdded(Worker addedWorker, PhotonPlayer player)
    {
        if (DropdownPlayersListSelectedPlayer == player)
        {
            AddPlayerWorkerButton(addedWorker);
        }
    }

    private void Start()
    {
        SimulationManagerComponent.OtherPlayerWorkerAdded += OnOtherPlayerWorkerAdded;
        DropdownPlayersList.onValueChanged.AddListener(OnDropdownPlayersListValueChanged);
        InitDropdownPlayersList();
    }

    private void DisplayWorkerAbilities(Worker playerWorker)
    {
        string abilities = string.Empty;

        foreach (KeyValuePair<ProjectTechnology, float> workerAbilityPair in playerWorker.Abilites)
        {
            abilities += string.Format("{0} {1}{2}",
                                       workerAbilityPair.Key.ToString().PadRight(20),
                                       workerAbilityPair.Value.ToString(),
                                       Environment.NewLine);
        }

        InputFieldWorkerAbilities.text = abilities;
    }

    private void AddListViewPlayersWorkersButtons(PhotonPlayer otherPlayer)
    {
        List<Worker> playerWorkers = SimulationManagerComponent.OtherPlayersWorkers[otherPlayer];

        foreach (Worker playerWorker in playerWorkers)
        {
            AddPlayerWorkerButton(playerWorker);
        }
    }

    private void OnListViewPlayersWorkersButtonClicked()
    {
        //We know its worker list button because its clicked event has been just called
        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
        Worker selectedWorker = ListViewPlayersWorkersButtonWorkerMap[selectedButton];
        DisplayWorkerAbilities(selectedWorker);
    }

    private void InitDropdownPlayersList()
    {
        List<string> dropdownOptions = new List<string>();

        if (false == PhotonNetwork.offlineMode)
        {
            foreach (KeyValuePair<PhotonPlayer, List<Worker>> workersListPair in SimulationManagerComponent.OtherPlayersWorkers)
            {
                if (PhotonNetwork.player.ID == workersListPair.Key.ID)
                {
                    continue;
                }

                string dropdownOptionText = string.Format("({0}) {1}",
                                                          workersListPair.Key.ID,
                                                          workersListPair.Key.NickName);
                dropdownOptions.Add(dropdownOptionText);
            }
        }

        DropdownPlayersList.AddOptions(dropdownOptions);
    }

    /*Public methods*/

}
