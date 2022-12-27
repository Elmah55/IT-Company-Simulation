using ITCompanySimulation.Character;
using ITCompanySimulation.Core;
using ITCompanySimulation.Project;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ITCompanySimulation.Company
{
    /// <summary>
    /// This class handles updating state of company. It includes updating company's workers
    /// state, scrum processes, charging money for company's expenses, etc.
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
        private SimulationManager SimulationManagerComponent;
        private int TotalMonthlyExpenses;
        /// <summary>
        /// This list hold reference to workers whom satifaction level fell below
        /// threshold level. It is stored to prevent notifying player more than one time
        /// </summary>
        private List<LocalWorker> WorkersSatisfactionNotificationSent = new List<LocalWorker>();

        /*Public consts fields*/

        /// <summary>
        /// How often binded project should be updated.
        /// This time is seconds in game time (scaled time)
        /// </summary>
        public const float PROJECT_UPDATE_FREQUENCY = 10f;

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
            foreach (LocalWorker companyWorker in SimulationManagerComponent.ControlledCompany.Workers)
            {
                companyWorker.DaysOfHolidaysLeft = LocalWorker.DAYS_OF_HOLIDAYS_PER_YEAR;
            }
        }

        private void HandleWorkerSalaries()
        {
            foreach (LocalWorker companyWorker in SimulationManagerComponent.ControlledCompany.Workers)
            {
                int salaryAmount;

                if (companyWorker.DaysInCompany >= 31)
                {
                    //Worker has been hired for at least one month
                    //for sure (31 days or more). Pay full salary
                    salaryAmount = companyWorker.Salary;
                }
                else
                {
                    //Check if worker was hired for whole month when month
                    //has less than 31 days
                    int daysInCurrentMonth = System.DateTime.DaysInMonth(
                        GameTimeComponent.CurrentDate.Year,
                        GameTimeComponent.CurrentDate.Month);

                    if (companyWorker.DaysInCompany >= daysInCurrentMonth)
                    {
                        salaryAmount = companyWorker.Salary;
                    }
                    else
                    {
                        //Worker was not hired for whole month. Calculate how many % of
                        //salary should be paid

                        float salaryPercentage = companyWorker.DaysInCompany / (float)daysInCurrentMonth;
                        salaryAmount = (int)(companyWorker.Salary * salaryPercentage);
                    }
                }

                SimulationManagerComponent.ControlledCompany.Balance -= salaryAmount;
                TotalMonthlyExpenses += salaryAmount;
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
                LocalWorker companyWorker = SimulationManagerComponent.ControlledCompany.Workers[i];
                companyWorker.DaysInCompany += 1;

                if (true == companyWorker.Available)
                {
                    ++companyWorker.DaysSinceAbsent;
                }

                SimulateWorkerAbsence(companyWorker);
                SimulateWorkerSatisfaction(companyWorker);
            }
        }

        private void SimulateWorkerSatisfaction(LocalWorker companyWorker)
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
        private void CalculateWorkerSatisfactionSalaryDays(LocalWorker companyWorker)
        {
            companyWorker.Satiscation -= WORKER_DAILY_SATISFACTION_LOSS;
            //Satisfaction is percent value
            companyWorker.Satiscation = Mathf.Clamp(companyWorker.Satiscation, 0.0f, 100.0f);

            float notifySatisfactionLvl = 30f;

            if (companyWorker.Satiscation > notifySatisfactionLvl)
            {
                for (int i = 0; i < WorkersSatisfactionNotificationSent.Count; i++)
                {
                    if (companyWorker == WorkersSatisfactionNotificationSent[i])
                    {
                        WorkersSatisfactionNotificationSent.RemoveAt(i);
                        break;
                    }
                }
            }
            else if (false == WorkersSatisfactionNotificationSent.Contains(companyWorker))
            {
                string notification = string.Format("Your worker's {0} {1} satisfaction level fell below {2} %. " +
                     "Try to increase it as soon as possible or worker will leave your company !",
                     companyWorker.Name, companyWorker.Surename, (int)notifySatisfactionLvl);
                SimulationManagerComponent.NotificatorComponent.Notify(notification);
                WorkersSatisfactionNotificationSent.Add(companyWorker);
            }
        }

        private void SimulateWorkerAbsence(LocalWorker companyWorker)
        {
            --companyWorker.DaysUntilAvailable;

            if (true == companyWorker.Available)
            {
                SimulateWorkerSickness(companyWorker);
            }

            if (true == companyWorker.Available && companyWorker.DaysOfHolidaysLeft > 0)
            {
                SimulateWorkerHoliday(companyWorker);
            }

            if (0 == companyWorker.DaysUntilAvailable)
            {
                companyWorker.Available = true;
            }
        }

        private void SimulateWorkerSickness(LocalWorker companyWorker)
        {
            //What is the probability of worker being sick (in %)
            float notSickProbability = companyWorker.DaysSinceAbsent * 0.001f;
            float randomNumber = Random.Range(0f, 100f);

            if (randomNumber <= notSickProbability)
            {
                //Worker is sick
                int sicknessDuration = Random.Range(LocalWorker.MIN_SICKNESS_DURATION, LocalWorker.MAX_SICKNESS_DURATION);
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

        private void SimulateWorkerHoliday(LocalWorker companyWorker)
        {
            //What is the probability of worker going to holidays (in %)
            float holidayProbability = companyWorker.DaysSinceAbsent * 0.001f;
            float randomNumber = Random.Range(0f, 100f);

            if (randomNumber <= holidayProbability)
            {
                //Worker is on holidays
                int holidayDuration = Random.Range(LocalWorker.MIN_HOLIDAY_DURATION, LocalWorker.MAX_HOLIDAY_DURATION);
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

        private void OnCompanyProjectAdded(Scrum scrumObj)
        {
            scrumObj.BindedProject.Completed += OnCompanyProjectCompleted;
        }

        private void OnCompanyProjectRemoved(Scrum scrumObj)
        {
            //Project removed before being completed (project canceled). Company should be charged penalty
            if (false == scrumObj.BindedProject.IsCompleted)
            {
                LocalProject canceledProject = scrumObj.BindedProject;
                SimulationManagerComponent.ControlledCompany.Balance -= canceledProject.CompletionTimeExceededPenalty;
                string notificationTxt = string.Format("Project {0} canceled, {1} $ penalty has been charged",
                                                       canceledProject.Name,
                                                       canceledProject.CompletionTimeExceededPenalty);
                SimulationManagerComponent.NotificatorComponent.Notify(notificationTxt);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                string debugInfo = string.Format("[{3}] Project canceled\nName: {0}\nID: {1}\nPenalty: {2}",
                                                 canceledProject.Name,
                                                 canceledProject.ID,
                                                 canceledProject.CompletionTimeExceededPenalty,
                                                 this.GetType().Name);
                Debug.Log(debugInfo);
#endif
            }
        }

        private void OnCompanyProjectCompleted(LocalProject newProject)
        {
            SimulationManagerComponent.ControlledCompany.Balance +=
                newProject.CompletionBonus;
            newProject.Completed -= OnCompanyProjectCompleted;
        }

        private void OnCompanyWorkerSalaryChanged(SharedWorker worker)
        {
            LocalWorker companyWorker = (LocalWorker)worker;
            float satisfactionChange = CalculateSatisfactionSalaryRaise(companyWorker);
            companyWorker.Satiscation += satisfactionChange;
            //Satisfaction is percent value
            companyWorker.Satiscation = Mathf.Clamp(companyWorker.Satiscation, 0f, 100f);
        }

        /// <summary>
        /// Calculates satisfaction based on salary change amount
        /// </summary>
        private float CalculateSatisfactionSalaryRaise(LocalWorker companyWorker)
        {
            //The greater is last salary change compared to overall salary of worker the greater is satisfaction level
            float satisfactionChange = companyWorker.LastSalaryChange / (float)(companyWorker.Salary - companyWorker.LastSalaryChange);
            satisfactionChange *= 100.0f;
            return satisfactionChange;
        }

        private IEnumerator UpdateCompanyProjectsCoroutine()
        {
            while (true == SimulationManagerComponent.IsSimulationActive)
            {
                yield return new WaitForSeconds(PROJECT_UPDATE_FREQUENCY);

                for (int i = 0; i < SimulationManagerComponent.ControlledCompany.ScrumProcesses.Count; i++)
                {
                    SimulationManagerComponent.ControlledCompany.ScrumProcesses[i].UpdateProgress();
                }
            }
        }

        private void OnCompanyWorkerRemoved(LocalWorker removedWorker)
        {
            removedWorker.SalaryChanged -= OnCompanyWorkerSalaryChanged;
        }

        private void OnCompanyWorkerAdded(LocalWorker addedWorker)
        {
            addedWorker.SalaryChanged += OnCompanyWorkerSalaryChanged;
        }

        private void Start()
        {
            GameTimeComponent = GetComponent<GameTime>();
            SimulationManagerComponent = GetComponent<SimulationManager>();

            GameTimeComponent.MonthChanged += HandleCompanyExpenses;
            GameTimeComponent.DayChanged += UpdateWorkersState;
            GameTimeComponent.YearChanged += HandleWorkerHolidayLimits;

            SimulationManagerComponent.ControlledCompany.ProjectAdded += OnCompanyProjectAdded;
            SimulationManagerComponent.ControlledCompany.ProjectRemoved += OnCompanyProjectRemoved;
            SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnCompanyWorkerRemoved;
            SimulationManagerComponent.ControlledCompany.WorkerAdded += OnCompanyWorkerAdded;

            foreach (LocalWorker companyWorker in SimulationManagerComponent.ControlledCompany.Workers)
            {
                companyWorker.SalaryChanged += OnCompanyWorkerSalaryChanged;
            }

            SimulationManagerComponent.SimulationStarted += () => { StartCoroutine(UpdateCompanyProjectsCoroutine()); };
        }

        /*Public methods*/
    }
}
