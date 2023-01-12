using UnityEngine;
using ITCompanySimulation.Character;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using ITCompanySimulation.Project;
using ITCompanySimulation.Core;
using UnityEngine.Events;

namespace ITCompanySimulation.UI
{
    public class UIProjectsCompanyProjects : UIProjects
    {
        /*Private consts fields*/

        /*Private fields*/

        private SimulationManager SimulationManagerComponent;
        private GameTime GameTimeComponent;
        [SerializeField]
        private DraggableListViewElement ListViewWorkerElementPrefab;
        [SerializeField]
        private ControlListView ListViewCompanyProjects;
        [SerializeField]
        private ControlListViewDrop ListViewAvailableWorkers;
        [SerializeField]
        private ControlListViewDrop ListViewAssignedWorkers;
        [SerializeField]
        private TextMeshProUGUI TextProjectEstimatedCompletionTime;
        [SerializeField]
        private TextMeshProUGUI TextListViewCompanyProjects;
        [SerializeField]
        private TextMeshProUGUI TextListViewAvailableWorkers;
        [SerializeField]
        private TextMeshProUGUI TextListViewAssignedWorkers;
        [SerializeField]
        private TextMeshProUGUI TextProgressBarProject;
        [SerializeField]
        private ProgressBar ProgressBarProject;
        [SerializeField]
        private GameObject PanelAssignWorkers;
        [SerializeField]
        private Button ButtonAssignWorkers;
        [SerializeField]
        private Button ButtonCancelProject;
        private InfoWindow InfoWindowComponent;
        /// <summary>
        /// Scrum object of project that is currently selected
        /// </summary>
        private Scrum SelectedScrum;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            InfoWindowComponent = InfoWindow.Instance;
            SimulationManagerComponent = SimulationManager.Instance;
            GameTimeComponent = SimulationManagerComponent.gameObject.GetComponent<GameTime>();
        }

        private void Start()
        {
            ButtonSelectorProjects = new ButtonSelector();

            //Add workers that were in company before of start of this script
            foreach (LocalWorker worker in SimulationManagerComponent.ControlledCompany.Workers)
            {
                OnControlledCompanyWorkerAdded(worker);
            }

            //Add projects that were in company before of start of this script
            foreach (Scrum scm in SimulationManagerComponent.ControlledCompany.ScrumProcesses)
            {
                OnControlledCompanyProjectAdded(scm);
            }

            SimulationManagerComponent.ControlledCompany.WorkerAdded += OnControlledCompanyWorkerAdded;
            SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnControlledCompanyWorkerRemoved;
            SimulationManagerComponent.ControlledCompany.ProjectAdded += OnControlledCompanyProjectAdded;
            SimulationManagerComponent.ControlledCompany.ProjectRemoved += OnControlledCompanyProjectRemoved;
            ButtonSelectorProjects.SelectedButtonChanged += OnProjectListViewSelectedElementChanged;
            ListViewAssignedWorkers.ControlAdded += OnListViewAssignedWorkersControlAdded;
            ListViewAssignedWorkers.ControlRemoved += OnListViewAssignedWorkersControlRemoved;
            GameTimeComponent.DayChanged += OnGameTimeComponentDayChanged;

            SetProjectInfo();
            SetListViewAvailableWorkersText();
            SetListViewCompanyProjectsText();
            ListViewAssignedWorkers.transform.parent.gameObject.SetActive(false);
            ProgressBarProject.MinimumValue = 0f;
            ProgressBarProject.MaximumValue = 100f;
            ProgressBarProject.Value = ProgressBarProject.MinimumValue;
        }

        private void OnDisable()
        {
            PanelAssignWorkers.SetActive(false);
        }

        #region Events callbacks

        private void OnProjectProgressUpdated(LocalProject proj)
        {
            ListViewElement element = ListViewCompanyProjects.FindElement(proj);
            element.Text.text = GetProjectListViewElementText(proj);
            SetListViewElementProgressBar(proj);

            if (null != SelectedScrum
                && SelectedScrum.BindedProject == proj)
            {
                int estimatedCompletionTime = SelectedScrum.GetProjectEstimatedCompletionTime();
                TextProjectEstimatedCompletionTime.text = GetEstimatedCompletionTimeText(estimatedCompletionTime);
                SetProjectProgressBar(proj);
            }
        }

