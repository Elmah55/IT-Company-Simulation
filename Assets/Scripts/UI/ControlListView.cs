using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is class for handling list view of UI controls.
/// Added controls will be ordered and placed according to
/// attached layout component
/// </summary>
public class ControlListView : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    /*Public fields*/

    /// <summary>
    /// All added controls will be children of this transform
    /// so they can be placed correctly by layout component
    /// </summary>
    public RectTransform ControlsGridParentTransform;
    /// <summary>
    /// List of controls that this list view holds
    /// </summary>
    public List<GameObject> Controls { get; private set; } = new List<GameObject>();

    /*Private methods*/

    /*Public methods*/

    public void AddControl(GameObject control)
    {
        RectTransform controlTransformComponent = control.GetComponent<RectTransform>();

        if (null == controlTransformComponent)
        {
            throw new ArgumentException("Control must have RectTransform component attached");
        }

        controlTransformComponent.SetParent(ControlsGridParentTransform, false);
        Controls.Add(control);
    }

    public void RemoveControl(GameObject control, bool removeGameObject)
    {
        if (true == removeGameObject)
        {
            RemoveControl(control);
        }
        else
        {
            Controls.Remove(control);
        }
    }

    public void RemoveControl(GameObject control)
    {
        GameObject.Destroy(control);
        Controls.Remove(control);
    }

    /// <summary>
    /// Removes control with given controls collection index
    /// </summary>
    public void RemoveControl(int index)
    {
        RemoveControl(Controls[index]);
    }

    public void RemoveAllControls()
    {
        foreach (GameObject control in Controls)
        {
            GameObject.Destroy(control);
        }

        Controls.Clear();
    }
}
