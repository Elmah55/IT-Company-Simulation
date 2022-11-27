using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ITCompanySimulation.Event;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Component controlling UI for audio settings
    /// </summary>
    public class UISettingsAudio : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private TextMeshProUGUI TextMasterVolume;
        [SerializeField]
        private TextMeshProUGUI TextUIVolume;
        [SerializeField]
        private TextMeshProUGUI TextMusicVolume;
        [SerializeField]
        private Slider SliderMasterVolume;
        [SerializeField]
        private Slider SliderUIVolume;
        [SerializeField]
        private Slider SliderMusicVolume;
        [SerializeField]
        private UISoundRequestEvent UISoundPlayRequest;
        [SerializeField]
        private AudioClip ClipMenuButtonClick;
        [SerializeField]
        private AudioVolumeSettings VolumeSettings;
        [SerializeField]
        private VoidEvent VolumeSettingsUpdateEvent;
        /// <summary>
        /// Determines whether this component has finished initialization.
        /// </summary>
        private bool IsInitialized = false;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void ApplySettings()
        {
            //Apply settings only when initialization is finished to not update
            //settings when sliders' values are initialized
            if (true == IsInitialized)
            {
                VolumeSettings.MasterVolume = SliderMasterVolume.value;
                VolumeSettings.MusicVolume = SliderMusicVolume.value;
                VolumeSettings.UIVolume = SliderUIVolume.value;
                VolumeSettingsUpdateEvent.RaiseEvent();
            }
        }

        private void Start()
        {
            SliderMasterVolume.value = VolumeSettings.MasterVolume;
            SliderMusicVolume.value = VolumeSettings.MusicVolume;
            SliderUIVolume.value = VolumeSettings.UIVolume;
            IsInitialized = true;
        }

        private void OnDisable()
        {
            ApplySettings();
        }

        /*Public methods*/

        public void OnSliderMasterVolumeValueChanged(float value)
        {
            ApplySettings();
        }

        public void OnSliderUIVolumeValueChanged(float value)
        {
            ApplySettings();
            UISoundRequestEventArgs args = new UISoundRequestEventArgs(ClipMenuButtonClick, true);
            UISoundPlayRequest.RaiseEvent(args);

        }

        public void OnSliderMusicVolumeValueChanged(float value)
        {
            ApplySettings();
        }
    }
}