        private void OnListViewAssignedWorkersControlRemoved(GameObject ctrl)
        {
            LocalWorker worker = (LocalWorker)ctrl.GetComponent<ListViewElement>().RepresentedObject;

            //On control removed from list view check will be peformed to check
            //if control was removed because worker was removed from project.
            //If player moved worker's control to other list if condition will be true.
            //If worker got removed for other reason (i.e level of satisfaction fell
            //below threshold) if condition will be false
            if (true == SelectedScrum.BindedProject.Workers.Contains(worker))
            {
                SelectedScrum.BindedProject.RemoveWorker(worker);
            }
        }

        private void OnListViewAssignedWorkersControlAdded(GameObject ctrl)
        {
            LocalWorker worker = (LocalWorker)ctrl.GetComponent<ListViewElement>().RepresentedObject;
            SelectedScrum.BindedProject.AddWorker(worker);
        }

        private void OnControlledCompanyWorkerRemoved(SharedWorker companyWorker)
        {
            RemoveWorkerListViewElement(companyWorker);
            SetListViewAvailableWorkersText();
        }

        private void OnControlledCompanyWorkerAdded(LocalWorker companyWorker)
        {
            ListViewElement newElement = CreateWorkerListViewElement(companyWorker);
            ListViewAvailableWorkers.AddControl(newElement.gameObject);
            SetListViewAvailableWorkersText();
        }

        private ListViewElement CreateWorkerListViewElement(LocalWorker companyWorker)
        {
            DraggableListViewElement newElement =
                (DraggableListViewElement)UIWorkers.CreateWorkerListViewElement(companyWorker, ListViewWorkerElementPrefab, TooltipComponent);

            newElement.BeginDragParentTransform = (RectTransform)gameObject.transform;
            newElement.gameObject.SetActive(true);
            newElement.Text.text = GetWorkerListViewElementText(companyWorker);
            newElement.RepresentedObject = companyWorker;

            return newElement;
        }

        private void OnControlledCompanyProjectAdded(Scrum scrumObj)
        {
            if (false == scrumObj.BindedProject.IsCompleted)
            {
                ListViewElement newElement = CreateListViewElement(scrumObj.BindedProject);
                newElement.Text.text = GetProjectListViewElementText(scrumObj.BindedProject);
                newElement.FrontImage.sprite = scrumObj.BindedProject.Icon;
                ButtonSelectorProjects.AddButton(newElement.Button);
                ProgressBar listViewElementProgressBar = newElement.GetComponentInChildren<ProgressBar>();
                listViewElementProgressBar.Value = scrumObj.BindedProject.Progress;

                ListViewCompanyProjects.AddControl(newElement.gameObject);

                scrumObj.BindedProject.ProgressUpdated += OnProjectProgressUpdated;
                scrumObj.BindedProject.WorkerRemoved += OnProjectWorkerRemoved;
                SetListViewCompanyProjectsText();
            }
        }

        private void OnControlledCompanyProjectRemoved(Scrum scrumObj)
        {
            ListViewElement projectListViewElement = ListViewCompanyProjects.FindElement(scrumObj.BindedProject);
            Button listViewElementButton = projectListViewElement.GetComponent<Button>();
            ButtonSelectorProjects.RemoveButton(listViewElementButton);
            ListViewCompanyProjects.RemoveControl(projectListViewElement.gameObject);
            SetListViewCompanyProjectsText();
        }

