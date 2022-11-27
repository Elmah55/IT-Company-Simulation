using UnityEngine;

namespace ITCompanySimulation.UI
{
    [CreateAssetMenu(fileName = "AudioVolumeSettings", menuName = "ITCompanySimulation/Settings/AudioSettings")]
    public class AudioVolumeSettings : ScriptableObject
    {
        /*Private consts fields*/

        /*Private fields*/

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

        /*Public methods*/
    }
}
