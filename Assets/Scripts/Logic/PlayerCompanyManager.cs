using UnityEngine;

/// <summary>
/// This class handles updating state of company. It includes updating company's workers
/// state, charging money for company's expenses, etc.
/// </summary>
public class PlayerCompanyManager : MonoBehaviour
{
    /*Private consts fields*/

    /// <summary>
    /// How many it will cost monthly to have one worker.
    /// This cost is expenses other than salary (electricty,
    /// cleaning office, etc.)
    /// </summary>
    private const int MONTHLY_COST_PER_WORKER = 50;
    /// <summary>
    /// How many percent of satisfaction worker will lose each day
    /// </summary>
    private const float WORKER_DAILY_SATISFACTION_LOSS = 0.06f;
    /// <summary>
    /// If satisfaction level of worker will fall below this level he will leave 
    /// the company
    /// </summary>
    private const float WORKER_SATISFACTION_LEAVE_TRESHOLD = 20.0f;

    /*Private fields*/

    private GameTime GameTimeComponent;
    private MainSimulationManager SimulationManagerComponent;
    private int TotalMonthlyExpenses;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    /// <summary>
    /// This expenses will be handled at start of every month
    /// </summary>
    private void HandleCompanyExpenses()
    {
        TotalMonthlyExpenses = 0;
        HandleWorkerSalaries();
        HandleWorkerExpenses();

        string notification = string.Format("Your company spent {0} $ this month",
            TotalMonthlyExpenses);
        SimulationManagerComponent.NotificatorComponent.Notify(notification);
    }

    /// <summary>
    /// Holidays limits for each worker in company will be renewed every year
    /// </summary>
    private void HandleWorkerHolidayLimits()
    {
        foreach (Worker companyWorker in SimulationManagerComponent.ControlledCompany.Workers)
        {
            companyWorker.DaysOfHolidaysLeft = Worker.DAYS_OF_HOLIDAYS_PER_YEAR;
        }
    }

    private void HandleWorkerSalaries()
    {
        foreach (Worker companyWorker in SimulationManagerComponent.ControlledCompany.Workers)
        {
            SimulationManagerComponent.ControlledCompany.Balance -= companyWorker.Salary;
            TotalMonthlyExpenses += companyWorker.Salary;
        }
    }

    /// <summary>
    /// These are expenses that each worker generates for company
    /// </summary>
    private void HandleWorkerExpenses()
    {
        int companyWorkerExpenses =
            SimulationManagerComponent.ControlledCompany.Workers.Count * MONTHLY_COST_PER_WORKER;
        SimulationManagerComponent.ControlledCompany.Balance -= companyWorkerExpenses;
        TotalMonthlyExpenses += companyWorkerExpenses;
    }

    private void UpdateWorkersState()
    {
        for (int i = 0; i < SimulationManagerComponent.ControlledCompany.Workers.Count; i++)
        {
            Worker companyWorker = SimulationManagerComponent.ControlledCompany.Workers[i];
            companyWorker.DaysInCompany += 1;

            if (true == companyWorker.Available)
            {
                ++companyWorker.DaysSinceAbsent;
            }

            SimulateWorkerAbsence(companyWorker);
            SimulateWorkerSatisfaction(companyWorker);
        }
    }

    private void SimulateWorkerSatisfaction(Worker companyWorker)
    {
        CalculateWorkerSatisfactionSalaryDays(companyWorker);

        if (companyWorker.Satiscation < WORKER_SATISFACTION_LEAVE_TRESHOLD)
        {
            SimulationManagerComponent.ControlledCompany.RemoveWorker(companyWorker);
        }
    }

    /// <summary>
    /// Calculates satisfaction based on days since salary raise
    /// </summary>
    private void CalculateWorkerSatisfactionSalaryDays(Worker companyWorker)
    {
        companyWorker.Satiscation -= WORKER_DAILY_SATISFACTION_LOSS;
        //Satisfaction is percent value
        companyWorker.Satiscation = Mathf.Clamp(companyWorker.Satiscation, 0.0f, 100.0f);

        float notifySatisfactionLvl = 30f;
        if (companyWorker.Satiscation <= notifySatisfactionLvl)
        {
            string notification = string.Format("Your worker's {0} {1} satisfaction level fell below {2}. " +
                 "Try to increase it as soon as possible or worker will leave your company !",
                 companyWorker.Name, companyWorker.Surename, (int)notifySatisfactionLvl);
            SimulationManagerComponent.NotificatorComponent.Notify(notification);
        }
    }

    private void SimulateWorkerAbsence(Worker companyWorker)
    {
        --companyWorker.DaysUntilAvailable;

        if (true == companyWorker.Available)
        {
            SimulateWorkerSickenss(companyWorker);
            SimulateWorkerHoliday(companyWorker);
        }
        else if (0 == companyWorker.DaysUntilAvailable)
        {
            companyWorker.Available = true;
        }
    }

