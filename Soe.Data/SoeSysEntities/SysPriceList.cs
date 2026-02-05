namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPriceList")]
    public partial class SysPriceList : SysEntity
    {
        public int SysPriceListId { get; set; }

        public int SysPriceListHeadId { get; set; }

        public int SysProductId { get; set; }
        public SysProduct SysProduct { get; set; }

        public decimal GNP { get; set; }
        public decimal? SalesPrice { get; set; }
        public decimal? NetPrice { get; set; }

        [StringLength(50)]
        public string PurchaseUnit { get; set; }

        [StringLength(50)]
        public string SalesUnit { get; set; }

        public bool EnvironmentFee { get; set; }

        public bool Storage { get; set; }

        [StringLength(50)]
        public string ReplacesProduct { get; set; }

        public decimal? PackageSizeMin { get; set; }

        public decimal? PackageSize { get; set; }

        [StringLength(100)]
        public string ProductLink { get; set; }

        public DateTime? PriceChangeDate { get; set; }

        public int SysWholesellerId { get; set; }

        public int PriceStatus { get; set; }

        [StringLength(50)]
        public string Code { get; set; }

        public virtual SysPriceListHead SysPriceListHead { get; set; }
    }
}
