namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class SysPageStatus : SysEntity
    {
        public int SysPageStatusId { get; set; }

        public int SysFeatureId { get; set; }

        public int BetaStatus { get; set; }

        public int LiveStatus { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public virtual SysFeature SysFeature { get; set; }
    }
}
