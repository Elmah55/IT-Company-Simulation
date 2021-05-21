namespace ITCompanySimulation.Settings
{
    /// <summary>
    /// This class defines settings of simulation
    /// </summary>
    public static class SimulationSettings
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        public const int MAX_TARGET_BALANCE = 1000000;
        public const int MIN_TARGET_BALANCE = MIN_INITIAL_BALANCE + 1;
        public const int MIN_MINIMAL_BALANCE = -1000000;
        public const int MAX_MINIMAL_BALANCE = MAX_TARGET_BALANCE;
        public const int MIN_INITIAL_BALANCE = 20000;

        /*Public fields*/

        /// <summary>
        /// How much money should player's company have to win the game
        /// </summary>
        public static int TargetBalance { get; set; } = MAX_TARGET_BALANCE;
        /// <summary>
        /// How much money each of players' company should have at beggining
        /// of game
        /// </summary>
        public static int InitialBalance { get; set; } = 80000;
        /// <summary>
        /// If the balance of player's company is lower than this then he loses
        /// </summary>
        public static int MinimalBalance { get; set; } = MIN_MINIMAL_BALANCE;

        /*Private methods*/

        /*Public methods*/
    }
}
