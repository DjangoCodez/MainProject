namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class TimeScheduleTypeCopyItem
    {
        public int TimeScheduleTypeId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAll { get; set; }
        public bool IsNotScheduleTime { get; set; }
        public bool UseScheduleTimeFactor { get; set; }
        public int State { get; set; }
        public bool ShowInTerminal { get; set; }
        public bool IgnoreIfExtraShift { get; set; }
        public bool IsBilagaJ { get; set; }
        public int? TimeDeviationCauseId { get; set; }
    }
}
