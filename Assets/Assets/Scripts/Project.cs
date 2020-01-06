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
    private int m_DaysSinceStart;

    /*Public consts fields*/

    public const int MAX_WORKERS_PER_PROJECT = 5;

    /*Public fields*/

    public event ProjectAction DaysSinceStartUpdated;
    public event ProjectAction Completed;
    public event ProjectAction ProgressUpdated;
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
                ProgressUpdated?.Invoke(this);

                if (m_Progress >= 100.0f)
                {
                    m_Progress = Mathf.Clamp(m_Progress, 0.0f, 100.0f);
                    IsCompleted = true;
                    Completed?.Invoke(this);
                }
            }
        }
    }
    /// <summary>
    /// Idicates whether the project is completed
    /// </summary>
    public bool IsCompleted { get; private set; }
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
    public int DaysSinceStart
    {
        get
        {
            return m_DaysSinceStart;
        }

        set
        {
            if (value != m_DaysSinceStart)
            {
                m_DaysSinceStart = value;
                DaysSinceStartUpdated?.Invoke(this);
            }
        }
    }

    /*Private methods*/

    /*Public methods*/

    public Project(string projectName)
    {
        this.Name = projectName;

        Workers = new List<Worker>();
        UsedTechnologies = new List<ProjectTechnology>();
    }
}
