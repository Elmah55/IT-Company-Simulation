using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Groups buttons that can be selected.
/// <para>
/// It allows to make multiple groups where in each group one button can be selected.
/// Using this class removes unity's limitation of choosing only one UI element at a time.
/// </para>
/// </summary>
public class ButtonSelector : IButtonSelector
{
    /*Private consts fields*/

    /*Private fields*/

    private Color SelectedButtonNormalColor;
    private List<Button> Buttons = new List<Button>();

    /*Public consts fields*/

    /*Public fields*/

    public event Action<Button> SelectedButtonChanged;
    public Button SelectedButton { get; private set; }

    /*Private methods*/

    private void OnButtonDeselected(Button deselectedButton)
    {
        if (Selectable.Transition.ColorTint == deselectedButton.transition)
        {
            ColorBlock deselectedButtonColors = deselectedButton.colors;
            deselectedButtonColors.normalColor = SelectedButtonNormalColor;
            deselectedButton.colors = deselectedButtonColors;
        }
    }

    private void OnButtonClicked()
    {
        //We know its worker list button because its clicked event has been just called
        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
        Button buttonComponent = selectedButton.GetComponent<Button>();

        if (buttonComponent != SelectedButton)
        {
            //Deselect previously selected button
            DeselectButton();
            SelectedButton = buttonComponent;

            //Save button normal visuals to restore it when button is deselected
            //Set button's normal visuals as selected visuals. This way when unity event system
            //selects another UI element this button's visuals will be same as selected visuals.
            switch (buttonComponent.transition)
            {
                case Selectable.Transition.ColorTint:
                    ColorBlock selectedButtonColors = buttonComponent.colors;
                    SelectedButtonNormalColor = selectedButtonColors.normalColor;
                    selectedButtonColors.normalColor = selectedButtonColors.selectedColor;
                    buttonComponent.colors = selectedButtonColors;
                    break;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                case Selectable.Transition.None:
                case Selectable.Transition.SpriteSwap:
                case Selectable.Transition.Animation:
                    Debug.LogWarningFormat("[{0}] Unsupported button transition: {1}",
                        this.GetType().Name,
                        buttonComponent.transition.ToString());
                    break;
#endif
                default:
                    break;
            }


            SelectedButtonChanged?.Invoke(buttonComponent);
        }
    }

    /*Public methods*/

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

    public void DeselectButton()
    {
        if (null != SelectedButton)
        {
            OnButtonDeselected(SelectedButton);
            SelectedButton = null;
        }
    }
}
