using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ITCompanySimulation.Character;
using TMPro;
using ITCompanySimulation.Core;
using ITCompanySimulation.Company;

namespace ITCompanySimulation.UI
{
    public class UIWorkersWorkersMarket : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private ControlListView ListViewMarketWorkers;
        private SimulationManager SimulationManagerComponent;
        private WorkersMarket WorkersMarketComponent;
        private GameTime GameTimeComponent;
        [SerializeField]
        private Button ButtonHireWorker;
        [SerializeField]
        private ListViewElement WorkerListViewElementPrefab;
        [SerializeField]
        private TextMeshProUGUI TextName;
        [SerializeField]
        private TextMeshProUGUI TextExpierience;
        [SerializeField]
        private TextMeshProUGUI TextMarketWorkersListView;
        [SerializeField]
        private TextMeshProUGUI TextSalary;
        [SerializeField]
        private WorkerAbilitiesDisplay AbilitiesDisplay;
        [SerializeField]
        private Tooltip TooltipComponent;
        private InfoWindow InfoWindowComponent;
        private IButtonSelector WorkersButtonSelector = new ButtonSelector();
        private SharedWorker SelectedWorker;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void InitializeWorkersListView(ControlListView listView, List<SharedWorker> workers)
        {
            foreach (SharedWorker singleWorker in workers)
            {
                AddWorkerListViewElement(singleWorker, listView);
            }
        }

        private void SetWorkerInfoText(SharedWorker selectedWorker)
        {
            if (null != selectedWorker)
            {
                TextName.gameObject.SetActive(true);
                TextSalary.gameObject.SetActive(true);
                TextExpierience.gameObject.SetActive(true);

                TextName.text = UIWorkers.GetWorkerNameString(selectedWorker);
                TextSalary.text = UIWorkers.GetWorkerSalaryString(selectedWorker);
                TextExpierience.text = UIWorkers.GetWorkerExpierienceString(selectedWorker);
            }
            else
            {
                TextName.gameObject.SetActive(false);
                TextSalary.gameObject.SetActive(false);
                TextExpierience.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// This method will enable or disable buttons used
        /// for hiring of firing workers based on which worker
        /// is selected (market worker or company worker)
        /// </summary>
        private void SetActionButtonsState(SharedWorker selectedWorker)
        {
            if (null != selectedWorker)
            {
                ButtonHireWorker.interactable = SimulationManagerComponent.ControlledCompany.CanHireWorker;
            }
            else
            {
                ButtonHireWorker.interactable = false;
            }
        }

        #region Events callbacks

        private void OnMarketWorkerAdded(SharedWorker addedWorker)
        {
            AddWorkerListViewElement(addedWorker, ListViewMarketWorkers);
            SetMarketWorkersListViewText();
        }

        private void OnMarketWorkerRemoved(SharedWorker removedWorker)
        {
            RemoveWorkerListViewElement(removedWorker, ListViewMarketWorkers);
            SetMarketWorkersListViewText();
        }

        private void OnSelectedWorkerButtonChanged(Button workerButton)
        {
            if (null != workerButton)
            {
                ListViewElement selectedElement = workerButton.gameObject.GetComponent<ListViewElement>();
                SelectedWorker = (SharedWorker)selectedElement.RepresentedObject;

                SetActionButtonsState(SelectedWorker);
                SetWorkerInfoText(SelectedWorker);
                AbilitiesDisplay.DisplayWorkerAbilities(SelectedWorker);
            }
            else
            {
                SetWorkerInfoText(null);
                SetActionButtonsState(null);
                AbilitiesDisplay.DisplayWorkerAbilities(null);
            }
        }

        #endregion

        private void SetMarketWorkersListViewText()
        {
            TextMarketWorkersListView.text = string.Format("Market workers ({0})",
                WorkersMarketComponent.Workers.Count);
        }

        private void AddWorkerListViewElement(SharedWorker worker, ControlListView listView)
        {
            ListViewElement element = UIWorkers.CreateWorkerListViewElement(worker, WorkerListViewElementPrefab, TooltipComponent);

            Button buttonComponent = element.GetComponent<Button>();
            MousePointerEvents mousePtrEvts = element.GetComponent<MousePointerEvents>();

            mousePtrEvts.PointerDoubleClick.AddListener(() =>
               {
                   if (true == SimulationManagerComponent.ControlledCompany.CanHireWorker)
                   {
                       OnHireWorkerButtonClicked();
                   }
                   else
                   {
                       string info = string.Format("You have reached maximum number of workers in company ({0})",
                                                   PlayerCompany.MAX_WORKERS_PER_COMPANY);
                       InfoWindowComponent.ShowOk(info, null);
                   }
               });


            listView.AddControl(element.gameObject);
            WorkersButtonSelector.AddButton(buttonComponent);
        }

        private void RemoveWorkerListViewElement(SharedWorker worker, ControlListView listView)
        {
            ListViewElement element = listView.FindElement(worker);
            Button buttonComponent = element.GetComponent<Button>();

            listView.RemoveControl(element.gameObject);
            WorkersButtonSelector.RemoveButton(buttonComponent);
        }

        private void Awake()
        {
            InfoWindowComponent = InfoWindow.Instance;
            SimulationManagerComponent = SimulationManager.Instance;
            GameTimeComponent = SimulationManagerComponent.gameObject.GetComponent<GameTime>();
            WorkersMarketComponent = SimulationManagerComponent.gameObject.GetComponent<WorkersMarket>();
        }

        private void Start()
        {
            List<SharedWorker> marketWorkers = WorkersMarketComponent.Workers.Values.ToList();
            InitializeWorkersListView(ListViewMarketWorkers, marketWorkers);
            marketWorkers = SimulationManagerComponent.ControlledCompany.Workers.Cast<SharedWorker>().ToList();

            WorkersMarketComponent.WorkerAdded += OnMarketWorkerAdded;
            WorkersMarketComponent.WorkerRemoved += OnMarketWorkerRemoved;

            WorkersButtonSelector.SelectedButtonChanged += OnSelectedWorkerButtonChanged;

            SetWorkerInfoText(SelectedWorker);
            SetActionButtonsState(SelectedWorker);
            SetMarketWorkersListViewText();
        }

        /*Public methods*/

        public void OnHireWorkerButtonClicked()
        {
            WorkersMarketComponent.RequestWorker(SelectedWorker);
        }
    }
}