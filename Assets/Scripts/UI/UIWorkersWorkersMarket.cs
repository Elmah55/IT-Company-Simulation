using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ITCompanySimulation.Character;
using TMPro;
using System.Text;
using UnityEngine.EventSystems;

namespace ITCompanySimulation.UI
{
    public class UIWorkersWorkersMarket : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        /// <summary>
        /// Used to map button in workers list to appriopriate worker
        /// </summary>
        private Dictionary<SharedWorker, ListViewElement> WorkerListViewMap;
        [SerializeField]
        private ControlListView ListViewMarketWorkers;
        [SerializeField]
        private ControlListView ListViewCompanyWorkers;
        [SerializeField]
        private MainSimulationManager SimulationManagerComponent;
        [SerializeField]
        private WorkersMarket WorkersMarketComponent;
        [SerializeField]
        private GameTime GameTimeComponent;
        [SerializeField]
        private Button ButtonHireWorker;
        [SerializeField]
        private Button ButtonFireWorker;
        [SerializeField]
        private ListViewElement WorkerListViewElementPrefab;
        [SerializeField]
        private TextMeshProUGUI TextName;
        [SerializeField]
        private TextMeshProUGUI TextExpierience;
        [SerializeField]
        private TextMeshProUGUI TextAbilities;
        [SerializeField]
        private TextMeshProUGUI TextMarketWorkersListView;
        [SerializeField]
        private TextMeshProUGUI TextCompanyWorkerListView;
        [SerializeField]
        private TextMeshProUGUI TextSalary;
        [SerializeField]
        private Tooltip TooltipComponent;
        private IButtonSelector WorkersButtonSelector = new ButtonSelector();
        private SharedWorker SelectedWorker;
        private static StringBuilder StrBuilder = new StringBuilder();

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

        private ListViewElement CreateWorkerListViewElement(SharedWorker worker, ListViewElement prefab)
        {
            ListViewElement el = GameObject.Instantiate<ListViewElement>(prefab);

            el.Text.text = GetWorkerListViewElementText(worker);

            return el;
        }

        private string GetWorkerListViewElementText(SharedWorker worker)
        {
            return string.Format("{0} {1}\n{2} days of expierience\n{3} $ / Month",
                worker.Name, worker.Surename, worker.ExperienceTime, worker.Salary);
        }

        private string GetWorkerAbilitiesText(SharedWorker worker)
        {
            StrBuilder.Clear();
            StrBuilder.Append("Abilities:\n");

            for (int i = 0; i < worker.Abilites.Count; i++)
            {
                KeyValuePair<ProjectTechnology, float> ability = worker.Abilites.ElementAt(i);

                StrBuilder.AppendFormat("{0} {1}\n",
                    EnumToString.ProjectTechnologiesStrings[ability.Key],
                    ability.Value.ToString("0.00"));
            }

            return StrBuilder.ToString();
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


                TextAbilities.text = GetWorkerAbilitiesText(selectedWorker);
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

        private void SetWorkerSalaryText(SharedWorker selectedWorker)
        {
            TextSalary.text = string.Format("Salary: {0} $",
                selectedWorker.Salary);
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
                //Does not have company assigned so its market worker
                if (false == (selectedWorker is LocalWorker))
                {
                    ButtonFireWorker.interactable = false;
                    ButtonHireWorker.interactable = true;
                }
                else
                {
                    ButtonFireWorker.interactable = true;
                    ButtonHireWorker.interactable = false;
                }

                if (PlayerCompany.MAX_WORKERS_PER_COMPANY == SimulationManagerComponent.ControlledCompany.Workers.Count)
                {
                    ButtonHireWorker.interactable = false;
                }
            }
            else
            {
                ButtonFireWorker.interactable = false;
                ButtonHireWorker.interactable = false;
            }
        }

        #region Events callbacks

        private void OnWorkerSalaryChanged(SharedWorker companyWorker)
        {
            ListViewElement el = WorkerListViewMap[companyWorker];
            el.Text.text = GetWorkerListViewElementText(companyWorker);
        }

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

        private void OnCompanyWorkerAdded(SharedWorker addedWorker)
        {
            AddWorkerListViewElement(addedWorker, ListViewCompanyWorkers);
            SetCompanyWorkersListViewText();
        }

        private void OnCompanyWorkerRemoved(SharedWorker removedWorker)
        {
            RemoveWorkerListViewElement(removedWorker, ListViewCompanyWorkers);
            SetCompanyWorkersListViewText();
        }

