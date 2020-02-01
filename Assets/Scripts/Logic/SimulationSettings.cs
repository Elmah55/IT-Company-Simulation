using UnityEngine;

/// <summary>
/// This class defines settings of rules of simulation
/// </summary>
public class SimulationSettings : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    public const int MAX_TARGET_BALANCE = 10000000;
    public const int MIN_TARGET_BALANCE = 1;
    public const int MAX_INITIAL_BALANCE = MAX_TARGET_BALANCE - 1;
    public const int MIN_INITIAL_BALANCE = MIN_TARGET_BALANCE - 1;

    /*Public fields*/

    /// <summary>
    /// How much money should player's company have to win the game
    /// </summary>
    public int TargetBalance { get; set; }
    /// <summary>
    /// How much money each of players' company should have at beggining
    /// of game
    /// </summary>
    public int InitialBalance { get; set; }

    /*Private methods*/

    /*Public methods*/
}
