using ITCompanySimulation.Character;
using ITCompanySimulation.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ITCompanySimulation.Project
{
    /// <summary>
    /// This class represents IT project that company can work on
    /// </summary>
    public class LocalProject : SharedProject
    {
        /*Private consts fields*/

        /*Private fields*/

        private float m_Progress;
        private int m_DaysSinceStart;

        /*Public consts fields*/

        public const int MAX_WORKERS_PER_PROJECT = 5;

        /*Public fields*/

        public event LocalProjectAction DaysSinceStartUpdated;
        public event LocalProjectAction Completed;
        public event LocalProjectAction ProgressUpdated;
        public event LocalProjectAction Started;
        public event LocalProjectAction Stopped;
        public event SharedWorkerAction WorkerAdded;
        public event SharedWorkerAction WorkerRemoved;

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
                        IsActive = false;
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
        public bool IsActive { get; set; }
        /// <summary>
        /// True if project was started at least once (started for the first time)
        /// </summary>
        public bool StartedOnce { get; set; }
        /// <summary>
        /// Workers that are working on this project
        /// </summary>
        public List<LocalWorker> Workers { get; private set; }
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

        public LocalProject(string projectName) : base(projectName)
        {
            Workers = new List<LocalWorker>();
        }

        public LocalProject(SharedProject proj) : this(proj.Name)
        {
            this.ID = proj.ID;
            this.CompletionBonus = proj.CompletionBonus;
            this.UsedTechnologies = proj.UsedTechnologies;
            this.CompletionTime = proj.CompletionTime;
            this.NameIndex = proj.NameIndex;
            this.IconIndex = proj.IconIndex;
            this.Icon = proj.Icon;
        }

        public void Start()
        {
            if (false == IsActive)
            {
                StartedOnce = true;
                this.IsActive = true;
                this.Started?.Invoke(this);

                string debugInfo = string.Format(
                    "Project (ID: {0}) started",
                    this.ID);
                RestrictedDebug.Log(debugInfo);
            }
        }

        public void Stop()
        {
            if (true == IsActive)
            {
                this.IsActive = false;
                this.Stopped?.Invoke(this);

                string debugInfo = string.Format(
                    "Project (ID: {0}) stopped", this.ID);
                RestrictedDebug.Log(debugInfo);
            }
        }

        public void AddWorker(LocalWorker projectWorker)
        {
            projectWorker.AssignedProject = this;
            this.Workers.Add(projectWorker);
            WorkerAdded?.Invoke(projectWorker);

            string debugInfo = string.Format(
                "Worker added to project\n" +
                "PROJECT -------------------\n" +
                "Name: {0}\n" +
                "ID: {1}\n" +
                "WORKER -------------------\n" +
                "Name: {2} {3}\n" +
                "ID: {4}",
                this.Name, this.ID, projectWorker.Name, projectWorker.Surename, projectWorker.ID);
            RestrictedDebug.Log(debugInfo);
        }

        public void RemoveWorker(LocalWorker projectWorker)
        {
            projectWorker.AssignedProject = null;
            this.Workers.Remove(projectWorker);
            WorkerRemoved?.Invoke(projectWorker);

            string debugInfo = string.Format(
                "Worker removed from project\n" +
                "PROJECT -------------------\n" +
                "Name: {0}\n" +
                "ID: {1}\n" +
                "WORKER -------------------\n" +
                "Name: {2} {3}\n" +
                "ID: {4}",
                this.Name, this.ID, projectWorker.Name, projectWorker.Surename, projectWorker.ID);
            RestrictedDebug.Log(debugInfo);
        }
    }
}
