using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents IT project that company can work on
/// </summary>
public class Project
{
    /*Private consts fields*/

    /*Private fields*/

    private float m_Progress;

    /*Public consts fields*/

    public const int MAX_WORKERS_PER_PROJECT = 5;

    /*Public fields*/

    public event ProjectAction ProjectCompleted;
    public event ProjectAction ProjectProgressUpdated;
    public string Name { get; set; }
    /// <summary>
    /// Progress of project completed (in %)
    /// </summary>
    public float Progress
    {
        get
        {
            return m_Progress;
        }

        set
        {
            if (m_Progress != value)
            {
                m_Progress = value;
                ProjectProgressUpdated?.Invoke(this);

                if (m_Progress > 100.0f)
                {
                    m_Progress = Mathf.Clamp(m_Progress, 0.0f, 100.0f);
                    Completed = true;
                    ProjectCompleted?.Invoke(this);
                }
            }
        }
    }
    /// <summary>
    /// Idicates whether the project is completed
    /// </summary>
    public bool Completed { get; private set; }
    /// <summary>
    /// Is project active and its state can be updated (project in progress)
    /// </summary>
    public bool Active { get; set; }
    /// <summary>
    /// Workers that are working on this project
    /// </summary>
    public List<Worker> Workers { get; private set; }
    public List<ProjectTechnology> UsedTechnologies { get; private set; }
    public DateTime TimeOfStart { get; set; }
    public int DaysSinceStart { get; set; }

    /*Private methods*/

    /*Public methods*/

    public Project(string projectName)
    {
        this.Name = projectName;

        Workers = new List<Worker>();
        UsedTechnologies = new List<ProjectTechnology>();
    }
}
