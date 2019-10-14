using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the core class that runs the simulation
/// </summary>
public class GameSimulationEngine : MonoBehaviour
{
    /*Private consts fields*/

    /// <summary>
    /// How often should events in simulation should be updated.
    /// If this values is smaller it means that states of objects
    /// would be updated faster. 
    /// This value is seconds in game time (scaled time)
    /// </summary>
    private const float SIMULATION_UPDATE_FREQUENCY = 10.0f;

    /*Private fields*/

    /*Public consts fields*/

    /*Public fields*/

    /// <summary>
    /// Company that player will control during simulation
    /// </summary>
    public PlayerCompany PlayerCompanyInstance;

    /*Private methods*/

    /// <summary>
    /// Updates all events in simulation
    /// </summary>
    private void UpdateSimulationEvents()
    {
        foreach (IUpdatable updatableItem in PlayerCompanyInstance.ScrumProcesses)
        {
            updatableItem.UpdateState();
        }

        foreach (IUpdatable updatableItem in PlayerCompanyInstance.Workers)
        {
            updatableItem.UpdateState();
        }
    }

    /*Public methods*/

    // Start is called before the first frame update

    public void StartSimulation()
    {
        InvokeRepeating("UpdateSimulationEvents", 0.0f, SIMULATION_UPDATE_FREQUENCY);
    }

    public void StopSimulation()
    {
        CancelInvoke();
    }

    public void OnDestroy()
    {
        StopSimulation();
    }
}
