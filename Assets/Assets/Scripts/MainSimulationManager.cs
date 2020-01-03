using UnityEngine;

/// <summary>
/// This is core class for all aspects of gameplay that will
/// happen during running simulation (adding workers, claiming
/// projects, etc.)
/// </summary>
public class MainSimulationManager : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    private GameSettingsManager SettingsManager;

    /*Public consts fields*/

    /*Public fields*/

    public PlayerCompany ControlledCompany { get; private set; }

    /*Private methods*/

    private void CreateCompany()
    {
        //Testing scrum only for now
        ControlledCompany = new PlayerCompany("TEST COMPANY");
        Worker workerA = new Worker("Jan", "Kowalski");
        Worker workerB = new Worker("Adam", "Nowak");
        workerA.WorkingCompany = ControlledCompany;
        workerB.WorkingCompany = ControlledCompany;
        ControlledCompany.Workers.Add(workerA);
        ControlledCompany.Workers.Add(workerB);
        ControlledCompany.Balance = 10000000;
        Project testProject = new Project("TEST");
        testProject.UsedTechnologies.Add(ProjectTechnology.C);
        testProject.UsedTechnologies.Add(ProjectTechnology.Cpp);
        testProject.DaysSinceStart = 10;
        Scrum testScrum = gameObject.AddComponent(typeof(Scrum)) as Scrum;
        ControlledCompany.ScrumProcesses.Add(testScrum);
        testScrum.BindedProject = testProject;
    }

    /*Public methods*/

    public void Start()
    {
        //Obtain refence to game manager object wich was created in
        //menu scene
        GameObject gameManagerObject = GameObject.Find("GameManager");
        SettingsManager = GetComponent<GameSettingsManager>();

        //TEST
        CreateCompany();
    }
}
