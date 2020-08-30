using UnityEngine;
using UnityEngine.UI;
using ITCompanySimulation.Developing;
using ITCompanySimulation.Utilities;
using TMPro;

namespace ITCompanySimulation.UI
{
    public class UIProjectsProjectsMarket : UIProjects
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private ListViewElementProject ListViewProjectElementPrefab;
        /// <summary>
        /// Colors that will be applied to selected button component of list view element.
        /// </summary>
        [SerializeField]
        private ColorBlock ListViewElementSelectedColors;
        [SerializeField]
        [Tooltip("Color of project's list view element after being completed")]
        private Color CompletedProjectListViewElementColors;
        [SerializeField]
        private MainSimulationManager SimulationManagerComponent;
        [SerializeField]
        private TextMeshProUGUI TextMarketProjects;
        [SerializeField]
        private TextMeshProUGUI TextCompanyProjects;
        [SerializeField]
        private TextMeshProUGUI TextProjectName;
        [SerializeField]
        private TextMeshProUGUI TextCompleteBonus;
        [SerializeField]
        private TextMeshProUGUI TextUsedTechnologies;
        [SerializeField]
        private ControlListView ListViewMarketProjects;
        [SerializeField]
        private ControlListView ListViewCompanyProjects;
        [SerializeField]
        private ProjectsMarket ProjectsMarketComponent;
        [SerializeField]
        private Button ButtonTakeProject;
        private SharedProject SelectedProject;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            ButtonSelectorProjects = new ButtonSelector(ListViewElementSelectedColors);

            foreach (SharedProject proj in ProjectsMarketComponent.Projects)
            {
                OnProjectsMarketProjectAdded(proj);
            }

            foreach (Scrum scm in SimulationManagerComponent.ControlledCompany.ScrumProcesses)
            {
                OnControlledCompanyProjectAdded(scm);
            }

            SimulationManagerComponent.ControlledCompany.ProjectAdded += OnControlledCompanyProjectAdded;
            ProjectsMarketComponent.ProjectAdded += OnProjectsMarketProjectAdded;
            ProjectsMarketComponent.ProjectRemoved += OnProjectsMarketProjectRemoved;
            ButtonSelectorProjects.SelectedButtonChanged += OnButtonSelectorProjectsSelectedButtonChanged;

            SetProjectInfoText(SelectedProject);
        }

        private void SetProjectInfoText(SharedProject proj)
        {
            if (null != proj)
            {
                TextProjectName.gameObject.SetActive(true);
                TextCompleteBonus.gameObject.SetActive(true);
                TextUsedTechnologies.gameObject.SetActive(true);

                TextProjectName.text = proj.Name;
                TextCompleteBonus.text = string.Format("{0} $", proj.CompleteBonus);
                TextUsedTechnologies.text = string.Format("{0}", GetProjectTechnologiesString(proj));
            }
            else
            {
                TextProjectName.gameObject.SetActive(false);
                TextCompleteBonus.gameObject.SetActive(false);
                TextUsedTechnologies.gameObject.SetActive(false);
            }
        }

        private void SetProjectMarketButton()
        {
            ButtonTakeProject.interactable = SelectedProject is SharedProject;
        }

        private void SetListViewMarketProjectsText()
        {
            TextMarketProjects.text = string.Format("Market projects ({0})",
                ProjectsMarketComponent.Projects.Count);
        }

        private void SetListViewCompanyProjectsText()
        {
            TextCompanyProjects.text = string.Format(
                "Company projects ({0})",
                SimulationManagerComponent.ControlledCompany.ScrumProcesses.Count);
        }

        #region Events callbacks

        private void OnButtonSelectorProjectsSelectedButtonChanged(Button btn)
        {
            if (null != btn)
            {
                ListViewElementProject el = btn.GetComponent<ListViewElementProject>();
                SelectedProject = el.Project;
            }
            else
            {
                SelectedProject = null;
            }

            SetProjectInfoText(SelectedProject);
            SetProjectMarketButton();
        }

        private void OnProjectsMarketProjectRemoved(SharedProject proj)
        {
            ListViewElementProject element = GetProjectListViewElement(ListViewMarketProjects, proj);

            if (null == ListViewElementPool)
            {
                ListViewElementPool = new ObjectPool<ListViewElementProject>();
            }

            element.gameObject.SetActive(false);
            ListViewMarketProjects.RemoveControl(element.gameObject, false);
            ButtonSelectorProjects.RemoveButton(element.Button);
            ListViewElementPool.AddObject(element);
            SetListViewMarketProjectsText();
        }

        private void OnProjectsMarketProjectAdded(SharedProject proj)
        {
            ListViewElementProject newElement = CreateListViewElement(proj);
            ButtonSelectorProjects.AddButton(newElement.Button);
            ListViewMarketProjects.AddControl(newElement.gameObject);
            newElement.Text.text = GetProjectListViewElementText(proj);
            SetListViewMarketProjectsText();
        }

        private void OnControlledCompanyProjectAdded(Scrum scrumObj)
        {
            ListViewElementProject newElement = CreateListViewElement(scrumObj.BindedProject);
            ButtonSelectorProjects.AddButton(newElement.GetComponent<Button>());
            ListViewCompanyProjects.AddControl(newElement.gameObject);
            newElement.Text.text = base.GetProjectListViewElementText(scrumObj.BindedProject);

            scrumObj.BindedProject.ProgressUpdated += OnProjectProgressUpdated;
            scrumObj.BindedProject.Completed += OnProjectCompleted;

            SetListViewCompanyProjectsText();
        }

        private void OnProjectCompleted(LocalProject proj)
        {
            ListViewElementProject element = GetProjectListViewElement(ListViewCompanyProjects, proj);
            element.BackgroundImage.color = CompletedProjectListViewElementColors;
        }

        private void OnProjectProgressUpdated(LocalProject proj)
        {
            ListViewElementProject element = GetProjectListViewElement(ListViewCompanyProjects, proj);
            element.Text.text = base.GetProjectListViewElementText(proj);
        }

        private string GetProjectListViewElementText(SharedProject proj)
        {
            return string.Format("{0}\nCompletion bonus: {1} $\n",
                                 proj.Name,
                                 proj.CompleteBonus);
        }

        #endregion

        /*Public methods*/

        public void OnButtonTakeProjectClicked()
        {
            LocalProject proj = new LocalProject(SelectedProject);
            ProjectsMarketComponent.RemoveProject(SelectedProject);
            SimulationManagerComponent.ControlledCompany.AddProject(proj);
        }
    }
}