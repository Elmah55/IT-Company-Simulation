using UnityEngine;
using ITCompanySimulation.Event;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Component that allows requesting playing given audio clip and applying provided audio settings.
    /// </summary>
    public class UISoundRequestSource : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private UISoundRequestEvent SoundRequestEventObject;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        /// <summary>
        /// Request playing provided clip as UI sound.
        /// </summary>
        public void RequestUISound(AudioClip clip)
        {
            UISoundRequestEventArgs args = new UISoundRequestEventArgs(clip);
            SoundRequestEventObject.RaiseEvent(args);
        }
    } 
}
