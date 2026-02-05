using System;

namespace SoftOne.Soe.Business.Core.PaymentIO.TotalIn
{

    public class TotalInFile
    {
        #region Members

        public DateTime PaidDate { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyAccount { get; set; }

        public decimal Amount { get; set; }
        public bool IsCredit { get; set; }

        public string Reference { get; set; }
        public string CustomerName { get; set; }
        #endregion

        #region Constructors
        public TotalInFile(DateTime paidDate, decimal amount, string reference, string currenyCode, string customerName, bool isCredit = false)
        {
            PaidDate = paidDate;
            if (isCredit)
                Amount = -amount;
            else
                Amount = amount;
            Reference = reference;
            CurrencyCode = currenyCode;
            CustomerName = customerName;
            IsCredit = isCredit;
        }

        #endregion

    }
}