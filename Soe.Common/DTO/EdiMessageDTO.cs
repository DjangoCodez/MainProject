using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class EdiMessageDTO
    {
        public TermGroup_EDISourceType EDISourceType { get; set; }
        public SysWholesellerEdiIdEnum SysWholesellerEdi { get; set; }
        public MessageMessageInfo MessageInfo { get; set; }
        public MessageSeller Seller { get; set; }
        public MessageBuyer Buyer { get; set; }
        public MessageHead Head { get; set; }
        public List<MessageRow> Row { get; set; }

        public partial class MessageMessageInfo
        {
            public string MessageSenderId { get; set; }
            public string MessageType { get; set; }
            public DateTime? MessageDate { get; set; }
            public string MessageImageFileName { get; set; }
        }

        public partial class MessageSeller
        {
            public string SellerId { get; set; }
            public string SellerOrganisationNumber { get; set; }
            public string SellerVatNumber { get; set; }
            public string SellerName { get; set; }
            public string SellerAddress { get; set; }
            public string SellerPostalCode { get; set; }
            public string SellerPostalAddress { get; set; }
            public string SellerCountryCode { get; set; }
            public string SellerPhone { get; set; }
            public string SellerFax { get; set; }
            public string SellerReference { get; set; }
            public string SellerReferencePhone { get; set; }
        }

        public partial class MessageBuyer
        {
            public string BuyerId { get; set; }
            public string BuyerOrganisationNumber { get; set; }
            public string BuyerVatNumber { get; set; }
            public string BuyerName { get; set; }
            public string BuyerAddress { get; set; }
            public string BuyerPostalCode { get; set; }
            public string BuyerPostalAddress { get; set; }
            public string BuyerCountryCode { get; set; }
            public string BuyerReference { get; set; }
            public string BuyerPhone { get; set; }
            public string BuyerFax { get; set; }
            public string BuyerEmailAddress { get; set; }
            public string BuyerDeliveryName { get; set; }
            public string BuyerDeliveryCoAddress { get; set; }
            public string BuyerDeliveryAddress { get; set; }
            public string BuyerDeliveryPostalCode { get; set; }
            public string BuyerDeliveryPostalAddress { get; set; }
            public string BuyerDeliveryCountryCode { get; set; }
            public string BuyerDeliveryNoteText { get; set; }
            public string BuyerDeliveryGoodsMarking { get; set; }
        }

        public partial class MessageHead
        {
            public string HeadInvoiceNumber { get; set; }
            public string HeadInvoiceOcr { get; set; }
            public string HeadInvoiceType { get; set; }
            public DateTime? HeadInvoiceDate { get; set; }
            public DateTime? HeadInvoiceDueDate { get; set; }
            public DateTime? HeadDeliveryDate { get; set; }
            public string HeadBuyerOrderNumber { get; set; }
            public string HeadBuyerProjectNumber { get; set; }
            public string HeadBuyerInstallationNumber { get; set; }
            public string HeadSellerOrderNumber { get; set; }
            public string HeadPostalGiro { get; set; }
            public string HeadBankGiro { get; set; }
            public string HeadBank { get; set; }
            public string HeadBicAddress { get; set; }
            public string HeadIbanNumber { get; set; }
            public string HeadCurrencyCode { get; set; }
            public string HeadVatPercentage { get; set; }
            public string HeadPaymentConditionDays { get; set; }
            public string HeadPaymentConditionText { get; set; }
            public string HeadInterestPaymentPercent { get; set; }
            public string HeadInterestPaymentText { get; set; }
            public string HeadInvoiceGrossAmount { get; set; }
            public string HeadInvoiceNetAmount { get; set; }
            public string HeadVatBasisAmount { get; set; }
            public decimal HeadVatAmount { get; set; }
            public string HeadFreightFeeAmount { get; set; }
            public string HeadHandlingChargeFeeAmount { get; set; }
            public string HeadInsuranceFeeAmount { get; set; }
            public string HeadRemainingFeeAmount { get; set; }
            public string HeadDiscountAmount { get; set; }
            public string HeadRoundingAmount { get; set; }
            public string HeadInvoiceArrival { get; set; }
            public string HeadInvoiceAuthorized { get; set; }
            public string HeadInvoiceAuthorizedBy { get; set; }
            public string HeadBonusAmount { get; set; }
        }

        public partial class MessageRow
        {
            public string RowSellerArticleNumber { get; set; }
            public string RowSellerArticleDescription1 { get; set; }
            public string RowSellerArticleDescription2 { get; set; }
            public string RowSellerRowNumber { get; set; }
            public string RowBuyerArticleNumber { get; set; }
            public string RowBuyerRowNumber { get; set; }
            public DateTime? RowDeliveryDate { get; set; }
            public string RowBuyerReference { get; set; }
            public string RowBuyerObjectId { get; set; }
            public decimal RowQuantity { get; set; }
            public string RowUnitCode { get; set; }
            public string RowUnitPrice { get; set; }
            public string RowDiscountPercent { get; set; }
            public string RowDiscountAmount { get; set; }
            public string RowDiscountPercent1 { get; set; }
            public string RowDiscountAmount1 { get; set; }
            public string RowDiscountPercent2 { get; set; }
            public string RowDiscountAmount2 { get; set; }
            public string RowNetAmount { get; set; }
            public string RowVatAmount { get; set; }
            public decimal RowVatPercentage { get; set; }
        }
    }
}


