﻿using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This component will resize control by given % when pointer is above it.
/// This can be used for selection effect
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ControlResizer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /*Private consts fields*/

    /*Private fields*/

    private RectTransform ButtonTransform;
    private Vector2 NormalSize = Vector2.zero;
    private bool CheckPointerPostion;

    /*Public consts fields*/

    /*Public fields*/

    /// <summary>
    /// How many % control should be resized
    /// </summary>
    [Tooltip("How many % control should be resized")]
    public float ResizePercentage = 20f;

    /*Private methods*/

    private void OnDisable()
    {
        //Normal size will be initialized to zero at script start
        if (Vector2.zero != NormalSize)
        {
            ButtonTransform.sizeDelta = NormalSize;
        }
    }

    private void Start()
    {
        ButtonTransform = GetComponent<RectTransform>();
    }

    /*Public methods*/

    public void OnPointerEnter(PointerEventData eventData)
    {
        NormalSize = ButtonTransform.sizeDelta;
        float resizeFactor = ResizePercentage / 100f;
        float newWidth = NormalSize.x + (resizeFactor * NormalSize.x);
        float newHeigth = NormalSize.y + (resizeFactor * NormalSize.y);
        Vector2 newSize = new Vector2(newWidth, newHeigth);
        ButtonTransform.sizeDelta = newSize;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ButtonTransform.sizeDelta = NormalSize;
    }
}
