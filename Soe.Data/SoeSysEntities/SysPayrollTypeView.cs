namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPayrollTypeView")]
    public partial class SysPayrollTypeView : SysEntity
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysCountryId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysTermId { get; set; }

        public int? ParentId { get; set; }

        [Key]
        [Column(Order = 2)]
        public string Name { get; set; }
    }
}
