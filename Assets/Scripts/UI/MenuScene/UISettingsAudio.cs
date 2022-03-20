using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AudioSettings = ITCompanySimulation.Settings.AudioSettings;

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
        private SoundEffects SoundEffectsComponent;
        [SerializeField]
        private AudioClip ClipMenuButtonClick;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            SliderMasterVolume.value = AudioSettings.MasterVolume;
            SliderMusicVolume.value = AudioSettings.MusicVolume;
            SliderUIVolume.value = AudioSettings.UIVolume;
        }

        private void OnDisable()
        {
            AudioSettings.Apply(SliderMasterVolume.value,
                                SliderUIVolume.value,
                                SliderMusicVolume.value,
                                false);
        }


        /*Public methods*/

        public void OnSliderMasterVolumeValueChanged(float value)
        {
            AudioSettings.Apply(SliderMasterVolume.value,
                                SliderUIVolume.value,
                                SliderMusicVolume.value,
                                true);
        }

        public void OnSliderUIVolumeValueChanged(float value)
        {
            AudioSettings.Apply(SliderMasterVolume.value,
                                SliderUIVolume.value,
                                SliderMusicVolume.value,
                                true);
            SoundEffectsComponent.PlaySoundEffectExclusively(ClipMenuButtonClick);

        }

        public void OnSliderMusicVolumeValueChanged(float value)
        {
            AudioSettings.Apply(SliderMasterVolume.value,
                                SliderUIVolume.value,
                                SliderMusicVolume.value,
                                true);
        }
    }
}
