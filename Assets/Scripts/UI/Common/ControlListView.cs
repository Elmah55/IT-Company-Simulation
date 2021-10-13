using ITCompanySimulation.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    /// <param name="destroyGameObject">If true game object of list view control will be destroyed</param>
    public void RemoveControl(GameObject control, bool destroyGameObject = true)
    {
        if (true == destroyGameObject)
        {
            GameObject.Destroy(control);
        }

        Controls.Remove(control);
        ControlRemoved?.Invoke(control);
    }

    /// <summary>
    /// Removes control with given controls collection index
    /// </summary>
    public void RemoveControlAt(int index, bool destroyGameObject = true)
    {
        RemoveControl(Controls[index], destroyGameObject);
    }

    /// <summary>
    /// Removes all control in this list view
    /// </summary>
    public void RemoveAllControls(bool destroyGameObjects = true)
    {
        for (int i = Controls.Count - 1; i >= 0; i--)
        {
            RemoveControlAt(i, destroyGameObjects);
        }
    }

    /// <summary>
    /// Returns list view element that respresents object passed as argument.
    /// </summary>
    public ListViewElement FindElement(object representedObject)
    {
        ListViewElement result = null;

        foreach (var control in Controls)
        {
            ListViewElement elem = control.GetComponent<ListViewElement>();

            if (null != elem && elem.RepresentedObject == representedObject)
            {
                result = elem;
                break;
            }
        }

        return result;
    }
}
