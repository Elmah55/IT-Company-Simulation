using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents scrum methodology in developing projects.
/// It takes care of updating the binded project as the simulation is ongoing
/// as well as keeping track of scrum statistics
/// </summary>
public class Scrum : IUpdatable
{
    #region Fields

    /// <summary>
    /// Project binded to this instance of scrum
    /// </summary>
    public Project BindedProject { get; set; }
    #endregion

    #region Methods
    public void UpdateState()
    {
        float projectProgressValue = 1.0f;
        BindedProject.Progress += projectProgressValue;

        Debug.Log("Project progress: " + BindedProject.Progress);
    }

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
    #endregion
}
