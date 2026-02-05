using System;
using System.Collections.Generic;
using System.Text;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Core.PaymentIO.Cfp
{
    #region File - Level 0
    public class CfpFile
    {
        //public OpeningPost OpeningPost { get; set; }
        public List<Section> Sections { get; set; }
        public bool IsValid()
        {

            foreach (Section section in Sections)
            {
                if (!section.IsValid())
                    return false;
            }
            return true;
        }
        public CfpFile()
        {
            Sections = new List<Section>();
        }
    }
    #endregion

    #region Section - Level 1
    public class Section
    {
        public SenderPost SenderPost { get; set; }
        public SummaryPost SummaryPost { get; set; }
        public List<IPost> Posts { get; set; }
        public bool IsValid()
        {
            if (!SenderPost.IsValid())
                return false;
            if (!SummaryPost.IsValid())
                return false;

            foreach (IPost post in Posts)
            {
                if (!post.IsValid())
                    return false;
            }
            return true;
        }
        public Section()
        {
            Posts = new List<IPost>();
        }
    }
    #endregion

    #region Posts - Level 3



    public class SenderPost : IPost
    {
        #region Members
        public string PostType { get; set; }
        public string CustomerNumber { get; set; }
        public string SenderAccount { get; set; } //could be made int
        private string currencyCodePocket;
        public string CurrencyCodePocket
        {
            get { return currencyCodePocket.ToUpper(); }
            set { currencyCodePocket = value; }
        }

        private string currencyCode;
        public string CurrencyCode
        {
            get { return currencyCode.ToUpper(); }
            set { currencyCode = value; }
        }
        #endregion

        #region Constructors
        public SenderPost()
        {
            PostType = Utilities.PO3_OPENING_RECORD_TYPE;
        }

        public SenderPost(string customerNumber, string senderAccount, string currencyPocket, string currencyCode)
        {
            PostType = Utilities.PO3_OPENING_RECORD_TYPE;
            CustomerNumber = customerNumber;
            SenderAccount = senderAccount;
            currencyCodePocket = currencyPocket;
            this.currencyCode = currencyCode;
        }
        #endregion

        #region Help methods

        public bool IsValid()
        {
            return PostType == Utilities.PO3_OPENING_RECORD_TYPE;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(Utilities.PO3_OPENING_RECORD_TYPE);               // 1-4 = 4
            sb.Append(' ', 8);                                          // 5-12 = 8
            sb.Append(CustomerNumber.PadRight(10));                     // 13-22 = 10
            sb.Append(' ', 12);                                         // 23-34 = 12
            sb.Append(SenderAccount.PadRight(10));                      // 35-44 = 10
            sb.Append(currencyCode.PadRight(3));                        // 45-47 = 3
            sb.Append(' ', 6);                                          // 48-53 = 6
            sb.Append(currencyCodePocket.PadRight(3));                  // 54-57 = 3
            sb.Append(' ', 24);                                         // 58-80 = 24
            return sb.ToString();
        }
        #endregion
    }

    public class SummaryPost : IPost
    {
        #region Members
        public string PostType { get; set; }
        public long TotalAmount { get; set; }
        public int TotalNumberOfPosts { get; set; }
        #endregion

        #region Constructors
        public SummaryPost(string item)
        {
            PostType = Utilities.PO3_CLOSING_RECORD_TYPE;
            TotalAmount = Utilities.GetNumeric(item, 18, 13);
            TotalNumberOfPosts = Utilities.GetNumeric(item, 31, 6);
        }

        public SummaryPost(long totalAmount, int totalNumberOfPosts)
        {
            PostType = Utilities.PO3_CLOSING_RECORD_TYPE;
            TotalAmount = totalAmount;
            TotalNumberOfPosts = totalNumberOfPosts;
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return PostType == Utilities.PO3_CLOSING_RECORD_TYPE;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(Utilities.PO3_CLOSING_RECORD_TYPE);                // 1-4 = 4
            sb.Append(' ', 25);                                          // 5-29 = 25
            sb.Append(TotalNumberOfPosts.ToString("D7"));                // 30-36 = 7
            sb.Append(TotalAmount.ToString("D15"));                      // 37-51 = 15
            sb.Append(' ', 29);                                          // 52-80 = 29
            return sb.ToString();
        }
        #endregion
    }


    public class MessagePost : IPost
    {
        #region Members
        public string PostType { get; set; }
        public string SenderReference { get; set; }
        #endregion

        #region Constructors

        public MessagePost(string item)
        {
            PostType = Utilities.PO3_SENDERNOTES_RECORD_TYPE;
            SenderReference = item;
        }

        public MessagePost(int seqNr)
        {
            PostType = Utilities.PO3_SENDERNOTES_RECORD_TYPE;
            SenderReference = seqNr.ToString();
        }

        #endregion

        #region Help methods
        public bool IsValid()
        {
            if (PostType != Utilities.PO3_SENDERNOTES_RECORD_TYPE)
                return false;
            return true;
        }
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(Utilities.PO3_SENDERNOTES_RECORD_TYPE);   // 1-4 = 4
            sb.Append(' ', 27);                                 // 5-31 = 27
            sb.Append(SenderReference.PadRight(35));            // 32-66 = 35
            sb.Append(' ', 14);                                 // 67-80 = 14
            return sb.ToString();
        }
        #endregion
    }

    public class AmountPost : IPost
    {
        #region Members
        public string PostType { get; set; }
        public int PaymentMethod { get; set; }
        public string ReceiverAccountIdentity { get; set; }
        public string Message { get; set; }
        public long Amount { get; set; }
        public DateTime AccountDate { get; set; }
        public string CurrencyCode { get; set; }
        public int AccountingNumber { get; set; }

        #endregion

        #region Constructors
        public AmountPost() { }
        public AmountPost(string item)
        {
            int seqnr = 0;
            PostType = Utilities.PO3_PAYMENT_RECORD_TYPE;
            PaymentMethod = Utilities.GetNumeric(item, 3, 1);
            ReceiverAccountIdentity = item.Substring(4, 10);
            Message = item.Substring(14, 27);
            Amount = Utilities.GetNumeric(item, 41, 11);
            AccountDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.PG, item.Substring(52, 6));
            AccountingNumber = Utilities.GetNumeric(item, 58, 16);
            CurrencyCode = item.Substring(74, 3);
            int.TryParse(item.Substring(60, 19), out seqnr);
        }



        public AmountPost(int paymentMethod, string currencyCode, string paymentNumber, string message, long amount, DateTime date)
        {
            PostType = Utilities.PO3_PAYMENT_RECORD_TYPE;
            PaymentMethod = paymentMethod;
            CurrencyCode = currencyCode;
            ReceiverAccountIdentity = paymentNumber.ToString(); //paymentnr
            Message = message; //fakturanr
            Amount = amount;
            AccountDate = date;
        }
        #endregion

        #region Help methods

        public override string ToString()
        {
            string clearingNr = String.Empty;
            string bankAccount = String.Empty;
            string reference = String.Empty;
            if (PaymentMethod == 9)                                                 //Bank
            {
                if (ReceiverAccountIdentity.StartsWith("8"))                        //Swedbank
                {
                    clearingNr = ReceiverAccountIdentity.Left(5);
                    bankAccount = ReceiverAccountIdentity.Substring(5).PadRight(11);
                }
                else
                {
                    clearingNr = ReceiverAccountIdentity.Left(4) + " "; 
                    bankAccount = ReceiverAccountIdentity.Substring(4).PadRight(11);
                }
                reference = Message.PadRight(20) + "     ";
            }
            else
            {
                clearingNr = "     ";
                bankAccount = ReceiverAccountIdentity.PadRight(11);
                reference = Message.PadRight(25);
            }
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(Utilities.PO3_PAYMENT_RECORD_TYPE);                           // 1-4 = 4
            sb.AppendFormat(PaymentMethod.ToString("D2"));                          // 5-6 = 2
            sb.Append(clearingNr);                                                  // 7-11 = 5
            sb.Append(bankAccount);                                                 // 12-22 = 11
            sb.Append(' ', 2);                                                      // 23-24 = 2
            sb.Append(AccountDate.ToString("yyyyMMdd"));                            // 25-32 = 8
            sb.Append(Amount.ToString("D13"));                                      // 33-45 = 13
            sb.Append(Message.PadRight(25));                                        // 46-70 = 25
            sb.Append(' ', 10);                                                     // 75-80 = 5
            return sb.ToString();
        }
        public bool IsValid()
        {
            return PostType == Utilities.PO3_PAYMENT_RECORD_TYPE;
        }
        #endregion Help methods
    }

    public class AmountPostDebit : IPost
    {
        #region Members
        public string PostType { get; set; }
        public string Message { get; set; }
        public long Amount { get; set; }
        public string SenderReference { get; set; }
        #endregion

        #region Constructors

        public AmountPostDebit(string message, long amount, string senderReference)
        {
            PostType = Utilities.PO3_DEBIT_RECORD_TYPE;
            Message = message; //fakturanr
            Amount = amount;
            SenderReference = senderReference;
        }

        #endregion

        #region Help methods
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(Utilities.PO3_DEBIT_RECORD_TYPE);                           // 1-4 = 4
            sb.Append(Message.PadRight(35));                                      // 5-39 = 35
            sb.Append(Amount.ToString("D13"));                                    // 40-52 = 13
            sb.Append(SenderReference.PadRight(28));                              // 53-80 = 28
            return sb.ToString();
        }
        public bool IsValid()
        {
            return true;
        }

        #endregion
    }

    public class AmountPostCredit : IPost
    {
        #region Members
        public string PostType { get; set; }
        public long Amount { get; set; }
        public string Message { get; set; }
        public string SenderReference { get; set; }
        #endregion

        #region Constructors

        public AmountPostCredit(string message, long amount, string senderReference)
        {
            PostType = Utilities.PO3_CREDIT_RECORD_TYPE;
            Message = message; //fakturanr
            Amount = amount;
            SenderReference = senderReference;
        }

        #endregion

        #region Help methods
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(Utilities.PO3_CREDIT_RECORD_TYPE);                           // 1-4 = 4
            sb.Append(Message.PadRight(35));                                      // 5-39 = 35
            sb.Append(Amount.ToString("D13"));                                    // 40-52 = 13
            sb.Append(SenderReference.PadRight(28));                              // 53-80 = 28
            return sb.ToString();
        }
        public bool IsValid()
        {
            return true;
        }

        #endregion
    }

    #endregion



    #region Interfaces
    public interface IPost
    {
        string PostType { get; set; }
        bool IsValid();
    }

    #endregion

    #region DA1
    public class DA1Post
    {
        // Posttyp MH01/MH10
        public DateTime PayDate { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyAccount { get; set; }
        // Posttyp DR00
        public string ClearingNr { get; set; }
        public string ReceiverBankAccount { get; set; }
        public decimal Amount { get; set; }
        public bool IsCredit { get; set; }
        // Posttyp BA00
        public string Reference { get; set; }
        // Posttyp BE01
        public string Message { get; set; }
        public bool ReferenceIsInvoiceNr { get; set; }
        // Posttyp CNDB

        public DA1Post(DateTime payDate, decimal amount, string reference, string currenyCode, string message, bool isCredit = false, bool referenceIsInvoiceNr = false)
        {
            PayDate = payDate;
            if (isCredit)
                Amount = -amount;
            else
                Amount = amount;
            Reference = reference.Trim();
            CurrencyCode = currenyCode;
            IsCredit = isCredit;
            Message = message;
            ReferenceIsInvoiceNr = referenceIsInvoiceNr;
        }

    }
    #endregion
}