using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIProjects : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    /*Public fields*/

    public MainSimulationManager SimulationManagerScript;
    /// <summary>
    /// List of all projects in company will be placed here
    /// </summary>
    public Dropdown ProjectsListDropdown;
    public Text ProjectInfoText;

    /*Private methods*/

    private void DisplayProjectInfo(Project displayedProject)
    {
        string projectsInfo = string.Empty;

        foreach (Scrum scrumProcess in SimulationManagerScript.testCompany.ScrumProcesses)
        {
            Project companyProject = scrumProcess.BindedProject;

            string projectsInfoLine = string.Format("{0} {1}\n", companyProject.Name, companyProject.Progress);
            projectsInfo += projectsInfoLine;
        }

        ProjectInfoText.text = projectsInfo;
    }

    private void OnProjectsListDropdownValueChanged(int index)
    {
        DisplayProjectInfo(SimulationManagerScript.testCompany.ScrumProcesses[index].BindedProject);
    }

    /*Public methods*/

    // Start is called before the first frame update
    public void Start()
    {
        SimulationManagerScript = GetComponent<MainSimulationManager>();
        ProjectsListDropdown.onValueChanged.AddListener(OnProjectsListDropdownValueChanged);
    }

    public void OnEnable()
    {
        ProjectsListDropdown.options.Clear();

        foreach (Scrum scrumProcess in SimulationManagerScript.testCompany.ScrumProcesses)
        {
            Project companyProject = scrumProcess.BindedProject;
            Dropdown.OptionData projectOption = new Dropdown.OptionData(companyProject.Name);
            ProjectsListDropdown.options.Add(projectOption);
        }

        /* Display info when dropdown value has not been changed yet */
        int projectListDropdownSelectedIndex = ProjectsListDropdown.value;
        DisplayProjectInfo(SimulationManagerScript.testCompany.ScrumProcesses[projectListDropdownSelectedIndex].BindedProject);
    }
}
