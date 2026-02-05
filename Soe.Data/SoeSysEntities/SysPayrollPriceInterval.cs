namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPayrollPriceInterval")]
    public partial class SysPayrollPriceInterval : SysEntity
    {
        public int SysPayrollPriceIntervalId { get; set; }

        public int SysPayrollPriceId { get; set; }

        public decimal? FromInterval { get; set; }

        public decimal? ToInterval { get; set; }

        public decimal Amount { get; set; }

        public int AmountType { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        public virtual SysPayrollPrice SysPayrollPrice { get; set; }
    }
}
