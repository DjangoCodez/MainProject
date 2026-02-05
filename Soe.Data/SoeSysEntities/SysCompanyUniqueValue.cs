namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysCompanyUniqueValue")]
    public partial class SysCompanyUniqueValue : SysEntity
    {
        public int SysCompanyUniqueValueId { get; set; }

        [StringLength(255)]
        public string UniqueValue { get; set; }

        public int UniqueValueType { get; set; }

        public int SysCompanyId { get; set; }

        public DateTime Created { get; set; }
        
        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }
    
        public virtual SysCompany SysCompany { get; set; }
    }
}