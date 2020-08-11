using ITCompanySimulation.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


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
    /// List of controls that this list view holds
    /// </summary>
    public List<GameObject> Controls { get; private set; } = new List<GameObject>();
    /// <summary>
    /// Reference to object that will manage layout of objects in list view.
    /// This should be set in unity's inspector. All objects in this list will be
    /// attached as this object's children
    /// </summary>
    public GameObject Layout;
    public event UnityAction<GameObject> ControlAdded;
    public event UnityAction<GameObject> ControlRemoved;

    /*Private methods*/

    /*Public methods*/

    public virtual void AddControl(GameObject control)
    {
        control.transform.SetParent(Layout.transform, false);
        Controls.Add(control);
        ControlAdded?.Invoke(control);
    }

    /// <summary>
    /// Removes control from list view
    /// </summary>
    /// <param name="removeGameObject">If true game object of list view control will be removed</param>
    public void RemoveControl(GameObject control, bool removeGameObject = true)
    {
        if (true == removeGameObject)
        {
            GameObject.Destroy(control);
        }

        Controls.Remove(control);
        ControlRemoved?.Invoke(control);
    }

    /// <summary>
    /// Removes control with given controls collection index
    /// </summary>
    public void RemoveControlAt(int index)
    {
        RemoveControl(Controls[index]);
    }

    /// <summary>
    /// Removes all control in this list view
    /// </summary>
    public void RemoveAllControls(bool removeGameObjects = true)
    {
        if (true == removeGameObjects)
        {
            foreach (GameObject control in Controls)
            {
                GameObject.Destroy(control);
            }
        }

        for (int i = Controls.Count - 1; i >= 0; i--)
        {
            RemoveControlAt(i);
        }
    }
}