        private void OnProjectListViewSelectedElementChanged(Button btn)
        {
            if (null != SelectedScrum)
            {
                UnsubscribeProjectEvents();

                foreach (LocalWorker worker in SelectedScrum.BindedProject.Workers)
                {
                    ListViewElement element = ListViewAssignedWorkers.FindElement(worker);
                    element.gameObject.SetActive(false);
                }
            }

            if (null != btn)
            {
                ListViewElement element = btn.GetComponent<ListViewElement>();
                SelectedScrum = SimulationManagerComponent.ControlledCompany.ScrumProcesses.Find(x =>
                {
                    return element.RepresentedObject == x.BindedProject;
                });

                SetProjectInfo();
                SubscribeProjectEvents();
                SetListViewAssignedWorkersText();

                foreach (LocalWorker worker in SelectedScrum.BindedProject.Workers)
                {
                    ListViewElement elem = ListViewAssignedWorkers.FindElement(worker);
                    elem.gameObject.SetActive(true);
                }

                ButtonAssignWorkers.interactable = true;
                ButtonCancelProject.interactable = true;
            }
            else
            {
                ButtonAssignWorkers.interactable = false;
                ButtonCancelProject.interactable = false;
                SelectedScrum = null;
                SetProjectInfo();
                PanelAssignWorkers.SetActive(false);
            }
        }

        //In current implementation player will drag list view elements
        //so this callback will be fired when list view element is already placed
        //in correct list view, no need to add it
        private void OnSelectedProjectWorkerAdded(SharedWorker companyWorker)
        {
            SetListViewAvailableWorkersText();
            SetListViewAssignedWorkersText();
        }

        private void OnProjectWorkerRemoved(SharedWorker companyWorker)
        {
            ListViewElement element = ListViewAssignedWorkers.FindElement(companyWorker);

            //Check if list view element was dragged to other list view
            if (null != element)
            {
                RemoveWorkerListViewElement(element, ListViewAssignedWorkers);
                ListViewElement newElement = CreateWorkerListViewElement((LocalWorker)companyWorker);
                ListViewAvailableWorkers.AddControl(newElement.gameObject);
            }

            SetListViewAssignedWorkersText();
            SetListViewAvailableWorkersText();
            SetProjectInfo();
        }

        private void OnGameTimeComponentDayChanged()
        {
            foreach (GameObject listViewObject in ListViewAssignedWorkers.Controls)
            {
                ListViewElement element = listViewObject.GetComponent<ListViewElement>();
                element.Text.text = GetWorkerListViewElementText((LocalWorker)element.RepresentedObject);
            }

            foreach (GameObject listViewObject in ListViewAvailableWorkers.Controls)
            {
                ListViewElement element = listViewObject.GetComponent<ListViewElement>();
                element.Text.text = GetWorkerListViewElementText((LocalWorker)element.RepresentedObject);
            }
        }

        private void OnSelectedProjectCompletionTimeUpdated(SharedProject proj)
        {
            ListViewElement element = ListViewCompanyProjects.FindElement(proj);
            element.Text.text = GetProjectListViewElementText((LocalProject)proj);
        }

        #endregion

        private void SetProjectProgressBar(LocalProject proj)
        {
            if (null != proj)
            {
                ProgressBarProject.Value = proj.Progress;
                TextProgressBarProject.text = string.Format("Progress {0} %", proj.Progress.ToString("0.00"));
            }
            else
            {
                ProgressBarProject.Value = 0f;
                TextProgressBarProject.text = "Progress";
            }
        }

        /// <summary>
        /// Sets progress bar located on list view element
        /// </summary>
        private void SetListViewElementProgressBar(LocalProject proj)
        {
            //Progress bar located on list view element
            ProgressBar listViewProgressBar = ListViewCompanyProjects.FindElement(proj).GetComponentInChildren<ProgressBar>();

            if (null != proj)
            {
                listViewProgressBar.Value = proj.Progress;
            }
        }

        private void SetProjectInfoText(LocalProject proj)
        {
            if (null != proj)
            {
                TextProjectName.text = string.Format("Project name: {0}", proj.Name);
                TextCompleteBonus.text = string.Format("Completion bonus: <color=#4CD137>+ {0} $</color>", proj.CompletionBonus);
                TextPenalty.text = string.Format("Exceeding completion time penalty: <color=#E84118>- {0} $</color>", proj.CompletionTimeExceededPenalty);
                TextUsedTechnologies.text = string.Format("Used technologies: {0}", GetProjectTechnologiesString(proj));
                TextCompletionTime.text = string.Format("Time for completion: {0} days", proj.CompletionTime);
                TextProjectEstimatedCompletionTime.text = GetEstimatedCompletionTimeText(SelectedScrum.GetProjectEstimatedCompletionTime());
            }
            else
            {
                TextProjectName.text = string.Empty;
                TextCompleteBonus.text = string.Empty;
                TextUsedTechnologies.text = string.Empty;
                TextCompletionTime.text = string.Empty;
                TextPenalty.text = string.Empty;
                TextProjectEstimatedCompletionTime.text = string.Empty;
            }
        }

