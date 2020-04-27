using UnityEngine;
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
        //Game object will be started on simulation finish

        string finishGameInfoMsg = string.Empty;

        switch (SimulationManagerComponent.FinishReason)
        {
            case SimulationFinishReason.PlayerCompanyReachedTargetBalance:
                PhotonPlayer winnerPlayer = PhotonNetwork.playerList.First(x => x.ID == SimulationManagerComponent.WinnerPhotonPlayerID);
                string winnerInfo = (winnerPlayer.IsLocal ? "You have" : winnerPlayer.NickName) + " won";
                finishGameInfoMsg = string.Format("Game finished ! {0}", winnerInfo);
                break;
            //This will be called only on local client
            case SimulationFinishReason.PlayerCompanyReachedMinimalBalance:
                finishGameInfoMsg = string.Format("Your company's balance reached minimum allowed balance ({0} $)",
                                                         SimulationManagerComponent.GameManagerComponent.SettingsOfSimulation.MinimalBalance);
                break;
            case SimulationFinishReason.OnePlayerInRoom:
                finishGameInfoMsg = "You are the only player left in simulation. You have won !";
                break;
            default:
                finishGameInfoMsg = "Game finished";
                break;
        }

        FinishGameInfoText.text = finishGameInfoMsg;
    }

    /*Public methods*/

    public void OnButtonOKClicked()
    {
        SimulationManagerComponent.GameManagerComponent.FinishGame();
    }
}
