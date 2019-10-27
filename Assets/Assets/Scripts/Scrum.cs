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

    /*Private fields*/

    private GameTime GameTimeComponent;
    /// <summary>
    /// How many days since game start passed since start 
    /// of game when project was last updated
    /// </summary>
    private int ProjectLastUpdateDaysSinceStart;

    /*Public consts fields*/

    /// <summary>
    /// Project binded to this instance of scrum
    /// </summary>
    public Project BindedProject { get; set; }

    /*Public fields*/

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
            projectProgressValue += projectWorker.Score;
        }

        return projectProgressValue;
    }


    private void UpdateProjectWorkers()
    {
        //How many days of expierience should be added to each of workers
        int experienceDays = GameTimeComponent.DaysSinceStart - ProjectLastUpdateDaysSinceStart;

        foreach (Worker projectWorker in BindedProject.Workers)
        {
            projectWorker.ExperienceTime += experienceDays;

            foreach (ProjectTechnology usedTechnolody in BindedProject.UsedTechnologies)
            {
                projectWorker.Abilites[usedTechnolody] += ABILITY_UPDATE_VALUE;
            }
        }
    }

    private IEnumerator UpdateProject()
    {
        while (true)
        {
            BindedProject.Progress += CalculateProjectProgress();
            Debug.Log("Project progress: " + BindedProject.Progress);

            yield return new WaitForSeconds(PROJECT_UPDATE_FREQUENCY);
        }
    }

    private void OnProjectFinished(Project finishedProject)
    {
        StopProject();
        Debug.Log("Project finished");
    }

    /*Public methods*/

    public void Start()
    {
        GameTimeComponent = GetComponent<GameTime>();
        ProjectLastUpdateDaysSinceStart = GameTimeComponent.DaysSinceStart;
    }

    public void StartProject()
    {
        foreach (Worker companyWorker in BindedProject.Workers)
        {
            Debug.LogFormat("Worker {0} {1}",
                new object[] { companyWorker.Name, companyWorker.Surename });
        }

        BindedProject.OnProjectCompleted += OnProjectFinished;
        BindedProject.TimeOfStart = GameTimeComponent.CurrentTime;
        StartCoroutine(UpdateProject());
    }

    public void StopProject()
    {
        StopCoroutine(UpdateProject());
    }
}
