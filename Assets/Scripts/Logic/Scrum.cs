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
            Debug.Log("Project progress: " + BindedProject.Progress);
        }
    }

    private void OnProjectFinished(Project finishedProject)
    {
        StopCoroutine(ProjectUpdateCoroutine);
        Debug.Log("Project finished");
    }

    private void Start()
    {
        GameTimeComponent = GetComponent<GameTime>();
        GameTimeComponent.DayChanged += OnGameTimeDayChanged;
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

        foreach (Worker companyWorker in BindedProject.Workers)
        {
            Debug.LogFormat("Worker {0} {1}",
                new object[] { companyWorker.Name, companyWorker.Surename });
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
}
