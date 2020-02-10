using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles panel with buttons for activating od disabling other UI elements
/// </summary>
public class UISelectionPanel : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    /// <summary>
    /// Stores button that selects active panel 
    /// </summary>
    [SerializeField]
    private Button[] PanelSelectionButtons;
    private ButtonSelector PanelSelectionButtonsSelector = new ButtonSelector();

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void Start()
    {
        foreach (Button panelSelectionButton in PanelSelectionButtons)
        {
            PanelSelectionButtonsSelector.Buttons.Add(panelSelectionButton);
        }
    }

    /*Public methods*/
}
