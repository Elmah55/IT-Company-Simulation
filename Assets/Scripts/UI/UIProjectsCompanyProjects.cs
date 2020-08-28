using UnityEngine;
using ITCompanySimulation.Character;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using ITCompanySimulation.Developing;

namespace ITCompanySimulation.UI
{
    public class UIProjectsCompanyProjects : UIProjects
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private MainSimulationManager SimulationManagerComponent;
        [SerializeField]
        private GameTime GameTimeComponent;
        [SerializeField]
        private ListViewElementWorker ListViewWorkerElementPrefab;
        /// <summary>
        /// Colors that will be applied to selected button component of list view element.
        /// </summary>
        [SerializeField]
        private ColorBlock ListViewElementSelectedColors;
        [SerializeField]
        [Tooltip("Color of project's list view element after being completed")]
        private Color CompletedProjectListViewElementColors;
        [SerializeField]
        private ControlListView ListViewCompanyProjects;
        [SerializeField]
        private ControlListViewDrop ListViewAvailableWorkers;
        [SerializeField]
        private ControlListViewDrop ListViewAssignedWorkers;
        [SerializeField]
        private TextMeshProUGUI TextProjectName;
        [SerializeField]
        private TextMeshProUGUI TextProjectTechnologies;
        [SerializeField]
        private TextMeshProUGUI TextProjectEstimatedCompletionTime;
        [SerializeField]
        private TextMeshProUGUI TextProjectCompletionBonus;
        [SerializeField]
        private TextMeshProUGUI TextListViewCompanyProjects;
        [SerializeField]
        private TextMeshProUGUI TextListViewAvailableWorkers;
        [SerializeField]
        private TextMeshProUGUI TextListViewAssignedWorkers;
        [SerializeField]
        private Button ButtonStartProject;
        [SerializeField]
        private Button ButtonStopProject;
        private RectTransform TransformComponent;
        /// <summary>
        /// Scrum object of project that is currently selected
        /// </summary>
        private Scrum SelectedScrum;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            ButtonSelectorProjects = new ButtonSelector(ListViewElementSelectedColors);
            TransformComponent = GetComponent<RectTransform>();

            //Add workers that were in company before of start of this script
            foreach (SharedWorker worker in SimulationManagerComponent.ControlledCompany.Workers)
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
            ButtonSelectorProjects.SelectedButtonChanged += OnProjectListViewSelectedElementChanged;
            ListViewAssignedWorkers.ControlAdded += OnListViewAssignedWorkersControlAdded;
            ListViewAssignedWorkers.ControlRemoved += OnListViewAssignedWorkersControlRemoved;
            GameTimeComponent.DayChanged += OnGameTimeComponentDayChanged;

