using ITCompanySimulation.Character;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ITCompanySimulation.Core;
using ITCompanySimulation.Utilities;
using ITCompanySimulation.Project;

namespace ITCompanySimulation.UI
{
    public class UIWorkersCompanyWorkers : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private IButtonSelector WorkersButtonsSelector = new ButtonSelector();
        [SerializeField]
        private ListViewElement WorkerListViewElementPrefab;
        [SerializeField]
        private ControlListView ListViewCompanyWorkers;
        [SerializeField]
        private Button ButtonFireWorker;
        [SerializeField]
        private Button ButtonGiveSalaryRaise;
        [SerializeField]
        private Slider SliderSalaryRaiseAmount;
        [SerializeField]
        private TextMeshProUGUI TextSliderSalaryRaiseAmount;
        [SerializeField]
        private TextMeshProUGUI TextName;
        [SerializeField]
        private TextMeshProUGUI TextSalary;
        [SerializeField]
        private TextMeshProUGUI TextDaysInCompany;
        [SerializeField]
        private TextMeshProUGUI TextExpierience;
        [SerializeField]
        private TextMeshProUGUI TextListViewWorkers;
        [SerializeField]
        private ProgressBar ProgressBarSatisfaction;
        [SerializeField]
        private WorkerAbilitiesDisplay AbilitiesDisplay;
        private SimulationManager SimulationManagerComponent;
        [SerializeField]
        private Tooltip TooltipComponent;
        private InfoWindow InfoWindowComponent;
        private LocalWorker SelectedWorker;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /// <summary>
        /// Set values in components displaying information about worker.
        /// </summary>
        private void SetWorkerInfo()
        {
            TextName.text = UIWorkers.GetWorkerNameString(SelectedWorker);
            TextSalary.text = UIWorkers.GetWorkerSalaryString(SelectedWorker);
            TextExpierience.text = UIWorkers.GetWorkerExpierienceString(SelectedWorker);
            TextDaysInCompany.text = UIWorkers.GetWorkerDaysInCompanyString(SelectedWorker);
            ProgressBarSatisfaction.Text.text = UIWorkers.GetWorkerSatisfactionString(SelectedWorker);
            ProgressBarSatisfaction.Value = SelectedWorker.Satiscation;
            AbilitiesDisplay.DisplayWorkerAbilities(SelectedWorker);
        }

        private string GetWorkerListViewElementText(LocalWorker worker)
        {
            return string.Format("{0} {1}\nSatisfaction {2} %\n{3} $ / Month",
                worker.Name, worker.Surename, worker.Satiscation.ToString("0.00"), worker.Salary);
        }

        private ListViewElement CreateWorkerListViewElement(LocalWorker companyWorker)
        {
            ListViewElement element =
                UIWorkers.CreateWorkerListViewElement(companyWorker, WorkerListViewElementPrefab, TooltipComponent);
            element.Text.text = GetWorkerListViewElementText(companyWorker);
            Button buttonComponent = element.GetComponent<Button>();

            WorkersButtonsSelector.AddButton(buttonComponent);
            ListViewCompanyWorkers.AddControl(element.gameObject);

            //List view element contains information about satisfaction so it should be updated whenever it changes
            companyWorker.SatisfactionChanged += OnWorkerSatisfactionChanged;

            return element;
        }

        private void InitWorkersView()
        {
            foreach (LocalWorker companyWorker in SimulationManagerComponent.ControlledCompany.Workers)
            {
                CreateWorkerListViewElement(companyWorker);
            }

            //This is called to update all UI elements associated with this slider
            OnSalaryRaiseAmountSliderValueChanged(SliderSalaryRaiseAmount.value);
            ToggleWorkerInfo(false);
            ButtonGiveSalaryRaise.interactable = false;
            SliderSalaryRaiseAmount.interactable = false;
            ProgressBarSatisfaction.MinimumValue = 0f;
            ProgressBarSatisfaction.MaximumValue = 100f;
            ProgressBarSatisfaction.Value = 0f;
        }

