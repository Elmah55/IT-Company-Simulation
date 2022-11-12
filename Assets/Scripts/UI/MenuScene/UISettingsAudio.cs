using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AudioSettings = ITCompanySimulation.Settings.AudioSettings;
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
        private AudioSettings AudioSettingsObject;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            SliderMasterVolume.value = AudioSettingsObject.MasterVolume;
            SliderMusicVolume.value = AudioSettingsObject.MusicVolume;
            SliderUIVolume.value = AudioSettingsObject.UIVolume;
        }

        private void OnDisable()
        {
            AudioSettingsObject.Apply(SliderMasterVolume.value,
                                      SliderUIVolume.value,
                                      SliderMusicVolume.value,
                                      false);
        }


        /*Public methods*/

        public void OnSliderMasterVolumeValueChanged(float value)
        {
            AudioSettingsObject.Apply(SliderMasterVolume.value,
                                      SliderUIVolume.value,
                                      SliderMusicVolume.value,
                                      true);
        }

        public void OnSliderUIVolumeValueChanged(float value)
        {
            AudioSettingsObject.Apply(SliderMasterVolume.value,
                                      SliderUIVolume.value,
                                      SliderMusicVolume.value,
                                      true);
            UISoundRequestEventArgs args = new UISoundRequestEventArgs(ClipMenuButtonClick, true);
            UISoundPlayRequest.RaiseEvent(args);

        }

        public void OnSliderMusicVolumeValueChanged(float value)
        {
            AudioSettingsObject.Apply(SliderMasterVolume.value,
                                      SliderUIVolume.value,
                                      SliderMusicVolume.value,
                                      true);
        }
    }
}
