using System;
using UnityEngine.UI;

public interface IButtonSelector
{
    Button SelectedButton { get; }
    void AddButton(Button buttonComponent);
    bool RemoveButton(Button buttonComponent);
    void RemoveAllButtons();
    /// <summary>
    /// Deselects currently selected button
    /// </summary>
    void DeselectButton();
    event Action<Button> SelectedButtonChanged;
}
