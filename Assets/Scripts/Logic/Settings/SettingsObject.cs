using UnityEngine;

namespace ITCompanySimulation.Settings
{
    /// <summary>
    /// Object storing settings that cannot be set in UI.
    /// Used to create object that allows setting values in inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "GeneralSettings", menuName = "ITCompanySimulation/Settings/GeneralSettings")]
    public class SettingsObject : ScriptableObject
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        //Below fields are used to set value of coresponding
        //fields in ApplicationManager.

        public bool UseRoom;
        public bool OfflineMode;
        [Tooltip("Balance of company needed to win a simulation")]
        [Range(SimulationSettings.MIN_TARGET_BALANCE,SimulationSettings.MAX_TARGET_BALANCE)]
        public int TargetBalance;
        [Tooltip("Company will start simulation with this balance")]
        [Range(SimulationSettings.MIN_INITIAL_BALANCE, SimulationSettings.MAX_INITIAL_BALANCE)]
        public int InitialBalance;
        [Tooltip("After player's company reaches this balance player loses simulation")]
        [Range(SimulationSettings.MIN_MINIMAL_BALANCE, SimulationSettings.MAX_MINIMAL_BALANCE)]
        public int MinimalBalance;

        /*Private methods*/

        /*Public methods*/
    }
}
