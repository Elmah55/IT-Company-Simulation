using UnityEngine;

public class UISettingsCredentials : MonoBehaviour
{
    public PlayerInfoSettings SettingsManager;

    private void OnEnable()
    {
        SettingsManager.LoadSettings();
    }
}
