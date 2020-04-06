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

    /*Private fields*/

    private GameTime GameTimeComponent;
    private MainSimulationManager SimulationManagerComponent;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    /// <summary>
    /// This expenses will be handled at start of every month
    /// </summary>
    private void HandleCompanyExpenses()
    {
        HandleWorkerSalaries();
        HandleWorkerExpenses();
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
    }

    private void UpdateWorkersState()
    {
        foreach (Worker companyWorker in SimulationManagerComponent.ControlledCompany.Workers)
        {
            companyWorker.DaysInCompany += 1;

            if (true == companyWorker.Available)
            {
                ++companyWorker.DaysSinceAbsent;
            }

            SimulateWorkerAbsence(companyWorker);
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

    private void Start()
    {
        GameTimeComponent = GetComponent<GameTime>();
        SimulationManagerComponent = GetComponent<MainSimulationManager>();

        GameTimeComponent.MonthChanged += HandleCompanyExpenses;
        GameTimeComponent.DayChanged += UpdateWorkersState;
        GameTimeComponent.YearChanged += HandleWorkerHolidayLimits;

        SimulationManagerComponent.ControlledCompany.ProjectAdded += OnCompanyProjectAdded;
    }

    /*Public methods*/
}
