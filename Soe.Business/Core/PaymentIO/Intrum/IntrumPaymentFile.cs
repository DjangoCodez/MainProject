using System;

namespace SoftOne.Soe.Business.Core.PaymentIO.Intrum
{

    public class IntrumPaymentFile
    {
        #region Members

        public DateTime PaidDate { get; set; }
        public string CurrencyCode { get; set; }

        public decimal Amount { get; set; }

        public string InvoiceNr { get; set; }
        public string CustomerNr { get; set; }
        public string Reference { get; set; }
        public int PaymentTransactionType { get; set; }
        


        #endregion

        #region Constructors
        public IntrumPaymentFile(DateTime paidDate, decimal amount, string invoiceNr, string reference, string currenyCode, string customerNr, int paymentTransactionType)
        {
            PaidDate = paidDate;
            Amount = amount;
            Reference = reference;
            InvoiceNr = invoiceNr;
            CurrencyCode = currenyCode;
            CustomerNr = customerNr;
            PaymentTransactionType = paymentTransactionType;
        }

        #endregion

    }
}