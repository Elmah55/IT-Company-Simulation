using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// UI that allows to control video related settings, like resolution, framerate, etc.
    /// </summary>
    public class UISettingsVideo : MonoBehaviour
    {
        /*Private consts fields*/

        private const int MAX_TARGET_FRAME_RATE = 250;

        /*Private fields*/

        /// <summary>
        /// Dropdown that contains available screen resolutions
        /// </summary>
        [SerializeField]
        private TMP_Dropdown DropdownResolutions;
        [SerializeField]
        private TMP_Dropdown DropdownFullscreenMode;
        [SerializeField]
        private TMP_Dropdown DropdownQualitySettings;
        [SerializeField]
        private Slider SliderTargetFrameRate;
        [SerializeField]
        private Toggle ToggleVSync;
        [SerializeField]
        private TextMeshProUGUI TextTargetFrameRate;
        //Stored state of controls to restore it in case user
        //changed value but did not apply settings and then closed
        //video settings panel
        private int DropdownResolutionsValue;
        private int DropdownFullscreenModeValue;
        private int DropdownQualitySettingsValue;
        private bool ToggleVSyncOn;
        private float SliderTargetFrameRateValue;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void InitDropdownAntiAliasing()
        {
            List<string> dropdownOptions = new List<string>(QualitySettings.names);
            DropdownQualitySettings.AddOptions(dropdownOptions);
            DropdownQualitySettings.value = QualitySettings.GetQualityLevel();
        }

        private void InitDropdownResolutions()
        {
            int currentResolutionIndex = 0;

            foreach (var res in Screen.resolutions)
            {
                string dropdownOptionText = string.Format("{0} x {1}",
                    res.width, res.height);
                TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(dropdownOptionText);
                DropdownResolutions.options.Add(data);

                //Set currently select resolution as dropdown value
                if (Screen.currentResolution.width == res.width && Screen.currentResolution.height == res.height)
                {
                    currentResolutionIndex = DropdownResolutions.options.Count - 1;
                }
            }

            DropdownResolutions.value = currentResolutionIndex;
        }

        private void InitDropdownFullscreenMode()
        {
            int fullScreenModeCount = Enum.GetValues(typeof(FullScreenMode)).Length;

            for (int i = 0; i < fullScreenModeCount; i++)
            {
                string dropdownOptionText = EnumToString.GetString((FullScreenMode)i);
                TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(dropdownOptionText);
                DropdownFullscreenMode.options.Add(data);
            }

            DropdownFullscreenMode.value = (int)Screen.fullScreenMode;
        }

        private void InitSliderTargetFrameRate()
        {
            SliderTargetFrameRate.minValue = 0;
            SliderTargetFrameRate.maxValue = MAX_TARGET_FRAME_RATE;
            SliderTargetFrameRate.value = Application.targetFrameRate;
            SliderTargetFrameRate.gameObject.SetActive(!ToggleVSync.isOn);
            SetTextTargetFrameRate((int)SliderTargetFrameRate.value);
            ToggleVSync.isOn = QualitySettings.vSyncCount != 0;
        }

        private void SetTextTargetFrameRate(int fps)
        {
            string value = (false == ToggleVSync.isOn) ?
                (fps == 0 ? "MAX FPS" : (fps.ToString() + " FPS")) : "V-Sync";
            TextTargetFrameRate.text = string.Format("Target Frame Rate ({0})",
                value);
        }

        private void Start()
        {
            InitDropdownResolutions();
            InitDropdownFullscreenMode();
            InitDropdownAntiAliasing();
            InitSliderTargetFrameRate();

            ToggleVSyncOn = ToggleVSync.isOn;
            SliderTargetFrameRateValue = SliderTargetFrameRate.value;
            DropdownFullscreenModeValue = DropdownFullscreenMode.value;
            DropdownQualitySettingsValue = DropdownQualitySettings.value;
            DropdownResolutionsValue = DropdownResolutions.value;
        }

        private void OnEnable()
        {
            ToggleVSync.isOn = ToggleVSyncOn;
            SliderTargetFrameRate.gameObject.SetActive(!ToggleVSync.isOn);
            SliderTargetFrameRate.value = SliderTargetFrameRateValue;
            DropdownFullscreenMode.value = DropdownFullscreenModeValue;
            DropdownQualitySettings.value = DropdownQualitySettingsValue;
            DropdownResolutions.value = DropdownResolutionsValue;
        }

        /*Public methods*/

        public void OnSliderTargetFrameRateValueChanged(float value)
        {
            SetTextTargetFrameRate((int)SliderTargetFrameRate.value);
        }

        public void OnTogleVSyncValueChanged(bool value)
        {
            SliderTargetFrameRate.gameObject.SetActive(!ToggleVSync.isOn);
            SetTextTargetFrameRate((int)SliderTargetFrameRate.value);
        }

        public void OnApplyButtonClicked()
        {
            //Apply all settings

            //Quality level
            QualitySettings.SetQualityLevel(DropdownQualitySettings.value, true);
            DropdownQualitySettingsValue = DropdownQualitySettings.value;

            //Resolution
            Resolution newResolution = Screen.resolutions[DropdownResolutions.value];
            DropdownResolutionsValue = DropdownResolutions.value;
            Screen.SetResolution(newResolution.width, newResolution.height, Screen.fullScreen);

            //Full screen mode
            FullScreenMode mode = (FullScreenMode)DropdownFullscreenMode.value;

            if (mode != Screen.fullScreenMode)
            {
                DropdownFullscreenModeValue = DropdownFullscreenMode.value;
                Screen.fullScreenMode = mode;
            }

            //Framerate
            QualitySettings.vSyncCount = ToggleVSync.isOn ? 1 : 0;
            ToggleVSyncOn = ToggleVSync.isOn;
            SliderTargetFrameRateValue = SliderTargetFrameRate.value;

            if (0 == QualitySettings.vSyncCount)
            {
                int targetFrameRate = (int)SliderTargetFrameRate.value;
                //If target frame rate is set to 0 render with maximum possible framerate
                //indicated by value -1
                targetFrameRate = targetFrameRate == 0 ? -1 : targetFrameRate;
                Application.targetFrameRate = targetFrameRate;
            }
        }
    }
}
