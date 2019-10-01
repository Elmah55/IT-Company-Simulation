using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles UI in main menu scene
/// </summary>
public class MenuUIManager : MonoBehaviour
{
    public GameObject MenuPanel;
    public GameObject SettingsPanel;

    private UIManager UIManagerBase;

    public void Start()
    {
        UIManagerBase = new UIManager(MenuPanel);
    }

    public void EnableMenuPanel()
    {
        UIManagerBase.EnablePanel(MenuPanel);
    }

    public void EnableSettingsPanel()
    {
        UIManagerBase.EnablePanel(SettingsPanel);
    }
}
