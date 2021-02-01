using ITCompanySimulation.Character;
using ITCompanySimulation.Core;
using System.Collections.Generic;
using System.Linq;
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

        [SerializeField]
        private MainSimulationManager SimulationManagerComponent;
        [SerializeField]
        private ControlListView ListViewOtherPlayersWorkers;
        [SerializeField]
        private ControlListView ListViewCompanyWorkers;
        [SerializeField]
        private ListViewElementWorker WorkerListViewElementPrefab;
        [SerializeField]
        private Button ButtonHireWorker;
        [SerializeField]
        private TextMeshProUGUI TextName;
        [SerializeField]
        private TextMeshProUGUI TextExpierience;
        [SerializeField]
        private TextMeshProUGUI TextAbilities;
        [SerializeField]
        private TextMeshProUGUI TextSalary;
        [SerializeField]
        private TextMeshProUGUI TextListViewOtherPlayersWorkers;
        [SerializeField]
        private TextMeshProUGUI TextListViewCompanyWorkers;
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
        /// Maps button in players list view to its coresponding worker
        /// </summary>
        private Dictionary<SharedWorker, ListViewElementWorker> WorkerListViewMap;
        /// <summary>
        /// Maps photon player to its index in players dropdown
        /// </summary>
        private Dictionary<PhotonPlayer, int> PhotonPlayerDropdownMap = new Dictionary<PhotonPlayer, int>();
        private IButtonSelector WorkersButtonSelector = new ButtonSelector();

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        #region Event callbacks

        private void OnDropdownPlayersListValueChanged(int index)
        {
            if (1 != PhotonNetwork.room.PlayerCount)
            {
                List<SharedWorker> playerWorkers;

                if (null != SelectedPlayer)
                {
                    playerWorkers = SimulationManagerComponent.OtherPlayersWorkers[SelectedPlayer];
                    foreach (SharedWorker worker in playerWorkers)
                    {
                        RemoveWorkerListViewElement(worker, ListViewOtherPlayersWorkers);
                    }
                }

                SelectedPlayer = PhotonNetwork.playerList[index];
                playerWorkers = SimulationManagerComponent.OtherPlayersWorkers[SelectedPlayer];

                foreach (SharedWorker worker in playerWorkers)
                {
                    AddWorkerListViewElement(worker, ListViewOtherPlayersWorkers);
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
                ListViewElementWorker element = selectedButton.GetComponent<ListViewElementWorker>();
                SelectedWorker = WorkerListViewMap.First(x => x.Value == element).Key;
                SetWorkerInfoText(SelectedWorker);
                SubscribeToSelectedWorkerEvents();
                ButtonHireWorker.interactable = (false == SelectedWorker is LocalWorker)
                    && (PlayerCompany.MAX_WORKERS_PER_COMPANY > SimulationManagerComponent.ControlledCompany.Workers.Count);
            }
            else
            {
                SetWorkerInfoText(null);
                ButtonHireWorker.interactable = false;
            }
        }

        private void OnControlledCompanyWorkerRemoved(SharedWorker companyWorker)
        {
            RemoveWorkerListViewElement(companyWorker, ListViewCompanyWorkers);
            SetTextListViewCompanyWorkers();
            UnsubscribeFromWorkerEvents(companyWorker);
        }

        private void OnControlledCompanyWorkerAdded(SharedWorker companyWorker)
        {
            AddWorkerListViewElement(companyWorker, ListViewCompanyWorkers);
            SetTextListViewCompanyWorkers();
            SubscribeToWorkerEvents(companyWorker);
        }

        private void OnWorkerSalaryChanged(SharedWorker companyWorker)
        {
            if (null != SelectedWorker && companyWorker.ID == SelectedWorker.ID)
            {
                TextSalary.text = UIWorkers.GetWorkerSalaryString(companyWorker);
            }

            ListViewElementWorker elem = null;

            if (companyWorker is LocalWorker)
            {
                elem = UIWorkers.GetWorkerListViewElement(companyWorker, ListViewCompanyWorkers);
            }
            else
            {
                elem = UIWorkers.GetWorkerListViewElement(companyWorker, ListViewOtherPlayersWorkers);
            }

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
            SelectedWorker.ExpierienceTimeChanged -= OnWorkerSalaryChanged;
        }

        private void SetTextListViewCompanyWorkers()
        {
            TextListViewCompanyWorkers.text = string.Format("Company workers ({0}/{1})",
                SimulationManagerComponent.ControlledCompany.Workers.Count,
                PlayerCompany.MAX_WORKERS_PER_COMPANY);
        }

        private void SetTextListViewOtherPlayersWorkers()
        {
            if (null != SelectedPlayer)
            {
                TextListViewOtherPlayersWorkers.text = string.Format("{0}'s workers ({1}/{2})",
                    SelectedPlayer.NickName,
                    SimulationManagerComponent.OtherPlayersWorkers[SelectedPlayer].Count,
                    PlayerCompany.MAX_WORKERS_PER_COMPANY);
            }
            else
            {
                TextListViewOtherPlayersWorkers.text = "Other player's workers";
            }
        }

        private void AddWorkerListViewElement(SharedWorker playerWorker, ControlListView listView)
        {
            ListViewElementWorker element =
                UIWorkers.CreateWorkerListViewElement(playerWorker, WorkerListViewElementPrefab, TooltipComponent);

            Button buttonComponent = element.GetComponent<Button>();

            if (null == WorkerListViewMap)
            {
                WorkerListViewMap = new Dictionary<SharedWorker, ListViewElementWorker>();
            }

            if (listView == ListViewOtherPlayersWorkers)
            {
                MousePointerEvents mouseEvts = element.GetComponent<MousePointerEvents>();
                mouseEvts.PointerDoubleClick += () =>
                {
                    OnButtonHireWorkerClick();
                };
            }

            WorkerListViewMap.Add(playerWorker, element);
            listView.AddControl(element.gameObject);
            WorkersButtonSelector.AddButton(buttonComponent);
        }

        private void RemoveWorkerListViewElement(SharedWorker worker, ControlListView listView)
        {
            if (null != WorkerListViewMap)
            {
                ListViewElementWorker element = WorkerListViewMap[worker];
                Button buttonComponent = element.Button;

                WorkerListViewMap.Remove(worker);
                WorkersButtonSelector.RemoveButton(buttonComponent);
                listView.RemoveControl(element.gameObject);
            }
        }

        private void Start()
        {
            SimulationManagerComponent.ControlledCompany.WorkerAdded += OnControlledCompanyWorkerAdded;
            SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnControlledCompanyWorkerRemoved;
            SimulationManagerComponent.OtherPlayerWorkerAdded += OnOtherPlayerWorkerAdded;
            SimulationManagerComponent.OtherPlayerWorkerRemoved += OnOtherPlayerWorkerRemoved;
            DropdownPlayersList.onValueChanged.AddListener(OnDropdownPlayersListValueChanged);
            WorkersButtonSelector.SelectedButtonChanged += OnWorkersSelectedButtonChanged;
            InitDropdownPlayersList();
            //Initialize player's workers list at script start
            OnDropdownPlayersListValueChanged(DropdownPlayersList.value);

            foreach (LocalWorker worker in SimulationManagerComponent.ControlledCompany.Workers)
            {
                OnControlledCompanyWorkerAdded(worker);
            }

            foreach (var playerWorkers in SimulationManagerComponent.OtherPlayersWorkers)
            {
                foreach (var worker in playerWorkers.Value)
                {
                    SubscribeToWorkerEvents(worker);
                }
            }

            OnWorkersSelectedButtonChanged(null);
            SetTextListViewCompanyWorkers();
            SetTextListViewOtherPlayersWorkers();
        }

        private void AddListViewPlayersWorkersElements(PhotonPlayer otherPlayer)
        {
            List<SharedWorker> playerWorkers = SimulationManagerComponent.OtherPlayersWorkers[otherPlayer];

            foreach (SharedWorker playerWorker in playerWorkers)
            {
                AddWorkerListViewElement(playerWorker, ListViewOtherPlayersWorkers);
            }
        }

        private void InitDropdownPlayersList()
        {
            List<string> dropdownOptions = new List<string>();

            if (false == PhotonNetwork.offlineMode)
            {
                foreach (KeyValuePair<PhotonPlayer, List<SharedWorker>> workersListPair in SimulationManagerComponent.OtherPlayersWorkers)
                {
                    string dropdownOptionText = string.Format("({0}) {1}",
                                                              workersListPair.Key.ID,
                                                              workersListPair.Key.NickName);

                    PhotonPlayerDropdownMap.Add(workersListPair.Key, dropdownOptions.Count);
                    dropdownOptions.Add(dropdownOptionText);
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

                TextAbilities.text = UIWorkers.GetWorkerAbilitiesString(selectedWorker);
                RectTransform textTransform = TextAbilities.rectTransform.parent.GetComponent<RectTransform>();
                textTransform.sizeDelta = new Vector2(textTransform.sizeDelta.x, TextAbilities.preferredHeight);
            }
            else
            {
                TextName.text = string.Empty;
                TextSalary.text = string.Empty;
                TextAbilities.text = string.Empty;
                TextExpierience.text = string.Empty;
            }
        }

        /*Public methods*/

        public void OnButtonHireWorkerClick()
        {
            Button selectedWorkerButton = WorkersButtonSelector.GetSelectedButton();
            ListViewElementWorker element = selectedWorkerButton.GetComponent<ListViewElementWorker>();
            SharedWorker selectedWorker = WorkerListViewMap.First(x => x.Value == element).Key;

            string infoWindowText = string.Format("Do you want to hire this worker ? It will cost you {0} $", selectedWorker.HireSalary);
            InfoWindowComponent.ShowOkCancel(infoWindowText, () =>
             {
                 SimulationManagerComponent.HireOtherPlayerWorker(SelectedPlayer, selectedWorker);
             }, null);
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            base.OnPhotonPlayerDisconnected(otherPlayer);

            int playerDropdownIndex = PhotonPlayerDropdownMap[otherPlayer];
            PhotonPlayerDropdownMap.Remove(otherPlayer);
            DropdownPlayersList.options.RemoveAt(playerDropdownIndex);

            if (SelectedPlayer.ID == otherPlayer.ID)
            {
                ListViewOtherPlayersWorkers.RemoveAllControls();
                WorkersButtonSelector.RemoveAllButtons();
            }
        }
    }
}
