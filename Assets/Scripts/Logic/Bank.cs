using System;
using System.Collections.Generic;
using UnityEngine;

public class Bank : MonoBehaviour
{
    /*Private consts fields*/

    /// <summary>
    /// Interest rate of bank 
    /// </summary>
    private const float INTEREST_RATE = 0.1f;

    /*Private fields*/

    private MainSimulationManager SimulationManagerComponent;
    private GameTime GameTimeComponent;
    private bool AllLoansPaidOff;

    /*Public consts fields*/

    /// <summary>
    /// How many active loans player can have at a time
    /// </summary>
    public const int MAX_LOANS_COUNT = 5;

    /*Public fields*/

    public List<BankLoan> Loans { get; private set; } = new List<BankLoan>();
    /// <summary>
    /// Determines maximum amount of one loan. It is based
    /// on set target balance of simulation
    /// </summary>
    public int MaxLoanAmout { get; private set; }
    public int MinLoanAmount { get; private set; }
    /// <summary>
    /// New loan has been taken by player
    /// </summary>
    public event LoanAction LoanAdded;

    /*Private methods*/

    private void UpdateLoan()
    {
        if (false == AllLoansPaidOff)
        {
            List<BankLoan> paidLoans = new List<BankLoan>();

            foreach (BankLoan loan in Loans)
            {
                if (false == loan.IsPaidOff)
                {
                    PaySinglePayment(loan);
                }
                else
                {
                    paidLoans.Add(loan);
                }
            }

            AllLoansPaidOff = (paidLoans.Count == Loans.Count);

            foreach (BankLoan loan in paidLoans)
            {
                Loans.Remove(loan);
            }
        }
    }

    private void PaySinglePayment(BankLoan loan)
    {
        int companyPayment;

        //This might happen because single payment is floor of calculated value
        //so last payment may be bigger than the others
        if (loan.PaymentsPaid == loan.PaymentsCount - 1 && loan.AmountPaid < loan.Amount)
        {
            companyPayment = loan.Amount - loan.AmountPaid;
        }
        else
        {
            companyPayment = loan.SinglePayment;
        }

        ++loan.PaymentsPaid;
        loan.AmountPaid += companyPayment;
        SimulationManagerComponent.ControlledCompany.Balance -= companyPayment;
    }

    private void Start()
    {
        SimulationManagerComponent = GetComponent<MainSimulationManager>();
        GameTimeComponent = GetComponent<GameTime>();

        MaxLoanAmout = SimulationManagerComponent.GameManagerComponent.SettingsOfSimulation.TargetBalance / 10;
        MinLoanAmount = MaxLoanAmout / 10;

        GameTimeComponent.MonthChanged += UpdateLoan;
    }

    /*Public methods*/

    public void TakeLoan(int amount, int paymentsCount)
    {
        int amountToPayOff = CalculateLoanAmountWithInterest(amount);
        BankLoan newLoan = new BankLoan(amountToPayOff, paymentsCount);
        SimulationManagerComponent.ControlledCompany.Balance += amount;
        Loans.Add(newLoan);
        LoanAdded?.Invoke(newLoan);
        AllLoansPaidOff = false;

        if (Loans.Count > MAX_LOANS_COUNT)
        {
            throw new InvalidOperationException(
                "Number of active loans cannot be greater than " + MAX_LOANS_COUNT);
        }
    }

    /// <summary>
    /// Calculates loan amount with interest rate included
    /// </summary>
    public int CalculateLoanAmountWithInterest(int basicAmount)
    {
        int actualAmount = (int)(basicAmount + (basicAmount * INTEREST_RATE));
        return actualAmount;
    }
}