        private void SetProjectInfo()
        {
            SetProjectInfoText(SelectedScrum?.BindedProject);
            SetProjectProgressBar(SelectedScrum?.BindedProject);
        }

        private void RemoveWorkerListViewElement(ListViewElement element, ControlListView listView)
        {
            listView.RemoveControl(element.gameObject);
        }

        private void RemoveWorkerListViewElement(SharedWorker companyWorker)
        {
            LocalWorker worker = (LocalWorker)companyWorker;
            ControlListViewDrop workerListView = (null == worker.AssignedProject) ? ListViewAvailableWorkers : ListViewAssignedWorkers;
            ListViewElement listViewElement = workerListView.FindElement(companyWorker);
            RemoveWorkerListViewElement(listViewElement, workerListView);
        }

        private string GetEstimatedCompletionTimeText(int days)
        {
            string estimatedCompletionStr = "Estimated completion time: ";

            if (-1 != days)
            {
                estimatedCompletionStr = string.Format("Estimated completion time: {0} days",
                                                       days);
            }

            return estimatedCompletionStr;
        }

        private string GetWorkerListViewElementText(LocalWorker worker)
        {
            string elementText;
            string absenceString = string.Empty;

            if (false == worker.Available)
            {
                switch (worker.AbsenceReason)
                {
                    case WorkerAbsenceReason.Sickness:
                        absenceString = "Sick";
                        break;
                    case WorkerAbsenceReason.Holiday:
                        absenceString = "On holidays";
                        break;
                    default:
                        break;
                }
            }

            elementText = string.Format("{0} {1}\n{2} days of expierience\n{3}",
                                                 worker.Name,
                                                 worker.Surename,
                                                 worker.ExperienceTime,
                                                 absenceString);
            return elementText;
        }

        private void SubscribeProjectEvents()
        {
            SelectedScrum.BindedProject.WorkerAdded += OnSelectedProjectWorkerAdded;
            SelectedScrum.BindedProject.CompletionTimeUpdated += OnSelectedProjectCompletionTimeUpdated;
        }

        private void UnsubscribeProjectEvents()
        {
            SelectedScrum.BindedProject.WorkerAdded -= OnSelectedProjectWorkerAdded;
            SelectedScrum.BindedProject.CompletionTimeUpdated -= OnSelectedProjectCompletionTimeUpdated;
        }

        private void SetListViewAssignedWorkersText()
        {
            TextListViewAssignedWorkers.text = string.Format(
                "Assigned workers ({0})",
                SelectedScrum.BindedProject.Workers.Count);
        }

        private void SetListViewCompanyProjectsText()
        {
            TextListViewCompanyProjects.text = string.Format(
                "Company projects ({0})",
                SimulationManagerComponent.ControlledCompany.ScrumProcesses.Count);
        }

        private void SetListViewAvailableWorkersText()
        {
            int availableWorkersCount =
                SimulationManagerComponent.ControlledCompany.Workers.Count(
                x => x.AssignedProject == null);

            TextListViewAvailableWorkers.text = string.Format(
                "Available workers ({0})",
                availableWorkersCount);
        }

        /*Public methods*/

        public void OnButtonCancelProjectClicked()
        {
            string infoWindowTxt = string.Format("Do you really want to cancel project {0} ? " +
                                                 "Your company will be charged {1} $ of penalty.",
                                                 SelectedScrum.BindedProject.Name,
                                                 SelectedScrum.BindedProject.CompletionTimeExceededPenalty);
            UnityAction okAction = () =>
              {
                  SimulationManagerComponent.ControlledCompany.RemoveProject(SelectedScrum.BindedProject);
              };

            InfoWindowComponent.ShowOkCancel(infoWindowTxt, okAction, null);
        }
    }
}
