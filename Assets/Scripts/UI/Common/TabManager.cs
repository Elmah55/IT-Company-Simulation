using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// This class handles panel with buttons for activating od disabling other UI elements
/// </summary>
public class TabManager : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    [SerializeField]
    private TabSelectionData[] TabSelections;
    private GameObject ActiveTab;
    private IButtonSelector TabButtonSelector;
    /// <summary>
    /// Maps button to game object that should be activated by the button.
    /// </summary>
    private Dictionary<Button, GameObject> TabButtonToTabMap = new Dictionary<Button, GameObject>();

    [Serializable]
    private struct TabSelectionData
    {
        [Tooltip("Buttons that will activate tab.")]
        public Button TabButton;
        [Tooltip("Tab that will be activated by button.")]
        public GameObject Tab;
    }

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void Start()
    {
        TabButtonSelector = new ButtonSelector();
        TabButtonSelector.SelectedButtonChanged += OnTabButtonSelectorSelectedButtonChanged;

        foreach (TabSelectionData selectionData in TabSelections)
        {
            TabButtonToTabMap.Add(selectionData.TabButton, selectionData.Tab);
            TabButtonSelector.AddButton(selectionData.TabButton);
        }

        if (0 < TabSelections.Length)
        {
            TabSelectionData defaultSelection = TabSelections[0];
            //Button selector checks selected element on onClick event
            defaultSelection.TabButton.Select();
            defaultSelection.TabButton.onClick.Invoke();
        }
    }

    private void OnTabButtonSelectorSelectedButtonChanged(Button selectedButton)
    {
        ActiveTab?.SetActive(false);
        ActiveTab = TabButtonToTabMap[selectedButton];
        ActiveTab.SetActive(true);
    }

    /*Public methods*/
}
