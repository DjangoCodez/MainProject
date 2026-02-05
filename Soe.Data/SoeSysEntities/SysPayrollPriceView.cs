namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPayrollPriceView")]
    public partial class SysPayrollPriceView : SysEntity
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysPayrollPriceId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysCountryId { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysTermId { get; set; }

        [Key]
        [Column(Order = 3)]
        public string Name { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Type { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(10)]
        public string Code { get; set; }

        [Key]
        [Column(Order = 6)]
        public decimal Amount { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AmountType { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FromDate { get; set; }

        [Key]
        [Column(Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysPayrollPriceIntervalId { get; set; }

        public decimal? FromInterval { get; set; }

        public decimal? ToInterval { get; set; }

        public decimal? IntervalAmount { get; set; }

        public int? IntervalAmountType { get; set; }
    }
}
