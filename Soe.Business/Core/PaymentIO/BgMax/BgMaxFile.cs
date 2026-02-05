using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.PaymentIO.BgMax
{
    #region File - Level 0 (Container)
    public class BgMaxFile
    {
        #region Members
        public SectionStart SectionStart { get; set; }
        public SectionEnd SectionEnd { get; set; }
        public List<Section> Sections { get; set; }
        #endregion

        #region Constructors
        public BgMaxFile()
        {
            Sections = new List<Section>();
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            if (SectionEnd == null || !SectionEnd.IsValid())
                return false;
            if (SectionStart == null || !SectionStart.IsValid())
                return false;
            if (Sections.Count < 1)
                return false;
            foreach (ISection section in Sections)
            {
                if (!section.IsValid())
                    return false;
            }
            return true;
        }
        #endregion
    }
    #endregion

    #region Section - Level 1
    public class Section : ISection
    {
        #region Members
        public PaymentStart PaymentStart { get; set; }
        public List<PaymentGroup> PaymentGroups { get; set; }
        public PaymentEnd PaymentEnd { get; set; }
        #endregion

        #region Constructors
        public Section()
        {
            PaymentGroups = new List<PaymentGroup>();
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            if (PaymentEnd == null || !PaymentEnd.IsValid())
                return false;
            if (PaymentStart == null || !PaymentStart.IsValid())
                return false;
            if (PaymentGroups.Count < 1)
                return false;
            foreach (IPaymentGroup paymentGroup in PaymentGroups)
            {
                if (!paymentGroup.IsValid())
                    return false;
            }
            return true;
        }
        #endregion
    }
    public class SectionStart : ISection
    {
        #region Members
        public int TransactionCode { get; set; }
        public string LayoutName { get; set; }
        public string LayoutNumber { get; set; }
        public DateTime WriteDate { get; set; }
        string testMarkup;
        public bool IsTest
        {
            get
            {
                return testMarkup != "P";
            }
            set {
                testMarkup = value ? "T" : "P";
            }
        }
        #endregion

        #region Constructors
        public SectionStart(string item)
        {
            TransactionCode = Utilities.GetNumeric(item, 0, 2);
            LayoutName = "BGMAX";

            //LayoutNumber = item.Substring(7, 2);
            LayoutNumber = item.Substring(22, 2);
            
            //WriteDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.BGMax, item.Substring(9, 18));
            WriteDate = Utilities.GetDateTime(TermGroup_SysPaymentMethod.BGMax, item.Substring(24, 20));
            
            //testMarkup = item.Substring(19, 1);
            testMarkup = item.Substring(44, 1);
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return TransactionCode == (int)BgMaxTransactionCodes.SectionStart;
        }

        #endregion
    }
    public class SectionEnd : ISection
    {
        #region Members
        public int TransactionCode { get; set; }
        public string NumberOfPaymentPosts { get; set; }
        public string NumberOfReductionPosts { get; set; }
        public string NumberOfExtraReferencePosts { get; set; }
        public string NumberOfInsertionPosts { get; set; }
        #endregion

        #region Constructors
        public SectionEnd(string item)
        {
            TransactionCode = Utilities.GetNumeric(item, 0, 2);
            NumberOfPaymentPosts = item.Substring(2, 8);
            NumberOfReductionPosts = item.Substring(10, 8);
            NumberOfExtraReferencePosts = item.Substring(18, 8);
            NumberOfInsertionPosts = item.Substring(26, 8);
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return TransactionCode == (int)BgMaxTransactionCodes.SectionEnd;
        }

        #endregion
    }
    #endregion

    #region Payment - Level 2
    public class PaymentGroup : IPaymentGroup
    {
        #region Members
        public List<IPaymentPost> Posts { get; set; }
        #endregion

        #region Constructors
        public PaymentGroup()
        {
            Posts = new List<IPaymentPost>();
        }
        #endregion

        #region Help methods
        public void Add(string item)
        {
            Posts.Add(new PaymentPost(item));
        }
        public bool IsValid()
        {
            if (Posts.FindAll(IsPaymentPost).Count > 1)
                return false;

            foreach (IPaymentPost post in Posts)
            {
                if (!post.IsValid())
                    return false;
            }
            return true;
        }
        private bool IsPaymentPost(IPaymentPost item)
        {
            if (item is PaymentPost && item.TransactionCode == 20)
                return true;
            else
                return false;
        }

        //private bool IsNamePost(IPaymentPost item)
        //{
        //    if (item is NamePost)
        //        return true;
        //    return false;
        //}
        //private bool IsAddress1Post(IPaymentPost item)
        //{
        //    if (item is AddressPost1)
        //        return true;
        //    return false;
        //}
        //private bool IsAddress2Post(IPaymentPost item)
        //{
        //    if (item is AddressPost2)
        //        return true;
        //    return false;
        //}
        //private bool IsOrganisationPost(IPaymentPost item)
        //{
        //    if (item is OrganizationNumberPost)
        //        return true;
        //    return false;
        //}
        #endregion

    }
    public class PaymentStart : IPaymentGroup
    {
        #region Members
        public int TransactionCode { get; set; }
        public string ReceiverBGCNumber { get; set; }
        public string ReceiverPGNumber { get; set; }
        public string CurrencyCode { get; set; }
        #endregion

        #region Constructors
        public PaymentStart(string item)
        {
            TransactionCode = Utilities.GetNumeric(item, 0, 2);
            ReceiverBGCNumber = item.Substring(2, 10);
            ReceiverPGNumber = item.Substring(12, 10); //special rule to write this if exporting later on
            CurrencyCode = item.Substring(22, 3);
            //rest blank
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return TransactionCode == (int)BgMaxTransactionCodes.PaymentGroupStart;
        }

        #endregion
    }
    public class PaymentEnd : IPaymentGroup
    {
        #region Members
        public int TransactionCode { get; set; }
        public string ReceiverBankAccountNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentSequenceNumber { get; set; }
        public int PaymentAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string NumberOfPayments { get; set; }
        public string PaymentType { get; set; }

        #endregion

        #region Constructors
        public PaymentEnd(string item)
        {
            TransactionCode = Utilities.GetNumeric(item, 0, 2);
            ReceiverBankAccountNumber = Utilities.RemoveLeadingZeros(item.Substring(2, 35));
            PaymentDate = Utilities.GetDate(item.Substring(37, 8));
            PaymentSequenceNumber = item.Substring(45, 5);
            PaymentAmount = Utilities.GetNumeric(item, 50, 18);
            CurrencyCode = item.Substring(68, 3);
            NumberOfPayments = item.Substring(71, 8);
            PaymentType = item.Substring(79, 1);
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            return TransactionCode == (int)BgMaxTransactionCodes.PaymentGroupEnd;
        }

        #endregion
    }
    #endregion

    #region PaymentPosts - Level 3
    public class PaymentPost : IPaymentPost
    {
        #region Members
        public int TransactionCode { get; set; }
        public string SenderBGCNumber { get; set; }
        public string Reference { get; set; }
        public int ReferenceCode { get; set; }
        public int PaymentAmount { get; set; }
        public int PaymentChannelCode { get; set; }
        public string BGCPaymentSequenceNumber { get; set; }
        public int InvoiceImageMark { get; set; }
        public int ReductionCode { get; set; }
        public string InformationText { get; set; }
        #endregion

        #region Constructors
        public PaymentPost(string item)
        {
            TransactionCode = Utilities.GetNumeric(item, 0, 2);
            SenderBGCNumber = item.Substring(2, 10);
            Reference = item.Substring(12, 25).Trim();
            PaymentAmount = Utilities.GetNumeric(item, 37, 18);
            if (TransactionCode == (int)BgMaxTransactionCodes.PaymentReductionPost)
                PaymentAmount = - PaymentAmount;
            ReferenceCode = Utilities.GetNumeric(item, 55, 1);
            PaymentChannelCode = Utilities.GetNumeric(item, 56, 1);
            BGCPaymentSequenceNumber = item.Substring(57, 12);
            InvoiceImageMark = Utilities.GetNumeric(item, 69, 1);
            ReductionCode = Utilities.GetNumeric(item, 70, 1);

            #region For Export
            switch (ReferenceCode)
            {
                case 0:
                    //0 -:- Referensfältet är blankt. Kan bero på att betalaren inte angivit någon referens i
                    //betalningen. Kan också förekomma då avtal om utökad blankettregistrering finns, se 1.7
                    //Extra referensnummerposter, punkt 3.
                    break;
                case 2:
                    //2 N:hb Referensfältet innehåller ett korrekt OCR-referensnummer enligt avtal om OCRreferenskontroll
                    //inklusive eventuellt avtal om utökad blankettregistrering med OCRreferenskontroll.
                    break;
                case 3:
                    //3 A:- Referensfältet innehåller en eller flera referenser.
                    //Om det endast är en referens betyder det att den är felaktig alternativt att betalningen är till
                    //ett bankgironummer som inte har avtal om OCR-referenskontroll.
                    //Om det är flera referenser kan en eller flera av dessa vara korrekta enligt avtal om OCRreferenskontroll.
                    //Dessa redovisas då som Extra referensnummerposter
                    break;
                case 4:
                    //4 A:vb Referensfältet innehåller en korrekt referens enligt avtal om utökad blankettregistrering.
                    //Som korrekt referens räknas också referenser vid avtal om utökad blankettregistrering utan
                    //kontroll av referens. Betalningar med korrekt referens till ett bankgironummer som har
                    //avtal om utökad blankettregistrering med OCR-kontroll redovisas med värde 2.
                    break;
                case 5:
                    //5 A:- Referensfältet innehåller en felaktig referens enligt avtal om utökad blankettregistrering.
                    break;
            }

            switch (PaymentChannelCode)
            {
                case 1:
                    //1 Betalningen är en elektronisk betalning från bank.
                    break;
                case 2:
                    //2 Betalningen är en elektronisk betalning från tjänsten
                    //Leverantörsbetalningar (LB).
                    break;
                case 3:
                    //3 Betalningen är en blankettbetalning.
                    break;
            }
            switch (InvoiceImageMark)
            {
                case 0:
                    //0 Ingen avibild finns. Innebär att betalning har gjorts
                    //elektroniskt eller med OCR-avi.
                    break;
                case 1:
                    //1 Avibild finns. Innebär att betalning gjorts med girering-
                    ///inbetalningsavi.
                    break;
            }

            switch (ReductionCode)
            {
                case 0:
                    //0 Helt avdrag och ingen rest.
                    break;
                case 1:
                    //1 Delavdrag, rest finns.
                    break;
                case 2:
                    //2 Slutligt avdrag där delavdrag förekommit, ingen rest.
                    break;
            }
            #endregion
        }
        #endregion

        #region Help methods
        public bool IsValid()
        {
            switch (TransactionCode)
            {
                case (int)BgMaxTransactionCodes.PaymentPost:
                case (int)BgMaxTransactionCodes.PaymentReductionPost:
                case (int)BgMaxTransactionCodes.ReferenceNumberPost1:
                case (int)BgMaxTransactionCodes.ReferenceNumberPost2:
                    return true;
                default:
                    return false;
            }
        }
        #endregion
    }

    #region Optional posttypes

    /*public class InformationPost: IPaymentPost
    {
        public int TransactionCode { get; set; }
        public string InformationText { get; set; }

        public InformationPost(string item)
        {
            TransactionCode = Utilities.GetNumeric(item, 0, 2);
            InformationText = item.Substring(2, 50).Trim();
        }
        public bool IsValid()
        {
            return !String.IsNullOrEmpty(InformationText);
        }
    }
    public class NamePost:IPaymentPost
    {
        public NamePost(string item) 
        { 
        }
        public bool IsValid()
        {
            return false;
        }
    }
    public class AddressPost1:IPaymentPost
    {
        public AddressPost1(string item) 
        { 
        }
        public bool IsValid()
        {
            return false;
        }
    }
 
    public class AddressPost2:IPaymentPost
    {
        public AddressPost2(string item)
        { 
        }
        public bool IsValid()
        {
            return false;
        }
    } 
    public class OrganizationNumberPost:IPaymentPost
    {
        public OrganizationNumberPost(string item)
        { 
        }
        public bool IsValid()
        {
            return false;
        }
    }
    */
    #endregion
    #endregion

    #region Interfaces
    public interface ISection
    {
        bool IsValid();
    }
    public interface IPaymentGroup
    {
        bool IsValid();
    }
    public interface IPaymentPost
    {
        int TransactionCode { get; set; }
        bool IsValid();
    }
    #endregion
}
