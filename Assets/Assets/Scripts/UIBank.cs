using UnityEngine;
using UnityEngine.UI;

public class UIBank : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    /*Public consts fields*/

    /*Public fields*/

    public Text LoanAmountText;
    public Slider LoanAmountSlider;
    public Bank BankComponent;
    public MainSimulationManager SimulationManagerComponent;
    public InputField CompanyBalanceInputField;
    public InputField AmountToPayInputField;

    /*Private methods*/

    private void OnLoanPaid()
    {
        SetMoneyAmounts();
    }

    private void SetMoneyAmounts()
    {
        CompanyBalanceInputField.text =
            SimulationManagerComponent.ControlledCompany.Balance.ToString() + " $";
        AmountToPayInputField.text = BankComponent.AmountToPayOff.ToString() + " $";
    }

    private void Start()
    {
        BankComponent.LoanPaid += OnLoanPaid;
        SetMoneyAmounts();
    }

    /*Public methods*/

    public void UpdateLoanAmount(float value)
    {
        string loanAmountText = string.Format("Amount: {0}$", (int)value);
        LoanAmountText.text = loanAmountText;
    }

    public void OnLoanButtonClicked()
    {
        BankComponent.TakeLoan((int)LoanAmountSlider.value);
        SetMoneyAmounts();
    }
}