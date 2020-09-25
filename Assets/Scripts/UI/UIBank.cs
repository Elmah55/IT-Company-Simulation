using ITCompanySimulation.Economy;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ITCompanySimulation.Core;

namespace ITCompanySimulation.UI
{
    public class UIBank : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private Bank BankComponent;
        [SerializeField]
        private TextMeshProUGUI TextLoanAmount;
        [SerializeField]
        private TextMeshProUGUI TextLoanPayments;
        [SerializeField]
        private TextMeshProUGUI TextSinglePayment;
        [SerializeField]
        private TextMeshProUGUI TextSliderLoanAmount;
        [SerializeField]
        private TextMeshProUGUI TextSliderPaymentsCount;
        [SerializeField]
        private Slider SliderLoanAmount;
        [SerializeField]
        private Slider SliderLoanPaymentsCount;
        [SerializeField]
        private MainSimulationManager SimulationManagerComponent;
        [SerializeField]
        private Button TakeLoanButton;
        [SerializeField]
        private ProgressBar ProgressBarLoan;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void SetLoanAmountText(BankLoan loan)
        {
            if (null != loan)
            {
                TextLoanAmount.gameObject.SetActive(true);
                TextLoanPayments.gameObject.SetActive(true);
                TextSinglePayment.gameObject.SetActive(true);

                TextLoanAmount.text = string.Format("Amount paid: {0} $ / {1} $",
                    BankComponent.Loan.AmountPaid,
                    BankComponent.Loan.Amount);

                TextLoanPayments.text = string.Format("Payments paid: {0} / {1}",
                    BankComponent.Loan.PaymentsPaid,
                    BankComponent.Loan.PaymentsCount);

                TextSinglePayment.text = string.Format("Single payment: {0}",
                    BankComponent.Loan.SinglePayment);
            }
            else
            {
                TextLoanAmount.gameObject.SetActive(false);
                TextLoanPayments.gameObject.SetActive(false);
                TextSinglePayment.gameObject.SetActive(false);
            }
        }

        private void OnLoadAdded(BankLoan newLoan)
        {
            newLoan.SinglePaymentPaid += OnLoanSinglePaymentPaid;
            newLoan.PaidOff += OnLoanPaidOff;
            TakeLoanButton.interactable = false;

            if (true == gameObject.activeSelf)
            {
                SetLoanAmountText(newLoan);
            }
        }

        private void OnLoanSinglePaymentPaid(BankLoan loan)
        {
            if (true == gameObject.activeSelf)
            {
                SetLoanAmountText(loan);
                ProgressBarLoan.Value = loan.AmountPaid;
            }
        }

        private void OnLoanPaidOff(BankLoan loan)
        {
            loan.PaidOff -= OnLoanPaidOff;
            loan.SinglePaymentPaid -= OnLoanSinglePaymentPaid;
            TakeLoanButton.interactable = true;
        }

        private void InitLoanSliders()
        {
            SliderLoanPaymentsCount.minValue = BankLoan.MIN_PAYMENTS_COUNT;
            SliderLoanPaymentsCount.maxValue = BankLoan.MAX_PAYMENTS_COUNT;
            SliderLoanAmount.minValue = BankComponent.MinLoanAmount;
            SliderLoanAmount.maxValue = BankComponent.MaxLoanAmout;
        }

        private void OnEnable()
        {
            SetLoanAmountText(BankComponent.Loan);
        }

        private void Start()
        {
            InitLoanSliders();
            UpdateLoanAmount(SliderLoanAmount.value);
            UpdateLoanPaymentsCount(SliderLoanPaymentsCount.value);
            BankComponent.LoanAdded += OnLoadAdded;
            ProgressBarLoan.MinimumValue = 0f;
            ProgressBarLoan.MaximumValue = 0f;
        }

        private void UpdateLoanAmount(float value)
        {
            int actualLoanAmount = BankComponent.CalculateLoanAmountWithInterest((int)value);
            string loanAmountText = string.Format("Amount: {0} $ ({1} $ to pay off)", (int)value, actualLoanAmount);
            TextSliderLoanAmount.text = loanAmountText;
            UpdateLoanPaymentsCount(SliderLoanPaymentsCount.value);
        }

        private void UpdateLoanPaymentsCount(float value)
        {
            int valueInt = (int)value;
            int actualLoanAmount = BankComponent.CalculateLoanAmountWithInterest((int)SliderLoanAmount.value);
            int singlePayment = (int)Mathf.Ceil(actualLoanAmount / valueInt);
            string loanPaymentsText = string.Format("Payments: {0} ({1} $ / Month)", valueInt, singlePayment);
            TextSliderPaymentsCount.text = loanPaymentsText;
        }

        /*Public methods*/

        public void OnTakeLoanButtonClicked()
        {
            BankComponent.TakeLoan((int)SliderLoanAmount.value, (int)SliderLoanPaymentsCount.value);
            TakeLoanButton.interactable = false;
            ProgressBarLoan.MaximumValue = BankComponent.Loan.Amount;
        }

        public void OnSliderLoanAmountValueChanged(float value)
        {
            UpdateLoanAmount(value);
        }

        public void OnSliderPaymentsCountValueChanged(float value)
        {
            UpdateLoanPaymentsCount(value);
        }
    }
}