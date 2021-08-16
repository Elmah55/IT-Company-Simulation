using UnityEngine;
using ITCompanySimulation.Core;
using UnityEngine.SceneManagement;
using TMPro;

namespace ITCompanySimulation.UI
{
    public class UILoadingScreen : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private ApplicationManager ApplicationManagerComponent;
        [SerializeField]
        private ProgressBar ProgressBarSceneLoading;
        [SerializeField]
        private TextMeshProUGUI TextSceneLoadingProgress;
        /// <summary>
        /// Object with loading screen elements disabled when scene is loaded so
        /// it doesn't collide with objects in other scenes (i.e camera component)
        /// </summary>
        [SerializeField]
        private GameObject LoadingScreenObject;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            ApplicationManagerComponent.SceneStartedLoading += OnSceneStartedLoading;
            ApplicationManagerComponent.SceneFinishedLoading += OnSceneFinishedLoading;
            ApplicationManagerComponent.SceneLoadingProgressChanged += OnSceneLoadingProgressChanged;
            ProgressBarSceneLoading.Value = 0f;
            SetTextSceneLoadingProgress(0f);
        }

        private void SetTextSceneLoadingProgress(float progress)
        {
            TextSceneLoadingProgress.text = string.Format("Loading {0} %", (int)progress);
        }

        private void OnSceneLoadingProgressChanged(float progress)
        {
            ProgressBarSceneLoading.Value = progress;
            SetTextSceneLoadingProgress(progress);
        }

        private void OnSceneStartedLoading(Scene loadedScene)
        {
            LoadingScreenObject.SetActive(true);
        }

        private void OnSceneFinishedLoading(Scene loadedScene)
        {
            LoadingScreenObject.SetActive(false);
            ProgressBarSceneLoading.Value = 0f;
            SetTextSceneLoadingProgress(0f);
        }

        /*Public methods*/
    }
}
