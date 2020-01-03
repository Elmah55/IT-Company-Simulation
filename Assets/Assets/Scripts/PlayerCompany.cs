using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the IT company that player
/// can control
/// </summary>
public class PlayerCompany : Company
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    public const int MAX_WORKERS_PER_COMPANY = 10;

    /*Public fields*/

    public List<Worker> Workers { get; private set; }
    /// <summary>
    /// List of scrum processes for this company. Every project
    /// has its own scrum instance
    /// </summary>
    public List<Scrum> ScrumProcesses { get; private set; }
    public List<Project> CompletedProjects { get; private set; }
    public event WorkerAction WorkerAdded;
    public event WorkerAction WorkerRemoved;

    /*Private methods*/

    /*Public methods*/

    public PlayerCompany(string name) : base(name)
    {
        Workers = new List<Worker>();
        ScrumProcesses = new List<Scrum>();
    }

    public void AddProject(Project projectInstance)
    {
        //Scrum instance for project that will be added
        Scrum projectScrum = new Scrum();
        projectScrum.BindedProject = projectInstance;
        ScrumProcesses.Add(projectScrum);
    }

    public void AddWorker(Worker workerToAdd)
    {
        Workers.Add(workerToAdd);
        WorkerAdded?.Invoke(workerToAdd);
    }

    public void RemoveWorker(Worker workerToRemove)
    {
        Workers.Remove(workerToRemove);
        WorkerRemoved?.Invoke(workerToRemove);
    }
}