            SetProjectInfo();
            SetProjectButtons();
            SetListViewAvailableWorkersText();
            SetListViewCompanyProjectsText();
            ListViewAssignedWorkers.transform.parent.gameObject.SetActive(false);
        }

        #region Events callbacks

        private void OnProjectProgressUpdated(LocalProject proj)
        {
            ListViewElementProject element = GetProjectListViewElement(ListViewCompanyProjects, proj);
            element.Text.text = GetProjectListViewElementText(proj);

            if (null != SelectedScrum
                && SelectedScrum.BindedProject == proj)
            {
                TextProjectEstimatedCompletionTime.text = string.Format("Estimated completion time: {0} days",
                                                             SelectedScrum.GetProjectEstimatedCompletionTime());
            }
        }

        private void OnProjectCompleted(LocalProject proj)
        {
            ListViewElementProject element = GetProjectListViewElement(ListViewCompanyProjects, proj);
            element.BackgroundImage.color = CompletedProjectListViewElementColors;
            SetProjectButtons();
        }

        //On control removed from list view check will be peformed to check
        //if control was removed because worker was removed from project.
        //If player moved worker's control to other list if condition will be true.
        //If worker got removed for other reason (i.e level of satisfaction fell
        //below threshold) if condition will be false
        private void OnListViewAssignedWorkersControlRemoved(GameObject ctrl)
        {
            SharedWorker worker = ctrl.GetComponent<ListViewElementWorker>().Worker;

            if (true == SelectedScrum.BindedProject.Workers.Contains(worker))
            {
                SelectedScrum.BindedProject.RemoveWorker((LocalWorker)worker);
            }
        }

        private void OnListViewAssignedWorkersControlAdded(GameObject ctrl)
        {
            SharedWorker worker = ctrl.GetComponent<ListViewElementWorker>().Worker;
            SelectedScrum.BindedProject.AddWorker((LocalWorker)worker);
        }

        private void OnControlledCompanyWorkerRemoved(SharedWorker companyWorker)
        {
            RemoveWorkerListViewElement(companyWorker);
            SetListViewAvailableWorkersText();
        }

        private void OnControlledCompanyWorkerAdded(SharedWorker companyWorker)
        {
            LocalWorker worker = (LocalWorker)companyWorker;
            ListViewElementWorker newElement =
                UIWorkers.CreateWorkerListViewElement(companyWorker, ListViewWorkerElementPrefab, TooltipComponent);
            UIElementDrag drag = newElement.GetComponent<UIElementDrag>();
            drag.DragParentTransform = gameObject.GetComponent<RectTransform>();
            newElement.Text.text = GetWorkerListViewElementText(worker);

            ListViewAvailableWorkers.AddControl(newElement.gameObject);

            SetListViewAvailableWorkersText();
        }

        private void OnControlledCompanyProjectAdded(Scrum scrumObj)
        {
            ListViewElementProject newElement = CreateListViewElement(scrumObj.BindedProject);
            newElement.Text.text = GetProjectListViewElementText(scrumObj.BindedProject);
            ButtonSelectorProjects.AddButton(newElement.Button);

            ListViewCompanyProjects.AddControl(newElement.gameObject);

            scrumObj.BindedProject.ProgressUpdated += OnProjectProgressUpdated;
            scrumObj.BindedProject.Completed += OnProjectCompleted;
            SetListViewCompanyProjectsText();
        }

        private void OnProjectListViewSelectedElementChanged(Button btn)
        {
            if (null != SelectedScrum)
            {
                UnsubscribeProjectEvents();

                foreach (LocalWorker worker in SelectedScrum.BindedProject.Workers)
                {
                    ListViewElementWorker element = UIWorkers.GetWorkerListViewElement(worker, ListViewAssignedWorkers);
                    element.gameObject.SetActive(false);
                }
            }

            if (null != btn)
            {
                ListViewElementProject element = btn.GetComponent<ListViewElementProject>();
                SelectedScrum = SimulationManagerComponent.ControlledCompany.ScrumProcesses.Find(x =>
                {
                    return element.Project == x.BindedProject;
                });

                SetProjectInfo();
                SetProjectButtons();
                SubscribeProjectEvents();
                SetListViewAssignedWorkersText();
                ListViewAssignedWorkers.transform.parent.gameObject.SetActive(true);

                foreach (LocalWorker worker in SelectedScrum.BindedProject.Workers)
                {
                    ListViewElementWorker elem = UIWorkers.GetWorkerListViewElement(worker, ListViewAssignedWorkers);
                    elem.gameObject.SetActive(true);
                }
            }
            else
            {
                SelectedScrum = null;
                SetProjectInfo();
                SetProjectButtons();
                ListViewAssignedWorkers.transform.parent.gameObject.SetActive(false);
            }
        }

        private void OnSelectedProjectWorkerAdded(SharedWorker companyWorker)
        {
            SetProjectButtons();
            SetListViewAssignedWorkersText();
            SetListViewAvailableWorkersText();
        }

        private void OnSelectedProjectWorkerRemoved(SharedWorker companyWorker)
        {
            ListViewElementWorker element = UIWorkers.GetWorkerListViewElement(companyWorker, ListViewAssignedWorkers);

            //Check if list view element was dragged to other list view
            if (null != element)
            {
                ListViewAssignedWorkers.RemoveControl(element.gameObject);
            }

            SetProjectButtons();
            SetListViewAssignedWorkersText();
            SetListViewAvailableWorkersText();
        }

        private void OnSelectedProjectStopped(LocalProject proj)
        {
            SetProjectButtons();
        }

        private void OnSelectedProjectStarted(LocalProject proj)
        {
            SetProjectButtons();
        }

        private void OnGameTimeComponentDayChanged()
        {
            foreach (GameObject listViewObject in ListViewAssignedWorkers.Controls)
            {
                ListViewElementWorker element = listViewObject.GetComponent<ListViewElementWorker>();
                element.Text.text = GetWorkerListViewElementText((LocalWorker)element.Worker);
            }

            foreach (GameObject listViewObject in ListViewAvailableWorkers.Controls)
            {
                ListViewElementWorker element = listViewObject.GetComponent<ListViewElementWorker>();
                element.Text.text = GetWorkerListViewElementText((LocalWorker)element.Worker);
            }
        }

        #endregion

        private void SetProjectButtons()
        {
            ButtonStartProject.interactable = (null != SelectedScrum
                && false == SelectedScrum.BindedProject.Active
                && false == SelectedScrum.BindedProject.IsCompleted)
                && SelectedScrum.BindedProject.Workers.Count > 0;

            ButtonStopProject.interactable = (null != SelectedScrum
                && true == SelectedScrum.BindedProject.Active);
        }

        private void SetProjectInfo()
        {
            if (null != SelectedScrum)
            {
                TextProjectName.text =
            string.Format("Name: {0}", SelectedScrum.BindedProject.Name);
                TextProjectCompletionBonus.text =
                    string.Format("Completion bonus: {0} $", SelectedScrum.BindedProject.CompleteBonus);

                TextProjectTechnologies.text =
                    string.Format("Used technologies: {0}", GetProjectTechnologiesString(SelectedScrum.BindedProject));

                int estimatedCompletion = SelectedScrum.GetProjectEstimatedCompletionTime();
                string estimatedCompletionStr;

                if (-1 == estimatedCompletion)
                {
                    estimatedCompletionStr = "Estimated completion time:";
                }
                else
                {
                    estimatedCompletionStr = string.Format("Estimated completion time: {0} days",
                                                           estimatedCompletion);
                }

                TextProjectEstimatedCompletionTime.text = estimatedCompletionStr;
            }
            else
            {
                TextProjectName.text = "Name:";
                TextProjectCompletionBonus.text = "Completion bonus:";
                TextProjectTechnologies.text = "Used technologies:";
                TextProjectEstimatedCompletionTime.text = "Estimated completion time:";
            }
        }

        private void RemoveWorkerListViewElement(SharedWorker companyWorker)
        {
            LocalWorker worker = (LocalWorker)companyWorker;
            ControlListViewDrop workerListView = (null == worker.AssignedProject) ? ListViewAvailableWorkers : ListViewAssignedWorkers;
            ListViewElementWorker listViewElement = UIWorkers.GetWorkerListViewElement(companyWorker, workerListView);

            //Worker might be already removed by other callback
            if (null != listViewElement)
            {
                workerListView.RemoveControl(listViewElement.gameObject);
            }
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
            SelectedScrum.BindedProject.WorkerRemoved += OnSelectedProjectWorkerRemoved;
            SelectedScrum.BindedProject.WorkerAdded += OnSelectedProjectWorkerAdded;
            SelectedScrum.BindedProject.Stopped += OnSelectedProjectStopped;
            SelectedScrum.BindedProject.Started += OnSelectedProjectStarted;
        }

        private void UnsubscribeProjectEvents()
        {
            SelectedScrum.BindedProject.WorkerRemoved -= OnSelectedProjectWorkerRemoved;
            SelectedScrum.BindedProject.WorkerAdded -= OnSelectedProjectWorkerAdded;
            SelectedScrum.BindedProject.Stopped -= OnSelectedProjectStopped;
            SelectedScrum.BindedProject.Started -= OnSelectedProjectStarted;
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

        public void OnButtonStartProjectClicked()
        {
            SelectedScrum.StartProject();
        }

        public void OnButtonStopProjectClicked()
        {
            SelectedScrum.StartProject();
        }
    }
}
