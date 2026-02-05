namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysTimeInterval")]
    public partial class SysTimeInterval : SysEntity
    {
        public int SysTimeIntervalId { get; set; }

        public int SysTermId { get; set; }

        public int Period { get; set; }

        public int Start { get; set; }

        public int StartOffset { get; set; }

        public int Stop { get; set; }

        public int StopOffset { get; set; }

        public int Sort { get; set; }
    }
}
