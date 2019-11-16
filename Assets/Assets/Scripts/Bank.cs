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

    /*Public fields*/

    public int AmountToPayOff { get; private set; }
    /// <summary>
    /// Single payment of loan occured
    /// </summary>
    public event Action LoanPaid;

    /*Private methods*/

    private IEnumerator UpdateLoan()
    {
        while (true)
        {
            yield return new WaitForSeconds(LOAN_UPDATE_FREQUENCY);

            if (AmountToPayOff > 0)
            {
                int amountPaid = AmountToPayOff / 100;
                SimulationManagerComponent.ControlledCompany.Balance -= amountPaid;
                AmountToPayOff -= amountPaid;
                AmountToPayOff = Mathf.Clamp(AmountToPayOff, 0, int.MaxValue);
                LoanPaid?.Invoke();
            }
            else
            {
                StopCoroutine(LoanUpdateCoroutine);
            }
        }
    }

    private void Start()
    {
        SimulationManagerComponent = GetComponent<MainSimulationManager>();
    }

    /*Public methods*/

    public void TakeLoan(int amount)
    {
        AmountToPayOff += amount + (int)(amount * INTEREST_RATE);

        if (false == UpdateLoanActive)
        {
            LoanUpdateCoroutine = StartCoroutine(UpdateLoan());
            UpdateLoanActive = true;
        }
    }
}
