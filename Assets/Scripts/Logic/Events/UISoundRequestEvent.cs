using UnityEngine;
using UnityEngine.Events;

namespace ITCompanySimulation.Event
{
    /// <summary>
    /// Event used for requesting UI sound to be played by audio manager.
    /// </summary>
    [CreateAssetMenu(fileName = "UISoundRequestEvent", menuName = "ITCompanySimulation/Events/UISoundRequestEvent")]
    public class UISoundRequestEvent : ScriptableObject
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        public event UnityAction<UISoundRequestEventArgs> SoundRequested;

        /*Private methods*/

        /*Public methods*/

        /// <param name="clip">Clip that will be requested</param>
        /// <param name="playExclusively">If true clip will only play if there is no other clip playing currently</param>
        public void RaiseEvent(UISoundRequestEventArgs args)
        {
            SoundRequested?.Invoke(args);
        }
    }
}
