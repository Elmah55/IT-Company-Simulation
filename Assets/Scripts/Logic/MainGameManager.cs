using UnityEngine.SceneManagement;

/// <summary>
/// This class handles main game aspects like loading scenes and
/// handling multiplayer connection
/// </summary>
public class MainGameManager : Photon.PunBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    private MultiplayerConnection MultiplayerConnectionComponent;
    private bool MenuSceneLoadedAfterGameFinish;

    /*Public consts fields*/

    public const string GAME_VERSION = "1.0";

    /*Public fields*/

    /// <summary>
    /// When this is set to true simulation will be run in
    /// Offline Mode. It means that this client won't be connected
    /// to server and simulation will be run in local environment.
    /// This will allow to run some of game mechanism like generation
    /// of projects and workers without actually connecting to server
    /// </summary>
    public bool OfflineMode;

    /*Private methods*/

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        MultiplayerConnectionComponent = GetComponent<MultiplayerConnection>();
    }

    /*Public methods*/

    public void StartGame()
    {
        if (false == OfflineMode)
        {
            MultiplayerConnectionComponent.JoinOrCreateRoom();
        }
        else
        {
            SceneManager.LoadScene((int)SceneIndex.Game);
        }
    }

    public void FinishGame()
    {
        if (false == OfflineMode)
        {
            MultiplayerConnectionComponent.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene((int)SceneIndex.Menu);
            //There is already another game object existing with this
            //script when menu scene is loaded
            Destroy(this.gameObject);
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        SceneManager.LoadScene((int)SceneIndex.Game);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        //TODO: Find way to prevent spawning other GameManager
        //object when menu scene is loaded so destroying of this
        //object is not needed and client won't have connect to
        //photon server every time menu scene is loaded
        SceneManager.LoadScene((int)SceneIndex.Menu);
        //There is already another game object existing with this
        //script when menu scene is loaded
        Destroy(this.gameObject);
    }
}
