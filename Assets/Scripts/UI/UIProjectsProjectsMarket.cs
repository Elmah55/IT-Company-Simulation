using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ITCompanySimulation.Developing;
using TMPro;
using ITCompanySimulation.Character;
using System;

namespace ITCompanySimulation.UI
{
    public class UIProjectsProjectsMarket : UIProjects
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private ListViewElementWorker ListViewProjectElementPrefab;
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
        private Dictionary<SharedProject, ListViewElementWorker> ProjectListViewMap;
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

        private void SetListViewCompanyProjectsText()
        {
            TextCompanyProjects.text = string.Format(
                "Company projects ({0})",
                SimulationManagerComponent.ControlledCompany.ScrumProcesses.Count);
        }

        private void SetProjectInfoText(SharedProject proj)
        {
            if (null != proj)
            {
                TextProjectName.gameObject.SetActive(true);
                TextCompleteBonus.gameObject.SetActive(true);
                TextUsedTechnologies.gameObject.SetActive(true);

                TextProjectName.text = string.Format("Name: {0}", proj.Name);
                TextCompleteBonus.text = string.Format("Complete bonus: {0} $", proj.CompleteBonus);
                TextUsedTechnologies.text = string.Format("Used technologies: {0}", GetProjectTechnologiesString(proj));
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

        #region Events callbacks

        private void OnButtonSelectorProjectsSelectedButtonChanged(Button btn)
        {
            if (null != btn)
            {
                ListViewElementWorker el = btn.GetComponent<ListViewElementWorker>();
                SelectedProject = ProjectListViewMap.First(x => x.Value == el).Key;
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
            ListViewElementWorker el = ProjectListViewMap[proj];
            ListViewMarketProjects.RemoveControl(el.gameObject);
            ProjectListViewMap.Remove(proj);

            TextMarketProjects.text = string.Format("Market projects ({0})", 
                ProjectsMarketComponent.Projects.Count);
        }

        private void OnProjectsMarketProjectAdded(SharedProject proj)
        {
            ListViewElementWorker newElement = CreateProjectListViewElement(proj, ListViewProjectElementPrefab);
            ButtonSelectorProjects.AddButton(newElement.GetComponent<Button>());

            if (null == ProjectListViewMap)
            {
                ProjectListViewMap = new Dictionary<SharedProject, ListViewElementWorker>();
            }

            ProjectListViewMap.Add(proj, newElement);
            ListViewMarketProjects.AddControl(newElement.gameObject);

            TextMarketProjects.text = string.Format("Market projects ({0})", 
                ProjectsMarketComponent.Projects.Count);
        }

        private void OnControlledCompanyProjectAdded(Scrum scrumObj)
        {
            ListViewElementWorker newElement = base.CreateProjectListViewElement(
                scrumObj.BindedProject, ListViewProjectElementPrefab);
            ButtonSelectorProjects.AddButton(newElement.GetComponent<Button>());

            if (null == ProjectListViewMap)
            {
                ProjectListViewMap = new Dictionary<SharedProject, ListViewElementWorker>();
            }

            ProjectListViewMap.Add(scrumObj.BindedProject, newElement);
            ListViewCompanyProjects.AddControl(newElement.gameObject);

            scrumObj.BindedProject.ProgressUpdated += OnProjectProgressUpdated;
            scrumObj.BindedProject.Completed += OnProjectCompleted;

            TextCompanyProjects.text = string.Format("Company projects ({0})",
                SimulationManagerComponent.ControlledCompany.ScrumProcesses.Count);
        }

        private void OnProjectCompleted(LocalProject proj)
        {
            ListViewElementWorker e = ProjectListViewMap.First(
                x => x.Key == proj).Value;
            e.BackgroundImage.color = CompletedProjectListViewElementColors;
        }

        private void OnProjectProgressUpdated(LocalProject proj)
        {
            ListViewElementWorker e = ProjectListViewMap.First(x => x.Key == proj).Value;
            e.Text.text = base.GetProjectListViewElementText(proj);
        }

        private string GetProjectListViewElementText(SharedProject proj)
        {
            return string.Format("{0}\nCompletion bonus: {1} $\n",
                                 proj.Name,
                                 proj.CompleteBonus);
        }

        private ListViewElementWorker CreateProjectListViewElement(SharedProject proj, ListViewElementWorker prefab)
        {
            ListViewElementWorker newElement = GameObject.Instantiate<ListViewElementWorker>(prefab);
            newElement.Text.text = GetProjectListViewElementText(proj);

            return newElement;
        }

        #endregion

        /*Public methods*/

        public void OnButtonTakeProjectClicked()
        {
            ProjectsMarketComponent.RemoveProject(SelectedProject);
            LocalProject proj = new LocalProject(SelectedProject);
            SimulationManagerComponent.ControlledCompany.AddProject(proj);
        }
    }
}