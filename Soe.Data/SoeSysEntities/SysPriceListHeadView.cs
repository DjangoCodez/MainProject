namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPriceListHeadView")]
    public partial class SysPriceListHeadView : SysEntity
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysPriceListHeadId { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime Date { get; set; }

        public int? Version { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Provider { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysWholesellerId { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(100)]
        public string Wholesellername { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WholesellerSysCountryId { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WholesellerType { get; set; }
    }
}
