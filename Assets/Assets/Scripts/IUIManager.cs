using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIManager
{
    /// <summary>
    /// Enables passed panel and deactivates panel that was
    /// previously active
    /// </summary>
    void EnablePanel(GameObject panel);
}
