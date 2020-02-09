using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIBank : MonoBehaviour
{
    /*Private consts fields*/

    private readonly Color SELECTED_LOAN_BUTTON_COLOR = Color.gray;

    /*Private fields*/

    /// <summary>
    /// Used to store text reference to each of buttons text to update
    /// it when loan state changes
    /// </summary>
    private Dictionary<BankLoan, Text> LoanButtonsText;
    /// <summary>
    /// Used to store text reference to each of buttons to delete
    /// it from list view when loan is paid off
    /// </summary>
    private Dictionary<BankLoan, GameObject> LoanButtons;
    /// <summary>
    /// Button that is currently selected in list of loans
    /// </summary>
    private GameObject LoanSelectedButton;
    /// <summary>
    /// Used to restore button's colors to default value
    /// </summary>
    private ColorBlock LoanButtonColors;
    /// <summary>
    /// Loan that is currently selected from loans list
    /// </summary>
    private BankLoan SelectedLoan;

    /*Public consts fields*/

    /*Public fields*/

    public InputField LoanPaymentsCountInputField;
    public InputField LoanAmountInputField;
    public Text LoanAmountText;
    public Text LoanPaymentsText;
    public Slider LoanAmountSlider;
    public Slider LoanPaymentsCountSlider;
    public Bank BankComponent;
    public MainSimulationManager SimulationManagerComponent;
    public InputField CompanyBalanceInputField;
    public InputField AmountToPayInputField;
    public GameObject LoanListViewButtonPrefab;
    public Button TakeLoanButton;
    public ControlListView LoanListView;

    /*Private methods*/

    private void OnLoadAdded(BankLoan newLoan)
    {
        GameObject newLoanButton = GameObject.Instantiate(LoanListViewButtonPrefab);
        Button buttonComponent = newLoanButton.GetComponent<Button>();
        buttonComponent.onClick.AddListener(OnLoanButtonClicked);
        Text buttonText = newLoanButton.GetComponentInChildren<Text>();
        buttonText.text = string.Format("{0} / {1} $", newLoan.AmountPaid, newLoan.Amount);
        LoanButtons.Add(newLoan, newLoanButton);
        LoanButtonsText.Add(newLoan, buttonText);
        LoanListView.AddControl(newLoanButton);

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
        Text loanText = LoanButtonsText[loan];
        loanText.text = string.Format("{0} / {1} $", loan.AmountPaid, loan.Amount);

        SetMoneyAmounts();

        if (null != SelectedLoan)
        {
            SetLoanInfo();
        }
    }

    private void OnLoanPaidOff(BankLoan loan)
    {
        GameObject loanButton = LoanButtons[loan];
        LoanListView.RemoveControl(loanButton);
        LoanButtonsText.Remove(loan);
        LoanButtons.Remove(loan);

        TakeLoanButton.interactable = true;
    }

    private void OnLoanButtonClicked()
    {
        //We know its worker list button because its clicked event has been just called
        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;

        if (selectedButton != LoanSelectedButton)
        {
            if (null != LoanSelectedButton)
            {
                Button previouslySelectedLoanButtonComponent = LoanSelectedButton.GetComponent<Button>();
                //Restore default values for button that was previously selected
                previouslySelectedLoanButtonComponent.colors = LoanButtonColors;
            }

            Button buttonComponent = selectedButton.GetComponent<Button>();
            ColorBlock buttonColors = buttonComponent.colors;
            LoanButtonColors = buttonColors;
            //We remember button as selected long as any other worker button
            //won't be selected. That's why color will always stay even when
            //button is not reckognized as selected anymore by unity UI engine
            buttonColors.normalColor = SELECTED_LOAN_BUTTON_COLOR;
            buttonColors.selectedColor = SELECTED_LOAN_BUTTON_COLOR;
            buttonComponent.colors = buttonColors;

            LoanSelectedButton = selectedButton;
            SelectedLoan = LoanButtons.First(x => x.Value == LoanSelectedButton).Key;

            SetLoanInfo();
        }
    }

    private void SetLoanInfo()
    {
        LoanAmountInputField.text = SelectedLoan.Amount.ToString();
        LoanPaymentsCountInputField.text = string.Format(
            "{0} / {1}", SelectedLoan.PaymentsPaid, SelectedLoan.PaymentsCount);
    }

    private void Start()
    {
        LoanButtons = new Dictionary<BankLoan, GameObject>();
        LoanButtonsText = new Dictionary<BankLoan, Text>();
        SetMoneyAmounts();
        UpdateLoanAmount(LoanAmountSlider.value);
        UpdateLoanPaymentsCount(LoanPaymentsCountSlider.value);
        BankComponent.LoanAdded += OnLoadAdded;
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

        if (BankComponent.Loans.Count==Bank.MAX_LOANS_COUNT)
        {
            TakeLoanButton.interactable = false;
        }
    }
}