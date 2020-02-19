using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class UIProjectsProjectsMarket : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    private IButtonSelector MarketProjectsButtonSelector = new ButtonSelector();
    private Project SelectedProject;
    /// <summary>
    /// Dictionary for mapping each of project list view button to
    /// its coresponding project
    /// </summary>
    private Dictionary<Button, Project> ButtonProjectDicionary = new Dictionary<Button, Project>();
    [SerializeField]
    private ControlListView ListViewProjectsMarket;
    [SerializeField]
    private ProjectsMarket ProjectsMarketComponent;
    [SerializeField]
    private Button MarketProjectListViewButtonPrefab;
    [SerializeField]
    private MainSimulationManager MainSimulationManagerComponent;
    [SerializeField]
    private Button ButtonTakeProject;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void InitListViewProjectsMarket()
    {
        foreach (Project marketProject in ProjectsMarketComponent.Projects)
        {
            Button projectsMartketListViewButton = CreateProjectsMarketListViewButton(marketProject);
        }
    }

    private void OnMarketProjectsListViewButtonClicked(Button selectedButton)
    {
        SelectedProject = ButtonProjectDicionary[selectedButton];
        DisplaySelectedProjectInfo();
        ButtonTakeProject.interactable = true;
    }

    private void DisplaySelectedProjectInfo()
    {

    }

    private void OnMarketProjectRemoved(Project projectRemoved)
    {
        Button removedProjectButton =
            ButtonProjectDicionary.First(x => x.Value == projectRemoved).Key;
        ButtonProjectDicionary.Remove(removedProjectButton);
        ListViewProjectsMarket.RemoveControl(removedProjectButton.gameObject);
        MarketProjectsButtonSelector.RemoveButton(removedProjectButton);
    }

    private void OnMarketProjectAdded(Project proj)
    {
        CreateProjectsMarketListViewButton(proj);
    }

    private Button CreateProjectsMarketListViewButton(Project marketProject)
    {
        Button projectButton = GameObject.Instantiate<Button>(MarketProjectListViewButtonPrefab);
        Text textComponent = projectButton.GetComponentInChildren<Text>();

        textComponent.text = string.Format("{0} / {1} $", marketProject.Name, marketProject.CompleteBonus);

        ListViewProjectsMarket.AddControl(projectButton.gameObject);
        ButtonProjectDicionary.Add(projectButton, marketProject);
        MarketProjectsButtonSelector.AddButton(projectButton);

        return projectButton;
    }

    private void Start()
    {
        ProjectsMarketComponent.ProjectAdded += OnMarketProjectAdded;
        ProjectsMarketComponent.ProjectRemoved += OnMarketProjectRemoved;
        MarketProjectsButtonSelector.SelectedButtonChanged += OnMarketProjectsListViewButtonClicked;

        InitListViewProjectsMarket();
    }

    /*Public methods*/

    public void OnTakeProjectButtonClick()
    {
        if (null != SelectedProject)
        {
            ProjectsMarketComponent.RemoveProject(SelectedProject);
            MainSimulationManagerComponent.ControlledCompany.AddProject(SelectedProject);
            SelectedProject = null;
            ButtonTakeProject.interactable = false;
        }
    }
}