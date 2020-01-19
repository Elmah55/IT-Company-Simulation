using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bank : MonoBehaviour
{
    /*Private consts fields*/

    private const float LOAN_UPDATE_FREQUENCY = 5.0f;
    /// <summary>
    /// Interest rate of bank 
    /// </summary>
    private const float INTEREST_RATE = 0.1f;

    /*Private fields*/

    private MainSimulationManager SimulationManagerComponent;
    private Coroutine LoanUpdateCoroutine;
    private bool UpdateLoanActive;

    /*Public consts fields*/

    /// <summary>
    /// How many active loans player can have at a time
    /// </summary>
    public const int MAX_LOANS_COUNT = 5;

    /*Public fields*/

    public List<BankLoan> Loans { get; private set; }
    /// <summary>
    /// New loan has been taken by player
    /// </summary>
    public event LoanAction LoanAdded;

    /*Private methods*/

    private IEnumerator UpdateLoan()
    {
        while (true)
        {
            yield return new WaitForSeconds(LOAN_UPDATE_FREQUENCY);

            bool loansPaidOff = true;

            foreach (BankLoan loan in Loans)
            {
                if (false == loan.IsPaidOff)
                {
                    loansPaidOff = false;
                    PaySinglePayment(loan);
                }
            }

            //All loans paid off
            if (true == loansPaidOff)
            {
                StopCoroutine(LoanUpdateCoroutine);
                UpdateLoanActive = false;
            }
        }
    }

    private void PaySinglePayment(BankLoan loan)
    {
        ++loan.PaymentsPaid;
        loan.AmountPaid += loan.SinglePayment;


        int companyPayment;

        //This might happen because single payment is calculated with ceil function
        if (loan.AmountPaid > loan.Amount)
        {
            loan.AmountPaid = Mathf.Clamp(loan.AmountPaid, 0, loan.Amount);
            companyPayment = loan.AmountPaid - loan.Amount;
        }
        else
        {
            companyPayment = loan.SinglePayment;
        }

        SimulationManagerComponent.ControlledCompany.Balance -= companyPayment;
    }

    private void Start()
    {
        SimulationManagerComponent = GetComponent<MainSimulationManager>();
        Loans = new List<BankLoan>();
    }

    /*Public methods*/

    public void TakeLoan(int amount, int paymentsCount)
    {
        int amountToPayOff = CalculateLoanAmountWithInterest(amount);
        BankLoan newLoan = new BankLoan(amountToPayOff, paymentsCount);
        SimulationManagerComponent.ControlledCompany.Balance += amount;
        Loans.Add(newLoan);
        LoanAdded?.Invoke(newLoan);

        if (Loans.Count > MAX_LOANS_COUNT)
        {
            throw new InvalidOperationException(
                "Number of active loans cannot be greater than " + MAX_LOANS_COUNT);
        }

        if (false == UpdateLoanActive)
        {
            LoanUpdateCoroutine = StartCoroutine(UpdateLoan());
            UpdateLoanActive = true;
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
