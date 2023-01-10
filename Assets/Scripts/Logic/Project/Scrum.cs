using ITCompanySimulation.Character;
using ITCompanySimulation.Company;
using ITCompanySimulation.Core;
using ITCompanySimulation.Utilities;
using System;
using UnityEngine;

namespace ITCompanySimulation.Project
{
    /// <summary>
    /// This class represents scrum methodology in developing projects.
    /// It takes care of updating the binded project as the simulation is ongoing
    /// as well as keeping track of scrum statistics
    /// </summary>
    public class Scrum
    {
        /*Private consts fields*/

        /// <summary>
        /// How much ability value should be added to worker during
        /// one update
        /// </summary>
        private const float ABILITY_UPDATE_VALUE = 0.06f;
        private const int DAYS_PER_SPRINT = 30;

        /*Private fields*/

        private GameTime GameTimeComponent;
        private SimulationManager SimulationManagerComponent;
        /// <summary>
        /// How many days since start of current sprint
        /// </summary>
        private int CurrentSprintDays;
        private SprintStage m_CurrentSprintStage;
        private int m_SprintNumber = 1;
        private LocalProject m_BindedProject;

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Project binded to this instance of scrum
        /// </summary>
        public LocalProject BindedProject { get; private set; }
        /// <summary>
        /// Number of sprint sprint is currently in progress
        /// </summary>
        public int SprintNumber
        {
            get
            {
                return m_SprintNumber;
            }

            set
            {
                if (value != m_SprintNumber)
                {
                    m_SprintNumber = value;
                    SprintNumberChanged?.Invoke(this);
                }
            }
        }
        public SprintStage CurrentSprintStage
        {
            get
            {
                return m_CurrentSprintStage;
            }

            private set
            {
                if (value != m_CurrentSprintStage)
                {
                    m_CurrentSprintStage = value;
                    SprintStageChanged?.Invoke(this);
                }
            }
        }
        public event ScrumAtion SprintStageChanged;
        public event ScrumAtion SprintNumberChanged;

        /*Private methods*/

        /// <summary>
        /// Calculates value (in %) of project advancment
        /// for single update
        /// </summary>
        /// <returns></returns>
        private float CalculateProjectProgress()
        {
            float projectProgressValue = 0.0f;
            float projectProgressMultiplier = 0.07f;

            foreach (LocalWorker projectWorker in BindedProject.Workers)
            {
                if (true == projectWorker.Available)
                {
                    projectProgressValue += projectWorker.Score;
                }
            }

            projectProgressValue *= projectProgressMultiplier;

            return projectProgressValue;
        }

        private void UpdateProjectWorkersAbilites()
        {
            foreach (LocalWorker projectWorker in BindedProject.Workers)
            {
                if (true == projectWorker.Available)
                {
                    foreach (ProjectTechnology usedTechnolody in BindedProject.UsedTechnologies)
                    {
                        projectWorker.UpdateAbility(usedTechnolody, ABILITY_UPDATE_VALUE);
                    }
                }
            }
        }

        private void UpdateProjectWorkersExpierience()
        {
            foreach (LocalWorker projectWorker in BindedProject.Workers)
            {
                if (true == projectWorker.Available)
                {
                    ++projectWorker.ExperienceTime;
                }
            }
        }

        private void UpdateBindedProject()
        {
            BindedProject.Progress += CalculateProjectProgress();
        }

        private void OnProjectFinished(LocalProject finishedProject)
        {
            for (int i = BindedProject.Workers.Count - 1; i >= 0; i--)
            {
                BindedProject.RemoveWorker(BindedProject.Workers[i]);
            }

            string playerNotification = string.Format("Project {0} finished. Your company has earned {1} $",
                finishedProject.Name, finishedProject.CompletionBonus);
            SimulationManagerComponent.NotificatorComponent.Notify(playerNotification);

            string debugInfo = string.Format("Project {0} (ID {2}) finished. {1} $ added to company's balance",
                finishedProject.Name, finishedProject.CompletionBonus, finishedProject.ID);
            RestrictedDebug.Log(debugInfo);
        }

        private void OnBindedProjectWorkerAdded(SharedWorker worker)
        {
            if (BindedProject.Workers.Count > 0 && false == BindedProject.IsActive && false == BindedProject.IsCompleted)
            {
                StartProject();
            }
        }

