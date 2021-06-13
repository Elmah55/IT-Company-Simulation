using ITCompanySimulation.Utilities;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace ITCompanySimulation.Settings
{
    public static class AudioSettings
    {
        /*Private consts fields*/

        /// <summary>
        /// Default value for string values in PlayerPrefs
        /// </summary>
        private const string DEFAULT_STRING_KEY_VALUE = "";
        /// <summary>
        /// Default value for float values in PlayerPrefs
        /// </summary>
        private const float DEFAULT_FLOAT_KEY_VALUE = -1f;

        //Keys values for accessing settings from
        //PlayerPrefs class

        private const string MASTER_VOLUME_KEY = "MasterVolume";
        private const string UI_VOLUME_KEY = "UIVolume";
        private const string MUSIC_VOLUME_KEY = "MusicVolume";

        //Names of AudioMixer exposed parameters
        private const string MUSIC_GROUP_VOLUME_PARAMETER_NAME = "MusicVolume";
        private const string UI_GROUP_VOLUME_PARAMETER_NAME = "UIVolume";
        private const string MASTER_GROUP_VOLUME_PARAMETER_NAME = "MasterVolume";

        /// <summary>
        /// Minimum possible volume level that can be assigned to AudioMixer group.
        /// This will be used to mute group when linear value is 0.
        /// </summary>
        private const float AUDIO_MIXER_MIN_DB_VOLUME_VALUE = -80f;

        /*Private fields*/

        private static AudioMixer AudioMixerComponent;

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Level of music volume (in %)
        /// </summary>
        public static float MusicVolume { get; private set; }
        /// <summary>
        /// Level of UI volume (in %)
        /// </summary>
        public static float UIVolume { get; private set; }
        /// <summary>
        /// Level of master volume (in %)
        /// </summary>
        public static float MasterVolume { get; private set; }

        /*Private methods*/

        static AudioSettings()
        {
            //Get handle to music audio source to get audio mixer. All audio sources will use same audio mixer
            AudioSource musicAudioSource =
                GameObject.FindGameObjectWithTag("ApplicationManager").GetComponent<AudioSource>();
            AudioMixerComponent = musicAudioSource.outputAudioMixerGroup.audioMixer;
        }

        /// <summary>
        /// Checks if volume value is within range (0-1)
        /// </summary>
        /// <param name="value">Value thath will be checked</param>
        private static void CheckVolumeValue(float value)
        {
            if (value > 1f || value < 0f)
            {
                string exceptionMsg = string.Format("Value of percent argument should be in range 0-1. Actual value: {0}",
                                                    value);
                throw new ArgumentOutOfRangeException(exceptionMsg);
            }
        }

        /// <summary>
        /// Checks if param has default value. If it has it loads param
        /// from AudioMixer instead of from registry. Default value of params
        /// means it wasn't saved to registry before.
        /// </summary>
        /// <returns>Param value from AudioMixer (converted to linear value (0-100%)) if param has default value
        /// otherwise returns same value as provided in argument</returns>
        private static float CheckDefaultParam(float param, string audioMixerParamName)
        {
            if (DEFAULT_FLOAT_KEY_VALUE == param)
            {
                param = GetAudioMixerParam(audioMixerParamName);
                param = Utils.MapDbToLinear(param);
            }

            return param;
        }

        /// <summary>
        /// Sets parameter with given name in AudioMixer.
        /// </summary>
        private static void SetAudioMixerParam(string paramName, float value)
        {
            bool setFloatResult = AudioMixerComponent.SetFloat(paramName, value);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (false == setFloatResult)
            {
                Debug.LogWarningFormat("[0] Could not set value of param \"{1}\" in AudioMixer: {2}",
                                        MethodBase.GetCurrentMethod().DeclaringType,
                                        paramName,
                                        AudioMixerComponent.name);
            }
#endif
        }

        /// <summary>
        /// Gets parameter with given name from AudioMixer.
        /// </summary>
        private static float GetAudioMixerParam(string paramName)
        {
            float param = float.NaN;
            bool getFloatResult = AudioMixerComponent.GetFloat(paramName, out param);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (false == getFloatResult)
            {
                Debug.LogWarningFormat("[0] Could not get value of param \"{1}\" in AudioMixer: {2}",
                                        MethodBase.GetCurrentMethod().DeclaringType,
                                        paramName,
                                        AudioMixerComponent.name);
            }
#endif

            return param;
        }

        /// <summary>
        /// Returns volume converted to dB scale
        /// </summary>
        /// <returns></returns>
        private static float GetdBVolume(float volumeLevel)
        {
            float dBVolume = Utils.MapLinearTodB(volumeLevel);

            if (true == float.IsNaN(dBVolume))
            {
                dBVolume = AUDIO_MIXER_MIN_DB_VOLUME_VALUE;
            }

            return dBVolume;
        }

        /*Public methods*/

        /// <summary>
        /// Apply settings for audio volume
        /// </summary>
        /// <param name="masterVolumeLevel">Level of master volume (in %)</param>
        /// <param name="UIVolumeLevel">Level of master volume (in %)</param>
        /// <param name="musicVolumeLevel">Level of master volume (in %)</param>
        /// <param name="preview">If true only level of volume will be changed without saving changes to registry</param>
        public static void Apply(float masterVolumeLevel, float UIVolumeLevel, float musicVolumeLevel, bool preview)
        {
            CheckVolumeValue(masterVolumeLevel);
            CheckVolumeValue(UIVolumeLevel);
            CheckVolumeValue(musicVolumeLevel);

            if (false == preview)
            {
                PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolumeLevel);
                PlayerPrefs.SetFloat(UI_VOLUME_KEY, UIVolumeLevel);
                PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolumeLevel);

                MasterVolume = masterVolumeLevel;
                UIVolume = UIVolumeLevel;
                MusicVolume = musicVolumeLevel;
            }

            //Value mapped from linear scale to dB log scale
            float masterVolumedBValue = GetdBVolume(masterVolumeLevel);
            SetAudioMixerParam(MASTER_GROUP_VOLUME_PARAMETER_NAME, masterVolumedBValue);
            float UIVolumedBValue = GetdBVolume(UIVolumeLevel);
            SetAudioMixerParam(UI_GROUP_VOLUME_PARAMETER_NAME, UIVolumedBValue);
            float musicVolumedBValue = GetdBVolume(musicVolumeLevel);
            SetAudioMixerParam(MUSIC_GROUP_VOLUME_PARAMETER_NAME, musicVolumedBValue);
        }

        /// <summary>
        /// Loads settings from registry
        /// </summary>
        public static void Load()
        {
            MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_FLOAT_KEY_VALUE);
            MasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, DEFAULT_FLOAT_KEY_VALUE);
            UIVolume = PlayerPrefs.GetFloat(UI_VOLUME_KEY, DEFAULT_FLOAT_KEY_VALUE);

            MusicVolume = CheckDefaultParam(MusicVolume, MUSIC_GROUP_VOLUME_PARAMETER_NAME);
            UIVolume = CheckDefaultParam(UIVolume, UI_GROUP_VOLUME_PARAMETER_NAME);
            MasterVolume = CheckDefaultParam(MasterVolume, MASTER_GROUP_VOLUME_PARAMETER_NAME);

            //Values of volume levels in dB log scale
            float musicVolumedB = GetdBVolume(MusicVolume);
            float UIVolumedB = GetdBVolume(UIVolume);
            float masterVolumedB = GetdBVolume(MasterVolume);

            SetAudioMixerParam(MUSIC_GROUP_VOLUME_PARAMETER_NAME, musicVolumedB);
            SetAudioMixerParam(UI_GROUP_VOLUME_PARAMETER_NAME, UIVolumedB);
            SetAudioMixerParam(MASTER_GROUP_VOLUME_PARAMETER_NAME, masterVolumedB);
        }
    }
}
