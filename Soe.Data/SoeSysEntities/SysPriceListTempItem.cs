namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPriceListTempItem")]
    public partial class SysPriceListTempItem : SysEntity
    {
        public int SysPriceListTempItemId { get; set; }

        public int SysPriceListTempHeadId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string ProductId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(256)]
        public string Name { get; set; }

        public decimal GNP { get; set; }
        public decimal? SalesPrice { get; set; }
        public decimal? NetPrice { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string Code { get; set; }

        [StringLength(50)]
        public string PurchaseUnit { get; set; }

        [StringLength(50)]
        public string SalesUnit { get; set; }

        public bool EnvironmentFee { get; set; }

        public bool Storage { get; set; }

        [StringLength(50)]
        public string EAN { get; set; }

        [StringLength(50)]
        public string ReplacesProduct { get; set; }
        [StringLength(100)]
        public string Manufacturer { get; set; }
        [StringLength(2048)]
        public string ExtendedInfo { get; set; }

        public decimal? PackageSizeMin { get; set; }

        public decimal? PackageSize { get; set; }

        [StringLength(100)]
        public string ProductLink { get; set; }

        public DateTime? PriceChangeDate { get; set; }

        public int PriceStatus { get; set; }

        public int Type { get; set; }

        public int SysCountryId { get; set; }

        public virtual SysPriceListTempHead SysPriceListTempHead { get; set; }
    }
}
