using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPAV3
{
    public enum AccountTransactionTypeIndicator {Unkown = 0, DBIT = 1, CRDT = 2 }
    public class SEPAFile
    {
        #region Members

        public DateTime? PaidDate { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyAccount { get; set; }

        public decimal Amount { get; set; }
        public bool IsCredit { get; set; }
        public string InvoiceNr { get; set; }
        public string Reference { get; set; }
        public string Name { get; set; }
        public string EndToEndId { get; set; }
        public string OCR { get; set; }
        public string BaseCurrency { get; set; }
        public decimal BaseCurrencyAmount { get; set; }
        public AccountTransactionTypeIndicator AccountTransactionType { get; set; }
        public SEPABankTransactionCode BankTransactionCode { get; set; }
        

        #endregion

        #region Constructors
        public SEPAFile(DateTime? paidDate, decimal amount, string invoiceNr, string currenyCode, string name, bool isCredit, string endToEndId, string reference, AccountTransactionTypeIndicator accountTransactionType, SEPABankTransactionCode transactionCode, string OCRnr = null)
        {
            PaidDate = paidDate;
            if (isCredit)
                Amount = -amount;
            else
                Amount = amount;
            Reference = reference;
            CurrencyCode = currenyCode;
            Name = name;
            IsCredit = isCredit;
            EndToEndId = endToEndId;
            InvoiceNr = invoiceNr;
            OCR = OCRnr;
            AccountTransactionType = accountTransactionType;
            BankTransactionCode = transactionCode;
        }

        public SEPAFile(DateTime? paidDate, decimal amount, string invoiceNr, string currenyCode, string name, string endToEndId)
        {
            PaidDate = paidDate;
            Amount = amount;
            Reference = "";
            CurrencyCode = currenyCode;
            Name = name;
            IsCredit = amount < 0;
            EndToEndId = endToEndId;
            InvoiceNr = invoiceNr;
            AccountTransactionType = AccountTransactionTypeIndicator.Unkown;
        }
        #endregion
    }

    public class SEPAMessageStatus
    {
        public string OrgMessageId { get; set; }
        public string MessageStatus { get; set; }
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

    public class SEPAAccountOnboarding
    {
        public bool Connected { get; set; }
        public string CompanyName { get; set; }
        public string OrgNr { get; set; }
        public string BIC { get; set; }
        public string IBAN { get; set; }
        public string BGNR { get; set; }
        public string Currency { get; set; }
    }
    public class SEPABankTransactionCodeDomain
    {
        public string Code { get; set; }
        public SEPABankTransactionCodeDomain(string code)
        {
            Code = code;
        }
        public bool IsPayment()
        {
            return Code == "PMNT";
        }
    }
    public class SEPABankTransactionCodeFamily
    {
        public string Code { get; set; }
        public SEPABankTransactionCodeFamily(string code)
        {
            Code = code;
        }
        public bool IsReceivedDirectDebit()
        {
            return Code == "RDDT";
        }
        public bool IsIssuedDirectDebit()
        {
            return Code == "IDDT";
        }
        public bool IsNotAvailable() {
            return Code == "NTAV";
        }
    }
    public class SEPABankTransactionCodeSubFamily
    {
        public string Code { get; set; }
        public SEPABankTransactionCodeSubFamily(string code)
        {
            Code = code;
        }
        public bool IsNotAvailable()
        {
            return Code == "NTAV";
        }
    }
    public class SEPABankTransactionCode
    {
        //Check this for more info (page 5):
        //https://www.handelsbanken.com/tron/xgpu/info/contents/v1/document/72-111397

        public SEPABankTransactionCodeDomain Domain { get; set; }
        public SEPABankTransactionCodeFamily Family { get; set; }
        public SEPABankTransactionCodeSubFamily SubFamily { get; set; }

        public SEPABankTransactionCode(string domain, string family, string subFamily)
        {
            Domain = new SEPABankTransactionCodeDomain(domain);
            Family = new SEPABankTransactionCodeFamily(family);
            SubFamily = new SEPABankTransactionCodeSubFamily(subFamily);
        }
        public bool IsReceivedDirectDebit()
        {
            //Supplier AUTOGIRO
            //RDDT = Received Direct Debit
            return Domain.IsPayment() && Family.IsReceivedDirectDebit() && SubFamily.IsNotAvailable();
        }
        public bool IsIssuedDirectDebit()
        {
            //Customer AUTOGIRO
            //IDDT = Issued Direct Debit
            return Domain.IsPayment() && Family.IsIssuedDirectDebit() && SubFamily.IsNotAvailable();
        }
    }
}