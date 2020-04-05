using UnityEngine;

public class BankLoan
{
    /*Private consts fields*/

    /*Private fields*/

    private int m_AmountPaid;

    /*Public consts fields*/

    public const int MIN_PAYMENTS_COUNT = 12;
    public const int MAX_PAYMENTS_COUNT = 60;

    /*Public fields*/

    /// <summary>
    /// How much money player has to pay
    /// to bank
    /// </summary>
    public int Amount { get; private set; }
    /// <summary>
    /// How much money was already paid
    /// </summary>
    public int AmountPaid
    {
        get
        {
            return m_AmountPaid;
        }

        set
        {
            m_AmountPaid = value;
            SinglePaymentPaid?.Invoke(this);

            if (m_AmountPaid == Amount)
            {
                PaidOff?.Invoke(this);
            }
        }
    }
    /// <summary>
    /// How many payments should be made to pay off
    /// this loan
    /// </summary>
    public int PaymentsCount { get; set; }
    /// <summary>
    /// How many payments have been paid already
    /// </summary>
    public int PaymentsPaid { get; set; }
    /// <summary>
    /// How much of loan will be paid with
    /// one payment
    /// </summary>
    public int SinglePayment { get; private set; }
    public bool IsPaidOff
    {
        get
        {
            return Amount == AmountPaid;
        }
    }
    /// <summary>
    /// Single payment of loan occured
    /// </summary>
    public event LoanAction SinglePaymentPaid;
    /// <summary>
    /// Loan has been just paid off
    /// </summary>
    public event LoanAction PaidOff;

    /*Private methods*/

    /*Public methods*/

    public BankLoan(int amount, int paymentsCount)
    {
        this.Amount = amount;
        this.PaymentsCount = paymentsCount;

        int singlePayment = amount / paymentsCount;
        this.SinglePayment = singlePayment;
    }
}
