using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// This class represents scrum methodology in developing projects.
/// It takes care of updating the binded project as the simulation is ongoing
/// as well as keeping track of scrum statistics
/// </summary>
public class Scrum : MonoBehaviour
{
    /*Private consts fields*/

    /// <summary>
    /// How often binded project should be updated.
    /// This time is seconds in game time (scaled time)
    /// </summary>
    private const float PROJECT_UPDATE_FREQUENCY = 10.0f;
    /// <summary>
    /// How much ability value should be added to worker during
    /// one update
    /// </summary>
    private const float ABILITY_UPDATE_VALUE = 0.001f;
    private const int DAYS_PER_SPRINT = 30;

    /*Private fields*/

    private GameTime GameTimeComponent;
    private MainSimulationManager SimulationManagerComponent;
    /// <summary>
    /// How many days since start of current sprint
    /// </summary>
    private int CurrentSprintDays;
    private Coroutine ProjectUpdateCoroutine;
    private SprintStage m_CurrentSprintStage;
    private int m_SprintNumber = 1;

    /*Public consts fields*/

    /*Public fields*/

    /// <summary>
    /// Project binded to this instance of scrum
    /// </summary>
    public Project BindedProject { get; set; }
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

        foreach (Worker projectWorker in BindedProject.Workers)
        {
            if (true == projectWorker.Available)
            {
                projectProgressValue += projectWorker.Score;
            }
        }

        return projectProgressValue;
    }

    private void UpdateProjectWorkersAbilites()
    {
        foreach (Worker projectWorker in BindedProject.Workers)
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
        foreach (Worker projectWorker in BindedProject.Workers)
        {
            if (true == projectWorker.Available)
            {
                projectWorker.ExperienceTime++;
            }
        }
    }

    private void UpdateBindedProject()
    {
        BindedProject.Progress += CalculateProjectProgress();
    }

    private IEnumerator UpdateProject()
    {
        while (true)
        {
            yield return new WaitForSeconds(PROJECT_UPDATE_FREQUENCY);

            UpdateProjectWorkersAbilites();
            UpdateBindedProject();
        }
    }

    private void OnProjectFinished(Project finishedProject)
    {
        StopCoroutine(ProjectUpdateCoroutine);
        string playerNotification = string.Format("Project {0} finished. Your company has earned {1} $",
            finishedProject.Name, finishedProject.CompleteBonus);
        SimulationManagerComponent.NotificatorComponent.Notify(playerNotification);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        string debugInfo = string.Format("Project {0} (ID {2}) finished. {1} $ added to company's balance",
            finishedProject.Name, finishedProject.CompleteBonus, finishedProject.ID);
        Debug.Log(debugInfo);
#endif
    }

    private void Start()
    {
        GameTimeComponent = GetComponent<GameTime>();
        GameTimeComponent.DayChanged += OnGameTimeDayChanged;
        SimulationManagerComponent = GetComponent<MainSimulationManager>();

        SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnControlledCompanyWorkerRemoved;
    }

    private void OnControlledCompanyWorkerRemoved(Worker companyWorker)
    {
        if (BindedProject == companyWorker.AssignedProject)
        {
            //TODO: Make leaving project event fire first (before worker leaves company).
            //Make this event fired by Company class
            BindedProject.RemoveWorker(companyWorker);

            if (0 == BindedProject.Workers.Count && true == BindedProject.Active)
            {
                StopProject();
            }
        }
    }

    private void UpdateSprintInfo()
    {
        if (true == BindedProject.Active)
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
    }

    private void OnGameTimeDayChanged()
    {
        if (true == BindedProject.StartedOnce && false == BindedProject.IsCompleted)
        {
            BindedProject.DaysSinceStart++;
        }

        UpdateProjectWorkersExpierience();
        UpdateSprintInfo();
    }

    /*Public methods*/

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

        if (false == BindedProject.StartedOnce)
        {
            BindedProject.Completed += OnProjectFinished;
            BindedProject.TimeOfStart = GameTimeComponent.CurrentTime;
        }

        BindedProject.Start();
        ProjectUpdateCoroutine = StartCoroutine(UpdateProject());
    }

    public void StopProject()
    {
        BindedProject.Stop();
        StopCoroutine(ProjectUpdateCoroutine);
    }

    /// <summary>
    /// Returns number of estimated days (game time) needed to complete binded project
    /// Returns -1 if estimated time is infinity
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
            secondsToCompletion *= PROJECT_UPDATE_FREQUENCY;
            daysToCompletionGameTime = Mathf.RoundToInt(secondsToCompletion / GameTime.SecondsInDay);
        }
        else
        {
            daysToCompletionGameTime = -1;
        }


        return daysToCompletionGameTime;
    }
}