        private void SubscribeToWorkerEvents(LocalWorker companyWorker)
        {
            companyWorker.SatisfactionChanged += OnCompanyWorkerSatisfactionChanged;
            companyWorker.DaysInCompanyChanged += OnCompanyWorkerDaysInCompanyChanged;
            companyWorker.SalaryChanged += OnCompanyWorkerSalaryChanged;
            companyWorker.ExpierienceTimeChanged += OnCompanyWorkerExpierienceTimeChanged;
        }

        private void UnsubscribeFromWorkerEvents(LocalWorker companyWorker)
        {
            companyWorker.SatisfactionChanged -= OnCompanyWorkerSatisfactionChanged;
            companyWorker.DaysInCompanyChanged -= OnCompanyWorkerDaysInCompanyChanged;
            companyWorker.SalaryChanged -= OnCompanyWorkerSalaryChanged;
            companyWorker.ExpierienceTimeChanged -= OnCompanyWorkerExpierienceTimeChanged;
        }

        private void RemoveWorkerListViewElement(LocalWorker worker, ControlListView listView)
        {
            ListViewElement element = listView.FindElement(worker);
            Button buttonComponent = element.GetComponent<Button>();
            WorkersButtonsSelector.RemoveButton(buttonComponent);
            ListViewCompanyWorkers.RemoveControl(element.gameObject);
            worker.SatisfactionChanged -= OnWorkerSatisfactionChanged;
        }

        private void SetTextListViewWorkers()
        {
            TextListViewWorkers.text =
                UIWorkers.GetCompanyWorkersListViewString(SimulationManagerComponent.ControlledCompany);
        }

        private string CreateWorkerInfoString(LocalWorker companyWorker)
        {
            string workerInfo = string.Format("Name: {0}\n" +
                                              "Surename: {1}\n" +
                                              "Abilities: ",
                                              companyWorker.Name, companyWorker.Surename);

            foreach (KeyValuePair<ProjectTechnology, SafeFloat> workerAbility in companyWorker.Abilites)
            {
                string abilityName = EnumToString.GetString(workerAbility.Key);
                workerInfo += string.Format("{0} {1} | ", abilityName, workerAbility.Value.Value.ToString("0.00"));
            }

            workerInfo += string.Format("\nSalary: {0}$\n" +
                                        "Satisfaction: {1}%\n" +
                                        "Days in company: {2}\n",
                                        companyWorker.Salary, companyWorker.Satiscation.ToString("0.00"),
                                        companyWorker.DaysInCompany);

            return workerInfo;
        }

        private void Awake()
        {
            InfoWindowComponent = InfoWindow.Instance;
            SimulationManagerComponent = SimulationManager.Instance;
        }

        private void Start()
        {
            SliderSalaryRaiseAmount.onValueChanged.AddListener(OnSalaryRaiseAmountSliderValueChanged);
            SimulationManagerComponent.ControlledCompany.WorkerAdded += OnControlledCompanyWorkerAdded;
            SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnControlledCompanyWorkerRemoved;
            WorkersButtonsSelector.SelectedButtonChanged += OnWorkersButtonsSelectorSelectedButtonChanged;
            InitWorkersView();
            SetTextListViewWorkers();
        }

        /// <summary>
        /// Toggles worker info on/off
        /// </summary>
        /// <param name="on">True to toggle on, false to toggle off</param>
        private void ToggleWorkerInfo(bool on)
        {
            TextName.gameObject.SetActive(on);
            TextSalary.gameObject.SetActive(on);
            TextExpierience.gameObject.SetActive(on);
            TextDaysInCompany.gameObject.SetActive(on);
            TextName.gameObject.SetActive(on);
            ProgressBarSatisfaction.Text.gameObject.SetActive(on);
            ProgressBarSatisfaction.Value = on ? ProgressBarSatisfaction.Value : 0f;
        }

        #region Event callbacks

        private void OnWorkerSatisfactionChanged(SharedWorker companyWorker)
        {
            LocalWorker localCompanyWorker = (LocalWorker)companyWorker;
            ListViewElement element = ListViewCompanyWorkers.FindElement(companyWorker);
            element.Text.text = GetWorkerListViewElementText(localCompanyWorker);
            ProgressBarSatisfaction.Value = localCompanyWorker.Satiscation;
        }

