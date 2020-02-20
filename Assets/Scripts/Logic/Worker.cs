using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

public class Worker : ISharedObject
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    /// <summary>
    /// How many days of holidays per year worker can use in one year
    /// </summary>
    public const int DAYS_OF_HOLIDAYS_PER_YEAR = 25;
    public const int BASE_SALARY = 800;
    /// <summary>
    /// The maximum value of one ability that worker
    /// can have
    /// </summary>
    public const float MAX_ABILITY_VALUE = 10.0f;

    /*Public fields*/

    public string Name { get; private set; }
    public string Surename { get; private set; }
    /// <summary>
    /// Company that this worker is working in
    /// </summary>
    public PlayerCompany WorkingCompany { get; set; }
    /// <summary>
    /// Combination of all player's attributes. It's player's overall score
    /// and indicates how effective worker can work
    /// </summary>
    public float Score
    {
        get
        {
            float score = 1.0f;

            if (null != AssignedProject)
            {
                foreach (ProjectTechnology workerAbility in Abilites.Keys)
                {
                    if (AssignedProject.UsedTechnologies.Contains(workerAbility))
                    {
                        score += Abilites[workerAbility] * 0.1f;
                    }
                }
            }

            score += ExperienceTime;
            score *= 0.005f;

            return score;
        }
    }
    /// <summary>
    /// List of abilites that worker has. Each ability can be on different level
    /// depending on worker experience. Ability level is indicated by value assigned
    /// to each of abilities key
    /// </summary>
    public Dictionary<ProjectTechnology, float> Abilites { get; set; }
    public Project AssignedProject { get; set; }
    /// <summary>
    /// Shows how much of time of experience working in project worker has.
    /// This values is days in game time
    /// </summary>
    public int ExperienceTime { get; set; }
    public int Salary { get; set; }
    /// <summary>
    /// Salary that needs to be offered to worker to hire him
    /// from other player's company
    /// </summary>
    public int HireSalary
    {
        get
        {
            return (int)(Salary * 1.25f);
        }
    }
    /// <summary>
    /// Unique ID of worker
    /// </summary>
    public int ID { get; set; }
    /// <summary>
    /// For how many days worker is employed in current company
    /// </summary>
    public int DaysInCompany { get; set; }
    /// <summary>
    /// Indicates whether worker can contribute in project. If this is set to false
    /// worker won't be considered when calculating project progress. This can be
    /// used to simulate events like sickness or holiday leave of worker
    /// </summary>
    public bool Available { get; set; } = true;
    /// <summary>
    /// How many days have passed since worker was not available
    /// </summary>
    public int DaysSinceAbsent { get; set; }
    /// <summary>
    /// Defines absence type of worker
    /// </summary>
    public WorkerAbsenceReason AbsenceReason { get; set; }
    /// <summary>
    /// How many days in game time will pass until worker will be available again
    /// </summary>
    public int DaysUntilAvailable { get; set; }
    /// <summary>
    /// How many days of holidays worker can yet use. This value should be reset
    /// every new year in game time
    /// </summary>
    public int DaysOfHolidaysLeft { get; set; } = DAYS_OF_HOLIDAYS_PER_YEAR;
    public event WorkerAction AbsenceStarted;

    /*Private methods*/

    /*Public methods*/

    public void StartAbsence()
    {
        this.AbsenceStarted.Invoke(this);
    }

    public static byte[] Serialize(object workerObject)
    {
        Worker workerToSerialize = (Worker)workerObject;

        byte[] nameBytes = Encoding.Unicode.GetBytes(workerToSerialize.Name);
        byte[] surenameBytes = Encoding.Unicode.GetBytes(workerToSerialize.Surename);
        byte[] expierienceTimeBytes = BitConverter.GetBytes(workerToSerialize.ExperienceTime);
        byte[] salaryBytes = BitConverter.GetBytes(workerToSerialize.Salary);
        byte[] abilitiesBytes;
        byte[] IDBytes = BitConverter.GetBytes(workerToSerialize.ID);

        //This is used so receiving client knows how much bytes of name and surename should be read
        byte[] nameSize = BitConverter.GetBytes(nameBytes.Length);
        byte[] surenameSize = BitConverter.GetBytes(surenameBytes.Length);
        byte[] abilitiesSize;

        using (MemoryStream memStream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(memStream, workerToSerialize.Abilites);
            abilitiesBytes = memStream.ToArray();
        }

        abilitiesSize = BitConverter.GetBytes(abilitiesBytes.Length);

        int workerBytesSize = nameBytes.Length
                            + surenameBytes.Length
                            + nameSize.Length
                            + surenameSize.Length
                            + abilitiesSize.Length
                            + abilitiesBytes.Length
                            + expierienceTimeBytes.Length
                            + salaryBytes.Length
                            + IDBytes.Length;

        byte[] workerBytes = new byte[workerBytesSize];
        int offset = 0;

        Array.Copy(nameSize, 0, workerBytes, offset, nameSize.Length);
        offset += nameSize.Length;
        Array.Copy(nameBytes, 0, workerBytes, offset, nameBytes.Length);
        offset += nameBytes.Length;
        Array.Copy(surenameSize, 0, workerBytes, offset, surenameSize.Length);
        offset += surenameSize.Length;
        Array.Copy(surenameBytes, 0, workerBytes, offset, surenameBytes.Length);
        offset += surenameBytes.Length;
        Array.Copy(abilitiesSize, 0, workerBytes, offset, abilitiesSize.Length);
        offset += abilitiesSize.Length;
        Array.Copy(abilitiesBytes, 0, workerBytes, offset, abilitiesBytes.Length);
        offset += abilitiesBytes.Length;
        Array.Copy(expierienceTimeBytes, 0, workerBytes, offset, expierienceTimeBytes.Length);
        offset += expierienceTimeBytes.Length;
        Array.Copy(salaryBytes, 0, workerBytes, offset, expierienceTimeBytes.Length);
        offset += salaryBytes.Length;
        Array.Copy(IDBytes, 0, workerBytes, offset, IDBytes.Length);

        return workerBytes;
    }

    public static object Deserialize(byte[] workerBytes)
    {
        int offset = 0;

        int nameSize = BitConverter.ToInt32(workerBytes, offset);
        offset += sizeof(int);
        string name = Encoding.Unicode.GetString(workerBytes, offset, nameSize);
        offset += nameSize;
        int surenameSize = BitConverter.ToInt32(workerBytes, offset);
        offset += sizeof(int);
        string surename = Encoding.Unicode.GetString(workerBytes, offset, surenameSize);
        offset += surenameSize;

        Worker deserializedWorker = new Worker(name, surename);

        int abilitiesSize = BitConverter.ToInt32(workerBytes, offset);
        offset += sizeof(int);

        Dictionary<ProjectTechnology, float> abilities;

        using (MemoryStream memStream = new MemoryStream(workerBytes, offset, abilitiesSize))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            abilities = (Dictionary<ProjectTechnology, float>)formatter.Deserialize(memStream);
        }

        offset += abilitiesSize;
        int expierienceTime = BitConverter.ToInt32(workerBytes, offset);
        offset += sizeof(int);
        int salary = BitConverter.ToInt32(workerBytes, offset);
        offset += sizeof(int);
        int workerID = BitConverter.ToInt32(workerBytes, offset);

        deserializedWorker.Abilites = abilities;
        deserializedWorker.ExperienceTime = expierienceTime;
        deserializedWorker.Salary = salary;
        deserializedWorker.ID = workerID;

        return deserializedWorker;
    }

    public Worker(string name, string surename)
    {
        this.Name = name;
        this.Surename = surename;
        this.Salary = BASE_SALARY;
    }
}
