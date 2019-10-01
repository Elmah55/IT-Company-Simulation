using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    /// <summary>
    /// Panel that was lately active. Used when
    /// activating new panel.
    /// </summary>
    private GameObject LastActivePanel;

    /// <param name="initialActivePanel">Panel that is active by default</param>
    public UIManager(GameObject initialActivePanel)
    {
        LastActivePanel = initialActivePanel;
    }

    public void EnablePanel(GameObject panel)
    {
        if (null != LastActivePanel)
        {
            LastActivePanel.SetActive(false);
        }

        panel.SetActive(true);
        LastActivePanel = panel;
    }
}
