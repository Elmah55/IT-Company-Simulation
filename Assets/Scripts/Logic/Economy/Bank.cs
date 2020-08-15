using System;
using System.Collections.Generic;
using UnityEngine;

namespace ITCompanySimulation.Economy
{
    /// <summary>
    /// This class represents bank that can lend loans to player's company
    /// Player can have only one active loan
    /// </summary>
    public class Bank : MonoBehaviour
    {
        /*Private consts fields*/

        /// <summary>
        /// Interest rate of bank (in %)
        /// </summary>
        private const float INTEREST_RATE = 0.1f;

        /*Private fields*/

        private MainSimulationManager SimulationManagerComponent;
        private GameTime GameTimeComponent;

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Loan that player took from bank
        /// </summary>
        public BankLoan Loan { get; private set; }
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
            if (null != Loan && false == Loan.IsPaidOff)
            {
                PaySinglePayment(Loan);
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

        }

        /*Public methods*/

        public void TakeLoan(int amount, int paymentsCount)
        {
            if (Loan != null && false == Loan.IsPaidOff)
            {
                throw new InvalidOperationException(
                    "Only one loan can be active at a time");
            }

            int amountToPayOff = CalculateLoanAmountWithInterest(amount);
            Loan = new BankLoan(amountToPayOff, paymentsCount);
            SimulationManagerComponent.ControlledCompany.Balance += amount;
            GameTimeComponent.MonthChanged += UpdateLoan;
            LoanAdded?.Invoke(Loan);
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
}
