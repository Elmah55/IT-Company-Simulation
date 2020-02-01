using UnityEngine;

public class UISettingsCredentials : MonoBehaviour
{
    public PlayerInfo SettingsManager;

    private void OnEnable()
    {
        SettingsManager.Load();
    }
}
