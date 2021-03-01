using ITCompanySimulation.Developing;

namespace ITCompanySimulation.Character
{
    /// <summary>
    /// Represents worker in local simulation world
    /// </summary>
    public class LocalWorker : SharedWorker
    {
        /*Private consts fields*/

        /*Private fields*/

        private int m_DaysInCompany;
        private float m_Satisfaction = DEFAULT_SATISFACTION_LEVEL;
        private bool m_Available = true;
        private int m_DaysOfHolidaysLeft = DAYS_OF_HOLIDAYS_PER_YEAR;

        /*Public consts fields*/

        public const int MIN_SICKNESS_DURATION = 1;
        public const int MAX_SICKNESS_DURATION = 25;
        public const int MIN_HOLIDAY_DURATION = 1;
        public const int MAX_HOLIDAY_DURATION = DAYS_OF_HOLIDAYS_PER_YEAR;
        public const float DEFAULT_SATISFACTION_LEVEL = 50.0f;
        /// <summary>
        /// How many days of holidays per year worker can use in one year
        /// </summary>
        public const int DAYS_OF_HOLIDAYS_PER_YEAR = 25;

        /*Public fields*/

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
                            score += Abilites[workerAbility].Value * 0.5f;
                        }
                    }
                }

                score += ExperienceTime;
                score *= 0.005f;

                return score;
            }
        }
        public LocalProject AssignedProject { get; set; }
        /// <summary>
        /// Amount of salary increase or decrease last time it was changed
        /// </summary>
        public int LastSalaryChange { get; set; }
        /// <summary>
        /// For how many days worker is employed in current company
        /// </summary>
        public int DaysInCompany
        {
            get
            {
                return m_DaysInCompany;
            }

            set
            {
                if (m_DaysInCompany != value)
                {
                    m_DaysInCompany = value;
                    DaysInCompanyChanged?.Invoke(this);
                }
            }
        }
        /// <summary>
        /// Indicates whether worker can contribute in project. If this is set to false
        /// worker won't be considered when calculating project progress. This can be
        /// used to simulate events like sickness or holiday leave of worker. Changing its
        /// value will invoke AbsenceStarted or AbsenceFinished
        /// </summary>
        public bool Available
        {
            get
            {
                return m_Available;
            }

            set
            {
                if (m_Available != value)
                {
                    m_Available = value;

                    if (true == m_Available)
                    {
                        AbsenceFinished?.Invoke(this);
                    }
                    else
                    {
                        AbsenceStarted?.Invoke(this);
                    }
                }
            }
        }
        /// <summary>
        /// How many days have passed since worker was not available
        /// </summary>
        public int DaysSinceAbsent { get; set; }
        /// <summary>
        /// Indicates level of satisfaction of worker in %.
        /// Level of satisfaction describers how good worker feels
        /// while working in company. If level falls below some value
        /// he will leave company
        /// </summary>
        public float Satiscation
        {
            get
            {
                return m_Satisfaction;
            }

            set
            {
                if (value != m_Satisfaction)
                {
                    m_Satisfaction = value;
                    SatisfactionChanged?.Invoke(this);
                }
            }
        }
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
        public int DaysOfHolidaysLeft
        {
            get
            {
                return m_DaysOfHolidaysLeft;
            }

            set
            {
                if (value != m_DaysOfHolidaysLeft)
                {
                    m_DaysOfHolidaysLeft = value;
                    DaysOfHolidaysLeftChanged?.Invoke(this);
                }
            }
        }
        public override int Salary
        {
            get
            {
                return base.Salary;
            }

            set
            {
                if (value > base.Salary)
                {
                    LastSalaryChange = value - base.Salary;
                }

                base.Salary = value;
            }
        }
        public event LocalWorkerAction AbsenceStarted;
        public event LocalWorkerAction AbsenceFinished;
        public event LocalWorkerAction DaysInCompanyChanged;
        public event LocalWorkerAction DaysOfHolidaysLeftChanged;
        public event LocalWorkerAction SatisfactionChanged;

        /*Private methods*/

        /*Public methods*/

        public LocalWorker(string name, string surename, Gender gender) : base(name, surename, gender) { }

        public LocalWorker(SharedWorker worker) : base(worker) { }
    }
}
