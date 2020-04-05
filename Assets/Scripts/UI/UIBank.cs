using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIBank : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    /// <summary>
    /// Used to store text reference to each of buttons text to update
    /// it when loan state changes
    /// </summary>
    private Dictionary<BankLoan, Text> LoanButtonsTextMap = new Dictionary<BankLoan, Text>();
    /// <summary>
    /// Used to store text reference to each of buttons to delete
    /// it from list view when loan is paid off
    /// </summary>
    private Dictionary<BankLoan, Button> LoanButtonsMap = new Dictionary<BankLoan, Button>();
    private IButtonSelector LoansButtonSelector = new ButtonSelector();
    /// <summary>
    /// Loan that is currently selected from loans list
    /// </summary>
    private BankLoan SelectedLoan;
    [SerializeField]
    private Bank BankComponent;
    [SerializeField]
    private InputField LoanPaymentsCountInputField;
    [SerializeField]
    private InputField LoanAmountInputField;
    [SerializeField]
    private Text LoanAmountText;
    [SerializeField]
    private Text LoanPaymentsText;
    [SerializeField]
    private Slider LoanAmountSlider;
    [SerializeField]
    private Slider LoanPaymentsCountSlider;
    [SerializeField]
    private MainSimulationManager SimulationManagerComponent;
    [SerializeField]
    private InputField CompanyBalanceInputField;
    [SerializeField]
    private InputField AmountToPayInputField;
    [SerializeField]
    private GameObject LoanListViewButtonPrefab;
    [SerializeField]
    private Button TakeLoanButton;
    [SerializeField]
    private ControlListView LoanListView;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void OnLoadAdded(BankLoan newLoan)
    {
        GameObject newLoanButton = GameObject.Instantiate(LoanListViewButtonPrefab);
        Button buttonComponent = newLoanButton.GetComponent<Button>();
        Text buttonText = newLoanButton.GetComponentInChildren<Text>();
        buttonText.text = string.Format("{0} / {1} $", newLoan.AmountPaid, newLoan.Amount);
        LoanButtonsMap.Add(newLoan, buttonComponent);
        LoanButtonsTextMap.Add(newLoan, buttonText);
        LoanListView.AddControl(newLoanButton);
        LoansButtonSelector.AddButton(buttonComponent);

        newLoan.SinglePaymentPaid += OnLoanSinglePaymentPaid;
        newLoan.PaidOff += OnLoanPaidOff;

        if (Bank.MAX_LOANS_COUNT == BankComponent.Loans.Count)
        {
            TakeLoanButton.interactable = false;
        }
    }

    private void SetMoneyAmounts()
    {
        CompanyBalanceInputField.text =
            SimulationManagerComponent.ControlledCompany.Balance.ToString() + " $";
    }

    private void OnLoanSinglePaymentPaid(BankLoan loan)
    {
        Text loanText = LoanButtonsTextMap[loan];
        loanText.text = string.Format("{0} / {1} $", loan.AmountPaid, loan.Amount);

        SetMoneyAmounts();

        if (null != SelectedLoan)
        {
            SetLoanInfo();
        }
    }

    private void OnLoanPaidOff(BankLoan loan)
    {
        Button loanButton = LoanButtonsMap[loan];
        LoanListView.RemoveControl(loanButton.gameObject);
        LoanButtonsTextMap.Remove(loan);
        LoanButtonsMap.Remove(loan);
        LoansButtonSelector.RemoveButton(loanButton);

        TakeLoanButton.interactable = true;
    }

    private void OnLoanSelectedButtonChanged(Button clickedButton)
    {
        if (null != clickedButton)
        {
            SelectedLoan = LoanButtonsMap.First(x => x.Value == clickedButton).Key;
            SetLoanInfo();
        }
        else
        {
            ClearLoanInfo();
        }
    }

    private void SetLoanInfo()
    {
        LoanAmountInputField.text = SelectedLoan.Amount.ToString();
        LoanPaymentsCountInputField.text = string.Format(
            "{0} / {1}", SelectedLoan.PaymentsPaid, SelectedLoan.PaymentsCount);
    }

    private void ClearLoanInfo()
    {
        LoanAmountInputField.text = string.Empty;
        LoanPaymentsCountInputField.text = string.Empty;
    }

    private void InitLoanSliders()
    {
        LoanAmountSlider.minValue = BankComponent.MinLoanAmount;
        LoanAmountSlider.maxValue = BankComponent.MaxLoanAmout;
        LoanPaymentsCountSlider.minValue = BankLoan.MIN_PAYMENTS_COUNT;
        LoanPaymentsCountSlider.maxValue = BankLoan.MAX_PAYMENTS_COUNT;
    }

    private void Start()
    {
        InitLoanSliders();
        SetMoneyAmounts();
        UpdateLoanAmount(LoanAmountSlider.value);
        UpdateLoanPaymentsCount(LoanPaymentsCountSlider.value);
        BankComponent.LoanAdded += OnLoadAdded;
        LoansButtonSelector.SelectedButtonChanged += OnLoanSelectedButtonChanged;
    }

    /*Public methods*/

    public void UpdateLoanAmount(float value)
    {
        int actualLoanAmount = BankComponent.CalculateLoanAmountWithInterest((int)value);
        string loanAmountText = string.Format("Amount: {0} $ ({1} $ to pay off)", (int)value, actualLoanAmount);
        LoanAmountText.text = loanAmountText;
        UpdateLoanPaymentsCount(LoanPaymentsCountSlider.value);
    }

    public void UpdateLoanPaymentsCount(float value)
    {
        int valueInt = (int)value;
        int actualLoanAmount = BankComponent.CalculateLoanAmountWithInterest((int)LoanAmountSlider.value);
        int singlePayment = (int)Mathf.Ceil(actualLoanAmount / valueInt);
        string loanPaymentsText = string.Format("Payments: {0} (Single payment: {1} $)", valueInt, singlePayment);
        LoanPaymentsText.text = loanPaymentsText;
    }

    public void OnTakeLoanButtonClicked()
    {
        BankComponent.TakeLoan((int)LoanAmountSlider.value, (int)LoanPaymentsCountSlider.value);
        SetMoneyAmounts();

        if (BankComponent.Loans.Count == Bank.MAX_LOANS_COUNT)
        {
            TakeLoanButton.interactable = false;
        }
    }
}