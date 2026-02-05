namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysExtraField")]
    public partial class SysExtraField : SysEntity
    {
        public int SysExtraFieldId { get; set; }

        public int SysCountryId { get; set; }

        public int SysTermGroupId { get; set; }

        public int SysTermId { get; set; }

        public int SysType { get; set; }

        public int Entity { get; set; }

        public int Type { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        public virtual SysCountry SysCountry { get; set; }
    }
}
