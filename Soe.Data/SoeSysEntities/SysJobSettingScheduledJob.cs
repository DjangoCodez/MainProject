using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftOne.Soe.Data
{
    [Table("SysJobSettingScheduledJob")]
    public partial class SysJobSettingScheduledJob
    {
        [Key]
        [Column(Order = 1)]
        public int SysJobSettingId { get; set; }
        [Key]
        [Column(Order = 2)]
        public int SysScheduledJobId { get; set; }

        public virtual SysJobSetting SysJobSetting { get; set; }
        public virtual SysScheduledJob SysScheduledJob { get; set; }
    }
}
