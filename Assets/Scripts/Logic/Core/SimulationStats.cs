using System;

namespace ITCompanySimulation.Core
{
    /// <summary>
    /// This class is used for collecting statistics related to simulation
    /// </summary>
    public class SimulationStats
    {
        /*Private consts fields*/

        /*Private fields*/

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
        public int DaysSinceStart { get; set; }
        /// <summary>
        /// How much money has player's company earned
        /// TODO: Fix counting this. First time initial
        /// company balance is set it is counted as money earned
        /// </summary>
        public int MoneyEarned { get; set; }
        /// <summary>
        /// How much money has player's company spent
        /// </summary>
        public int MoneySpent { get; set; }
        /// <summary>
        /// Total number of workers hired
        /// </summary>
        public int WorkersHired { get; set; }
        /// <summary>
        /// Number of workers hired from other players
        /// </summary>
        public int OtherPlayersWorkersHired { get; set; }
        /// <summary>
        /// Total number of workers that left company
        /// </summary>
        public int WorkersLeftCompany { get; set; }
        /// <summary>
        /// Total number of completed projects by player's company
        /// </summary>
        public int ProjectsCompleted { get; set; }

        /*Private methods*/

        /*Public methods*/
    }
}
