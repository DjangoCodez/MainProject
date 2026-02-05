using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftOne.Soe.Data
{
    [Table("SysJobSettingJob")]
    public partial class SysJobSettingJob
    {
        [Key]
        [Column(Order = 1)]
        public int SysJobSettingId { get; set; }
        [Key]
        [Column(Order = 2)]
        public int SysJobId { get; set; }
        public virtual SysJob SysJob { get; set; }
        public virtual SysJobSetting SysJobSetting { get; set; }
    }
}
