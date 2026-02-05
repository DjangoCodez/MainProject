namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPayrollStartValue")]
    public partial class SysPayrollStartValue : SysEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysPayrollStartValueId { get; set; }

        public int SysCountryId { get; set; }

        public int SysTermGroupId { get; set; }

        public int SysTermId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string PayrollProductNr { get; set; }
    }
}
