using UnityEngine;

/// <summary>
/// This class handles updating state of company. It includes updating company's workers
/// state, charging money for company's expenses, etc.
/// </summary>
public class PlayerCompanyManager : MonoBehaviour
{
    /*Private consts fields*/

    /// <summary>
    /// Which day of month monthly expenses will be charged
    /// </summary>
    private const int DAY_NUMBER_OF_MONTHY_EXPENSES = 1;
    /// <summary>
    /// How many it will cost monthly to have one worker.
    /// This cost is expenses other than salary (electricty,
    /// cleaning office, etc.)
    /// </summary>
    private const int MONTHLY_COST_PER_WORKER = 50;

    /*Private fields*/

    /*Public consts fields*/

    /*Public fields*/

    public GameTime GameTimeComponent;
    public MainSimulationManager SimulationManagerComponent;

    /*Private methods*/

    private void HandleCompanyExpenses()
    {
        if (DAY_NUMBER_OF_MONTHY_EXPENSES == GameTimeComponent.CurrentTime.Day)
        {
            HandleWorkerSalaries();
            HandleWorkerExpenses();
        }
    }

    private void HandleWorkerSalaries()
    {
        foreach (Worker companyWorker in SimulationManagerComponent.ControlledCompany.Workers)
        {
            SimulationManagerComponent.ControlledCompany.Balance -= companyWorker.Salary;
        }
    }

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
        GameTimeComponent.DayChanged += HandleCompanyExpenses;
        GameTimeComponent.DayChanged += UpdateWorkersState;

        SimulationManagerComponent.ControlledCompany.ProjectAdded += OnCompanyProjectAdded;
    }

    /*Public methods*/
}
