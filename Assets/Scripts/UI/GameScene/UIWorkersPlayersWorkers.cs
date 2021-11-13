using ITCompanySimulation.Character;
using ITCompanySimulation.Company;
using ITCompanySimulation.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This class handles display of UI with workers of other
    /// players in simulation
    /// </summary>
    public class UIWorkersPlayersWorkers : Photon.PunBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private SimulationManager SimulationManagerComponent;
        [SerializeField]
        private ControlListView ListViewOtherPlayersWorkers;
        [SerializeField]
        private ListViewElement WorkerListViewElementPrefab;
        [SerializeField]
        private Button ButtonHireWorker;
        [SerializeField]
        private TextMeshProUGUI TextName;
        [SerializeField]
        private TextMeshProUGUI TextExpierience;
        [SerializeField]
        private TextMeshProUGUI TextSalary;
        [SerializeField]
        private TextMeshProUGUI TextListViewOtherPlayersWorkers;
        [SerializeField]
        private WorkerAbilitiesDisplay AbilitiesDisplay;
        /// <summary>
        /// Dropdown that holds list of other players in room
        /// </summary>
        [SerializeField]
        private TMP_Dropdown DropdownPlayersList;
        [SerializeField]
        private Tooltip TooltipComponent;
        [SerializeField]
        private InfoWindow InfoWindowComponent;
        private PhotonPlayer SelectedPlayer;
        private SharedWorker SelectedWorker;
        /// <summary>
        /// Maps photon player to its index in players dropdown
        /// </summary>
        private Dictionary<PhotonPlayer, TMP_Dropdown.OptionData> PhotonPlayerDropdownOptionMap =
            new Dictionary<PhotonPlayer, TMP_Dropdown.OptionData>();
        private IButtonSelector WorkersButtonSelector = new ButtonSelector();

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        #region Event callbacks

        private void OnDropdownPlayersListValueChanged(int index)
        {
            if (1 != PhotonNetwork.room.PlayerCount)
            {
                Dictionary<int, SharedWorker> playerWorkers;

                if (null != SelectedPlayer)
                {
                    playerWorkers = SimulationManagerComponent.PlayerDataMap[SelectedPlayer.ID].Workers;
                    foreach (var workerPair in playerWorkers)
                    {
                        RemoveWorkerListViewElement(workerPair.Value, ListViewOtherPlayersWorkers);
                    }
                }

                SelectedPlayer = PhotonNetwork.otherPlayers[index];
                SelectedWorker = null;
                playerWorkers = SimulationManagerComponent.PlayerDataMap[SelectedPlayer.ID].Workers;
                SetTextListViewOtherPlayersWorkers();

                foreach (var workerPair in playerWorkers)
                {
                    AddWorkerListViewElement(workerPair.Value, ListViewOtherPlayersWorkers);
                }
            }
        }

        private void OnOtherPlayerWorkerAdded(SharedWorker worker, PhotonPlayer player)
        {
            if (SelectedPlayer.ID == player.ID)
            {
                AddWorkerListViewElement(worker, ListViewOtherPlayersWorkers);
                SetTextListViewOtherPlayersWorkers();
            }

            SubscribeToWorkerEvents(worker);
        }

        private void OnOtherPlayerWorkerRemoved(SharedWorker removedWorker, PhotonPlayer player)
        {
            if (SelectedPlayer.ID == player.ID)
            {
                RemoveWorkerListViewElement(removedWorker, ListViewOtherPlayersWorkers);
                SetTextListViewOtherPlayersWorkers();
            }

            UnsubscribeFromWorkerEvents(removedWorker);
        }

        private void OnWorkersSelectedButtonChanged(Button selectedButton)
        {
            if (null != SelectedWorker)
            {
                UnsubscribeFromSelectedWorkerEvents();
            }

            if (null != selectedButton)
            {
                ListViewElement element = selectedButton.GetComponent<ListViewElement>();
                SelectedWorker = (SharedWorker)element.RepresentedObject;
                SetWorkerInfoText(SelectedWorker);
                SubscribeToSelectedWorkerEvents();
                ButtonHireWorker.interactable = SimulationManagerComponent.ControlledCompany.CanHireWorker;
            }
            else
            {
                SelectedWorker = null;
                SetWorkerInfoText(null);
                ButtonHireWorker.interactable = false;
            }
        }

        private void OnWorkerSalaryChanged(SharedWorker companyWorker)
        {
            if (null != SelectedWorker && companyWorker.ID == SelectedWorker.ID)
            {
                TextSalary.text = UIWorkers.GetWorkerSalaryString(companyWorker);
            }

            ListViewElement elem = ListViewOtherPlayersWorkers.FindElement(companyWorker);
            elem.Text.text = UIWorkers.GetWorkerListViewElementText(companyWorker);
        }

        private void OnSelectedWorkerExpierienceTimeChanged(SharedWorker companyWorker)
        {
            TextExpierience.text = UIWorkers.GetWorkerExpierienceString(companyWorker);
        }

        #endregion

        private void SubscribeToWorkerEvents(SharedWorker worker)
        {
            worker.SalaryChanged += OnWorkerSalaryChanged;
        }

        private void UnsubscribeFromWorkerEvents(SharedWorker worker)
        {
            worker.SalaryChanged -= OnSelectedWorkerExpierienceTimeChanged;
        }

        private void SubscribeToSelectedWorkerEvents()
        {
            SelectedWorker.ExpierienceTimeChanged += OnSelectedWorkerExpierienceTimeChanged;
        }

        private void UnsubscribeFromSelectedWorkerEvents()
        {
            SelectedWorker.ExpierienceTimeChanged -= OnSelectedWorkerExpierienceTimeChanged;
        }

        private void SetTextListViewOtherPlayersWorkers()
        {
            if (null != SelectedPlayer)
            {
                TextListViewOtherPlayersWorkers.text = string.Format("{0}'s workers ({1}/{2})",
                    SelectedPlayer.NickName,
                    SimulationManagerComponent.PlayerDataMap[SelectedPlayer.ID].Workers.Count,
                    PlayerCompany.MAX_WORKERS_PER_COMPANY);
            }
            else
            {
                TextListViewOtherPlayersWorkers.text = "Other player's workers";
            }
        }

        private void AddWorkerListViewElement(SharedWorker playerWorker, ControlListView listView)
        {
            ListViewElement element =
                UIWorkers.CreateWorkerListViewElement(playerWorker, WorkerListViewElementPrefab, TooltipComponent);

            Button buttonComponent = element.GetComponent<Button>();

            if (listView == ListViewOtherPlayersWorkers)
            {
                MousePointerEvents mouseEvts = element.GetComponent<MousePointerEvents>();
                mouseEvts.PointerDoubleClick.AddListener(() =>
               {
                   if (true == SimulationManagerComponent.ControlledCompany.CanHireWorker)
                   {
                       OnButtonHireWorkerClick();
                   }
               });
            }

            listView.AddControl(element.gameObject);
            WorkersButtonSelector.AddButton(buttonComponent);
        }

        private void RemoveWorkerListViewElement(SharedWorker worker, ControlListView listView)
        {
            ListViewElement element = listView.FindElement(worker);
            Button buttonComponent = element.Button;
            WorkersButtonSelector.RemoveButton(buttonComponent);
            listView.RemoveControl(element.gameObject);
        }

        private void Awake()
        {
            GameObject scriptsGameObject = GameObject.FindGameObjectWithTag("ScriptsGameObject");
            SimulationManagerComponent = scriptsGameObject.GetComponent<SimulationManager>();
        }

        private void Start()
        {
            SimulationManagerComponent.OtherPlayerWorkerAdded += OnOtherPlayerWorkerAdded;
            SimulationManagerComponent.OtherPlayerWorkerRemoved += OnOtherPlayerWorkerRemoved;
            DropdownPlayersList.onValueChanged.AddListener(OnDropdownPlayersListValueChanged);
            WorkersButtonSelector.SelectedButtonChanged += OnWorkersSelectedButtonChanged;
            InitDropdownPlayersList();
            //Initialize player's workers list at script start
            OnDropdownPlayersListValueChanged(DropdownPlayersList.value);

            foreach (var playerWorkers in SimulationManagerComponent.PlayerDataMap)
            {
                foreach (var worker in playerWorkers.Value.Workers)
                {
                    SubscribeToWorkerEvents(worker.Value);
                }
            }

            OnWorkersSelectedButtonChanged(null);
            SetTextListViewOtherPlayersWorkers();
        }

        private void AddListViewPlayersWorkersElements(PhotonPlayer otherPlayer)
        {
            Dictionary<int, SharedWorker> playerWorkers = SimulationManagerComponent.PlayerDataMap[otherPlayer.ID].Workers;

            foreach (var workerPair in playerWorkers)
            {
                AddWorkerListViewElement(workerPair.Value, ListViewOtherPlayersWorkers);
            }
        }

        private void InitDropdownPlayersList()
        {
            List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();

            if (false == PhotonNetwork.offlineMode)
            {
                foreach (PhotonPlayer player in PhotonNetwork.otherPlayers)
                {
                    string dropdownOptionText = string.Format("({0}) {1}",
                                                              player.ID,
                                                              player.NickName);
                    TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(dropdownOptionText);
                    PhotonPlayerDropdownOptionMap.Add(player, option);
                    dropdownOptions.Add(option);
                }
            }

            DropdownPlayersList.AddOptions(dropdownOptions);
        }

        private void SetWorkerInfoText(SharedWorker selectedWorker)
        {
            if (null != selectedWorker)
            {
                TextName.text = UIWorkers.GetWorkerNameString(selectedWorker);
                TextSalary.text = UIWorkers.GetWorkerSalaryString(selectedWorker);
                TextExpierience.text = UIWorkers.GetWorkerExpierienceString(selectedWorker);
                AbilitiesDisplay.DisplayWorkerAbilities(selectedWorker);
            }
            else
            {
                TextName.text = string.Empty;
                TextSalary.text = string.Empty;
                AbilitiesDisplay.DisplayWorkerAbilities(null);
                TextExpierience.text = string.Empty;
            }
        }

        /*Public methods*/

        public void OnButtonHireWorkerClick()
        {
            Button selectedWorkerButton = WorkersButtonSelector.GetSelectedButton();

            string infoWindowText = string.Format("Do you want to hire this worker ? It will cost you {0} $", SelectedWorker.HireSalary);
            InfoWindowComponent.ShowOkCancel(infoWindowText, () =>
             {
                 SimulationManagerComponent.HireOtherPlayerWorker(SelectedPlayer, SelectedWorker);
             }, null);
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer disconnectedPlayer)
        {
            base.OnPhotonPlayerDisconnected(disconnectedPlayer);

            TMP_Dropdown.OptionData option = PhotonPlayerDropdownOptionMap[disconnectedPlayer];
            PhotonPlayerDropdownOptionMap.Remove(disconnectedPlayer);
            DropdownPlayersList.options.Remove(option);

            if (SelectedPlayer.ID == disconnectedPlayer.ID)
            {
                ListViewOtherPlayersWorkers.RemoveAllControls();
                SelectedPlayer = null;

                if (0 != DropdownPlayersList.options.Count)
                {
                    DropdownPlayersList.SetValueWithoutNotify(0);
                    OnDropdownPlayersListValueChanged(DropdownPlayersList.value);
                }
            }
        }
    }
}
