using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Business.Core.PaymentIO.Lb
{
    public class LbFile
    {
        #region Members
        
        public List<ILbFileContent> contents;   
        #endregion 

        #region Constructors
        public LbFile()
        {
            contents = new List<ILbFileContent>();
        }
        #endregion

        #region Help methods
        /// <summary>
        /// Validate if the files content meets the structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            //1. at least one section
            if (contents == null || contents.Count < 1)
                return false; 
            //2. recursive validation check
            foreach (ILbFileContent content in contents)
            {
                if (!content.IsValid())                
                    return false;
            }
            return true;
        }
        #endregion
    }

    #region File Sections
    public class LbCorrectionSection:ILbSection
    {
        #region Members
        public List<ILbFileContent> Posts { get; set; }
        #endregion

        #region Constructors
        public LbCorrectionSection()
        {
            Posts = new List<ILbFileContent>();
        }
        #endregion

        #region Help methods
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }
        #endregion
    }
    public class LbPaymentSection:ILbSection
    {
        #region Members
        public List<ILbFileContent> Posts { get; set; }
        #endregion

        #region Constructors
        public LbPaymentSection()
        {
            Posts = new List<ILbFileContent>();
        }
        #endregion

        #region private help methods
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            bool openingPostDefinesDate = false;
            if (Posts[0] is LbDomesticOpeningPost && !string.IsNullOrEmpty(((LbDomesticOpeningPost)Posts[0]).PaymentDate.Trim())) //opening post has payDate
                openingPostDefinesDate = true;

            foreach(ILbFileContent content in Posts) 
            {
                if (content is LbDomesticPostGroup)
                {
                    if (!((LbDomesticPostGroup)content).IsValid(openingPostDefinesDate))
                        return false;
                }
                else
                {
                    if (!content.IsValid())
                        return false;
                }
            }                    
            return true;
        }
        #endregion
    }
    #endregion

    #region File Posts
    
    #region Correction
    public class LbCorrectionPost :ILbFileContent
    {
        #region Members
        public string TransactionCode { get; set; }
        public int CorrectionCode {get;set;}
        public int SenderCustomerNumber{get;set;}
        public int SenderBgCode{get;set;}
        public int ReceiverBgCode{get;set;}
        public string Date{get;set;}
        public string NewDate{get;set;}
        public int Amount{get;set;}
        #endregion
        #region Constructors
        public LbCorrectionPost(int correctionCode, int senderCustomerNr, int senderBGC, int receiverBGC, string date, string newDate, int amount )
        {
            TransactionCode = Utilities.TRANSACTION_CODE_CORRECTION;
            CorrectionCode = correctionCode;
            SenderCustomerNumber = senderCustomerNr;
            SenderBgCode = senderBGC;
            ReceiverBgCode = receiverBGC;
            Date = date;
            NewDate = newDate;
            Amount = amount;
        }
        public LbCorrectionPost(string item)
        {
            TransactionCode = Utilities.TRANSACTION_CODE_CORRECTION;
            CorrectionCode = Utilities.GetNumeric(item,2, 2);
            SenderCustomerNumber = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item),4, 6);
            SenderBgCode = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item),10, 10);
            ReceiverBgCode = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item),20, 10);
            Date = item.Substring(30,6);
            NewDate = item.Substring(36,6);
            Amount = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item),42, 12);
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(TransactionCode);
            sb.Append(Utilities.AddLeadingZeroes(CorrectionCode.ToString(),2));
            sb.Append(Utilities.AddLeadingZeroes(SenderCustomerNumber.ToString(), 6));
            sb.Append(Utilities.AddLeadingZeroes(SenderBgCode.ToString(), 10));
            sb.Append(Utilities.AddLeadingZeroes(ReceiverBgCode.ToString(), 10));
            sb.Append(Utilities.AddPadding(Date, 6));
            sb.Append(Utilities.AddPadding(NewDate, 6));
            sb.Append(Utilities.AddLeadingZeroes(Amount.ToString(), 12));
            return sb.ToString();
        }
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }
        #endregion
    }
    #endregion

    #region Domestic
    public class LbDomesticSummaryPost:ILbPost
    {
        #region Members
        public bool IsExport{get;set;}
        public int TransactionCode { get; set; }
        public int BgCode { get; set; }
        public int NumberOfPaymentPosts { get; set; }
        public long TotalAmount { get; set; }
        public string NegateAmount { get; set; }
        public string ReservField { get; set; }
        #endregion
        #region Constructors
        public LbDomesticSummaryPost(int bgCode, int numberOfPaymentPosts, long totalAmount, string negateAmount)
        {
            IsExport = true;
            TransactionCode = (int)LbTransactionCodeDomestic.SummaryPost;
            BgCode = bgCode;
            NumberOfPaymentPosts = numberOfPaymentPosts; 
            TotalAmount = totalAmount;
            NegateAmount = negateAmount;
        }
        public LbDomesticSummaryPost(string item)
        {
            TransactionCode = (int)LbTransactionCodeDomestic.SummaryPost;
            BgCode = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item),2, 10);
            NumberOfPaymentPosts = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item),12,8);
            TotalAmount = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item),20,12);
            NegateAmount = item.Substring(32,1);
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(TransactionCode);
            sb.Append(Utilities.AddLeadingZeroes(BgCode.ToString(), 10));
            sb.Append(Utilities.AddLeadingZeroes(NumberOfPaymentPosts.ToString(), 8));
            sb.Append(Utilities.AddLeadingZeroes(TotalAmount > 0 ? TotalAmount.ToString() : Decimal.Negate(TotalAmount).ToString(), 12));
            sb.Append(Utilities.AddPadding(NegateAmount, 1));
            sb.Append(Utilities.AddPadding(string.Empty, 47));
            return sb.ToString();
        }
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }
        #endregion
    }
    public class LbDomesticOpeningPost:ILbPost
    {
        #region Members
        public bool IsExport{get;set;}
        public int TransactionCode { get; set; }
        public int BgCode { get; set; }
        public string CreatedDate { get; set; }
        public string CurrencyCode { get; set; }
        public string PaymentDate { get; set; }
        public string ReservField1 { get; set; }
        public string ReservField2 { get; set; }
        public int AccountingCode { get; set; }
        #endregion
        #region Constructors
        public LbDomesticOpeningPost(int bgCode, string createdDate, string paymentDate, string currencyCode)
        {
            IsExport = true;
            TransactionCode = (int)LbTransactionCodeDomestic.OpeningPost;
            BgCode = bgCode;
            CreatedDate = createdDate;
            PaymentDate = paymentDate;
            CurrencyCode = currencyCode;
        }
        public LbDomesticOpeningPost(string item)
        {
            TransactionCode = (int)LbTransactionCodeDomestic.OpeningPost;
            BgCode = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item), 2, 10);
            CreatedDate = item.Substring(12, 6);
            PaymentDate = item.Substring(40, 6);
            AccountingCode = Utilities.GetNumeric(item,46, 1);
            CurrencyCode = item.Substring(59, 3); 
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (!Utilities.IsNumeric(CreatedDate))
                return false;

            if (CurrencyCode != TermGroup_Currency.SEK.ToString() && CurrencyCode != TermGroup_Currency.EUR.ToString() && CurrencyCode.Trim() != Utilities.CURRENCY_EMPTY)
                return false;

            return PaymentDate.Trim() == Utilities.DATE_EMPTY || PaymentDate == Utilities.DATE_SEND_NOW || Utilities.IsNumeric(PaymentDate);
        }
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);

            sb.Append((int)LbTransactionCodeDomestic.OpeningPost);
            sb.Append(Utilities.AddLeadingZeroes(BgCode.ToString(), 10));
            sb.Append(CreatedDate);
            sb.Append(Utilities.POST_PRODUCT_NAME_LB);
            sb.Append(Utilities.AddPadding(PaymentDate, 6));
            sb.Append(Utilities.AddPadding(string.Empty, 13)); 
            sb.Append(Utilities.AddPadding(CurrencyCode, 3));
            sb.Append(Utilities.AddPadding(string.Empty, 18));

            return sb.ToString();
        }
        #endregion
    }
    public class LbDomesticPaymentPost : ILbPostGroupItem
    {
        #region Members
        public bool IsExport { get; set; }
        public bool CheckNum { get;set;}
        public int TransactionCode { get; set; }
        public string InvoiceNumber { get; set; }
        public long Amount { get; set; }
        public int BgCode { get; set; }
        public string Date { get; set; }
        public string ReservField { get; set; }
        public string Information { get; set; }
        public string PaymentTypeCode { get; set; }
        public int ReferalBankAccountNumber { get; set; }
        #endregion
        #region Constructors

        /// <summary>
        /// init for export
        /// </summary>
        /// <param name="transactionCode"></param>
        /// <param name="bgCode"></param>
        /// <param name="invoiceNumber"></param>
        /// <param name="amount"></param>
        /// <param name="date"></param>
        public LbDomesticPaymentPost(int transactionCode, int bgCode, string invoiceNumber, long amount, string date, string information, bool checkNum = false)
        {
            IsExport = true;
            TransactionCode = transactionCode;
            BgCode = bgCode;
            InvoiceNumber = invoiceNumber;
            Amount = amount;
            Date = date;
            Information = information;
            CheckNum = checkNum;
        }

        /// <summary>
        /// init for import
        /// </summary>
        /// <param name="item"></param>
        public LbDomesticPaymentPost(string item) 
        {
            TransactionCode = Utilities.GetNumeric(item,0,2);
            if(TransactionCode != (int)LbTransactionCodeDomestic.PlusgiroPost)
                CheckNum = Utilities.HasCheckNumber(item.Substring(2, 10)); 
            BgCode = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item), 2, 10);
            InvoiceNumber = item.Substring(12, 25);
            Amount = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item), 37, 12);
            Information = item.Substring(60, 20);

            switch (TransactionCode) //TODO: this section needs to be verified against specification, example file differs
            { 
                case 14:
                case 15:
                case 54:
                    Date = item.Substring(49, 6); //added to parse example file ^^
                    PaymentTypeCode = item.Substring(49, 1);
                    ReferalBankAccountNumber = Utilities.GetNumeric(item,50,10);
                    break;
                case 16:
                case 17:
                    Date = item.Substring(49, 6); 
                    PaymentTypeCode = item.Substring(55, 1);
                    break;
            }
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(TransactionCode);
            sb.Append(Utilities.PaymentNumber(BgCode.ToString(), 10, CheckNum));
            sb.Append(Utilities.AddPadding(InvoiceNumber, 25,true));
            sb.Append(Utilities.AddLeadingZeroes(Amount.ToString(), 12));
            sb.Append(Utilities.AddPadding(Date, 6,true));
            sb.Append(Utilities.AddPadding(string.Empty, 5));
            sb.Append(Utilities.AddPadding(Information, 20,true));
            return sb.ToString();
        }
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            //if tk15 then paymenttypecode != p
            return true;
        }

        public int? GetInvoiceSeqNrFromInformation()
        {
            if (!string.IsNullOrEmpty(Information))
            {
                try
                {
                    var invoiceSeqNrStr = Information.Remove(Information.IndexOf(","));
                    int invoiceSeqNr;
                    if (int.TryParse(invoiceSeqNrStr, out invoiceSeqNr))
                    {
                        return invoiceSeqNr;
                    }
                }
                catch
                { 
                    //should not happen and if it happens the return will be null 
                }
            }

            return null;
        }

        #endregion
    }
    public class LbDomesticNamePost : ILbPostGroupItem
    {
        #region Members
        public bool IsExport { get; set; }
        public bool CheckNum { get; set; }
        public int TransactionCode { get; set; }
        public string ReservField { get; set; }
        public int BgCode { get; set; }
        private string recipientsName;
        public string RecipientsName 
        {
            get { return recipientsName.ToUpper(); }
            set { recipientsName = value; }
        }
        public string CoAddress { get; set; }
        #endregion
        #region Constructors
        public LbDomesticNamePost(int recipientsPaymentNumber, string recipientsName, string coAddress)
        {
            IsExport = true;
            TransactionCode = (int)LbTransactionCodeDomestic.NamePost;
            BgCode = recipientsPaymentNumber;
            RecipientsName = recipientsName;
            CoAddress = coAddress;
        }
        public LbDomesticNamePost(string item)
        {
            TransactionCode = (int)LbTransactionCodeDomestic.NamePost;
            CheckNum = Utilities.HasCheckNumber(item.Substring(2, 10));
            BgCode = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item),6, 6);
            RecipientsName = item.Substring(12, 35);
            CoAddress = item.Substring(47, 33);
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(TransactionCode);
            sb.Append(Utilities.AddLeadingZeroes(string.Empty, 4)); 
            sb.Append(Utilities.PaymentNumber(BgCode.ToString(), 6, CheckNum));
            sb.Append(Utilities.AddPadding(RecipientsName, 35,true));
            sb.Append(Utilities.AddPadding(CoAddress, 33,true)); 
            return sb.ToString();
        }
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return !String.IsNullOrEmpty(BgCode.ToString().TrimStart("0".ToCharArray()));
        }

        #endregion
    }
    public class LbDomesticCreditInvoicePreviousRedictionPost : ILbPostGroupItem
    {
        #region Members
        public bool IsExport { get; set; }
        public bool CheckNum { get; set; }
        public int TransactionCode {get;set;}
        public int BgCode {get;set;}
        public string FirstReductionDate{get;set;}
        public int FirstReductionAmount{get;set;}
        public string SecondReductionDate{get;set;}
        public int SecondReductionAmount{get;set;}
        public string ThirdReductionDate{get;set;}
        public int ThirdReductionAmount{get;set;}
        public string ReservField{get;set;}
        
        #endregion
        #region Constructors
        public LbDomesticCreditInvoicePreviousRedictionPost(string item)
        {
            TransactionCode = (int)LbTransactionCodeDomestic.CreditInvoicePreviousReductionPost;
            CheckNum = Utilities.HasCheckNumber(item.Substring(2,10));
            BgCode = Utilities.GetNumeric(item,2,10);

            FirstReductionDate = item.Substring(12,6);
            FirstReductionAmount = Utilities.GetNumeric(item,18,12);
            
            SecondReductionDate = item.Substring(30,6);
            SecondReductionAmount = Utilities.GetNumeric(item,36,12);

            ThirdReductionDate = item.Substring(48,6);
            ThirdReductionAmount = Utilities.GetNumeric(item,54, 12);
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }
        #endregion
    }
    public class LbDomesticCreditInvoiceRestPost : ILbPostGroupItem
    {
        #region Members
        public bool IsExport { get; set; }
        public bool CheckNum { get; set; }
        public int TransactionCode {get;set;}
        public int BgCode {get;set;}
        public string Date{get;set;}
        public int OriginalAmount{get;set;}
        public int RestAmount{get;set;}
        public string ReservField{get;set;}
        #endregion
        #region Constructors
        public LbDomesticCreditInvoiceRestPost(string item)
        {
            TransactionCode = (int)LbTransactionCodeDomestic.CreditInvoiceRestPost;
            CheckNum = Utilities.HasCheckNumber(item.Substring(2, 10));
            BgCode = Utilities.GetNumeric(item,2, 10);
            Date = item.Substring(12,6);
            OriginalAmount = Utilities.GetNumeric(item,18,12);
            RestAmount = Utilities.GetNumeric(item,30,12);
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }
        #endregion
    }
    public class LbDomesticAssignmentObservationPost
    {
        #region Members
        public bool IsExport { get; set; }
        public int TransactionCode {get;set;}
        public string ObservationDate {get;set;}
        public int SumInvoiceReductionPerDay {get;set;}
        public string Negation1 {get;set;}
        public int SumInvoicePerDay{get;set;}
        public string Negation2 {get;set;}
        public int CommentCode {get;set;}
        
        #endregion
        #region Constructors
        public LbDomesticAssignmentObservationPost(string item)
        {
            TransactionCode = (int)LbTransactionCodeDomestic.AssignmentObservationPost;
            ObservationDate = item.Substring(2,6);
            SumInvoiceReductionPerDay = Utilities.GetNumeric(item,14,13);
            Negation1 = item.Substring(27,1);
            SumInvoicePerDay = Utilities.GetNumeric(item,28,13);
            Negation2 = item.Substring(41,1);
            CommentCode = Utilities.GetNumeric(item,42,2);
        }
        #endregion
    }
    public class LbDomesticCommentPost :ILbPostGroupItem
    {
        public int BgCode { get; set; }
        public bool IsExport { get; set; }
        public int TransactionCode{get;set;}
        public string ErrorCode{get;set;}
        public string Comment{get;set;}
        public LbDomesticCommentPost(string item)
        {
            TransactionCode = (int) LbTransactionCodeDomestic.CommentPost;
            ErrorCode = item.Substring(2,8);
            Comment = item.Substring(10,70);
        }
        public bool IsValid()
        {
            //could handle error information here and save the error code??
            return true;
        }
    }
    public class LbDomesticAddressPost : ILbPostGroupItem
    {
        #region Members
        public bool IsExport { get; set; }
        public bool CheckNum { get; set; }
        public int TransactionCode { get; set; }
        public string ReservField1 { get; set; }
        public int BgCode { get; set; }
        private string recipientsAddress;
        private string recipientsCounty;
        public string RecipientsAddress 
        {
            get { return recipientsAddress.ToUpper(); }
            set { recipientsAddress = value; }
        }
        public string RecipientsPostalCode { get; set; }
        public string RecipientsCounty 
        {
            get { return recipientsCounty.ToUpper(); }
            set { recipientsCounty = value; }
        }
        public string ReservField2 { get; set; }
        #endregion
        #region Constructors
        public LbDomesticAddressPost(int recipientsPaymentNumber, string recipientsAddress, string recipientsPostalCode, string recipientsCounty)
        {
            IsExport = true;
            TransactionCode = (int) LbTransactionCodeDomestic.AddressPost;
            BgCode = recipientsPaymentNumber;
            RecipientsAddress = recipientsAddress;
            RecipientsPostalCode = recipientsPostalCode;
            RecipientsCounty = recipientsCounty;
        }
        public LbDomesticAddressPost(string item)
        {
            TransactionCode = (int)LbTransactionCodeDomestic.AddressPost;
            ReservField1 = item.Substring(2, 4);
            CheckNum = Utilities.HasCheckNumber(item.Substring(6, 6));
            BgCode = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item), 6, 6);
            RecipientsAddress = item.Substring(12,35);
            RecipientsPostalCode = item.Substring(47,5);
            RecipientsCounty = item.Substring(52,20);
            ReservField2 = item.Substring(72, 8);
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return !String.IsNullOrEmpty(BgCode.ToString().TrimStart("0".ToCharArray()));
        }

        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);

            sb.Append(TransactionCode);
            sb.Append(Utilities.PaymentNumber(BgCode.ToString(), 12, CheckNum));
            sb.Append(Utilities.AddPadding(RecipientsAddress, 35));
            sb.Append(Utilities.AddPadding(RecipientsPostalCode, 5));
            sb.Append(Utilities.AddPadding(RecipientsCounty, 20));
            sb.Append(Utilities.AddPadding(string.Empty, 8));

            return sb.ToString();
        }
        #endregion
    }
    public class LbDomesticAccountPost : ILbPostGroupItem
    {
        #region Members
        public bool IsExport { get; set; }
        public bool CheckNum { get; set; }
        public int TransactionCode { get; set; }
        public string ReservField1 { get; set; }
        public int BgCode { get; set; }
        public string ClearingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string ReferenceText { get; set; }
        
        string code;
        public string Code { 
            get
            {
                return code;
            } 
            set 
            {
                code = value=="L" ? value : string.Empty;
            }
        }
        public string ReservField2 { get; set; }
        #endregion
        #region Constructors
        public LbDomesticAccountPost(int bgCode, string clearingNumber, string accountNumber, string referenceText, bool isSalary, bool checkNum = false)
        {
            IsExport = true;
            TransactionCode = (int)LbTransactionCodeDomestic.AccountPost;
            BgCode = bgCode;
            ClearingNumber = clearingNumber;
            AccountNumber = accountNumber;
            ReferenceText = referenceText;
            Code = isSalary ? "L" : "";
            CheckNum = checkNum;
        }
        public LbDomesticAccountPost(string item)
        {
            TransactionCode = Utilities.GetNumeric(item,0,2);
            CheckNum = Utilities.HasCheckNumber(item.Substring(2, 10));
            BgCode = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item),2,10);
            ClearingNumber = item.Substring(12,4);
            AccountNumber = item.Substring(16,12);
            ReferenceText = item.Substring(28,12);
            Code = item.Substring(40,1);
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (String.IsNullOrEmpty(BgCode.ToString().TrimStart("0".ToCharArray())))
                return false;
            
            return Code == "L" || Code.Trim() == string.Empty;
        }
        /// <summary>
        /// Serialize object to LB format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);

            sb.Append(TransactionCode);
            sb.Append(Utilities.PaymentNumber(BgCode.ToString(), 10, CheckNum));
            sb.Append(Utilities.AddLeadingZeroes(ClearingNumber, 4));
            sb.Append(Utilities.AddLeadingZeroes(AccountNumber, 12));
            sb.Append(Utilities.AddPadding(ReferenceText, 12));
            sb.Append(Utilities.AddPadding(Code, 1));
            sb.Append(Utilities.AddPadding(string.Empty, 39));
            return sb.ToString();  
            }
        #endregion
    }
    public class LbDomesticPostGroup: ILbPostGroup
    {
        #region Members
        public List<ILbPost> Posts { get; set; }
        public int BgCode { get; set; }
        #endregion
        #region Constructors
        public LbDomesticPostGroup(int bgCode)
        {
            BgCode = bgCode;
            Posts = new List<ILbPost>();
        }
        #endregion
        #region Help methods
        /// <summary>
        /// validate payment group
        /// </summary>
        /// <returns></returns>
        public bool IsValid(bool ParentPostDefinesDates)
        {
            int bgCode = 0;
            foreach (ILbPostGroupItem item in Posts)
            {
                if (bgCode == 0)
                    bgCode = item.BgCode;
                else if (bgCode != item.BgCode)
                    return false;

                if (!item.IsValid())
                    return false;

                if (ParentPostDefinesDates || !(item is LbDomesticPaymentPost)) continue;
                if(string.IsNullOrEmpty(((LbDomesticPaymentPost)item).Date))
                    return false;
            }
            return true;
        }
        public bool IsValid()
        {
            return IsValid(false);
        }
        #endregion
    }
    #endregion

    #region Foreign
    public class LbForeignOpeningPost : ILbPost
    {
        #region Members
        public bool IsExport { get; set; }
        public int TransactionCode { get; set; }
        public int SenderBGC { get; set; }
        public string SenderCreateDate { get; set; }
        private string senderName;
        private string senderAddress;
        public string SenderName 
        {
            get { return senderName.ToUpper(); }
            set { senderName = value; }
        }
        public string SenderAddress 
        {
            get { return senderAddress.ToUpper(); }
            set { senderAddress = value; }
        }
        public string PayDate { get; set; }
        public int LayoutCode { get; set; }
        public string ReservField { get; set; }
        #endregion
        #region Constructors
        public LbForeignOpeningPost(int senderBgc, string senderCreateDate, string senderName, string senderAddress, string payDate, int layoutCode, string reservField)
        {
            IsExport = true;
            TransactionCode = (int)LbTransactionCodeForeign.OpeningPost;
            SenderBGC = senderBgc;
            this.senderAddress = senderAddress;
            SenderCreateDate = senderCreateDate;
            this.senderName = senderName;
            PayDate = payDate;
            LayoutCode = layoutCode;
            ReservField = reservField;
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(TransactionCode);
            sb.Append(Utilities.AddLeadingZeroes(SenderBGC.ToString(), 8));
            sb.Append(Utilities.AddPadding(SenderCreateDate,6));
            sb.Append(Utilities.AddPadding(SenderName,22));
            sb.Append(Utilities.AddPadding(SenderAddress,35));
            sb.Append(Utilities.AddPadding(PayDate, 6));
            sb.Append(Utilities.AddPadding(LayoutCode.ToString(), 1));
            sb.Append(Utilities.AddPadding(ReservField, 1));
            return sb.ToString();
        }
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }
        #endregion
    }
    public class LbForeignNamePost : ILbPost
    {
        #region Members
        public bool IsExport { get; set; }
        public int TransactionCode { get; set; }
        public int ReceiverNumber { get; set; }
        public string ReservField { get; set; }

        private string receiverName1;
        public string ReceiverName1 
        {
            get { return receiverName1.ToUpper(); }
            set { receiverName1 = value; }
        }

        private string receiverName2;
        public string ReceiverName2 
        {
            get { return receiverName2.ToUpper(); }
            set { receiverName2 = value; }
        }
        
        #endregion
        #region Constructors
        public LbForeignNamePost(int receiverNumber, string receiverName1, string receiverName2)
        {
            IsExport = true;
            TransactionCode = (int)LbTransactionCodeForeign.NamePost;
            ReceiverNumber = receiverNumber;
            this.receiverName1 = receiverName1;
            this.receiverName2 = receiverName2;
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(TransactionCode);
            sb.Append(Utilities.AddLeadingZeroes(ReceiverNumber.ToString(), 7));
            sb.Append(Utilities.AddPadding(ReceiverName1, 30));
            sb.Append(Utilities.AddPadding(ReceiverName2, 35));
            sb.Append(Utilities.AddPadding(string.Empty, 7));
            return sb.ToString();
        }
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }
        #endregion
    }
    public class LbForeignAddressPost : ILbPost
    {
        #region Members
        public bool IsExport { get; set; }
        public int TransactionCode { get; set; }
        public int ReceiverNumber { get; set; }
        private string receiverAddressStreet;
        private string receiverPostalAddressIncCountry;
        public string ReceiverAddressStreet 
        {
            get { return receiverAddressStreet.ToUpper(); }
            set { receiverAddressStreet = value; }
        }
        public string ReceiverPostalAddressIncCountry 
        {
            get { return receiverPostalAddressIncCountry.ToUpper(); }
            set { receiverPostalAddressIncCountry = value; }
        }
        public string CurrencyAccountCode { get; set; }
        public string RiksbankCountryCode { get; set; }
        public string ReservField2 { get; set; }
        public string DebitCode { get; set; }
        public string PaymentType { get; set; }
        public string PaymentMethod { get; set; }

        #endregion
        #region Constructors
        public LbForeignAddressPost(int receiverNumber, string receiverAddressStreet, string receiverPostalAddressIncCountry, string currencyAccountCode, string riksbankCountryCode, string debitCode, string paymentType, string paymentMethod)
        {
            IsExport = true;
            TransactionCode = (int)LbTransactionCodeForeign.AddressPost;
            ReceiverNumber = receiverNumber;            
            this.receiverAddressStreet = receiverAddressStreet;
            this.receiverPostalAddressIncCountry = receiverPostalAddressIncCountry;
            RiksbankCountryCode = riksbankCountryCode;
            DebitCode = debitCode;
            PaymentType = paymentType;
            PaymentMethod = paymentMethod;
            CurrencyAccountCode = currencyAccountCode;
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(TransactionCode);
            sb.Append(Utilities.AddLeadingZeroes(ReceiverNumber.ToString(), 7));
            sb.Append(Utilities.AddPadding(ReceiverAddressStreet, 30));
            sb.Append(Utilities.AddPadding(ReceiverPostalAddressIncCountry, 35)); 
            sb.Append(CurrencyAccountCode.Substring(0, 1)); 
            sb.Append(Utilities.AddPadding(RiksbankCountryCode, 2)); 
            sb.Append(Utilities.AddPadding(string.Empty, 1)); 
            sb.Append(Utilities.AddPadding(DebitCode, 1));
            sb.Append(Utilities.AddPadding(PaymentType, 1));
            sb.Append(Utilities.AddPadding(PaymentMethod, 1));
            return sb.ToString();
        }
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }
        #endregion
    }
    public class LbForeignBankPost : ILbPost
    {
        #region Members
        public bool IsExport { get; set; }
        public int TransactionCode { get; set; }
        public int ReceiverNumber { get; set; }
        public string IdentificationField1 { get; set; }
        public string IdentificationField2 { get; set; }
        public string IdentificationField3 { get; set; }
        #endregion
        #region Constructors
        public LbForeignBankPost(int receiverNumber, string identificationField1, string identificationField2, string identificationField3)
        {
            IsExport = true;
            TransactionCode = (int)LbTransactionCodeForeign.BankPost;
            ReceiverNumber = receiverNumber;
            IdentificationField1 = identificationField1;
            IdentificationField2 = identificationField2;
            IdentificationField3 = identificationField3;
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(TransactionCode);
            sb.Append(Utilities.AddLeadingZeroes(ReceiverNumber.ToString(), 7));
            sb.Append(Utilities.AddPadding(IdentificationField1, 12));
            sb.Append(Utilities.AddPadding(IdentificationField2, 30));
            sb.Append(Utilities.AddPadding(IdentificationField3, 30));
            return sb.ToString();
        }
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }
        #endregion
    }
    public class LbForeignPaymentPost : ILbPost
    {
        #region Members
        public bool IsExport { get; set; }
        public int TransactionCode { get; set; }
        public int ReceiverNumber { get; set; }
        public string PaymentSpecification { get; set; }
        public string CustomerAmount { get; set; }
        public string CurrencyTermAccountNumber { get; set; }
        public string CurrencyCode { get; set; }
        public string LastObservationDate { get; set; }
        public int Code { get; set; }
        public string ReservField1 { get; set; }
        public string InvoiceAmount { get; set; }
        public string RiksbankIdentificationCode { get; set; }
        public string ReservField2 { get; set; } 
        #endregion
        #region Constructors
        public LbForeignPaymentPost(int transactionCode, int receiverNumber, string paymentSpecification, long customerAmount, string currencyTermAccountNumber, string currencyCode,string lastObservationDate, int code, long invoiceAmount, string riksbankIdentificationCode, string reserv2)
        {
            IsExport = true;
            TransactionCode = transactionCode;
            ReceiverNumber = receiverNumber;
            PaymentSpecification = paymentSpecification;
            CustomerAmount = Utilities.ConvertAmountField(customerAmount);
            CurrencyTermAccountNumber = currencyTermAccountNumber;
            CurrencyCode = currencyCode;
            LastObservationDate = lastObservationDate;
            Code=code; //0 eller bokslutskod
            InvoiceAmount = Utilities.ConvertAmountField(invoiceAmount);
            RiksbankIdentificationCode = riksbankIdentificationCode;
            ReservField2 = reserv2;
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(TransactionCode);
            sb.Append(Utilities.AddLeadingZeroes(ReceiverNumber.ToString(), 7));
            sb.Append(Utilities.AddPadding(PaymentSpecification, 25));
            sb.Append(Utilities.AddLeadingZeroes(CustomerAmount, 11));
            sb.Append(Utilities.AddLeadingZeroes(CurrencyTermAccountNumber, 10));
            sb.Append(Utilities.AddPadding(CurrencyCode, 3));
            sb.Append(Utilities.AddPadding(LastObservationDate,6));
            sb.Append(Utilities.AddPadding(Code.ToString(),1));
            sb.Append(Utilities.AddPadding(string.Empty,1));
            sb.Append(Utilities.AddLeadingZeroes(InvoiceAmount, 13));
            sb.Append(Utilities.AddPadding(RiksbankIdentificationCode,1));
            sb.Append(Utilities.AddPadding(string.Empty,1));
            return sb.ToString();
        }
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }
        #endregion
    }
    public class LbForeignRiksbankPost : ILbPost
    {
        #region Members
        public bool IsExport { get; set; }
        public int TransactionCode { get; set; }
        public int ReceiverNumber { get; set; }
        public string RiksbankCode { get; set; }
        public string ReservField { get; set; }
        #endregion
        #region Constructors
        public LbForeignRiksbankPost(int receiverNumber,string riksbankCode)
        {
            IsExport = true;
            TransactionCode = (int)LbTransactionCodeForeign.RiksbankPost;
            ReceiverNumber = receiverNumber;
            RiksbankCode = riksbankCode;
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(TransactionCode);
            sb.Append(Utilities.AddLeadingZeroes(ReceiverNumber.ToString(), 7));
            sb.Append(Utilities.AddPadding(RiksbankCode, 3));
            sb.Append(Utilities.AddPadding(string.Empty, 69));
            return sb.ToString();
        }
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }
        #endregion
    } 
    public class LbForeignSummaryPost : ILbPost
    {
        #region Members
        public bool IsExport { get; set; }
        public int TransactionCode { get; set; }
        public int SenderBGC { get; set; }
        public string TotalAmount { get; set; }
        public string ReservField1 { get; set; }
        public string ReservField2 { get; set; }
        public string TotalSumReceiverNumber { get; set; }
        public string NumberOfPaymentPosts { get; set; }
        public string ReservField3 { get; set; }
        public string TotalSumInvoiceAmount { get; set; }
        public string ReservField4 { get; set; }
        #endregion
        #region Constructors
        public LbForeignSummaryPost(int senderBgc,long totalAmount, long totalSumReceiverNumber, int numberOfPaymentPosts, long totalSumInvoiceAmount)
        {
            IsExport = true;
            TransactionCode = (int)LbTransactionCodeForeign.SummaryPost;
            SenderBGC = senderBgc;
            TotalAmount = Utilities.ConvertAmountField(totalAmount);
            TotalSumInvoiceAmount = Utilities.ConvertAmountField(totalSumInvoiceAmount); 
            TotalSumReceiverNumber = Utilities.ConvertAmountField(totalSumReceiverNumber);
            NumberOfPaymentPosts = Utilities.ConvertAmountField(numberOfPaymentPosts); 
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.BGC_LINE_MAX_LENGTH);
            sb.Append(TransactionCode);
            sb.Append(Utilities.AddLeadingZeroes(SenderBGC.ToString(), 8));
            sb.Append(Utilities.AddLeadingZeroes(TotalAmount, 12));
            sb.Append(Utilities.AddPadding(string.Empty, 6));
            sb.Append(Utilities.AddPadding(string.Empty, 4));
            sb.Append(Utilities.AddLeadingZeroes(TotalSumReceiverNumber, 12));
            sb.Append(Utilities.AddLeadingZeroes(NumberOfPaymentPosts, 12));
            sb.Append(Utilities.AddPadding(string.Empty,8));
            sb.Append(Utilities.AddLeadingZeroes(TotalSumInvoiceAmount, 15));
            sb.Append(Utilities.AddPadding(string.Empty, 2));
            return sb.ToString();
        }
        /// <summary>
        /// Validate if this object meets structure requirements
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return true;
        }
        #endregion
    }
    #endregion

    #endregion

    #region Interfaces
    public interface ILbPost:ILbFileContent
    {
        int TransactionCode{get;}
    }
    public interface ILbFileContent
    {
        bool IsValid();
    }
    public interface ILbSection:ILbFileContent
    {
        List<ILbFileContent> Posts { get; set; }
    }
    public interface ILbPostGroup:ILbFileContent
    {
        List<ILbPost> Posts { get; set; }
        int BgCode { get; set; }
        bool IsValid(bool ParentPostDefinesDates);
    }
    public interface ILbPostGroupItem : ILbPost
    {
        int BgCode { get; set; }
    }
    #endregion
}
