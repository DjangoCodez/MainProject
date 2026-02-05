namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysHoliday")]
    public partial class SysHoliday : SysEntity
    {
        public int SysHolidayId { get; set; }

        public int? SysDayTypeId { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }

        public DateTime Date { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int? SysHolidayTypeId { get; set; }

        public virtual SysDayType SysDayType { get; set; }

        public virtual SysHolidayType SysHolidayType { get; set; }
    }
}
