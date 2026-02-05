namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysCurrencyRate")]
    public partial class SysCurrencyRate : SysEntity
    {
        public int SysCurrencyRateId { get; set; }

        public decimal Rate { get; set; }

        public int SysCurrencyFromId { get; set; }

        public int SysCurrencyToId { get; set; }

        public DateTime Date { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public virtual SysCurrency SysCurrency { get; set; }

        public virtual SysCurrency SysCurrency1 { get; set; }
    }
}
