using UnityEngine;
using UnityEngine.UI;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This script sets up a sound effects on the button it is attached to
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MenuButtonSoundEffects : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private Button ButtonComponent;
        private SoundEffects SoundEffectsComponent;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            ButtonComponent = GetComponent<Button>();
            SoundEffectsComponent = GameObject.FindGameObjectWithTag("SoundEffects").GetComponent<SoundEffects>();
            AddSoundEffects();
        }

        /*Public methods*/

        /// <summary>
        /// Adds sound effects to button. This method is also called on script awake.
        /// This should be used if button listeners had been removed
        /// </summary>
        public void AddSoundEffects()
        {
            ButtonComponent.onClick.AddListener(() =>
            {
                SoundEffectsComponent.PlayUIButtonClickSound();
            });
        }
    }
}