        private void OnBindedProjectWorkerRemoved(SharedWorker worker)
        {
            if (0 == BindedProject.Workers.Count && true == BindedProject.IsActive)
            {
                StopProject();
            }
        }

        private void UpdateSprint()
        {
            CurrentSprintDays++;

            if (DAYS_PER_SPRINT == CurrentSprintDays)
            {
                CurrentSprintDays = 0;
                SprintNumber++;
            }

            if (CurrentSprintDays < 2)
            {
                CurrentSprintStage = SprintStage.Planning;
            }
            else if (CurrentSprintDays < 29)
            {
                CurrentSprintStage = SprintStage.Developing;
            }
            else
            {
                CurrentSprintStage = SprintStage.Retrospective;
            }
        }

        private void UpdateProjectCompletionTime()
        {
            if (false == BindedProject.IsCompleted)
            {
                --BindedProject.CompletionTime;
            }

            if (0 == BindedProject.CompletionTime)
            {
                SimulationManagerComponent.ControlledCompany.Balance -= BindedProject.CompletionTimeExceededPenalty;
                string notificationMessage = string.Format(
                    "Completion time of your project {0} (ID {1}) has been exceeded. Your company paid {2} $ of penalty",
                    BindedProject.Name, BindedProject.ID, BindedProject.CompletionTimeExceededPenalty);
                SimulationManagerComponent.NotificatorComponent.Notify(notificationMessage);
            }
        }

        private void OnGameTimeDayChanged()
        {
            if (true == BindedProject.StartedOnce && false == BindedProject.IsCompleted)
            {
                BindedProject.DaysSinceStart++;
            }

            if (true == BindedProject.IsActive)
            {
                UpdateProjectWorkersExpierience();
                UpdateSprint();
            }

            UpdateProjectCompletionTime();
        }

        /*Public methods*/

        /// <param name="bindedProject">Instance of project associated with this scrum instance</param>
        public Scrum(LocalProject bindedProject)
        {
            //Game object containing scripts
            GameObject scriptsObject = GameObject.FindGameObjectWithTag("ScriptsGameObject");
            SimulationManagerComponent = scriptsObject.GetComponent<SimulationManager>();
            GameTimeComponent = scriptsObject.GetComponent<GameTime>();

            this.BindedProject = bindedProject;
            this.BindedProject.Completed += OnProjectFinished;
            this.BindedProject.TimeOfStart = GameTimeComponent.CurrentDate;
            this.BindedProject.WorkerRemoved += OnBindedProjectWorkerRemoved;
            this.BindedProject.WorkerAdded += OnBindedProjectWorkerAdded;
            GameTimeComponent.DayChanged += OnGameTimeDayChanged;
        }

        public void UpdateProgress()
        {
            if (true == BindedProject.IsActive)
            {
                UpdateProjectWorkersAbilites();
                UpdateBindedProject();
            }
        }

        public void StartProject()
        {
            if (true == BindedProject.IsCompleted)
            {
                throw new InvalidOperationException("Cannot start project that is already completed");
            }

            if (0 == BindedProject.Workers.Count)
            {
                throw new InvalidOperationException("Cannot start project without no workers");
            }

            BindedProject.Start();
        }

        public void StopProject()
        {
            BindedProject.Stop();
        }

        /// <summary>
        /// Returns number of estimated days (game time) needed to complete binded project.
        /// Returns -1 if estimated time is infinity.
        /// </summary>
        public int GetProjectEstimatedCompletionTime()
        {
            float secondsToCompletion = (100 - this.BindedProject.Progress);
            float projSingleUpdateProgress = CalculateProjectProgress();
            int daysToCompletionGameTime;

            //Check if estimated time is not infinity
            if (0 != projSingleUpdateProgress)
            {
                secondsToCompletion /= projSingleUpdateProgress;
                secondsToCompletion *= PlayerCompanyManager.PROJECT_UPDATE_FREQUENCY;
                daysToCompletionGameTime = Mathf.RoundToInt(secondsToCompletion / GameTime.SecondsInDay);
            }
            else
            {
                daysToCompletionGameTime = -1;
            }

            return daysToCompletionGameTime;
        }
    }
}
