using UnityEngine;

namespace ITCompanySimulation.Event
{
    public class UISoundRequestEventArgs
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Clip that will be requested.
        /// </summary>
        public AudioClip Clip { get; }
        /// <summary>
        /// If true clip will only play if there is no other clip playing currently.
        /// </summary>
        public bool PlayExclusively { get; }

        /*Private methods*/

        /*Public methods*/

        public UISoundRequestEventArgs(AudioClip clip, bool playExclusively = false)
        {
            this.Clip = clip;
            this.PlayExclusively = playExclusively;
        }
    }
}
