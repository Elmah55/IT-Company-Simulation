using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// This class handles UI view which is displayed when game
/// is finished
/// </summary>
public class UIGameFinish : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    [SerializeField]
    private MainSimulationManager SimulationManagerComponent;
    /// <summary>
    /// Text that will display information about
    /// finished game
    /// </summary>
    [SerializeField]
    private Text FinishGameInfoText;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void Start()
    {
        //Game object will be started on game finish

        PhotonPlayer winnerPlayer = PhotonNetwork.playerList.First(x => x.ID == SimulationManagerComponent.WinnerPhotonPlayerID);
        string winnerInfo = (winnerPlayer.IsLocal ? "You" : winnerPlayer.NickName) + " won";
        string finishGameInfoMsg = string.Format("Game finished ! {0}", winnerInfo);
        FinishGameInfoText.text = finishGameInfoMsg;
    }

    /*Public methods*/

    public void OnButtonOKClicked()
    {
        SimulationManagerComponent.GameManagerComponent.FinishGame();
    }
}
