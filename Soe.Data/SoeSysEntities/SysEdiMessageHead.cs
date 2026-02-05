namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysEdiMessageHead")]
    public partial class SysEdiMessageHead : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysEdiMessageHead()
        {
            SysEdiMessageRow = new HashSet<SysEdiMessageRow>();
        }

        public int SysEdiMessageHeadId { get; set; }

        public Guid SysEdiMessageHeadGuid { get; set; }

        public int SysEdiMessageHeadStatus { get; set; }

        public int? SysCompanyId { get; set; }

        public int SysEdiMessageRawId { get; set; }

        public int EDISourceType { get; set; }

        public int? SysEdiType { get; set; }

        public int? SysEdiMsgId { get; set; }

        public int SysWholesellerId { get; set; }

        [StringLength(50)]
        public string MessageSenderId { get; set; }

        [StringLength(50)]
        public string MessageType { get; set; }

        public DateTime? MessageDate { get; set; }

        [StringLength(50)]
        public string SellerId { get; set; }

        [StringLength(512)]
        public string SellerOrganisationNumber { get; set; }

        [StringLength(50)]
        public string SellerVatNumber { get; set; }

        [StringLength(128)]
        public string SellerName { get; set; }

        [StringLength(128)]
        public string SellerAddress { get; set; }

        [StringLength(128)]
        public string SellerPostalCode { get; set; }

        [StringLength(128)]
        public string SellerPostalAddress { get; set; }

        [StringLength(128)]
        public string SellerCountryCode { get; set; }

        [StringLength(128)]
        public string SellerPhone { get; set; }

        [StringLength(128)]
        public string SellerFax { get; set; }

        public string SellerReference { get; set; }

        [StringLength(128)]
        public string SellerReferencePhone { get; set; }

        [StringLength(128)]
        public string BuyerId { get; set; }

        [StringLength(128)]
        public string BuyerOrganisationNumber { get; set; }

        [StringLength(128)]
        public string BuyerVatNumber { get; set; }

        [StringLength(128)]
        public string BuyerName { get; set; }

        [StringLength(128)]
        public string BuyerAddress { get; set; }

        [StringLength(128)]
        public string BuyerPostalCode { get; set; }

        [StringLength(128)]
        public string BuyerPostalAddress { get; set; }

        [StringLength(128)]
        public string BuyerCountryCode { get; set; }

        [StringLength(128)]
        public string BuyerReference { get; set; }

        [StringLength(128)]
        public string BuyerPhone { get; set; }

        [StringLength(128)]
        public string BuyerFax { get; set; }

        [StringLength(128)]
        public string BuyerEmailAddress { get; set; }

        [StringLength(128)]
        public string BuyerDeliveryName { get; set; }

        [StringLength(128)]
        public string BuyerDeliveryCoAddress { get; set; }

        [StringLength(128)]
        public string BuyerDeliveryAddress { get; set; }

        [StringLength(128)]
        public string BuyerDeliveryPostalCode { get; set; }

        [StringLength(128)]
        public string BuyerDeliveryPostalAddress { get; set; }

        [StringLength(128)]
        public string BuyerDeliveryCountryCode { get; set; }

        [StringLength(128)]
        public string BuyerDeliveryNoteText { get; set; }

        [StringLength(128)]
        public string BuyerDeliveryGoodsMarking { get; set; }

        [StringLength(128)]
        public string HeadInvoiceNumber { get; set; }

        [StringLength(128)]
        public string HeadInvoiceOcr { get; set; }

        [StringLength(50)]
        public string HeadInvoiceType { get; set; }

        public DateTime? HeadInvoiceDate { get; set; }

        public DateTime? HeadInvoiceDueDate { get; set; }

        public DateTime? HeadDeliveryDate { get; set; }

        [StringLength(128)]
        public string HeadBuyerOrderNumber { get; set; }

        [StringLength(128)]
        public string HeadBuyerProjectNumber { get; set; }

        [StringLength(128)]
        public string HeadBuyerInstallationNumber { get; set; }

        [StringLength(128)]
        public string HeadSellerOrderNumber { get; set; }

        [StringLength(128)]
        public string HeadPostalGiro { get; set; }

        [StringLength(128)]
        public string HeadBankGiro { get; set; }

        [StringLength(128)]
        public string HeadBank { get; set; }

        [StringLength(128)]
        public string HeadBicAddress { get; set; }

        [StringLength(128)]
        public string HeadIbanNumber { get; set; }

        [StringLength(128)]
        public string HeadCurrencyCode { get; set; }

        public decimal? HeadVatPercentage { get; set; }

        public int? HeadPaymentConditionDays { get; set; }

        [StringLength(128)]
        public string HeadPaymentConditionText { get; set; }

        [StringLength(128)]
        public string HeadInterestPaymentPercent { get; set; }

        [StringLength(128)]
        public string HeadInterestPaymentText { get; set; }

        public decimal HeadInvoiceGrossAmount { get; set; }

        public decimal HeadInvoiceNetAmount { get; set; }

        public decimal HeadVatBasisAmount { get; set; }

        public decimal HeadVatAmount { get; set; }

        public decimal HeadFreightFeeAmount { get; set; }

        public decimal HeadHandlingChargeFeeAmount { get; set; }

        public decimal HeadInsuranceFeeAmount { get; set; }

        public decimal HeadRemainingFeeAmount { get; set; }

        public decimal HeadDiscountAmount { get; set; }

        public decimal HeadRoundingAmount { get; set; }

        public decimal HeadBonusAmount { get; set; }

        [StringLength(128)]
        public string HeadInvoiceArrival { get; set; }

        [StringLength(128)]
        public string HeadInvoiceAuthorized { get; set; }

        [StringLength(128)]
        public string HeadInvoiceAuthorizedBy { get; set; }

        public int State { get; set; }

        public string XDocument { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? LastSendTry { get; set; }

        public DateTime? Sent { get; set; }

        public virtual SysCompany SysCompany { get; set; }

        public virtual SysWholeseller SysWholeseller { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysEdiMessageRow> SysEdiMessageRow { get; set; }
    }
}
