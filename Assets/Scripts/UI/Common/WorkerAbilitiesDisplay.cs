using UnityEngine;
using ITCompanySimulation.Character;
using System.Collections.Generic;
using System.Collections;
using ITCompanySimulation.Project;
using ITCompanySimulation.Utilities;

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
        private IObjectPool<ProgressBar> AbilitiesProgressBarPool = new ObjectPool<ProgressBar>();
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
            if (DisplayedWorker != worker)
            {
                if (null != DisplayedWorker)
                {
                    //Remove listener for previously displayed worker
                    DisplayedWorker.AbilityUpdated -= OnWorkerAbilityUpdated;
                }

                DisplayedWorker = worker;

                //Deactive all previously used progress bars and
                //return them to pool
                foreach (var ability in AbilityProgressBarMap)
                {
                    ability.Value.gameObject.SetActive(false);
                    AbilitiesProgressBarPool.AddObject(ability.Value);
                }

                AbilityProgressBarMap.Clear();

                if (null != DisplayedWorker)
                {
                    DisplayedWorker.AbilityUpdated += OnWorkerAbilityUpdated;

                    foreach (var workerAbility in worker.Abilites)
                    {
                        ProgressBar abilityProgerssBar = GetAbilityProgressBar();
                        SetAbilityProgressBar(workerAbility.Key, workerAbility.Value.Value, abilityProgerssBar);
                        //Set to 0 so progress bar animation can be run
                        abilityProgerssBar.Value = 0f;
                        AbilityProgressBarMap.Add(workerAbility.Key, abilityProgerssBar);
                    }

                    StopAllCoroutines();
                    StartCoroutine(ProgressBarSetValueAnimation());
                }
            }
        }

        /// <summary>
        /// Returns new ability progress bar.
        /// </summary>
        private ProgressBar GetAbilityProgressBar()
        {
            ProgressBar newProgressBar = AbilitiesProgressBarPool.GetObject();

            //No progress bars available in pool. Instantiate new object from prefab
            if (null == newProgressBar)
            {
                newProgressBar = GameObject.Instantiate(AbilityProgressBarPrefab, gameObject.transform);
                newProgressBar.MinimumValue = 0f;
                newProgressBar.MaximumValue = SharedWorker.MAX_ABILITY_VALUE;
            }

            newProgressBar.gameObject.SetActive(true);

            return newProgressBar;
        }

        /// <summary>
        /// Sets progress bar to display ability.
        /// </summary>
        /// <param name="abilityType">Type of ability to be displayed.</param>
        /// <param name="abilityValue">Value of ability to be displayed.</param>
        /// <param name="abilityProgressBar">Progress bar that will display ability.</param>
        private void SetAbilityProgressBar(ProjectTechnology abilityType, float abilityValue, ProgressBar abilityProgressBar)
        {
            string progressBarText = GetProgressBarText(abilityType, abilityValue);
            Color progressBarColor = AbilitiesColor[(int)abilityType];

            abilityProgressBar.ProgressImage.color = progressBarColor;
            abilityProgressBar.Text.text = progressBarText;
            abilityProgressBar.Value = abilityValue;
        }

        private void OnWorkerAbilityUpdated(SharedWorker worker, ProjectTechnology workerAbility, float workerAbilityValue)
        {
            ProgressBar abilityProgressBar = null;

            //Existing ability updated
            if (true == AbilityProgressBarMap.ContainsKey(workerAbility))
            {
                abilityProgressBar = AbilityProgressBarMap[workerAbility];
            }
            //New ability has been added to worker. Get new progress bard to display it
            else
            {
                abilityProgressBar = GetAbilityProgressBar();
                AbilityProgressBarMap.Add(workerAbility, abilityProgressBar);
            }

            SetAbilityProgressBar(workerAbility, worker.Abilites[workerAbility].Value, abilityProgressBar);
        }
    }

}