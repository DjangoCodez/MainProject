using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class MobileModifyBreakInputDTO : TimeEngineInputDTO
    {
        public DateTime Date { get; set; }
        public int TimeScheduleTemplatePeriodId { get; set; }
        public int EmployeeId { get; set; }
        public int ScheduleBreakBlockId { get; set; }
        public int TimeCodeBreakId { get; set; }
        public int TotalMinutes { get; set; }
        public MobileModifyBreakInputDTO(DateTime date, int scheduleBreakBlockId, int employeeId, int timeScheduleTemplatePeriodId, int timeCodeBreakId, int totalMinutes)
        {
            this.Date = date;
            this.ScheduleBreakBlockId = scheduleBreakBlockId;
            this.EmployeeId = employeeId;
            this.TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId;
            this.TimeCodeBreakId = timeCodeBreakId;
            this.TotalMinutes = totalMinutes;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
    }
    public class AddModifyTimeBlocksInputDTO : TimeEngineInputDTO
    {
        public List<TimeBlock> InputTimeBlocks { get; set; }
        public DateTime Date { get; set; }
        public int TimeScheduleTemplatePeriodId { get; set; }
        public int EmployeeId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public AddModifyTimeBlocksInputDTO(List<TimeBlock> inputTimeBlocks, DateTime date, int timeScheduleTemplatePeriodId, int employeeId, int? timeDeviationCauseId)
        {
            this.InputTimeBlocks = inputTimeBlocks;
            this.Date = date;
            this.TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId;
            this.EmployeeId = employeeId;
            this.TimeDeviationCauseId = timeDeviationCauseId;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return InputTimeBlocks?.Select(i => i.TimeBlockDateId).Distinct().Count();
        }
    }

    #endregion

    #region Output

    public class MobileModifyBreakOutputDTO : TimeEngineOutputDTO { }
    public class AddModifyTimeBlocksOutputDTO : TimeEngineOutputDTO { }

    #endregion
}
