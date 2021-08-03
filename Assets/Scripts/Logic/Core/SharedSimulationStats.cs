using System;
using UnityEngine.Events;

namespace ITCompanySimulation.Core
{
    /// <summary>
    /// This class is used for collecting statistics related to simulation.
    /// Represents statistic of other player in simulation.
    /// </summary>
    public class SharedSimulationStats
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
        /// Invoked when any of the stats is altered.
        /// </summary>
        public event UnityAction StatsUpdated;

        /*Private methods*/

        protected void OnStatsUpdated()
        {
            this.StatsUpdated?.Invoke();
        }

        /*Public methods*/

        public SharedSimulationStats() { }

        public SharedSimulationStats(LocalSimulationStats stats)
        {
            this.MoneyEarned = stats.MoneyEarned;
            this.MoneySpent = stats.MoneySpent;
            this.WorkersHired = stats.WorkersHired;
            this.OtherPlayersWorkersHired = stats.OtherPlayersWorkersHired;
            this.WorkersLeftCompany = stats.WorkersLeftCompany;
            this.ProjectsCompleted = stats.ProjectsCompleted;
            this.MoneyEarnedSet = stats.MoneyEarnedSet;
            this.CompanyBalance = stats.CompanyBalance;
        }

        public static byte[] Serialize(object statsObject)
        {
            SharedSimulationStats stats = (SharedSimulationStats)statsObject;

            byte[] moneyEarnedBytes = BitConverter.GetBytes(stats.MoneyEarned);
            byte[] moneySpentBytes = BitConverter.GetBytes(stats.MoneySpent);
            byte[] workersHiredBytes = BitConverter.GetBytes(stats.WorkersHired);
            byte[] otherPlayersWorkersHiredBytes = BitConverter.GetBytes(stats.OtherPlayersWorkersHired);
            byte[] workersLeftCompanyBytes = BitConverter.GetBytes(stats.WorkersLeftCompany);
            byte[] projectsCompletedBytes = BitConverter.GetBytes(stats.ProjectsCompleted);
            byte[] companyBalanceBytes = BitConverter.GetBytes(stats.CompanyBalance);

            int serializedSize =
                moneyEarnedBytes.Length +
                moneySpentBytes.Length +
                workersHiredBytes.Length +
                otherPlayersWorkersHiredBytes.Length +
                workersLeftCompanyBytes.Length +
                projectsCompletedBytes.Length +
                companyBalanceBytes.Length;

            byte[] serializedStats = new byte[serializedSize];
            int offset = 0;

            Array.Copy(moneyEarnedBytes, 0, serializedStats, offset, moneyEarnedBytes.Length);
            offset += moneyEarnedBytes.Length;
            Array.Copy(moneySpentBytes, 0, serializedStats, offset, moneySpentBytes.Length);
            offset += moneySpentBytes.Length;
            Array.Copy(workersHiredBytes, 0, serializedStats, offset, workersHiredBytes.Length);
            offset += workersHiredBytes.Length;
            Array.Copy(otherPlayersWorkersHiredBytes, 0, serializedStats, offset, otherPlayersWorkersHiredBytes.Length);
            offset += otherPlayersWorkersHiredBytes.Length;
            Array.Copy(workersLeftCompanyBytes, 0, serializedStats, offset, workersLeftCompanyBytes.Length);
            offset += workersLeftCompanyBytes.Length;
            Array.Copy(projectsCompletedBytes, 0, serializedStats, offset, projectsCompletedBytes.Length);
            offset += projectsCompletedBytes.Length;
            Array.Copy(companyBalanceBytes, 0, serializedStats, offset, companyBalanceBytes.Length);

            return serializedStats;
        }

        public static object Deserialize(byte[] statsBytes)
        {
            int offset = 0;
            int moneyEarned = BitConverter.ToInt32(statsBytes, offset);
            offset += sizeof(int);
            int moneySpent = BitConverter.ToInt32(statsBytes, offset);
            offset += sizeof(int);
            int workersHired = BitConverter.ToInt32(statsBytes, offset);
            offset += sizeof(int);
            int otherPlayerWorkersHired = BitConverter.ToInt32(statsBytes, offset);
            offset += sizeof(int);
            int workersLeftCompany = BitConverter.ToInt32(statsBytes, offset);
            offset += sizeof(int);
            int projectsCompleted = BitConverter.ToInt32(statsBytes, offset);
            offset += sizeof(int);
            int companyBalance = BitConverter.ToInt32(statsBytes, offset);

            SharedSimulationStats deserializedStats = new SharedSimulationStats();
            deserializedStats.MoneyEarned = moneyEarned;
            deserializedStats.MoneySpent = moneySpent;
            deserializedStats.WorkersHired = workersHired;
            deserializedStats.OtherPlayersWorkersHired = otherPlayerWorkersHired;
            deserializedStats.WorkersLeftCompany = workersLeftCompany;
            deserializedStats.ProjectsCompleted = projectsCompleted;
            deserializedStats.CompanyBalance = companyBalance;

            return deserializedStats;
        }
    }
}
