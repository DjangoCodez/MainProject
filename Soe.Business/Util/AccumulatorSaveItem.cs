namespace SoftOne.Soe.Business.Util
{
    public class AccumulatorSaveItem
    {
        public int TimeAccumulatorId { get; set; }
        public int Type { get; set; }
        public int? MinMinutes { get; set; }
        public int? MinTimeCodeId { get; set; }
        public int? MaxMinutes { get; set; }
        public int? MaxTimeCodeId { get; set; }
        public bool ShowOnPayrollSlip { get; set; }
    }
}
