using UnityEngine;
using ITCompanySimulation.Character;
using System.Collections.Generic;
using System.Collections;
using ITCompanySimulation.Project;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Scripts that controls UI element displaying worker abilities.
    /// </summary>
    public class WorkerAbilitiesDisplay : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        /// <summary>
        /// Dictionary mapping project technology to progress bar displaying worker's abilities.
        /// </summary>
        private Dictionary<ProjectTechnology, ProgressBar> AbilityProgressBarMap = new Dictionary<ProjectTechnology, ProgressBar>();
        [Tooltip("Color of progress bar showing each ability. Index of this array " +
        "corresponds to ability in ProjectTechnology enum.")]
        [SerializeField]
        private Color[] AbilitiesColor;
        /// <summary>
        /// Stored references to instantiated progress bars.
        /// </summary>
        private List<ProgressBar> AbilitiesProgressBars = new List<ProgressBar>();
        [SerializeField]
        private ProgressBar AbilityProgressBarPrefab;
        /// <summary>
        /// Reference to worker whose abilities are currently displayed.
        /// </summary>
        private SharedWorker DisplayedWorker;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private string GetProgressBarText(ProjectTechnology ability, float abilityValue)
        {
            string result = string.Format("{0}\n{1}",
                                          EnumToString.GetString(ability),
                                          abilityValue.ToString("0.00"));
            return result;
        }

        private IEnumerator ProgressBarSetValueAnimation()
        {
            //Number of finished animations
            int progressBarAnimationsFinished = 0;

            while ((null != DisplayedWorker) && (progressBarAnimationsFinished != DisplayedWorker.Abilites.Count))
            {
                foreach (var ability in DisplayedWorker.Abilites)
                {
                    ProgressBar progBar = AbilityProgressBarMap[ability.Key];

                    if (progBar.Value != ability.Value.Value)
                    {
                        float progBarValue = progBar.Value + 8f * Time.deltaTime;
                        progBarValue = Mathf.Clamp(progBarValue, 0f, ability.Value.Value);
                        progBar.Value = progBarValue;
                        progBar.Text.text = GetProgressBarText(ability.Key, progBar.Value);

                        if (progBar.Value == ability.Value.Value)
                        {
                            ++progressBarAnimationsFinished;
                        }
                    }
                }

                yield return null;
            }
        }

        /*Public methods*/

        public void DisplayWorkerAbilities(SharedWorker worker)
        {
            if (null != DisplayedWorker)
            {
                //Remove listener for previously displayed worker
                DisplayedWorker.AbilityUpdated -= OnWorkerAbilityUpdated;
            }

            DisplayedWorker = worker;
            AbilityProgressBarMap.Clear();

            foreach (ProgressBar abilityProgressBar in AbilitiesProgressBars)
            {
                abilityProgressBar.gameObject.SetActive(false);
            }

            if (null != DisplayedWorker)
            {
                DisplayedWorker.AbilityUpdated += OnWorkerAbilityUpdated;
                if (AbilitiesProgressBars.Count < worker.Abilites.Count)
                {
                    int progressBarsToInstantiate = worker.Abilites.Count - AbilitiesProgressBars.Count;

                    for (int i = 0; i < progressBarsToInstantiate; i++)
                    {
                        ProgressBar newProgressBar = GameObject.Instantiate(AbilityProgressBarPrefab, gameObject.transform);
                        newProgressBar.MinimumValue = 0f;
                        newProgressBar.MaximumValue = SharedWorker.MAX_ABILITY_VALUE;
                        AbilitiesProgressBars.Add(newProgressBar);
                    }
                }

                int j = 0;

                foreach (var workerAbility in worker.Abilites)
                {
                    string progressBarText = GetProgressBarText(workerAbility.Key, workerAbility.Value.Value);
                    Color progressBarColor = AbilitiesColor[(int)workerAbility.Key];
                    ProgressBar abilityProgerssBar = AbilitiesProgressBars[j];
                    abilityProgerssBar.gameObject.SetActive(true);
                    abilityProgerssBar.ProgressImage.color = progressBarColor;
                    abilityProgerssBar.Text.text = progressBarText;
                    abilityProgerssBar.Value = 0f;
                    AbilityProgressBarMap.Add(workerAbility.Key, abilityProgerssBar);
                    ++j;
                }

                StopAllCoroutines();
                StartCoroutine(ProgressBarSetValueAnimation());
            }
        }

        private void OnWorkerAbilityUpdated(SharedWorker worker, ProjectTechnology workerAbility, float workerAbilityValue)
        {
            ProgressBar abilityProgressBar = AbilityProgressBarMap[workerAbility];
            abilityProgressBar.Text.text = GetProgressBarText(workerAbility, workerAbilityValue);
            abilityProgressBar.Value = workerAbilityValue;
        }
    }

}