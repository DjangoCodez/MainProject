namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysHouseholdType")]
    public partial class SysHouseholdType : SysEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysHouseholdTypeId { get; set; }

        public int SysHouseholdTypeClassification { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }

        [StringLength(100)]
        public string XMLTagName { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        public int XMLOrder { get; set; }

        public int SysCountryId { get; set; }

        public virtual SysCountry SysCountry { get; set; }
    }
}
