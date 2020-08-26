using ITCompanySimulation.Character;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ITCompanySimulation.Developing
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
        public event WorkerAction WorkerAdded;
        public event WorkerAction WorkerRemoved;

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

                    if (m_Progress >= 100.0f)
                    {
                        m_Progress = Mathf.Clamp(m_Progress, 0.0f, 100.0f);
                        Active = false;
                        Completed?.Invoke(this);
                    }

                    ProgressUpdated?.Invoke(this);
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
            this.CompleteBonus = proj.CompleteBonus;
            this.UsedTechnologies = proj.UsedTechnologies;
        }

        public void Start()
        {
            if (false == Active)
            {
                StartedOnce = true;
                this.Active = true;
                this.Started?.Invoke(this);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                string debugInfo = string.Format(
                    "Project (ID: {0}) started",
                    this.ID);
                Debug.Log(debugInfo);
#endif
            }
        }

        public void Stop()
        {
            if (true == Active)
            {
                this.Active = false;
                this.Stopped?.Invoke(this);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                string debugInfo = string.Format(
                    "Project (ID: {0}) stopped",
                    this.ID);
                Debug.Log(debugInfo);
#endif
            }
        }

        public void AddWorker(LocalWorker projectWorker)
        {
            projectWorker.AssignedProject = this;
            this.Workers.Add(projectWorker);
            WorkerAdded?.Invoke(projectWorker);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string debugInfo = string.Format(
                "Worker added to project\n" +
                "PROJECT -------------------\n" +
                "Name: {0}\n" +
                "ID: {1}\n" +
                "WORKER -------------------\n" +
                "Name: {2} {3}\n" +
                "ID: {4}",
                this.Name, this.ID, projectWorker.Name, projectWorker.Surename, projectWorker.ID);
            Debug.Log(debugInfo);
#endif
        }

        public void RemoveWorker(LocalWorker projectWorker)
        {
            projectWorker.AssignedProject = null;
            this.Workers.Remove(projectWorker);
            WorkerRemoved?.Invoke(projectWorker);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string debugInfo = string.Format(
                "Worker removed from project\n" +
                "PROJECT -------------------\n" +
                "Name: {0}\n" +
                "ID: {1}\n" +
                "WORKER -------------------\n" +
                "Name: {2} {3}\n" +
                "ID: {4}",
                this.Name, this.ID, projectWorker.Name, projectWorker.Surename, projectWorker.ID);
            Debug.Log(debugInfo);
#endif
        }
    }
}
