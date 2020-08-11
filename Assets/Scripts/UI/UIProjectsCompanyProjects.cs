using System.Collections.Generic;
using UnityEngine;
using ITCompanySimulation.Character;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using ITCompanySimulation.Developing;
using System.Text;

namespace ITCompanySimulation.UI
{
    public class UIProjectsCompanyProjects : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private MainSimulationManager SimulationManagerComponent;
        [SerializeField]
        private GameTime GameTimeComponent;
        [SerializeField]
        private ListViewElement ListViewWorkerElementPrefab;
        [SerializeField]
        private ListViewElement ListViewProjectElementPrefab;
        /// <summary>
        /// Colors that will be applied to button component of list view element.
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
        /// Maps worker object to list view element that represents it
        /// </summary>
        private Dictionary<LocalWorker, ListViewElement> WorkerListViewMap;
        /// <summary>
        /// Maps worker object to list view element that represents it
        /// </summary>
        private Dictionary<Scrum, ListViewElement> ScrumListViewMap;
        private IButtonSelector ButtonSelectorProjects;
        /// <summary>
        /// Scrum object of project that is currently selected
        /// </summary>
        private Scrum SelectedScrum;
        private static StringBuilder StrBuilder = new StringBuilder();

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

        private void OnProjectProgressUpdated(LocalProject proj)
        {
            ListViewElement e = ScrumListViewMap.First(x => x.Key.BindedProject == proj).Value;
            e.Text.text = GetProjectListViewElementText(proj);

            if (null != SelectedScrum
                && SelectedScrum.BindedProject == proj)
            {
                TextProjectEstimatedCompletionTime.text = string.Format("Estimated completion time: {0} days",
                                                             SelectedScrum.GetProjectEstimatedCompletionTime());
            }
        }

        private void OnProjectCompleted(LocalProject proj)
        {
            ListViewElement e = ScrumListViewMap.First(
                x => x.Key.BindedProject == proj).Value;
            e.BackgroundImage.color = CompletedProjectListViewElementColors;
            SetProjectButtons();
        }

        #region Events callbacks

        //On control removed from list view check will be peformed to check
        //if control was removed because worker was removed from project.
        //If player moved worker's control to other list if condition will be true.
        //If worker got removed for other reason (i.e level of satisfaction fell
        //below threshold) if condition will be false

        private void OnListViewAssignedWorkersControlRemoved(GameObject ctrl)
        {
            LocalWorker worker = WorkerListViewMap.First(
                x => x.Value.gameObject == ctrl).Key;

            if (true == SelectedScrum.BindedProject.Workers.Contains(worker))
            {
                SelectedScrum.BindedProject.RemoveWorker(worker);
            }
        }

        private void OnListViewAssignedWorkersControlAdded(GameObject ctrl)
        {
            LocalWorker worker = WorkerListViewMap.First(
                x => x.Value.gameObject == ctrl).Key;
            SelectedScrum.BindedProject.AddWorker(worker);
        }

        private void OnControlledCompanyWorkerRemoved(SharedWorker companyWorker)
        {
            RemoveWorkerListViewElement(companyWorker);
            SetListViewAvailableWorkersText();
        }

        private void OnControlledCompanyWorkerAdded(SharedWorker companyWorker)
        {
            AddWorkerListViewElement(companyWorker);
            SetListViewAvailableWorkersText();
        }

        private void OnControlledCompanyProjectAdded(Scrum scrumObj)
        {
            AddProjectListViewElement(scrumObj);
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
                    ListViewElement e = WorkerListViewMap[worker];
                    e.gameObject.SetActive(false);
                }
            }

            if (null != btn)
            {
                KeyValuePair<Scrum, ListViewElement> pair = ScrumListViewMap.First(
                    x => x.Value.gameObject == btn.gameObject);
                SelectedScrum = pair.Key;

                SetProjectInfo();
                SetProjectButtons();
                SubscribeProjectEvents();
                SetListViewAssignedWorkersText();
                ListViewAssignedWorkers.transform.parent.gameObject.SetActive(true);

                foreach (LocalWorker worker in SelectedScrum.BindedProject.Workers)
                {
                    ListViewElement e = WorkerListViewMap[worker];
                    e.gameObject.SetActive(true);
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
            ListViewElement e = WorkerListViewMap[(LocalWorker)companyWorker];

            if (ListViewAssignedWorkers.Controls.Contains(e.gameObject))
            {
                ListViewAssignedWorkers.RemoveControl(e.gameObject);
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
            foreach (KeyValuePair<LocalWorker,ListViewElement> item in WorkerListViewMap)
            {
                item.Value.Text.text = GetWorkerListViewElementText(item.Key);
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

                for (int i = 0; i < SelectedScrum.BindedProject.UsedTechnologies.Count; i++)
                {
                    ProjectTechnology pt = SelectedScrum.BindedProject.UsedTechnologies[i];
                    StrBuilder.Append(EnumToString.ProjectTechnologiesStrings[pt]);

                    if (i != SelectedScrum.BindedProject.UsedTechnologies.Count - 1)
                    {
                        StrBuilder.Append(" / ");
                    }
                }

                TextProjectTechnologies.text =
                    string.Format("Used technologies: {0}", StrBuilder.ToString());
                StrBuilder.Clear();

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
            ListViewElement listViewElement = WorkerListViewMap[worker];
            ControlListViewDrop workerListView = (null == worker.AssignedProject) ? ListViewAvailableWorkers : ListViewAssignedWorkers;
            workerListView.RemoveControl(listViewElement.gameObject);
            WorkerListViewMap.Remove(worker);
        }

        private void AddWorkerListViewElement(SharedWorker companyWorker)
        {
            LocalWorker worker = (LocalWorker)companyWorker;
            ListViewElement newElement = GameObject.Instantiate<ListViewElement>(ListViewWorkerElementPrefab);
            newElement.GetComponent<UIElementDrag>().DragParentTransform = TransformComponent;
            string elementText = GetWorkerListViewElementText(worker);
            newElement.GetComponentInChildren<TextMeshProUGUI>().text = elementText;

            if (null == WorkerListViewMap)
            {
                WorkerListViewMap = new Dictionary<LocalWorker, ListViewElement>();
            }

            WorkerListViewMap.Add(worker, newElement);

            if (null == worker.AssignedProject)
            {
                ListViewAvailableWorkers.AddControl(newElement.gameObject);
            }
            else
            {
                ListViewAssignedWorkers.AddControl(newElement.gameObject);
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

        private void AddProjectListViewElement(Scrum scrumObj)
        {
            ListViewElement newElement = GameObject.Instantiate<ListViewElement>(ListViewProjectElementPrefab);
            newElement.Text.text = GetProjectListViewElementText(scrumObj.BindedProject);

            if (null == ScrumListViewMap)
            {
                ScrumListViewMap = new Dictionary<Scrum, ListViewElement>();
            }

            ScrumListViewMap.Add(scrumObj, newElement);
            ListViewCompanyProjects.AddControl(newElement.gameObject);

            Button elementButton = newElement.GetComponent<Button>();
            ButtonSelectorProjects.AddButton(elementButton);
        }

        private string GetProjectListViewElementText(LocalProject proj)
        {
            return string.Format("{0}\nCompletion bonus: {1} $\nProgress: {2} %",
                                 proj.Name,
                                 proj.CompleteBonus,
                                 proj.Progress.ToString("0.00"));
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
