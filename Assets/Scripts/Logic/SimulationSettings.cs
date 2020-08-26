using System;
/// <summary>
/// This class defines settings of simulation
/// </summary>
public class SimulationSettings
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    public const int MAX_TARGET_BALANCE = 1000000;
    public const int MIN_TARGET_BALANCE = MIN_INITIAL_BALANCE;
    public const int MIN_MINIMAL_BALANCE = -1000000;
    public const int MAX_MINIMAL_BALANCE = MAX_TARGET_BALANCE;
    public const int MIN_INITIAL_BALANCE = 20000;

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
    /// <summary>
    /// If the balance of player's company is lower than this then he loses
    /// </summary>
    public int MinimalBalance { get; set; }

    /*Private methods*/

    /*Public methods*/

    public SimulationSettings()
    {
        //Set default values
        TargetBalance = MAX_TARGET_BALANCE;
        InitialBalance = 80000;
        MinimalBalance = MIN_MINIMAL_BALANCE;
    }

    public SimulationSettings(int initialBalance, int targetBalance, int minBalance)
    {
        this.InitialBalance = initialBalance;
        this.TargetBalance = targetBalance;
        this.MinimalBalance = minBalance;
    }

    public static byte[] Serialize(object simulationSettings)
    {
        SimulationSettings settings = (SimulationSettings)simulationSettings;
        byte[] minimalBalanceArr = BitConverter.GetBytes(settings.MinimalBalance);
        byte[] targetBalanceArr = BitConverter.GetBytes(settings.TargetBalance);
        byte[] initialBalanceArr = BitConverter.GetBytes(settings.InitialBalance);

        int resultArrayOffset = 0;
        byte[] resultArray = 
            new byte[minimalBalanceArr.Length + targetBalanceArr.Length + initialBalanceArr.Length];

        Array.Copy(minimalBalanceArr, 0, resultArray, resultArrayOffset, minimalBalanceArr.Length);
        resultArrayOffset += minimalBalanceArr.Length;
        Array.Copy(targetBalanceArr, 0, resultArray, resultArrayOffset, targetBalanceArr.Length);
        resultArrayOffset += targetBalanceArr.Length;
        Array.Copy(initialBalanceArr, 0, resultArray, resultArrayOffset, initialBalanceArr.Length);

        return resultArray;
    }

    public static object Deserialize(byte[] byteArray)
    {
        SimulationSettings settings = new SimulationSettings();
        int byteArrayOffset = 0;
        settings.MinimalBalance = BitConverter.ToInt32(byteArray, byteArrayOffset);
        byteArrayOffset += sizeof(int);
        settings.TargetBalance = BitConverter.ToInt32(byteArray, byteArrayOffset);
        byteArrayOffset += sizeof(int);
        settings.InitialBalance = BitConverter.ToInt32(byteArray, byteArrayOffset);

        return settings;
    }
}
