using ITCompanySimulation.Character;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ITCompanySimulation.Core;
using ITCompanySimulation.Utilities;

namespace ITCompanySimulation.UI
{
    public class UIWorkersCompanyWorkers : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private IButtonSelector WorkersButtonsSelector = new ButtonSelector();
        [SerializeField]
        private ListViewElementWorker WorkerListViewElementPrefab;
        [SerializeField]
        private ControlListView ListViewWorkers;
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
        private TextMeshProUGUI TextAbilities;
        [SerializeField]
        private TextMeshProUGUI TextSalary;
        [SerializeField]
        private TextMeshProUGUI TextDaysInCompany;
        [SerializeField]
        private TextMeshProUGUI TextExpierience;
        [SerializeField]
        private TextMeshProUGUI TextSatisfaction;
        [SerializeField]
        private TextMeshProUGUI TextListViewWorkers;
        [SerializeField]
        private MainSimulationManager SimulationManagerComponent;
        [SerializeField]
        private Tooltip TooltipComponent;
        [SerializeField]
        private InfoWindow InfoWindowComponent;
        private LocalWorker SelectedWorker;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /// <summary>
        /// Set values of text component with information about worker
        /// </summary>
        private void SetWorkerText()
        {
            TextName.text = UIWorkers.GetWorkerNameString(SelectedWorker);
            SetWorkerAbilitiesText();
            TextSalary.text = UIWorkers.GetWorkerSalaryString(SelectedWorker);
            TextExpierience.text = UIWorkers.GetWorkerExpierienceString(SelectedWorker);
            TextDaysInCompany.text = UIWorkers.GetWorkerDaysInCompanyString(SelectedWorker);
            TextSatisfaction.text = UIWorkers.GetWorkerSatisfactionString(SelectedWorker);
        }

        private void SetWorkerAbilitiesText()
        {
            TextAbilities.text = UIWorkers.GetWorkerAbilitiesString(SelectedWorker);
            RectTransform transform = TextAbilities.transform.parent.GetComponent<RectTransform>();
            Vector2 newSize = new Vector2(transform.rect.width, TextAbilities.preferredHeight);
            transform.sizeDelta = newSize;
        }

        private string GetWorkerListViewElementText(LocalWorker worker)
        {
            return string.Format("{0} {1}\nSatisfaction {2} %\n{3} $ / Month",
                worker.Name, worker.Surename, worker.Satiscation.ToString("0.00"), worker.Salary);
        }

        private ListViewElementWorker CreateWorkerListViewElement(LocalWorker companyWorker)
        {
            ListViewElementWorker element =
                UIWorkers.CreateWorkerListViewElement(companyWorker, WorkerListViewElementPrefab, TooltipComponent);
            element.Text.text = GetWorkerListViewElementText(companyWorker);
            Button buttonComponent = element.GetComponent<Button>();

            WorkersButtonsSelector.AddButton(buttonComponent);
            ListViewWorkers.AddControl(element.gameObject);

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
            ToggleWorkerInfoText(false);
            ButtonGiveSalaryRaise.interactable = false;
            SliderSalaryRaiseAmount.interactable = false;
        }

        private void SubscribeToWorkerEvents(LocalWorker companyWorker)
        {
            companyWorker.AbilityUpdated += OnCompanyWorkerAbilityUpdated;
            companyWorker.SatisfactionChanged += OnCompanyWorkerSatisfactionChanged;
            companyWorker.DaysInCompanyChanged += OnCompanyWorkerDaysInCompanyChanged;
            companyWorker.SalaryChanged += OnCompanyWorkerSalaryChanged;
            companyWorker.ExpierienceTimeChanged += OnCompanyWorkerExpierienceTimeChanged;
        }

        private void UnsubscribeFromWorkerEvents(LocalWorker companyWorker)
        {
            companyWorker.AbilityUpdated -= OnCompanyWorkerAbilityUpdated;
            companyWorker.SatisfactionChanged -= OnCompanyWorkerSatisfactionChanged;
            companyWorker.DaysInCompanyChanged -= OnCompanyWorkerDaysInCompanyChanged;
            companyWorker.SalaryChanged -= OnCompanyWorkerSalaryChanged;
            companyWorker.ExpierienceTimeChanged -= OnCompanyWorkerExpierienceTimeChanged;
        }

