using UnityEngine;
using ITCompanySimulation.UI;
using ITCompanySimulation.Core;
using UnityEngine.Events;

public class UIGameSceneMenu : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    [SerializeField]
    private InfoWindow InfoWindowComponent;
    private MainGameManager GameManagerComponent;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void Start()
    {
        GameManagerComponent = GameObject.FindGameObjectWithTag("GameManager").GetComponent<MainGameManager>();
    }

    /*Public methods*/

    public void OnExitToMenuButtonClick()
    {
        string infoWindowText = "Do you really want to exit to main menu ?";

        UnityAction okAction = () =>
        {
            GameManagerComponent.FinishSession();
            GameManagerComponent.LoadScene(SceneIndex.Menu);
        };

        InfoWindowComponent.ShowOkCancel(infoWindowText, okAction, null);
    }
}
