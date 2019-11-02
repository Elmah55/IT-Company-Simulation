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

    public PlayerCompany testCompany;

    /*Public fields*/

    /*Private methods*/

    private void CreateCompany()
    {
        //Testing scrum only for now
        testCompany = new PlayerCompany("TEST COMPANY");
        Worker workerA = new Worker("Jan", "Kowalski");
        Worker workerB = new Worker("Adam", "Nowak");
        testCompany.Workers.Add(workerA);
        testCompany.Workers.Add(workerB);
        Project testProject = new Project("TEST");
        testProject.Workers.Add(workerA);
        testProject.Workers.Add(workerB);
        Scrum testScrum = gameObject.AddComponent(typeof(Scrum)) as Scrum;
        testCompany.ScrumProcesses.Add(testScrum);
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
