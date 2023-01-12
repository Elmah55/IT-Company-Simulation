using UnityEngine;
using UnityEngine.UI;
using ITCompanySimulation.Project;
using TMPro;
using ITCompanySimulation.Core;

namespace ITCompanySimulation.UI
{
    public class UIProjectsProjectsMarket : UIProjects
    {
        /*Private consts fields*/

        /*Private fields*/

        private SimulationManager SimulationManagerComponent;
        [SerializeField]
        private TextMeshProUGUI TextMarketProjects;
        [SerializeField]
        private ControlListView ListViewMarketProjects;
        private ProjectsMarket ProjectsMarketComponent;
        [SerializeField]
        private Button ButtonTakeProject;
        private SharedProject SelectedProject;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            SimulationManagerComponent = SimulationManager.Instance;
            ProjectsMarketComponent = SimulationManagerComponent.gameObject.GetComponent<ProjectsMarket>();
        }

        private void Start()
        {
            ButtonSelectorProjects = new ButtonSelector();

            foreach (var proj in ProjectsMarketComponent.Projects)
            {
                OnProjectsMarketProjectAdded(proj.Value);
            }

            ProjectsMarketComponent.ProjectAdded += OnProjectsMarketProjectAdded;
            ProjectsMarketComponent.ProjectRemoved += OnProjectsMarketProjectRemoved;
            ButtonSelectorProjects.SelectedButtonChanged += OnButtonSelectorProjectsSelectedButtonChanged;

            ButtonTakeProject.interactable = false;
            SetProjectInfoText(null);
        }

        private void SetProjectInfoText(SharedProject proj)
        {
            if (null != proj)
            {
                TextProjectName.text = string.Format("Project name: {0}", proj.Name);
                TextCompleteBonus.text = string.Format("Completion bonus: <color=#4CD137>+ {0} $</color>", proj.CompletionBonus);
                TextPenalty.text = string.Format("Exceeding completion time penalty: <color=#E84118>- {0} $</color>", proj.CompletionTimeExceededPenalty);
                TextUsedTechnologies.text = string.Format("Used technologies: {0}", GetProjectTechnologiesString(proj));
                TextCompletionTime.text = string.Format("Time for completion: {0} days", proj.CompletionTime);
            }
            else
            {
                TextProjectName.text = string.Empty;
                TextCompleteBonus.text = string.Empty;
                TextUsedTechnologies.text = string.Empty;
                TextCompletionTime.text = string.Empty;
                TextPenalty.text = string.Empty;
            }
        }

        private void SetProjectMarketButton()
        {
            if (null != SelectedProject)
            {
                ButtonTakeProject.interactable = (false == (SelectedProject is LocalProject));
            }
            else
            {
                ButtonTakeProject.interactable = false;
            }
        }

        private void SetListViewMarketProjectsText()
        {
            TextMarketProjects.text = string.Format("Market projects ({0})",
                ProjectsMarketComponent.Projects.Count);
        }

        #region Events callbacks

        private void OnButtonSelectorProjectsSelectedButtonChanged(Button btn)
        {
            if (null != btn)
            {
                ListViewElement el = btn.GetComponent<ListViewElement>();
                SelectedProject = (SharedProject)el.RepresentedObject;
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
            ListViewElement element = ListViewMarketProjects.FindElement(proj);

            element.gameObject.SetActive(false);
            ListViewMarketProjects.RemoveControl(element.gameObject, false);
            ButtonSelectorProjects.RemoveButton(element.Button);
            SetListViewMarketProjectsText();

            proj.CompletionTimeUpdated -= OnMarketProjectCompletionTimeUpdated;
        }

        private void OnProjectsMarketProjectAdded(SharedProject proj)
        {
            ListViewElement newElement = CreateListViewElement(proj);
            MousePointerEvents mousePtrEvts = newElement.GetComponent<MousePointerEvents>();

            mousePtrEvts.PointerDoubleClick.AddListener(() =>
             {
                 OnButtonTakeProjectClicked();
             });

            ButtonSelectorProjects.AddButton(newElement.Button);
            ListViewMarketProjects.AddControl(newElement.gameObject);
            newElement.Text.text = GetProjectListViewElementText(proj);
            newElement.FrontImage.sprite = proj.Icon;
            SetListViewMarketProjectsText();

            proj.CompletionTimeUpdated += OnMarketProjectCompletionTimeUpdated;
        }

        private void OnMarketProjectCompletionTimeUpdated(SharedProject proj)
        {
            ListViewElement element = ListViewMarketProjects.FindElement(proj);
            element.Text.text = GetProjectListViewElementText(proj);
        }

        private string GetProjectListViewElementText(SharedProject proj)
        {
            return string.Format("{0}\nCompletion bonus: {1} $\nCompletion time: {2} days",
                                 proj.Name,
                                 proj.CompletionBonus,
                                 proj.CompletionTime);
        }

        #endregion

        /*Public methods*/

        public void OnButtonTakeProjectClicked()
        {
            ProjectsMarketComponent.RequestProject(SelectedProject);
        }
    }
}