        private void OnSelectedWorkerButtonChanged(Button workerButton)
        {
            if (null != workerButton)
            {
                ListViewElement selectedElement = workerButton.gameObject.GetComponent<ListViewElement>();
                SelectedWorker = WorkerListViewMap.First(x => x.Value == selectedElement).Key;

                SetActionButtonsState(SelectedWorker);
                SetWorkerInfoText(SelectedWorker);
            }
            else
            {
                SetActionButtonsState(null);
            }
        }

        private void OnGameTimeDayChanged()
        {
            if (true == gameObject.activeSelf)
            {
                foreach (KeyValuePair<SharedWorker, ListViewElement> pair in WorkerListViewMap)
                {
                    pair.Value.Text.text = GetWorkerListViewElementText(pair.Key);
                }
            }
        }

        #endregion

        private void SetMarketWorkersListViewText()
        {
            TextMarketWorkersListView.text = string.Format("Market workers ({0})",
                WorkersMarketComponent.Workers.Count);
        }

        private void SetCompanyWorkersListViewText()
        {
            TextCompanyWorkerListView.text = string.Format("Company workers ({0} / {1})",
                SimulationManagerComponent.ControlledCompany.Workers.Count,
                PlayerCompany.MAX_WORKERS_PER_COMPANY);
        }

        private void AddWorkerListViewElement(SharedWorker worker, ControlListView listView)
        {
            ListViewElement element = CreateWorkerListViewElement(worker, WorkerListViewElementPrefab);

            MousePointerEvents mousePtrEvt = element.gameObject.AddComponent<MousePointerEvents>();

            mousePtrEvt.PointerEntered += () =>
              {
                  TooltipComponent.gameObject.SetActive(true);
                  TooltipComponent.Text = GetWorkerAbilitiesText(worker);
              };

            mousePtrEvt.PointerExited += () =>
              {
                  TooltipComponent.gameObject.SetActive(false);
              };

            Button buttonComponent = element.GetComponent<Button>();

            if (null == WorkerListViewMap)
            {
                WorkerListViewMap = new Dictionary<SharedWorker, ListViewElement>();
            }

            listView.AddControl(element.gameObject);
            WorkerListViewMap.Add(worker, element);
            WorkersButtonSelector.AddButton(buttonComponent);
            worker.SalaryChanged += OnWorkerSalaryChanged;
        }

        private void RemoveWorkerListViewElement(SharedWorker worker, ControlListView listView)
        {
            ListViewElement element = WorkerListViewMap.First(x => x.Key == worker).Value;
            Button buttonComponent = element.GetComponent<Button>();

            listView.RemoveControl(element.gameObject);
            WorkerListViewMap.Remove(worker);
            WorkersButtonSelector.RemoveButton(buttonComponent);
            worker.SalaryChanged -= OnWorkerSalaryChanged;
        }

        private void Start()
        {
            InitializeWorkersListView(ListViewMarketWorkers, WorkersMarketComponent.Workers);
            InitializeWorkersListView(ListViewCompanyWorkers, SimulationManagerComponent.ControlledCompany.Workers.Cast<SharedWorker>().ToList());

            SimulationManagerComponent.ControlledCompany.WorkerAdded += OnCompanyWorkerAdded;
            SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnCompanyWorkerRemoved;

            WorkersMarketComponent.WorkerAdded += OnMarketWorkerAdded;
            WorkersMarketComponent.WorkerRemoved += OnMarketWorkerRemoved;

            WorkersButtonSelector.SelectedButtonChanged += OnSelectedWorkerButtonChanged;

            GameTimeComponent.DayChanged += OnGameTimeDayChanged;

            SetWorkerInfoText(SelectedWorker);
            SetActionButtonsState(SelectedWorker);
        }

        /*Public methods*/

        public void OnHireWorkerButtonClicked()
        {
            WorkersMarketComponent.RemoveWorker(SelectedWorker);

            LocalWorker newLocalWorker = SelectedWorker as LocalWorker;

            if (null == newLocalWorker)
            {
                newLocalWorker = new LocalWorker(SelectedWorker);
            }

            SimulationManagerComponent.ControlledCompany.AddWorker(newLocalWorker);
        }

        public void OnFireWorkerButtonClicked()
        {
            SimulationManagerComponent.ControlledCompany.RemoveWorker((LocalWorker)SelectedWorker);
            WorkersMarketComponent.AddWorker(SelectedWorker);
        }
    }
}