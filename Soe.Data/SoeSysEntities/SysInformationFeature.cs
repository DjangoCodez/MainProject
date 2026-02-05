namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysInformationFeature")]
    public partial class SysInformationFeature : SysEntity
    {
        public int SysInformationFeatureId { get; set; }

        public int SysInformationId { get; set; }

        public int SysFeatureId { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        public virtual SysFeature SysFeature { get; set; }

        public virtual SysInformation SysInformation { get; set; }
    }
}
