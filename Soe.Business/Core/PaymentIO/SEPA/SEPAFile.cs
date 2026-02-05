using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPA
{

    public class SEPAFile
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
        public SEPAFile(DateTime paidDate, decimal amount, string reference, string currenyCode, string customerName, bool isCredit = false)
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

    public class SEPAMessageStatus
    {
        public string OrgMessageId { get; set; }
        public string MessageStatus { get; set; }
        public string MessageText { get; set; }
        public List<SEPATransactionStatus> PaymentStatuses { get; }
        public SEPAMessageStatus()
        {
            PaymentStatuses = new List<SEPATransactionStatus>();
        }
    }

    public class SEPATransactionStatus
    {
        public string OrgPaymentId { get; set; }
        public string Status { get; set; }
        public string OrgEndToEndId { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
    }
}