using ITCompanySimulation.Event;
using System.Collections;
using UnityEngine;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Component responisble for playing music when application is running.
    /// Only background music is handled, sound effects are handled by other
    /// components.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        [Tooltip("Collection of audio clips that music player will play")]
        private AudioClip[] SoundtrackClips;
        [Tooltip("If true player will play clips in random order. If false will play clips sequentially")]
        [SerializeField]
        private bool PlayRandom;
        [SerializeField]
        private AudioSource MusicAudioSource;
        [SerializeField]
        private AudioSource UIAudioSource;
        [SerializeField]
        private UISoundRequestEvent UISoundPlayRequested;

        /// <summary>
        /// Array index of music clip that will be played next.
        /// </summary>
        private int ClipIndex;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            UISoundPlayRequested.SoundRequested += OnUISoundRequested;
            StartCoroutine(PlayMusicClipsCouroutine());
        }

        /// <summary>
        /// This couroutine will play one clip after another
        /// </summary>
        private IEnumerator PlayMusicClipsCouroutine()
        {
            //Play songs only if there are clips available
            while (SoundtrackClips.Length != 0)
            {
                while (true == MusicAudioSource.isPlaying)
                {
                    yield return null;
                }

                //Audio clip that will be played next
                AudioClip clip;

                if (true == PlayRandom)
                {
                    int randomIndex = Random.Range(0, SoundtrackClips.Length);
                    clip = SoundtrackClips[randomIndex];
                    ClipIndex = randomIndex;
                }
                else
                {
                    clip = SoundtrackClips[ClipIndex++];

                    //Reached end of clips playlist, move back to beginning
                    if (ClipIndex == SoundtrackClips.Length)
                    {
                        ClipIndex = 0;
                    }
                }

                MusicAudioSource.clip = clip;
                MusicAudioSource.Play();
            }
        }

        private void OnUISoundRequested(UISoundRequestEventArgs args)
        {
            if (true == args.PlayExclusively)
            {
                if (false == UIAudioSource.isPlaying)
                {
                    UIAudioSource.PlayOneShot(args.Clip);
                }
            }
            else
            {
                UIAudioSource.PlayOneShot(args.Clip);
            }

        }

        /*Public methods*/
    }
}
