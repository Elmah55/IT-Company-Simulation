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
        [SerializeField]
        private AudioClip UIButtonClickSound;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            AudioSourceComponent = GetComponent<AudioSource>();
        }

        private void PlaySound(AudioClip sound)
        {
            AudioSourceComponent.clip = sound;
            AudioSourceComponent.Play();
        }

        /*Public methods*/

        public void PlayUIButtonClickSound()
        {
            PlaySound(UIButtonClickSound);
        }
    } 
}
