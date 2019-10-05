using UnityEngine.UI;

public class UIGameTextInfo : Photon.PunBehaviour
{
    private Text TextComponent;

    // Start is called before the first frame update
    void Start()
    {
        TextComponent = GetComponent<Text>();
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);

        string msg = string.Format("Player with ID {0} joined game", newPlayer.UserId);
        TextComponent.text = msg;
    }
}
