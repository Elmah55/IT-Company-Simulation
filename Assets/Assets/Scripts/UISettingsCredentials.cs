using UnityEngine;

public class UISettingsCredentials : MonoBehaviour
{
    public GameSettingsManager SettingsManager;

    private void OnEnable()
    {
        SettingsManager.LoadSettings();
    }
}
