using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    private Button SelectedButton;
    private List<Button> Buttons = new List<Button>();

    /*Public consts fields*/

    /*Public fields*/


    /*Private methods*/

    private void OnButtonClicked()
    {
        //We know its worker list button because its clicked event has been just called
        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
        Button buttonComponent = selectedButton.GetComponent<Button>();

        if (null != SelectedButton)
        {
            //Restore colors of previously selected button
            SelectedButton.colors = SavedButtonColors;
        }

        SelectedButton = buttonComponent;

        //Change colors of selcted button
        SavedButtonColors = SelectedButton.colors;
        SelectedButton.colors = SelectedButtonColors;
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
        return Buttons.Remove(buttonComponent);
    }

    public void RemoveAllButtons()
    {
        foreach (Button buttonComponent in Buttons)
        {
            buttonComponent.onClick.RemoveListener(OnButtonClicked);
        }

        Buttons.Clear();
    }

    public void SetSelectedButtonColor(ColorBlock selectedButtonColors)
    {
        this.SelectedButtonColors = selectedButtonColors;
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
