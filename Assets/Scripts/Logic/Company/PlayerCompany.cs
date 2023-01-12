using ITCompanySimulation.Character;
using System.Collections.Generic;
using UnityEngine;
using ITCompanySimulation.Project;
using ITCompanySimulation.Utilities;
using ITCompanySimulation.Core;

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

        private SafeInt m_Balance;
        private SimulationManager SimulationManagerComponent;

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
                m_Balance = new SafeInt(value);
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
        public List<LocalWorker> Workers { get; private set; } = new List<LocalWorker>();
        /// <summary>
        /// List of scrum processes for this company. Every project
        /// has its own scrum instance
        /// </summary>
        public List<Scrum> ScrumProcesses { get; private set; } = new List<Scrum>();
        public event LocalWorkerAction WorkerAdded;
        public event LocalWorkerAction WorkerRemoved;
        public event ScrumAtion ProjectAdded;
        public event ScrumAtion ProjectRemoved;
        public event BalanceChangeAction BalanceChanged;

        /*Private methods*/

        private void OnProjectAdded(Scrum projectScrum)
        {
            SimulationManagerComponent.NotificatorComponent.Notify("Project added to company");
            projectScrum.BindedProject.Completed += OnProjectCompleted;
        }

        private void OnProjectCompleted(LocalProject proj)
        {
            RemoveProject(proj);
        }

        /*Public methods*/

        public PlayerCompany(string name) : base(name)
        {
            SimulationManagerComponent = SimulationManager.Instance;
            ProjectAdded += OnProjectAdded;
        }

        public void AddProject(LocalProject projectToAdd)
        {
            Scrum newScrum = new Scrum(projectToAdd);
            ScrumProcesses.Add(newScrum);
            ProjectAdded?.Invoke(newScrum);

            string debugInfo = string.Format("Project added to company\nName: {0}\nID {1}\nComplete bonus: {2}",
                projectToAdd.Name, projectToAdd.ID, projectToAdd.CompletionBonus);
            RestrictedDebug.Log(debugInfo);
        }

        public void RemoveProject(LocalProject projectToRemove)
        {
            for (int i = 0; i < ScrumProcesses.Count; i++)
            {
                if (ScrumProcesses[i].BindedProject == projectToRemove)
                {
                    Scrum projectScrum = ScrumProcesses[i];
                    ScrumProcesses.RemoveAt(i);
                    ProjectRemoved?.Invoke(projectScrum);
                    break;
                }
            }

            string debugInfo = string.Format("Project removed from company\nName: {0}\nID {1}",
                projectToRemove.Name, projectToRemove.ID);
            RestrictedDebug.Log(debugInfo);
        }

        public void AddWorker(LocalWorker workerToAdd)
        {
            Workers.Add(workerToAdd);
            workerToAdd.WorkingCompany = this;
            workerToAdd.DaysInCompany = 0;
            WorkerAdded?.Invoke(workerToAdd);

            string debugInfo = string.Format("Worker added to company\nName: {0} {1}\nID: {2}\n",
                workerToAdd.Name, workerToAdd.Surename, workerToAdd.ID);
            RestrictedDebug.Log(debugInfo);
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

            string debugInfo = string.Format("Worker removed from company\nName {0} {1}\nID {2}\n",
                workerToRemove.Name, workerToRemove.Surename, workerToRemove.ID);
            RestrictedDebug.Log(debugInfo);
        }
    }
}
