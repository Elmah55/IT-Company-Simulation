using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class UIProjectsProjectsMarket : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    private Project SelectedProject;
    /// <summary>
    /// Dictionary for mapping each of project list view button to
    /// its coresponding project
    /// </summary>
    private Dictionary<GameObject, Project> ButtonProjectDicionary;

    /*Public consts fields*/

    /*Public fields*/

    public UIControlListView MarketProjectsListView;
    public ProjectsMarket ProjectsMarketComponent;
    public GameObject MarketProjectListViewButtonPrefab;
    public MainSimulationManager MainSimulationManagerComponent;

    /*Private methods*/

    private void OnMarketProjectsListViewButtonClicked()
    {
        //We know its project list view button because its clicked event has been just called
        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
        SelectedProject = ButtonProjectDicionary[selectedButton];

        DisplaySelectedProjectInfo();
    }

    private void DisplaySelectedProjectInfo()
    {

    }

    private void OnMarketProjectRemoved(Project projectRemoved)
    {
        GameObject removedProjectButton = 
            ButtonProjectDicionary.First(x => x.Value == projectRemoved).Key;
        ButtonProjectDicionary.Remove(removedProjectButton);
        MarketProjectsListView.RemoveControl(removedProjectButton);
    }

    private void Start()
    {
        ButtonProjectDicionary = new Dictionary<GameObject, Project>();
        ProjectsMarketComponent.ProjectRemoved += OnMarketProjectRemoved;

        foreach (Project marketProject in ProjectsMarketComponent.Projects)
        {
            GameObject newProjectButton = GameObject.Instantiate(MarketProjectListViewButtonPrefab);
            Button buttonComponent = newProjectButton.GetComponent<Button>();
            Text textComponent = newProjectButton.GetComponentInChildren<Text>();

            textComponent.text = string.Format("{0} / {1} $", marketProject.Name, marketProject.CompleteBonus);
            buttonComponent.onClick.AddListener(OnMarketProjectsListViewButtonClicked);

            MarketProjectsListView.AddControl(newProjectButton);
            ButtonProjectDicionary.Add(newProjectButton, marketProject);
        }
    }

    /*Public methods*/

    public void OnTakeProjectButtonClick()
    {
        if (null != SelectedProject)
        {
            ProjectsMarketComponent.RemoveProject(SelectedProject);
            MainSimulationManagerComponent.ControlledCompany.AddProject(SelectedProject);
            SelectedProject = null;
        }
    }
}