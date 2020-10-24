using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles panel with buttons for activating od disabling other UI elements
/// </summary>
public class TabManager : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    /// <summary>
    /// Stores buttons that are associated to their respective tabs
    /// </summary>
    [Tooltip("Buttons that will toggle tabs")]
    [SerializeField]
    private Button[] TabButtons;
    [Tooltip("Tabs game objects. Array index of tab should match sibling index of button")]
    [SerializeField]
    private GameObject[] Tabs;
    private GameObject ActiveTab;
    [SerializeField]
    private ColorBlock TabButtonColors;
    private IButtonSelector TabButtonSelector;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void Start()
    {
        TabButtonSelector = new ButtonSelector(TabButtonColors);
        TabButtonSelector.SelectedButtonChanged += OnTabButtonSelectorSelectedButtonChanged;

        foreach (Button panelSelectionButton in TabButtons)
        {
            TabButtonSelector.AddButton(panelSelectionButton);
        }

        if (0 < TabButtons.Length)
        {
            //Button selector checks selected element on onClick event
            TabButtons[0].Select();
            TabButtons[0].onClick.Invoke();
        }
    }

    private void OnTabButtonSelectorSelectedButtonChanged(Button obj)
    {
        if (null != ActiveTab)
        {
            ActiveTab.SetActive(false);
        }

        ActiveTab = Tabs[obj.transform.GetSiblingIndex()];
        ActiveTab.SetActive(true);
    }

    /*Public methods*/
}
