using UnityEngine;
using UnityEngine.UI;

public class UIGameBank : MonoBehaviour
{
    public Text LoanAmountText;

    public void UpdateLoanAmount(float value)
    {
        string loanAmountText = string.Format("Amount: {0}$", (int)value);
        LoanAmountText.text = loanAmountText;
    }
}
