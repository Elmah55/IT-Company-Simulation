using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This class saves pressed button as selected button and optionally
/// sets atributtes of selected button
/// </summary>
public class ButtonSelector
{
    /*Private consts fields*/

    /*Private fields*/

    private ColorBlock SavedButtonColors;
    private ColorBlock m_SelectedButtonColors;

    /*Public consts fields*/

    /*Public fields*/

    public ObservableCollection<Button> Buttons { get; private set; } = new ObservableCollection<Button>();
    public Button SelectedButton { get; private set; }
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

    private void OnButtonsGameObjectsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                Button addedButton = (Button)e.NewItems[0];
                addedButton.onClick.AddListener(OnButtonClicked);
                break;
            case NotifyCollectionChangedAction.Remove:
                Button removedButton = (Button)e.OldItems[0];
                removedButton.onClick.RemoveListener(OnButtonClicked);

                if (removedButton == SelectedButton)
                {
                    SelectedButton.colors = SavedButtonColors;
                    SelectedButton = null;
                }

                break;
            case NotifyCollectionChangedAction.Reset:
                if (null != SelectedButton)
                {
                    SelectedButton.colors = SavedButtonColors;
                    SelectedButton = null;
                }

                foreach (object resetedButtonObject in e.OldItems)
                {
                    Button resetedButton = (Button)resetedButtonObject;
                    resetedButton.onClick.RemoveListener(OnButtonClicked);
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                throw new InvalidOperationException(
                    "Operation is not allowed. Only Add, Remove, Move and Reset " +
                    "operation are allowed for collection of buttons");
            default:
                break;
        }
    }

    private void InitButtonsGameObjectsCollection()
    {
        Buttons.CollectionChanged += OnButtonsGameObjectsCollectionChanged;
    }

    /*Public methods*/

    public ButtonSelector()
    {
        SelectedButtonColors = new ColorBlock()
        {
            normalColor = Color.gray,
            selectedColor = Color.gray,
            colorMultiplier = 1.0f
        };

        InitButtonsGameObjectsCollection();
    }

    public ButtonSelector(ColorBlock selectedButtonColors)
    {
        this.SelectedButtonColors = selectedButtonColors;
        InitButtonsGameObjectsCollection();
    }
}
