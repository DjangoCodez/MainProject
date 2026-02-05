namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysVatRate")]
    public partial class SysVatRate : SysEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysVatAccountId { get; set; }

        public decimal VatRate { get; set; }

        public DateTime Date { get; set; }

        public int IsActive { get; set; }

        public virtual SysVatAccount SysVatAccount { get; set; }
    }
}
