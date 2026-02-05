namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysEdiMessageRow")]
    public partial class SysEdiMessageRow : SysEntity
    {
        public int SysEdiMessageRowId { get; set; }

        public int SysEdiMessageHeadId { get; set; }

        [StringLength(128)]
        public string RowSellerArticleNumber { get; set; }

        [StringLength(512)]
        public string RowSellerArticleDescription1 { get; set; }

        [StringLength(512)]
        public string RowSellerArticleDescription2 { get; set; }

        [StringLength(128)]
        public string RowSellerRowNumber { get; set; }

        [StringLength(128)]
        public string RowBuyerArticleNumber { get; set; }

        [StringLength(128)]
        public string RowBuyerRowNumber { get; set; }

        public DateTime? RowDeliveryDate { get; set; }

        [StringLength(128)]
        public string RowBuyerReference { get; set; }

        [StringLength(128)]
        public string RowBuyerObjectId { get; set; }

        public decimal? RowQuantity { get; set; }

        [StringLength(128)]
        public string RowUnitCode { get; set; }

        [StringLength(128)]
        public string RowUnitPrice { get; set; }

        public decimal? RowDiscountPercent { get; set; }

        public decimal? RowDiscountAmount { get; set; }

        public decimal? RowDiscountPercent1 { get; set; }

        public decimal? RowDiscountAmount1 { get; set; }

        public decimal? RowDiscountPercent2 { get; set; }

        public decimal? RowDiscountAmount2 { get; set; }

        public decimal? RowNetAmount { get; set; }

        public decimal? RowVatAmount { get; set; }

        public decimal? RowVatPercentage { get; set; }

        public int State { get; set; }

        public int SysProductId { get; set; }

        public virtual SysEdiMessageHead SysEdiMessageHead { get; set; }
    }
}
