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

    /*Private fields*/

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

    public void StartProject()
    {
        foreach (Worker companyWorker in BindedProject.Workers)
        {
            Debug.LogFormat("Worker {0} {1}",
                new object[] { companyWorker.Name, companyWorker.Surename });
        }

        BindedProject.OnProjectCompleted += OnProjectFinished;
        StartCoroutine(UpdateProject());
    }

    public void StopProject()
    {
        StopCoroutine(UpdateProject());
    }
}
