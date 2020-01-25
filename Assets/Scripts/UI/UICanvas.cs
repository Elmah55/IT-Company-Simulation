using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is main class for handling UI. It should be attached to canvas.
/// Class will manage children UI objects of canvas
/// </summary>
public class UICanvas : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    /*Public fields*/

    public MainSimulationManager SimulationManagerComponent;
    /// <summary>
    /// All other UI objects in scene. Only finish
    /// game UI object should not be added here to
    /// enable it when game is finished
    /// </summary>
    public GameObject[] UIObjects;
    public GameObject FinishGameUI;

    /*Private methods*/

    private void OnGameFinished()
    {
        foreach (GameObject UIGameObject in UIObjects)
        {
            UIGameObject.SetActive(false);
        }

        FinishGameUI.SetActive(true);
    }

    private void Start()
    {
        SimulationManagerComponent.GameFinished += OnGameFinished;
    }

    /*Public methods*/
}
