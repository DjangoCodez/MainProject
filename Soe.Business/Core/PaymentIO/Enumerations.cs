namespace SoftOne.Soe.Business.Core.PaymentIO
{
    #region Common
    internal enum ReceiverType
    { 
        Customer = 0,
        Supplier = 1,
    }
    #endregion

    #region BGMAX
    internal enum BgMaxCurrency
    {
        Undefined = 0,
        SEK = 1,
        USD = 2,
        EUR = 3,
    }
    internal enum BgMaxTransactionCodes
    {
        SectionStart = 1,
        SectionEnd = 70,
        PaymentGroupStart = 5,
        PaymentGroupEnd = 15,
        PaymentPost = 20,
        PaymentReductionPost = 21,
        ReferenceNumberPost1 = 22,
        ReferenceNumberPost2 = 23,
        InformationPost = 25,
        NamePost = 26,
        AddressPost1 = 27,
        AddressPost2 = 28,
        OrganizationNumberPost = 29,
    }
    #endregion

    #region LB
    internal enum LbCurrency
    {
        Undefined = 0,
        SEK = 1,
        USD = 2,
        EUR = 3,
    }
    #region LB Domestic
    internal enum LbTransactionCodeDomestic
    {
        OpeningPost = 11,
        PaymentPost = 14,
        ReductionPost = 15,
        CreditInvoiceObservationPost = 16,
        CreditInvoiceObservationCompletePost = 17,
        CreditInvoiceRestPost = 20,
        CreditInvoicePreviousReductionPost = 21,
        InformationPost = 25,
        NamePost = 26,
        AddressPost = 27,
        AccountPost = 40,
        Saldo2 = 41,
        Saldo3 = 42,
        AssignmentObservationPost = 43,
        CommentPost = 49,
        PlusgiroPost = 54,
        PlusgiroInformationPost = 65,
        SummaryPost = 29,
    }
    internal enum LbAccountingCode
    {
        PaymentSpecification = 1,
        PaymentObservation = 2,
        Correction = 5,
        RejectedPayment = 6,
        StoppedPayment = 7,
        RefundedPayment = 99999 // - 
    }
    #endregion

    #region LB Correction
    internal enum LbCorrectionCode
    {
        //TODO: 1-16
    }
    #endregion

    #region LB Foreign
    internal enum LbTransactionCodeForeign
    {
        OpeningPost = 0,
        NamePost = 2,
        AddressPost = 3,
        BankPost = 4,
        CreditInvoicePost = 5,
        InvoiceReductionPost = 6,
        RiksbankPost = 7,
        SummaryPost = 9,
    }
    #endregion
    #endregion

    #region PG
    
    internal enum PgPostType
    {
        OpeningPost = 0,
        ReceiverIdentityPost = 1,
        SenderPost = 2,
        ReceiverPost = 3,
        MessagePost = 4,
        DebitAmountPost = 5,
        CreditAmountPost = 6,
        SummaryPost = 7,
        Undefined = 999,

    }
    internal enum PgPaymentMethod
    { 
        TransferToPg = 3,
        Bgc = 4,
        PayoutPersistentNumber = 5,
        PayoutSequenceNumber = 6,
        Undefined = 999,
    }
    internal enum PgCurrency
    {
        Undefined = 0,
        SEK = 1,
        EUR = 3,
    }

    internal enum CfpPaymentType
    {
        PlusGiro = 0,
        PayingCard = 2,
        BankGiro = 5,
        BankAccount = 9,
    }
    #endregion

    #region Nets

    internal enum NetsCurrency
    {
        Undefined = 0,
        NOK = 7,
    }

    internal enum NetsRecordType
    {
        StartTransmissionRecord = 10,
        StartAssignmentRecord = 20,
        TransactionRecord1 = 30,
        TransactionRecord2 = 31,
        EndAssignmentRecord = 88, 
        EndTransmissionRecord = 89,
    }
    #endregion Nets

    #region TOTALIN
    internal enum TotalinTransactionCodes
    {
        SectionStart = 10,
        SectionEnd = 90,
        PaymentPost = 20,
        PaymentReductionPost = 25,
        ReferenceNumberPost = 30,
        InformationPost = 40,
        NamePost = 50,
        AddressPost1 = 51,
        AddressPost2 = 52,
        OtherNamePost = 61,
        OtherAddressPost1 = 62,
        OtherAddressPost2 = 63,
        ForeignPaymentPost = 70,
    }
    #endregion

    #region ISO

    public enum ISO_Payment_TransactionStatus
    {
        Unknown = 0,
        ACCP = 1,
        PART = 2,
        RJCT = 3,
        ACTC = 4,
        ACSP = 5,
        ACSC = 6,
        PDNG = 7,
        ACWC = 8
    }
    #endregion

}