    private void SimulateWorkerSickenss(Worker companyWorker)
    {
        //What is the probability of worker being sick (in %)
        int notSickProbability = 100 - (int)(companyWorker.DaysSinceAbsent * 0.2);
        int randomNumber = Random.Range(0, 100);

        if (randomNumber >= notSickProbability)
        {
            //Worker is sick
            int sicknessDuration = Random.Range(Worker.MIN_SICKNESS_DURATION, Worker.MAX_SICKNESS_DURATION);
            companyWorker.AbsenceReason = WorkerAbsenceReason.Sickness;
            companyWorker.DaysSinceAbsent = 0;
            companyWorker.DaysUntilAvailable = sicknessDuration;
            companyWorker.Available = false;

            string playerNotification = string.Format("Your worker {0} {1} just got sick ! {2} days until he will get back to work",
                companyWorker.Name, companyWorker.Surename, companyWorker.DaysUntilAvailable);
            SimulationManagerComponent.NotificatorComponent.Notify(playerNotification);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string debugInfo = string.Format("Worker {0} {1} (ID {2}) is sick\n{3} days until available\n",
                companyWorker.Name, companyWorker.Surename, companyWorker.ID, companyWorker.DaysUntilAvailable);
            Debug.Log(debugInfo);
#endif
        }
    }

    private void SimulateWorkerHoliday(Worker companyWorker)
    {
        //What is the probability of worker going to holidays (in %)
        int holidayProbability = 100 - (int)(companyWorker.DaysSinceAbsent * 0.2);
        int randomNumber = Random.Range(0, 100);


        if (randomNumber >= holidayProbability)
        {
            //Worker is on holidays
            int holidayDuration = Random.Range(Worker.MIN_HOLIDAY_DURATION, Worker.MAX_HOLIDAY_DURATION);
            holidayDuration = Mathf.Clamp(holidayDuration, 1, companyWorker.DaysOfHolidaysLeft);
            companyWorker.AbsenceReason = WorkerAbsenceReason.Holiday;
            companyWorker.DaysSinceAbsent = 0;
            companyWorker.DaysUntilAvailable = holidayDuration;
            companyWorker.DaysOfHolidaysLeft -= holidayDuration;
            companyWorker.Available = false;

            string playerNotification = string.Format("Your worker {0} {1} is on holidays now. {2} days until he will get back to work",
                companyWorker.Name, companyWorker.Surename, companyWorker.DaysUntilAvailable);
            SimulationManagerComponent.NotificatorComponent.Notify(playerNotification);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string debugInfo = string.Format("Worker {0} {1} (ID {2}) is on holidays\n{3} days until available\n",
                companyWorker.Name, companyWorker.Surename, companyWorker.ID, companyWorker.DaysUntilAvailable);
            Debug.Log(debugInfo);
#endif
        }
    }

    private void OnCompanyProjectAdded(Project newProject)
    {
        newProject.Completed += OnCompanyProjectCompleted;
    }

    private void OnCompanyProjectCompleted(Project newProject)
    {
        SimulationManagerComponent.ControlledCompany.Balance +=
            newProject.CompleteBonus;
        newProject.Completed -= OnCompanyProjectCompleted;
    }

    private void OnCompanyWorkerSalaryChanged(Worker companyWorker)
    {
        CalculateSatisfactionSalaryRaise(companyWorker);
    }

    /// <summary>
    /// Calculates satisfaction based on salary change amount
    /// </summary>
    private static void CalculateSatisfactionSalaryRaise(Worker companyWorker)
    {
        float satisfactionChange = companyWorker.LastSalaryChange / (float)(companyWorker.Salary - companyWorker.LastSalaryChange);
        satisfactionChange *= 100.0f;
        companyWorker.Satiscation += satisfactionChange;
        //Satisfaction is percent value
        companyWorker.Satiscation = Mathf.Clamp(companyWorker.Satiscation, 0.0f, 100.0f);
    }

    private void OnCompanyWorkerRemoved(Worker removedWorker)
    {
        removedWorker.SalaryChanged -= OnCompanyWorkerSalaryChanged;
    }

    private void OnCompanyWorkerAdded(Worker addedWorker)
    {
        addedWorker.SalaryChanged += OnCompanyWorkerSalaryChanged;
    }

    private void Start()
    {
        GameTimeComponent = GetComponent<GameTime>();
        SimulationManagerComponent = GetComponent<MainSimulationManager>();

        GameTimeComponent.MonthChanged += HandleCompanyExpenses;
        GameTimeComponent.DayChanged += UpdateWorkersState;
        GameTimeComponent.YearChanged += HandleWorkerHolidayLimits;

        SimulationManagerComponent.ControlledCompany.ProjectAdded += OnCompanyProjectAdded;
        SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnCompanyWorkerRemoved;
        SimulationManagerComponent.ControlledCompany.WorkerAdded += OnCompanyWorkerAdded;

        foreach (Worker companyWorker in SimulationManagerComponent.ControlledCompany.Workers)
        {
            companyWorker.SalaryChanged += OnCompanyWorkerSalaryChanged;
        }
    }

    /*Public methods*/
}
