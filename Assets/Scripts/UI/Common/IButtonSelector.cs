using System;
using UnityEngine.UI;

public interface IButtonSelector
{
    Button GetSelectedButton();
    void AddButton(Button buttonComponent);
    bool RemoveButton(Button buttonComponent);
    void RemoveAllButtons();
    void SetSelectedButtonColor(ColorBlock selectedButtonColors);
    /// <summary>
    /// Deselects currently selected button
    /// </summary>
    void DeselectButton();
    event Action<Button> SelectedButtonChanged;
}
