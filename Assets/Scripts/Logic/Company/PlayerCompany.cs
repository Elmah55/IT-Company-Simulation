using ITCompanySimulation.Character;
using System;
using System.Collections.Generic;
using UnityEngine;
using ITCompanySimulation.Developing;

/// <summary>
/// This class represents the IT company that player
/// can control
/// </summary>
public class PlayerCompany : Company
{
    /*Private consts fields*/

    /*Private fields*/

    /// <summary>
    /// Game object that is used as placeholder for
    /// scripts used in game
    /// </summary>
    private GameObject ScriptsGameObject;
    private int m_Balance;

    /*Public consts fields*/

    public const int MAX_WORKERS_PER_COMPANY = 10;

    /*Public fields*/

    /// <summary>
    /// Money balance of company
    /// </summary>
    public int Balance
    {
        get
        {
            return m_Balance;
        }

        set
        {
            m_Balance = value;
            BalanceChanged?.Invoke(m_Balance);
        }
    }
    public List<LocalWorker> Workers { get; private set; }
    /// <summary>
    /// List of scrum processes for this company. Every project
    /// has its own scrum instance
    /// </summary>
    public List<Scrum> ScrumProcesses { get; private set; }
    public List<LocalProject> CompletedProjects { get; private set; }
    public event LocalWorkerAction WorkerAdded;
    public event LocalWorkerAction WorkerRemoved;
    public event ScrumAtion ProjectAdded;
    public event Action<int> BalanceChanged;

    /*Private methods*/

    /*Public methods*/

    public PlayerCompany(string name, GameObject scriptsGameObject) : base(name)
    {
        Workers = new List<LocalWorker>();
        ScrumProcesses = new List<Scrum>();
        this.ScriptsGameObject = scriptsGameObject;
    }

    public void AddProject(LocalProject projectToAdd)
    {
        //Scrum instance for project that will be added
        Scrum projectScrum = (Scrum)ScriptsGameObject.AddComponent(typeof(Scrum));
        projectScrum.BindedProject = projectToAdd;
        ScrumProcesses.Add(projectScrum);
        ProjectAdded?.Invoke(projectScrum);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        string debugInfo = string.Format("Project added to company\nName {0}\nID {1}\nComplete bonus {2}",
            projectToAdd.Name, projectToAdd.ID, projectToAdd.CompletionBonus);
        Debug.Log(debugInfo);
#endif
    }

    public void AddWorker(LocalWorker workerToAdd)
    {
        Workers.Add(workerToAdd);
        workerToAdd.WorkingCompany = this;
        workerToAdd.DaysInCompany = 0;
        WorkerAdded?.Invoke(workerToAdd);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        string debugInfo = string.Format("Worker added to company\nName {0} {1}\nID {2}\n",
            workerToAdd.Name, workerToAdd.Surename, workerToAdd.ID);
        Debug.Log(debugInfo);
#endif
    }

    public void RemoveWorker(LocalWorker workerToRemove)
    {
        Workers.Remove(workerToRemove);
        workerToRemove.WorkingCompany = null;
        WorkerRemoved?.Invoke(workerToRemove);


#if DEVELOPMENT_BUILD || UNITY_EDITOR
        string debugInfo = string.Format("Worker removed from company\nName {0} {1}\nID {2}\n",
            workerToRemove.Name, workerToRemove.Surename, workerToRemove.ID);
        Debug.Log(debugInfo);
#endif
    }
}
