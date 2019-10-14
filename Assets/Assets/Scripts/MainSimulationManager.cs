using System.Collections;
using System.Collections.Generic;
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

    private GameSimulationEngine SimulationEngine;
    private GameSettingsManager SettingsManager;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void CreateCompany()
    {

    }

    /*Public methods*/

    public void Start()
    {
        /*Obtain refence to game manager object wich was created in
        menu scene*/
        GameObject gameManagerObject = GameObject.Find("GameManager");
        SimulationEngine = gameManagerObject.GetComponent<GameSimulationEngine>();
        SettingsManager = GetComponent<GameSettingsManager>();
    }
}
