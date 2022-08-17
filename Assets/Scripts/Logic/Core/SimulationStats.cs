using System;
using UnityEngine.Events;

namespace ITCompanySimulation.Core
{
    /// <summary>
    /// This class is used for collecting statistics related to simulation.
    /// Represents statistic of other player in simulation.
    /// </summary>
    public class SimulationStats
    {
        /*Private consts fields*/

        /*Private fields*/

        /// <summary>
        /// Flag to not count earned money when it is set
        /// for the first time (when initial company's balance
        /// is set).
        /// </summary>
        private bool MoneyEarnedSet = false;
        private int m_MoneyEarned = 0;
        private int m_MoneySpent = 0;
        private int m_WorkersHired = 0;
        private int m_OtherPlayersWorkersHired = 0;
        private int m_WorkersLeftCompany = 0;
        private int m_ProjectsCompleted = 0;
        private int m_CompanyBalance = 0;
        private int m_DaysSinceStart = 0;

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Amount of money that player's company has earned.
        /// </summary>
        public int MoneyEarned
        {
            get
            {
                return m_MoneyEarned;
            }

            set
            {
                if (true == MoneyEarnedSet)
                {
                    m_MoneyEarned = value;
                    OnStatsUpdated();
                }

                if (false == MoneyEarnedSet)
                {
                    MoneyEarnedSet = true;
                }
            }
        }
        /// <summary>
        /// How much money has player's company spent.
        /// </summary>
        public int MoneySpent
        {
            get
            {
                return m_MoneySpent;
            }

            set
            {
                m_MoneySpent = value;
                OnStatsUpdated();
            }
        }
        /// <summary>
        /// Total number of workers hired.
        /// </summary>
        public int WorkersHired
        {
            get
            {
                return m_WorkersHired;
            }

            set
            {
                m_WorkersHired = value;
                OnStatsUpdated();
            }
        }
        /// <summary>
        /// Number of workers hired from other players.
        /// </summary>
        public int OtherPlayersWorkersHired
        {
            get
            {
                return m_OtherPlayersWorkersHired;
            }

            set
            {
                m_OtherPlayersWorkersHired = value;
                OnStatsUpdated();
            }
        }
        /// <summary>
        /// Total number of workers that left company.
        /// </summary>
        public int WorkersLeftCompany
        {
            get
            {
                return m_WorkersLeftCompany;
            }

            set
            {
                m_WorkersLeftCompany = value;
                OnStatsUpdated();
            }
        }
        /// <summary>
        /// Total number of completed projects by player's company.
        /// </summary>
        public int ProjectsCompleted
        {
            get
            {
                return m_ProjectsCompleted;
            }

            set
            {
                m_ProjectsCompleted = value;
                OnStatsUpdated();
            }
        }
        /// <summary>
        /// Amount of money that company has.
        /// </summary>
        public int CompanyBalance
        {
            get
            {
                return m_CompanyBalance;
            }

            set
            {
                m_CompanyBalance = value;
                OnStatsUpdated();
            }
        }
        /// <summary>
        /// Stores the running time of simulation (from simulation start to simulation finish).
        /// This is real world time. This value is not related to session running time. Value
        /// of this field is not valid until end of simulation.
        /// </summary>
        public TimeSpan SimulationRunningTime { get; set; }
        /// <summary>
        /// How many in-simulation days have passed since start of simulation. Value of this field is
        /// not vaild until end of simulation.
        /// </summary>
        public int DaysSinceStart
        {
            get
            {
                return m_DaysSinceStart;
            }

            set
            {
                m_DaysSinceStart = value;
                OnStatsUpdated();
            }
        }
        /// <summary>
        /// Invoked when any of the stats is altered.
        /// </summary>
        public event UnityAction StatsUpdated;

        /*Private methods*/

        protected void OnStatsUpdated()
        {
            this.StatsUpdated?.Invoke();
        }

        /*Public methods*/
    }
}