        private void OnWorkersButtonsSelectorSelectedButtonChanged(Button btn)
        {
            if (null != SelectedWorker)
            {
                UnsubscribeFromWorkerEvents(SelectedWorker);
            }

            if (null != btn)
            {
                ListViewElement element = btn.GetComponent<ListViewElement>();
                SelectedWorker = (LocalWorker)element.RepresentedObject;
                SubscribeToWorkerEvents(SelectedWorker);
                ButtonFireWorker.interactable = true;
                ButtonGiveSalaryRaise.interactable = true;
                SliderSalaryRaiseAmount.interactable = true;
                SetWorkerInfo();
                ToggleWorkerInfo(true);
                AbilitiesDisplay.DisplayWorkerAbilities(SelectedWorker);
            }
            else
            {
                if (null != SelectedWorker)
                {
                    UnsubscribeFromWorkerEvents(SelectedWorker);
                }

                SelectedWorker = null;
                ButtonFireWorker.interactable = false;
                ButtonGiveSalaryRaise.interactable = false;
                SliderSalaryRaiseAmount.interactable = false;
                ToggleWorkerInfo(false);
                AbilitiesDisplay.DisplayWorkerAbilities(null);
            }
        }

        private void OnCompanyWorkerSatisfactionChanged(SharedWorker worker)
        {
            ProgressBarSatisfaction.Text.text = UIWorkers.GetWorkerSatisfactionString((LocalWorker)worker);
        }

        private void OnControlledCompanyWorkerRemoved(SharedWorker worker)
        {
            LocalWorker companyWorker = (LocalWorker)worker;
            RemoveWorkerListViewElement(companyWorker, ListViewCompanyWorkers);
            SetTextListViewWorkers();
        }

        private void OnControlledCompanyWorkerAdded(SharedWorker worker)
        {
            LocalWorker companyWorker = (LocalWorker)worker;
            CreateWorkerListViewElement(companyWorker);
            SetTextListViewWorkers();
        }

        private void OnCompanyWorkerDaysInCompanyChanged(SharedWorker companyWorker)
        {
            TextDaysInCompany.text = UIWorkers.GetWorkerDaysInCompanyString((LocalWorker)companyWorker);
        }

        private void OnCompanyWorkerExpierienceTimeChanged(SharedWorker companyWorker)
        {
            TextExpierience.text = UIWorkers.GetWorkerExpierienceString(companyWorker);
        }

        private void OnCompanyWorkerSalaryChanged(SharedWorker companyWorker)
        {
            TextSalary.text = UIWorkers.GetWorkerSalaryString(companyWorker);
        }

        #endregion

        /*Public methods*/

        public void OnButtonFireWorkerClicked()
        {
            Button selectedButton = WorkersButtonsSelector.SelectedButton;
            ListViewElement element = selectedButton.GetComponent<ListViewElement>();
            LocalWorker workerToRemove = (LocalWorker)element.RepresentedObject;

            string infoWindowMsg = string.Format("Do you really want to fire {0} {1} ?",
                                                 workerToRemove.Name,
                                                 workerToRemove.Surename);
            InfoWindowComponent.ShowOkCancel(infoWindowMsg,
                                             () => { SimulationManagerComponent.ControlledCompany.RemoveWorker(workerToRemove); },
                                             null);
        }

        public void OnButtonGiveSalaryRaiseClicked()
        {
            int salaryRaiseAmount = (int)SliderSalaryRaiseAmount.value;
            Button selectedButton = WorkersButtonsSelector.SelectedButton;
            ListViewElement element = selectedButton.GetComponent<ListViewElement>();
            LocalWorker companyWorker = (LocalWorker)element.RepresentedObject;
            companyWorker.Salary = companyWorker.Salary + salaryRaiseAmount;
        }

        public void OnSalaryRaiseAmountSliderValueChanged(float value)
        {
            TextSliderSalaryRaiseAmount.text = string.Format("{0} $", value);
        }
    }
}
