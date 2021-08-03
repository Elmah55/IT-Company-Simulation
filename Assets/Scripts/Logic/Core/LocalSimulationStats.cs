using System;

namespace ITCompanySimulation.Core
{
    /// <summary>
    /// This class is used for collecting statistics related to simulation.
    /// Represents statistic of local player.
    /// </summary>
    public class LocalSimulationStats : SharedSimulationStats
    {
        /*Private consts fields*/

        /*Private fields*/

        private int m_DaysSinceStart = 0;

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Stores the running time of simulation (from simulation start to simulation finish).
        /// This is real world time. This value is not related to session running time
        /// </summary>
        public TimeSpan SimulationRunningTime { get; set; }
        /// <summary>
        /// How many in-simulation days have passed since start of simulation
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

        /*Private methods*/

        /*Public methods*/
    } 
}
