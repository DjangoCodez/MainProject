namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysProductWholeSellerMappingView")]
    public partial class SysProductWholeSellerMappingView : SysEntity
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysProductId { get; set; }

        [StringLength(50)]
        public string ProductId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(256)]
        public string Name { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysPriceListHeadId { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysCountryId { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Type { get; set; }
    }
}
