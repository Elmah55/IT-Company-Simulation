using ITCompanySimulation.Character;
using System;
using System.Collections.Generic;
using UnityEngine;
using ITCompanySimulation.Developing;
using ITCompanySimulation.Utilities;

namespace ITCompanySimulation.Company
{
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
        private SafeInt m_Balance;

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
                return m_Balance.Value;
            }

            set
            {
                int balanceDelta = value - m_Balance.Value;
                m_Balance.Value = value;
                BalanceChanged?.Invoke(m_Balance.Value, balanceDelta);
            }
        }

        /// <summary>
        /// Checks if company can hire worker
        /// </summary>
        public bool CanHireWorker
        {
            get
            {
                bool result = this.Workers.Count < MAX_WORKERS_PER_COMPANY;
                return result;
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
        public event BalanceChangeAction BalanceChanged;

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
            string debugInfo = string.Format("[{3}] Project added to company\nName {0}\nID {1}\nComplete bonus {2}",
                projectToAdd.Name, projectToAdd.ID, projectToAdd.CompletionBonus, this.GetType().Name);
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
            string debugInfo = string.Format("[{3}] Worker added to company\nName {0} {1}\nID {2}\n",
                workerToAdd.Name, workerToAdd.Surename, workerToAdd.ID, this.GetType().Name);
            Debug.Log(debugInfo);
#endif
        }

        public void RemoveWorker(LocalWorker workerToRemove)
        {
            Workers.Remove(workerToRemove);
            workerToRemove.WorkingCompany = null;

            if (null != workerToRemove.AssignedProject)
            {
                workerToRemove.AssignedProject.RemoveWorker(workerToRemove);
            }

            WorkerRemoved?.Invoke(workerToRemove);


#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string debugInfo = string.Format("[{3}] Worker removed from company\nName {0} {1}\nID {2}\n",
                workerToRemove.Name, workerToRemove.Surename, workerToRemove.ID, this.GetType().Name);
            Debug.Log(debugInfo);
#endif
        }
    }
}
