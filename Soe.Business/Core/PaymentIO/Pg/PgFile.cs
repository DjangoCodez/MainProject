using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Business.Core.PaymentIO.Pg
{
    #region File - Level 0
    public class PgFile
    {
        public OpeningPost OpeningPost { get; set; }
        public List<Section> Sections { get; set; }
        public bool IsValid()
        {
            if(!OpeningPost.IsValid())
                return false;
            foreach (Section section in Sections)
            {
                if (!section.IsValid())
                    return false;
            }
            return true;
        }
        public PgFile()
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

    public class OpeningPost
    {
        #region Members
        public int PostType { get; set; }
        public string ServiceNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string SenderAccountNumber { get; set; }
		public string ReportRecipient { get; set; }
		public DateTime Date { get; set; }

        private string currencyCodePocket;
        public string CurrencyCodePocket 
        {
            get { return currencyCodePocket.ToUpper(); }
            set { currencyCodePocket = value; } 
        }

        private string currencyCodeAmount;
        public string CurrencyCodeAmount 
        {
            get { return currencyCodeAmount.ToUpper(); }
            set { currencyCodeAmount = value; } 
        }

		private int _productionNumber = 1;
		public int ProductionNumber
		{
			get
			{
				return _productionNumber;
			}
			set
			{
				if (value < 1 || value > 9)
				{
					throw new ArgumentException("Production number must be within 1-9");
				}
				_productionNumber = value;
			}
		}
        #endregion

        #region Constructors
        public OpeningPost(string item)
        {
            PostType = Utilities.GetNumeric(item, 0, 1);
            ServiceNumber = item.Substring(1, 4);
            CustomerNumber = item.Substring(5, 10);
            SenderAccountNumber = item.Substring(11, 10);
            ReportRecipient = item.Substring(21, 27);
            Date = Utilities.GetDateTime(TermGroup_SysPaymentMethod.PG,item.Substring(48, 6));
            currencyCodePocket = item.Substring(54, 3);
            currencyCodeAmount = item.Substring(57, 3);
        }

        public OpeningPost(string customerNumber,DateTime date, int productionNumber)
        {
            PostType = (int)PgPostType.OpeningPost;
            CustomerNumber = customerNumber;
            Date = date;
            ProductionNumber = productionNumber;
        }
        #endregion

        #region Help methods
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.PG_LINE_MAX_LENGTH);
            sb.Append(PostType); // 0 + 1 = 1
            sb.Append(Utilities.PadLeft(CustomerNumber, 5)); // 1 + 5 = 6
            sb.Append(Utilities.PadLeft(Utilities.ShortDateString(Date), 6)); //6 + 6 = 12
            sb.Append(ProductionNumber); //12 + 1 = 13
            sb.Append(Utilities.PadLeft(string.Empty, 87)); // 13 + 87 = 100
            return sb.ToString();
        }
        public bool IsValid()
        {
            return PostType == (int)PgPostType.OpeningPost;
        }

        #endregion
    }

    public class SenderPost : IPost
    {
        #region Members
        public int PostType { get; set; }
        public string CustomerNumber { get; set; }
        public int SenderAccount { get; set; } //could be made int
        public string SenderClassification1 { get; set; }
        public string SenderClassification2 { get; set; }
        
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

        public string RefundAccountNumber { get; set; }
        #endregion

        #region Constructors
        public SenderPost()
        {
            PostType = (int)PgPostType.SenderPost;
        }
        public SenderPost(string customerNumber, int senderAccount, string senderClassification1, string senderClassification2, string  currencyPocket, string currencyCode)
        {
            PostType = (int)PgPostType.SenderPost;
            CustomerNumber = customerNumber;
            SenderAccount = senderAccount;            
            SenderClassification1 = senderClassification1;
            SenderClassification2 = senderClassification2;
            currencyCodePocket = currencyPocket;
            this.currencyCode = currencyCode;
            RefundAccountNumber = string.Empty;
        }
        #endregion

        #region Help methods

        public bool IsValid()
        {
            return PostType == (int)PgPostType.SenderPost;
        }

        public override string ToString() 
        {
            var sb = new StringBuilder(Utilities.PG_LINE_MAX_LENGTH);
			sb.Append(PostType); // 0 + 1 = 1
			sb.Append(Utilities.PadLeft(CustomerNumber, 5)); // 1 + 5 = 6
            sb.Append(Utilities.PadLeft(SenderAccount.ToString(), 10)); // 6 + 10 = 16

            //sender code, optional
            sb.Append(Utilities.PadLeft(string.Empty, 2)); // 16 + 2 = 18
            
            sb.Append(Utilities.AddPadding(SenderClassification1,27)); // 18 + 27 = 45
            sb.Append(Utilities.AddPadding(SenderClassification2,27)); // 45 + 27 = 72
            sb.Append(Utilities.AddPadding(CurrencyCodePocket,3)); // 72 + 3 = 75
            sb.Append(Utilities.AddPadding(CurrencyCode,3)); // 75 + 3 = 78
            sb.Append(Utilities.AddPadding(string.Empty,13)); // 78 + 13 = 91
            
            //optional, blank = same as senders account
            sb.Append(Utilities.PadLeft(RefundAccountNumber, 9));  // 91 + 9 = 100
            
            return sb.ToString();
        }
        #endregion
    }
    public class SummaryPost : IPost
    {
        #region Members
        public int PostType { get; set; }
        public string CustomerNumber { get; set; }
        public int SenderAccount { get; set; }
        public string SenderCode { get; set; }
        public long TotalAmount { get; set; }
        public int TotalNumberOfPosts { get; set; }
        private string currencyCodePocket;
        public string CurrencyCodePocket
        {
            get { return currencyCodePocket.ToUpper(); }
            set { currencyCodePocket = value; }
        }

        private string currencyCodeAmount;
        public string CurrencyCodeAmount
        {
            get { return currencyCodeAmount.ToUpper(); }
            set { currencyCodeAmount = value; }
        }
        #endregion

        #region Constructors
        public SummaryPost(string item)
        {
            PostType = Utilities.GetNumeric(item, 0, 1);
            CustomerNumber = item.Substring(1, 5);
            SenderAccount = Utilities.GetNumeric(item, 6, 10);
            SenderCode = item.Substring(16, 2);
            TotalAmount = Utilities.GetNumeric(item, 18, 13);
            TotalNumberOfPosts = Utilities.GetNumeric(item, 31, 6);
            CurrencyCodePocket = item.Substring(54, 3);
            CurrencyCodeAmount = item.Substring(57, 3);
        }

        public SummaryPost(string customerNumber, int senderAccount, long totalAmount,int totalNumberOfPosts, string currencyCodePocket, string currencyCodeAmount)
        {
            PostType = (int)PgPostType.SummaryPost;
            CustomerNumber = customerNumber;
            SenderAccount = senderAccount;
            SenderCode = string.Empty;
            TotalAmount = totalAmount;
            TotalNumberOfPosts = totalNumberOfPosts;
            this.currencyCodePocket = currencyCodePocket;
            this.currencyCodeAmount = currencyCodeAmount;
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return PostType == (int)PgPostType.SummaryPost;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.PG_LINE_MAX_LENGTH);

            sb.Append(Utilities.AddPadding(PostType.ToString(),1)); // 0 + 1 = 1
            sb.Append(Utilities.AddPadding(CustomerNumber,5)); // 1 + 5 + 6
            sb.Append(Utilities.PadLeft(SenderAccount.ToString(), 10)); // 6 + 10 = 16
            sb.Append(Utilities.AddPadding(string.Empty,2)); //sendercode, optional 16 + 2 = 18
            sb.Append(Utilities.AddLeadingZeroes(Math.Abs(TotalAmount).ToString(),12)); // amount without sign 18 + 12 = 30
            sb.Append(Utilities.AddPadding(TotalAmount < 0 ? "-" : "+", 1)); // sign 30 + 1 = 31
            sb.Append(Utilities.AddPadding(string.Empty,32)); // 31 + 32 = 63
            sb.Append(Utilities.AddPadding(CurrencyCodePocket,3)); // 63 + 3 = 66
            sb.Append(Utilities.AddPadding(CurrencyCodeAmount,3)); // 66 + 3 = 69
            sb.Append(Utilities.AddPadding(string.Empty,31)); // 69 + 31 = 100

            return sb.ToString();
        }
        #endregion
    }

    public class ReceiverIdentityPost : IPost
    {
        #region Members
        public int PostType { get; set; }
        public int PaymentMethod { get; set; }
        public string ReceiverIdentity { get; set; }
        public int NumberOfPostsForEachRecipient { get; set; }
        #endregion

        #region Constructors
        public ReceiverIdentityPost(string item)
        {
            PostType = Utilities.GetNumeric(item, 0, 1);
            PaymentMethod = Utilities.GetNumeric(item, 3, 1);
            ReceiverIdentity = item.Substring(4, 10);
            NumberOfPostsForEachRecipient = Utilities.GetNumeric(item, 24, 4);
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return PostType == (int)PgPostType.ReceiverIdentityPost;
        }

        #endregion
    }

    public class ReceiverPost : IPost
    {
        #region Members
        public int PostType { get; set; }
        public int PaymentMethod { get; set; }
        public string ReceiverIdentity { get; set; } //levNr eller pNr
        public string PostalCode { get; set; }
        public int PaymentNumber { get; set; }

        private string receiverName;
        public string ReceiverName
        {
            get { return receiverName.ToUpper(); }
            set { receiverName = value; }
        }
        
        private string receiverAddress;
        public string ReceiverAddress
        {
            get { return receiverAddress.ToUpper(); }
            set { receiverAddress = value; }
        }

        private string county;
        public string County
        {
            get { return county.ToUpper(); }
            set { county = value; }
        }
        
        private string supplierName;
        public string SupplierName
        {
            get { return supplierName.ToUpper(); }
            set { supplierName = value; }
        }

        #endregion

        #region Constructors

        public ReceiverPost(string item, bool factory, int type)
        {
            PostType = Utilities.GetNumeric(item, 0, 1);
            if (factory)
                SupplierName = item.Substring(34, 22);
            else
            {
                ReceiverName = item.Substring(1, 33);
                switch (type)
                {
                    case 1:
                        ReceiverAddress = item.Substring(34, 16);
                        PostalCode = item.Substring(61, 5);
                        break;
                    case 2:
                        PaymentNumber = Utilities.GetNumeric(item,34, 15);
                        break;
                }
            }
        }

        public ReceiverPost(int paymentMethod, string receiverIdentity, string receiverPostalCode, string receiverName, string county)
        {         
            PostType = (int)PgPostType.ReceiverPost;
            PaymentMethod = paymentMethod;
            ReceiverIdentity = receiverIdentity;
            PostalCode = receiverPostalCode;
            this.receiverName = receiverName;
            this.county = county;
        }

        public ReceiverPost(int paymentMethod, string receiverIdentity, string receiverName, int paymentNumber)
        {
            PostType = (int)PgPostType.ReceiverPost;
            PaymentMethod = paymentMethod;
            ReceiverIdentity = receiverIdentity;
            this.receiverName = receiverName;
            PaymentNumber = paymentNumber;
        }

        #endregion

        #region Help methods
        public bool IsValid()
        {
            return PostType == (int)PgPostType.ReceiverPost;
        }

        public override string ToString()
        {
            if (PostType != 4 && PostType != 3)
            {
                throw new ArgumentException("Endast posttyp 3 och 4 är implementerad");
            }
            var sb = new StringBuilder(Utilities.PG_LINE_MAX_LENGTH);

            sb.Append(PostType); // 0 + 1 = 1
            sb.Append(Utilities.AddPadding(PaymentMethod.ToString(), 1)); // 1 + 1 = 2
            sb.Append(Utilities.AddPadding(string.Empty, 5)); // 2 + 5 = 7
            sb.Append(Utilities.PadLeft(ReceiverIdentity, 10)); // 7 + 10 = 17
            sb.Append(Utilities.AddPadding(string.Empty, 5)); // 17 + 5 = 22
            sb.Append(Utilities.AddPadding(ReceiverName, 33)); // 22 + 33 = 55
            sb.Append(Utilities.PadLeft(PaymentNumber.ToString(), 16)); // 55 + 16 = 71
            sb.Append(Utilities.AddPadding(string.Empty, 29)); // 71 + 29 = 100
            /*
            switch (PaymentMethod)
            {
                case 3:
                case 4: 
                    sb.Append(Utilities.AddPadding(string.Empty,5));
                    sb.Append(Utilities.AddLeadingZeroes(PaymentNumber.ToString(),16));
                    sb.Append(Utilities.AddPadding(string.Empty,19));
                    break;

                case 5:
                case 7:
                    sb.Append(Utilities.AddPadding(PostalCode,5));
                    sb.Append(Utilities.AddPadding(ReceiverAddress,26));
                    sb.Append(Utilities.AddPadding(County,13));
                    sb.Append(Utilities.AddPadding(string.Empty,5));
                    break;
                case 2:
                    break;
            }
             */
            return sb.ToString();
        }
        #endregion
    }

    #region Optional(?) Receiver Posts
    public class ReceiverPostPayment : IPost 
    {
        #region Members
        public int PostType { get; set; }
        #endregion

        #region Constructors
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return true;
        }
        #endregion
    }
    public class ReceiverPostTransfer : IPost 
    {
        #region Members
        public int PostType { get; set; }
        #endregion

        #region Constructors
        #endregion

        #region Help methods
        public bool IsValid()
        { 
            return true;
        }
        #endregion
    }
    #endregion

    //Används inte för närvarande
    public class MessagePost : IPost //TODO: verifiy against spec
    {
        #region Members
        public string InvoiceNrMatch { get; set; } //Using same logic as professional here
        public int PostType { get; set; }
        public string ReceiverIdentity { get; set; }
        public string MessageRow1 { get; set; }
        public string MessageRow2 { get; set; }
        public int PaymentMethod { get; set; }
        public string SenderReference { get; set; }
        public string Message { get; set; }
        public string VerificationNumber { get; set; }
        #endregion

        #region Constructors

        public MessagePost(string item) 
        {
            PostType = Utilities.GetNumeric(item, 0, 1);
            SenderReference = item.Substring(1,30);
            Message = item.Substring(31,40);
            VerificationNumber = item.Substring(71,7);
            InvoiceNrMatch = item.Substring(2, 22);
        }

        public MessagePost(int paymentMethod, string receiverIdentity, string messageRow1, string messageRow2)
        {
            PostType = (int)PgPostType.MessagePost;
            PaymentMethod = paymentMethod;
            ReceiverIdentity = receiverIdentity;
            MessageRow1 = messageRow1;
            MessageRow2 = messageRow2;
        }

        #endregion

        #region Help methods
        public bool IsValid()
        {
            if (PostType != (int)PgPostType.MessagePost)
                return false;
            return true;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Utilities.PG_LINE_MAX_LENGTH);
            sb.Append(PostType);
            sb.Append(PaymentMethod);
            sb.Append(string.Empty);
            sb.Append(ReceiverIdentity);
            sb.Append(MessageRow1);
            sb.Append(MessageRow2);
            sb.Append(string.Empty);
            return sb.ToString();
        }
        #endregion
    }
    
    public class AmountPost
    {
        #region Members
        public int PostType { get; set; }
        public int PaymentMethod { get; set; }
        public string ReceiverAccountIdentity { get; set; }
        public string Message { get; set; }
        public long Amount { get; set; }
        public DateTime AccountDate { get; set; }
        public string CurrencyCode { get; set; }
        public string SenderReference { get; set; }
        #endregion
    }
    public class AmountPostDebit : AmountPost, IPost
    {
        #region Members
        public int AccountingNumber { get; set; }
        public string SameDayExecution { get; set; }
        public string VerificationNumber { get; set; }
        #endregion

        #region Constructors
        public AmountPostDebit(string item)
        {
            int seqnr = 0;
            PostType = Utilities.GetNumeric(item, 0, 1);
            PaymentMethod = Utilities.GetNumeric(item, 3, 1); 
            ReceiverAccountIdentity = item.Substring(4, 10);
            Message = item.Substring(14, 27);
            Amount = Utilities.GetNumeric(item, 41, 11);
            AccountDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.PG, item.Substring(52, 6));
            AccountingNumber = Utilities.GetNumeric(item, 58, 16); 
            CurrencyCode = item.Substring(74, 3);
            SameDayExecution = item.Substring(77, 1);
            int.TryParse(item.Substring(60, 19), out seqnr);
            SenderReference = seqnr.ToString();
        }

        public AmountPostDebit(int paymentMethod, string currencyCode, int paymentNumber, string message, long amount, DateTime date, string verificationNumber, int seqNr) :
            this(paymentMethod, currencyCode, paymentNumber.ToString(), message, amount, date, verificationNumber, seqNr)
        {
        }

        public AmountPostDebit(int paymentMethod, string currencyCode, string paymentNumber, string message, long amount, DateTime date, string verificationNumber, int seqNr)
        {
            PostType = (int)PgPostType.DebitAmountPost;
            PaymentMethod = paymentMethod;
            CurrencyCode = currencyCode;
            SameDayExecution = string.Empty; //= no, default
            ReceiverAccountIdentity = paymentNumber; //paymentnr
            Message = message; //fakturanr
            Amount = amount;
            AccountDate = date;
            SenderReference = seqNr.ToString();//optional
            VerificationNumber = verificationNumber; //paymentNumber skickas tillbaks
        }
        #endregion

        #region Help methods
        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.PG_LINE_MAX_LENGTH);
            sb.Append(Utilities.AddPadding(PostType.ToString(), 1)); // 0 + 1 = 1
            sb.Append(Utilities.AddPadding(PaymentMethod.ToString(), 1)); // 1 + 1 = 2
            sb.Append(Utilities.AddPadding(string.Empty, 1)); // 2 + 1 = 3
            sb.Append(Utilities.AddPadding(CurrencyCode, 3)); // 3 + 3 = 6
            sb.Append(Utilities.AddPadding(SameDayExecution, 1)); // 6 + 1 = 7
            sb.Append(Utilities.PadLeft(ReceiverAccountIdentity, 10)); // 7 + 10 = 17
            sb.Append(Utilities.AddPadding(Message, 27)); // 17 + 27 = 44
            sb.Append(Utilities.AddLeadingZeroes(Amount.ToString(), 11)); // 44 + 11 = 55
            sb.Append(Utilities.AddPadding(Utilities.ShortDateString(AccountDate), 6)); // 55 + 6 = 61
            sb.Append(Utilities.AddPadding(SenderReference, 30)); // 61 + 30 = 91
            sb.Append(Utilities.AddPadding(VerificationNumber, 8)); // 91 + 8 = 99
            sb.Append(Utilities.AddPadding(string.Empty, 1)); // 99 + 1 = 100
            return sb.ToString();
        }
        public bool IsValid()
        {
            return PostType == (int)PgPostType.DebitAmountPost;
        }

        #endregion
    }

    public class AmountPostCredit : AmountPost, IPost
    {
        #region Members
        public int CreditInvoiceOriginalAmount { get; set; }
        public int AccountIndicator { get; set; }
        public DateTime FirstAccountDate { get; set; }
        public DateTime LastAccountDate { get; set; }
        #endregion

        #region Constructors
        public AmountPostCredit(string item) 
        {
            PostType = Utilities.GetNumeric(item, 0, 1);
            PaymentMethod = Utilities.GetNumeric(item, 3, 1);
            ReceiverAccountIdentity = item.Substring(4, 10);
            Message = item.Substring(14, 27);
            Amount = Utilities.GetNumeric(item, 41, 9);
            AccountDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.PG, item.Substring(52, 6));
            CreditInvoiceOriginalAmount = Utilities.GetNumeric(item, 58, 10);
            AccountIndicator = Utilities.GetNumeric(item, 69, 1); 
            CurrencyCode = item.Substring(74, 3);
        }

        public AmountPostCredit(int paymentMethod, string currencyCode, string paymentNumber, string message, long amount, DateTime firstAccountDate, DateTime lastAccountDate)
        {
            PostType = (int)PgPostType.CreditAmountPost;
            PaymentMethod = paymentMethod;
            CurrencyCode = currencyCode;
            ReceiverAccountIdentity = paymentNumber;
            Message = message;
            Amount = amount;
            FirstAccountDate = firstAccountDate;
            LastAccountDate = lastAccountDate;
            SenderReference = string.Empty; //optional
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return PostType == (int)PgPostType.CreditAmountPost;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Utilities.PG_LINE_MAX_LENGTH);

            sb.Append(Utilities.AddPadding(PostType.ToString(), 1)); // 0 + 1 = 1
            sb.Append(Utilities.AddPadding(PaymentMethod.ToString(), 1)); // 1 + 1 = 2
            sb.Append(Utilities.AddPadding(string.Empty, 1)); // 2 + 1 = 3
            sb.Append(Utilities.AddPadding(CurrencyCode, 3)); // 3 + 3 = 6
            sb.Append(Utilities.AddPadding(string.Empty, 1)); // 6 + 1 = 7
            sb.Append(Utilities.PadLeft(ReceiverAccountIdentity, 10)); // 7 + 10 = 17
            sb.Append(Utilities.AddPadding(Message, 27)); // 17 + 27 = 44
            sb.Append(Utilities.AddLeadingZeroes(Amount.ToString(), 11)); // 44 + 11 = 55
			sb.Append(Utilities.AddPadding(Utilities.ShortDateString(FirstAccountDate), 6)); // 55 + 6 = 61
			sb.Append(Utilities.AddPadding(Utilities.ShortDateString(LastAccountDate), 6)); // 61 + 6 = 67
            sb.Append(Utilities.AddPadding(SenderReference, 30)); // 67 + 30 = 97
            sb.Append(Utilities.AddPadding(string.Empty, 3)); // 97 + 3 = 100

            return sb.ToString();
        }
        #endregion
    }

    #endregion
   
    #region Interfaces
    public interface IPost
    {
        int PostType { get; set; }
        bool IsValid();
    }

    #endregion
}