        private void OnCompanyWorkerAbilityUpdated(SharedWorker worker, ProjectTechnology workerAbility, float workerAbilityValue)
        {
            LocalWorker companyWorker = (LocalWorker)worker;
            SetWorkerAbilitiesText();
        }

        private void RemoveWorkerListViewElement(LocalWorker worker, ControlListView listView)
        {
            ListViewElementWorker element = UIWorkers.GetWorkerListViewElement(worker, listView);
            Button buttonComponent = element.GetComponent<Button>();
            WorkersButtonsSelector.RemoveButton(buttonComponent);
            ListViewWorkers.RemoveControl(element.gameObject);
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
                string abilityName = EnumToString.ProjectTechnologiesStrings[workerAbility.Key];
                workerInfo += string.Format("{0} {1} | ", abilityName, workerAbility.Value.Value.ToString("0.00"));
            }

            workerInfo += string.Format("\nSalary: {0}$\n" +
                                        "Satisfaction: {1}%\n" +
                                        "Days in company: {2}\n",
                                        companyWorker.Salary, companyWorker.Satiscation.ToString("0.00"),
                                        companyWorker.DaysInCompany);

            return workerInfo;
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
        /// Toggles worker info text on/off
        /// </summary>
        /// <param name="on">True to toggle on, false to toggle off</param>
        private void ToggleWorkerInfoText(bool on)
        {
            TextName.gameObject.SetActive(on);
            TextAbilities.gameObject.SetActive(on);
            TextSalary.gameObject.SetActive(on);
            TextExpierience.gameObject.SetActive(on);
            TextDaysInCompany.gameObject.SetActive(on);
            TextName.gameObject.SetActive(on);
            TextSatisfaction.gameObject.SetActive(on);
        }

        #region Event callbacks

        private void OnWorkerSatisfactionChanged(SharedWorker companyWorker)
        {
            ListViewElementWorker element = UIWorkers.GetWorkerListViewElement(companyWorker, ListViewWorkers);
            element.Text.text = GetWorkerListViewElementText((LocalWorker)companyWorker);
        }

        private void OnWorkersButtonsSelectorSelectedButtonChanged(Button btn)
        {
            if (null != SelectedWorker)
            {
                UnsubscribeFromWorkerEvents(SelectedWorker);
            }

            if (null != btn)
            {
                ListViewElementWorker element = btn.GetComponent<ListViewElementWorker>();
                SelectedWorker = (LocalWorker)element.Worker;
                SubscribeToWorkerEvents(SelectedWorker);
                ButtonFireWorker.interactable = true;
                ButtonGiveSalaryRaise.interactable = true;
                SliderSalaryRaiseAmount.interactable = true;
                SetWorkerText();
                ToggleWorkerInfoText(true);
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
                ToggleWorkerInfoText(false);
            }
        }

        private void OnCompanyWorkerSatisfactionChanged(SharedWorker worker)
        {
            TextSatisfaction.text = UIWorkers.GetWorkerSatisfactionString((LocalWorker)worker);
        }

        private void OnControlledCompanyWorkerRemoved(SharedWorker worker)
        {
            LocalWorker companyWorker = (LocalWorker)worker;
            RemoveWorkerListViewElement(companyWorker, ListViewWorkers);
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
            Button selectedButton = WorkersButtonsSelector.GetSelectedButton();
            ListViewElementWorker element = selectedButton.GetComponent<ListViewElementWorker>();
            LocalWorker workerToRemove = (LocalWorker)element.Worker;

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
            Button selectedButton = WorkersButtonsSelector.GetSelectedButton();
            ListViewElementWorker element = selectedButton.GetComponent<ListViewElementWorker>();
            LocalWorker companyWorker = (LocalWorker)element.Worker;
            companyWorker.Salary = companyWorker.Salary + salaryRaiseAmount;
        }

        public void OnSalaryRaiseAmountSliderValueChanged(float value)
        {
            TextSliderSalaryRaiseAmount.text = string.Format("{0} $", value);
        }
    }
}
