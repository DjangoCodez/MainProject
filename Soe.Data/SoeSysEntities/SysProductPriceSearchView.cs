namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysProductPriceSearchView")]
    public partial class SysProductPriceSearchView : SysEntity
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ProductId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string ProductNumber { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(256)]
        public string Name { get; set; }

        [StringLength(50)]
        public string EAN { get; set; }

        [Key]
        [Column(Order = 3)]
        public decimal GNP { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysPriceListHeadId { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysWholesellerId { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(100)]
        public string Wholeseller { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PriceStatus { get; set; }

        [StringLength(50)]
        public string PurchaseUnit { get; set; }

        [StringLength(50)]
        public string SalesUnit { get; set; }

        [Key]
        [Column(Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PriceListOrigin { get; set; }

        [Key]
        [Column(Order = 9)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysPriceListProviderType { get; set; }

        [StringLength(50)]
        public string ProductCode { get; set; }

        [Column(Order = 11)]
        public decimal? SalesPrice { get; set; }
        [Column(Order = 12)]
        public decimal? NetPrice { get; set; }
    }
}
