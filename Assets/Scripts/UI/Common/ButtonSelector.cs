﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This class saves pressed button as selected button and optionally
/// sets atributtes of selected button
/// </summary>
public class ButtonSelector : IButtonSelector
{
    /*Private consts fields*/

    /*Private fields*/

    private ColorBlock SavedButtonColors;
    private ColorBlock m_SelectedButtonColors;
    private ColorBlock SelectedButtonColors
    {
        get
        {
            return m_SelectedButtonColors;
        }

        set
        {
            m_SelectedButtonColors = value;

            if (null != SelectedButton)
            {
                SelectedButton.colors = m_SelectedButtonColors;
            }
        }
    }
    private Button m_SelectedButton;
    private Button SelectedButton
    {
        get
        {
            return m_SelectedButton;
        }

        set
        {
            if (null != SelectedButton)
            {
                //Restore colors to previously selected button
                SelectedButton.colors = SavedButtonColors;
            }

            m_SelectedButton = value;

            if (null != SelectedButton)
            {
                SavedButtonColors = SelectedButton.colors;
                SelectedButton.colors = SelectedButtonColors;
            }
        }
    }
    private List<Button> Buttons = new List<Button>();

    /*Public consts fields*/

    /*Public fields*/

    public event Action<Button> SelectedButtonChanged;

    /*Private methods*/

    private void OnButtonClicked()
    {
        //We know its worker list button because its clicked event has been just called
        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
        Button buttonComponent = selectedButton.GetComponent<Button>();

        if (buttonComponent != SelectedButton)
        {
            SelectedButton = buttonComponent;
            SelectedButtonChanged?.Invoke(buttonComponent);
        }
    }

    /*Public methods*/

    public Button GetSelectedButton()
    {
        return SelectedButton;
    }

    public void AddButton(Button buttonComponent)
    {
        buttonComponent.onClick.AddListener(OnButtonClicked);
        Buttons.Add(buttonComponent);
    }

    public bool RemoveButton(Button buttonComponent)
    {
        buttonComponent.onClick.RemoveListener(OnButtonClicked);

        if (SelectedButton == buttonComponent)
        {
            SelectedButton = null;
            SelectedButtonChanged.Invoke(SelectedButton);
        }

        return Buttons.Remove(buttonComponent);
    }

    public void RemoveAllButtons()
    {
        while (0 != Buttons.Count)
        {
            RemoveButton(Buttons[Buttons.Count - 1]);
        }
    }

    public void SetSelectedButtonColor(ColorBlock selectedButtonColors)
    {
        this.SelectedButtonColors = selectedButtonColors;
    }

    public void DeselectButton()
    {
        SelectedButton = null;
    }

    public ButtonSelector()
    {
        SelectedButtonColors = new ColorBlock()
        {
            normalColor = Color.gray,
            selectedColor = Color.gray,
            highlightedColor = Color.gray,
            colorMultiplier = 1.0f
        };
    }

    public ButtonSelector(ColorBlock selectedButtonColors)
    {
        this.SelectedButtonColors = selectedButtonColors;
    }
}
