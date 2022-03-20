using UnityEngine;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This is component used for playing sound effects
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundEffects : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private AudioSource AudioSourceComponent;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            AudioSourceComponent = GetComponent<AudioSource>();
        }

        /*Public methods*/

        /// <summary>
        /// Plays audio sound effect.
        /// </summary>
        /// <param name="soundEffect">Sound effect that will be played.</param>
        public void PlaySoundEffect(AudioClip soundEffect)
        {
            AudioSourceComponent.PlayOneShot(soundEffect);
        }

        /// <summary>
        /// Stops effect currently playing and plays new effect. Note that during
        /// playing this effect other effects can be played if PlaySoundEffect is
        /// used.
        /// </summary>
        /// <param name="soundEffect">Sound effect that will be played.</param>
        public void PlaySoundEffectExclusively(AudioClip soundEffect)
        {
            AudioSourceComponent.clip = soundEffect;
            AudioSourceComponent.Stop();
            AudioSourceComponent.Play();
        }
    }
}
