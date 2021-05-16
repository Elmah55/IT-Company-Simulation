using System.Collections;
using UnityEngine;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Component responisble for playing music when application is running.
    /// Only background music is handled, sound effects are handled by other
    /// components.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        [Tooltip("Collection of audio clips that music player will play")]
        private AudioClip[] Clips;
        [Tooltip("If true player will play clips in random order. If false will play clips sequentially")]
        [SerializeField]
        private bool PlayRandom;
        private AudioSource AudioSourceComponent;
        /// <summary>
        /// Array index of clip that will be played next
        /// </summary>
        private int ClipIndex;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            AudioSourceComponent = GetComponent<AudioSource>();
            StartCoroutine(PlayClipsCouroutine());
        }

        /// <summary>
        /// This couroutine will play one clip after another
        /// </summary>
        private IEnumerator PlayClipsCouroutine()
        {
            //Play songs only if there are clips available
            while (Clips.Length != 0)
            {
                while (true == AudioSourceComponent.isPlaying)
                {
                    yield return null;
                }

                //Audio clip that will be played next
                AudioClip clip;

                if (true == PlayRandom)
                {
                    int randomIndex = Random.Range(0, Clips.Length);
                    clip = Clips[randomIndex];
                    ClipIndex = randomIndex;
                }
                else
                {
                    clip = Clips[ClipIndex++];

                    //Reached end of clips playlist, move back to beginning
                    if (ClipIndex == Clips.Length)
                    {
                        ClipIndex = 0;
                    }
                }

                AudioSourceComponent.clip = clip;
                AudioSourceComponent.Play();
            }
        }

        /*Public methods*/
    }
}
