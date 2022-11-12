using ITCompanySimulation.Utilities;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace ITCompanySimulation.Settings
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "ITCompanySimulation/Settings/AudioSettings")]
    public class AudioSettings : ScriptableObject
    {
        /*Private consts fields*/

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

        [SerializeField]
        private AudioMixer AudioMixerComponent;

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Level of music volume (in range 0-1)
        /// </summary>
        [Range(0f, 1f)]
        public float MusicVolume;
        /// <summary>
        /// Level of UI volume (in range 0-1)
        /// </summary>
        [Range(0f, 1f)]
        public float UIVolume;
        /// <summary>
        /// Level of master volume (in range 0-1)
        /// </summary>
        [Range(0f, 1f)]
        public float MasterVolume;

        /*Private methods*/

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
        /// Sets parameter with given name in AudioMixer.
        /// </summary>
        private void SetAudioMixerParam(string paramName, float value)
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
        private float GetAudioMixerParam(string paramName)
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
        private float GetdBVolume(float volumeLevel)
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
        /// <param name="preview">If true only level of volume will be changed without saving changes</param>
        public void Apply(float masterVolumeLevel, float UIVolumeLevel, float musicVolumeLevel, bool preview)
        {
            CheckVolumeValue(masterVolumeLevel);
            CheckVolumeValue(UIVolumeLevel);
            CheckVolumeValue(musicVolumeLevel);

            if (false == preview)
            {
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
        /// Initializes this instance with saved settings.
        /// </summary>
        public void Load()
        {
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
