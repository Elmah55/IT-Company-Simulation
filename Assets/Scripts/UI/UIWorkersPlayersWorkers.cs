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
        /// <summary>
        /// Dropdown that holds list of other players in room
        /// </summary>
        [SerializeField]
        private TMP_Dropdown DropdownPlayersList;
        [SerializeField]
        private Tooltip TooltipComponent;
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
            }
        }

        private void OnOtherPlayerWorkerRemoved(SharedWorker removedWorker, PhotonPlayer player)
        {
            if (SelectedPlayer.ID == player.ID)
            {
                Button selectedWorkerButton = WorkersButtonSelector.GetSelectedButton();

                WorkerListViewMap.Remove(removedWorker);
                WorkersButtonSelector.RemoveButton(selectedWorkerButton);
                ListViewOtherPlayersWorkers.RemoveControl(selectedWorkerButton.gameObject);
            }
        }

        private void OnWorkersSelectedButtonChanged(Button selectedButton)
        {
            if (null != SelectedWorker)
            {
                UnsubscribeFromWorkerEvents();
            }

            if (null != selectedButton)
            {
                ListViewElementWorker element = selectedButton.GetComponent<ListViewElementWorker>();
                SelectedWorker = WorkerListViewMap.First(x => x.Value == element).Key;
                SetWorkerInfoText(SelectedWorker);
                SubscribeToWorkerEvents();
                ButtonHireWorker.interactable = false == SelectedWorker is LocalWorker;
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
        }

        private void OnControlledCompanyWorkerAdded(SharedWorker companyWorker)
        {
            AddWorkerListViewElement(companyWorker, ListViewCompanyWorkers);
        }

        private void OnSelectedWorkerSalaryChanged(SharedWorker companyWorker)
        {
            SetWorkerSalaryText(companyWorker);
        }

        #endregion

        private void SubscribeToWorkerEvents()
        {
            SelectedWorker.SalaryChanged += OnSelectedWorkerSalaryChanged;
        }

        private void UnsubscribeFromWorkerEvents()
        {
            SelectedWorker.SalaryChanged -= OnSelectedWorkerSalaryChanged;
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

            WorkerListViewMap.Add(playerWorker, element);
            listView.AddControl(element.gameObject);
            WorkersButtonSelector.AddButton(buttonComponent);
        }

        private void RemoveWorkerListViewElement(SharedWorker playerWorker, ControlListView listView)
        {
            if (null != WorkerListViewMap)
            {
                ListViewElementWorker element = WorkerListViewMap[playerWorker];
                Button buttonComponent = element.GetComponent<Button>();

                WorkerListViewMap.Remove(playerWorker);
                listView.RemoveControl(element.gameObject);
                WorkersButtonSelector.RemoveButton(buttonComponent);
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

            OnWorkersSelectedButtonChanged(null);
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
                TextName.gameObject.SetActive(true);
                TextSalary.gameObject.SetActive(true);
                TextAbilities.gameObject.SetActive(true);
                TextExpierience.gameObject.SetActive(true);

                TextName.text = string.Format("Name: {0} {1}",
                    selectedWorker.Name, selectedWorker.Surename);

                SetWorkerSalaryText(selectedWorker);

                TextExpierience.text = string.Format("Expierience: {0} days",
                    selectedWorker.ExperienceTime);


                TextAbilities.text = UIWorkers.GetWorkerAbilitiesString(selectedWorker);
                RectTransform textTransform = TextAbilities.rectTransform;
                textTransform.sizeDelta = new Vector2(textTransform.sizeDelta.x, TextAbilities.preferredHeight);
            }
            else
            {
                TextName.gameObject.SetActive(false);
                TextSalary.gameObject.SetActive(false);
                TextAbilities.gameObject.SetActive(false);
                TextExpierience.gameObject.SetActive(false);
            }
        }

        private void SetWorkerSalaryText(SharedWorker worker)
        {
            TextSalary.text = string.Format("Salary: {0} $",
                worker.Salary);
        }

        /*Public methods*/

        public void OnButtonHireWorkerClick()
        {
            Button selectedWorkerButton = WorkersButtonSelector.GetSelectedButton();
            ListViewElementWorker element = selectedWorkerButton.GetComponent<ListViewElementWorker>();
            SharedWorker selectedWorker = WorkerListViewMap.First(x => x.Value == element).Key;
            SimulationManagerComponent.RemoveOtherPlayerControlledCompanyWorker(SelectedPlayer, selectedWorker.ID);

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

            if (SelectedPlayer.ID == otherPlayer.ID)
            {
                ListViewOtherPlayersWorkers.RemoveAllControls();
                WorkersButtonSelector.RemoveAllButtons();
            }
        }
    }
}
