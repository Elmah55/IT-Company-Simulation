using ITCompanySimulation.Core;
using ITCompanySimulation.Event;
using ITCompanySimulation.UI;
using ITCompanySimulation.Utilities;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace ITCompanySimulation.Settings
{
    public class AudioSettingsManager : MonoBehaviour
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
        /// <summary>
        /// Settings that will be used for audio playback.
        /// </summary>
        [SerializeField]
        private AudioVolumeSettings m_VolumeSettings;
        [SerializeField]
        private string AudioVolumeSettingsConfigFileName;
        [SerializeField]
        private VoidEvent VolumeSettingsUpdateEvent;

        /*Public consts fields*/

        /*Public fields*/

        public AudioVolumeSettings VolumeSettings
        {
            get
            {
                return m_VolumeSettings;
            }
        }

        /*Private methods*/

        /// <summary>
        /// Checks if volume value is within range (0-1).
        /// </summary>
        /// <param name="value">Value that will be checked</param>
        /// <returns>Provided value if it is inside allowed range. If provided value is out of range
        /// returned value is clamped to allowed range.</returns>
        private float CheckVolumeValue(float value)
        {
            if (value > 1f || value < 0f)
            {
                string msg = string.Format("Incorrect value of volume parameter ({0})", value);
                RestrictedDebug.Log(msg, LogType.Warning);

                value = Mathf.Clamp(value, 0f, 1f);
            }

            return value;
        }

        /// <summary>
        /// Sets parameter with given name in AudioMixer.
        /// </summary>
        private void SetAudioMixerParam(string paramName, float value)
        {
            bool setFloatResult = AudioMixerComponent.SetFloat(paramName, value);

            if (false == setFloatResult)
            {
                string msg = string.Format("Could not set value of param \"{0}\" in AudioMixer: {1}",
                                           paramName,
                                           AudioMixerComponent.name);
                RestrictedDebug.Log(msg, LogType.Warning);
            }
        }

        /// <summary>
        /// Gets parameter with given name from AudioMixer.
        /// </summary>
        private float GetAudioMixerParam(string paramName)
        {
            float param = float.NaN;
            bool getFloatResult = AudioMixerComponent.GetFloat(paramName, out param);

            if (false == getFloatResult)
            {
                string msg = string.Format("Could not get value of param \"{0}\" in AudioMixer: {1}",
                                           paramName,
                                           AudioMixerComponent.name);
                RestrictedDebug.Log(msg, LogType.Warning);
            }

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

        /// <summary>
        /// Initializes this instance with saved settings.
        /// </summary>
        private void Load()
        {
            IObjectStorage configStorage = ApplicationManager.Instance.ConfigStorage;
            configStorage.DeserializeObject(VolumeSettings, AudioVolumeSettingsConfigFileName);

            VolumeSettings.MusicVolume = CheckVolumeValue(m_VolumeSettings.MusicVolume);
            VolumeSettings.MasterVolume = CheckVolumeValue(m_VolumeSettings.MasterVolume);
            VolumeSettings.UIVolume = CheckVolumeValue(m_VolumeSettings.UIVolume);

            //Values of volume levels in dB log scale
            float musicVolumedB = GetdBVolume(m_VolumeSettings.MusicVolume);
            float UIVolumedB = GetdBVolume(m_VolumeSettings.UIVolume);
            float masterVolumedB = GetdBVolume(m_VolumeSettings.MasterVolume);

            SetAudioMixerParam(MUSIC_GROUP_VOLUME_PARAMETER_NAME, musicVolumedB);
            SetAudioMixerParam(UI_GROUP_VOLUME_PARAMETER_NAME, UIVolumedB);
            SetAudioMixerParam(MASTER_GROUP_VOLUME_PARAMETER_NAME, masterVolumedB);
        }

        private void Start()
        {
            Load();
            VolumeSettingsUpdateEvent.EventInvoked += OnVolumeSettingsUpdate;
        }

        private void OnVolumeSettingsUpdate()
        {
            Apply(VolumeSettings.MasterVolume, VolumeSettings.UIVolume, VolumeSettings.MusicVolume);
        }

        private void OnDestroy()
        {
            SaveSettings();
        }

        /// <summary>
        /// Apply settings for audio volume
        /// </summary>
        /// <param name="masterVolumeLevel">Level of master volume (in %)</param>
        /// <param name="UIVolumeLevel">Level of master volume (in %)</param>
        /// <param name="musicVolumeLevel">Level of master volume (in %)</param>
        /// <param name="preview">If true only level of volume will be changed without saving changes</param>
        private void Apply(float masterVolumeLevel, float UIVolumeLevel, float musicVolumeLevel)
        {
            CheckVolumeValue(masterVolumeLevel);
            CheckVolumeValue(UIVolumeLevel);
            CheckVolumeValue(musicVolumeLevel);

            //Value mapped from linear scale to dB log scale
            float masterVolumedBValue = GetdBVolume(masterVolumeLevel);
            SetAudioMixerParam(MASTER_GROUP_VOLUME_PARAMETER_NAME, masterVolumedBValue);
            float UIVolumedBValue = GetdBVolume(UIVolumeLevel);
            SetAudioMixerParam(UI_GROUP_VOLUME_PARAMETER_NAME, UIVolumedBValue);
            float musicVolumedBValue = GetdBVolume(musicVolumeLevel);
            SetAudioMixerParam(MUSIC_GROUP_VOLUME_PARAMETER_NAME, musicVolumedBValue);
        }

        /// <summary>
        /// Saves settings to storage so they can be restored when application is run again
        /// </summary>
        private void SaveSettings()
        {
            IObjectStorage configStorage = ApplicationManager.Instance.ConfigStorage;
            configStorage.SerializeObject(VolumeSettings, AudioVolumeSettingsConfigFileName);
        }

        /*Public methods*/
    }
}
