using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents IT project that company can work on
/// </summary>
public class Project : ISharedObject
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
    public event ProjectAction Started;
    public event ProjectAction Stopped;
    public event WorkerAction WorkerAdded;
    public event WorkerAction WorkerRemoved;

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
                    Active = false;
                    Completed?.Invoke(this);
                }
            }
        }
    }
    /// <summary>
    /// Idicates whether the project is completed
    /// </summary>
    public bool IsCompleted
    {
        get
        {
            return 100.0f == Progress;
        }
    }
    /// <summary>
    /// Is project active and its state can be updated (project in progress)
    /// </summary>
    public bool Active { get; set; }
    /// <summary>
    /// Workers that are working on this project
    /// </summary>
    public List<Worker> Workers { get; private set; }
    public List<ProjectTechnology> UsedTechnologies { get; set; }
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
    /// <summary>
    /// Used to assing unique ID for each project
    /// </summary>
    public int ID { get; set; }
    /// <summary>
    /// How much money company will receive after finishing project
    /// </summary>
    public int CompleteBonus { get; set; }
    /// <summary>
    /// Stores index of name from project's name table.
    /// Used for serialization
    /// </summary>
    public int ProjectNameIndex { get; set; }

    /*Private methods*/

    /*Public methods*/

    public static byte[] Serialize(object projectObject)
    {
        Project projectToSerialize = (Project)projectObject;

        byte[] nameIndexBytes = BitConverter.GetBytes(projectToSerialize.ProjectNameIndex);
        byte[] IDBytes = BitConverter.GetBytes(projectToSerialize.ID);
        byte[] completeBonusBytes = BitConverter.GetBytes(projectToSerialize.CompleteBonus);
        byte[] technologiesBytes = new byte[projectToSerialize.UsedTechnologies.Count];

        for (int i = 0; i < projectToSerialize.UsedTechnologies.Count; i++)
        {
            technologiesBytes[i] = (byte)projectToSerialize.UsedTechnologies[i];
        }

        //Used to store number of bytes used for technologies
        byte[] technologiesBytesSize = BitConverter.GetBytes(technologiesBytes.Length);

        int projectBytesSize = nameIndexBytes.Length
                             + IDBytes.Length
                             + completeBonusBytes.Length
                             + technologiesBytes.Length
                             + technologiesBytesSize.Length;

        byte[] projectBytes = new byte[projectBytesSize];
        int offset = 0;

        Array.Copy(nameIndexBytes, 0, projectBytes, offset, nameIndexBytes.Length);
        offset += nameIndexBytes.Length;
        Array.Copy(IDBytes, 0, projectBytes, offset, IDBytes.Length);
        offset += IDBytes.Length;
        Array.Copy(completeBonusBytes, 0, projectBytes, offset, completeBonusBytes.Length);
        offset += completeBonusBytes.Length;
        Array.Copy(technologiesBytesSize, 0, projectBytes, offset, technologiesBytesSize.Length);
        offset += technologiesBytesSize.Length;
        Array.Copy(technologiesBytes, 0, projectBytes, offset, technologiesBytes.Length);

        return projectBytes;
    }

    public static object Deserialize(byte[] projectBytes)
    {
        int offset = 0;
        int nameIndex;
        int ID;
        int completeBonus;
        int technologiesSize;
        List<ProjectTechnology> technologies = new List<ProjectTechnology>();

        nameIndex = BitConverter.ToInt32(projectBytes, offset);
        offset += sizeof(int);
        ID = BitConverter.ToInt32(projectBytes, offset);
        offset += sizeof(int);
        completeBonus = BitConverter.ToInt32(projectBytes, offset);
        offset += sizeof(int);
        technologiesSize = BitConverter.ToInt32(projectBytes, offset);

        for (int i = 0; i < technologiesSize; i++)
        {
            ProjectTechnology technology = (ProjectTechnology)projectBytes[offset];
            technologies.Add(technology);
            offset += sizeof(byte);
        }

        Project deserializedProject = new Project(ProjectData.Names[nameIndex]);
        deserializedProject.ID = ID;
        deserializedProject.CompleteBonus = completeBonus;
        deserializedProject.UsedTechnologies = technologies;

        return deserializedProject;
    }

    public Project(string projectName)
    {
        this.Name = projectName;

        Workers = new List<Worker>();
        UsedTechnologies = new List<ProjectTechnology>();
    }

    public void Start()
    {
        this.Active = true;
        this.Started?.Invoke(this);
    }

    public void Stop()
    {
        this.Active = false;
        this.Stopped?.Invoke(this);
    }

    public void AddWorker(Worker projectWorker)
    {
        projectWorker.AssignedProject = this;
        this.Workers.Add(projectWorker);
        WorkerAdded?.Invoke(projectWorker);
    }

    public void RemoveWorker(Worker projectWorker)
    {
        projectWorker.AssignedProject = null;
        this.Workers.Remove(projectWorker);
        WorkerRemoved.Invoke(projectWorker);
    }
}
