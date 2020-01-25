using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// This class handles UI view which is displayed when game
/// is finished
/// </summary>
public class UIGameFinish : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    /*Public fields*/

    public MainSimulationManager SimulationManagerComponent;
    /// <summary>
    /// Text that will display information about
    /// finished game
    /// </summary>
    public Text FinishGameInfoText;

    /*Private methods*/

    private void Start()
    {
        //Game object will be started on game finish

        string finishGameInfoMsg = "Game finished !";
        FinishGameInfoText.text = finishGameInfoMsg;
    }

    /*Public methods*/

    public void OnButtonOKClicked()
    {
        SimulationManagerComponent.GameManagerComponent.FinishGame();
    }
}
