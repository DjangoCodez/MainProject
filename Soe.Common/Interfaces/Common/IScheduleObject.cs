using System;

namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface IScheduleBlockObject
    {
        int TimeScheduleTemplateBlockId { get; }
        int? TimeScheduleTemplatePeriodId { get; }
        int? EmployeeId { get; }
        DateTime? Date { get; }
        DateTime StartTime { get; }
        DateTime StopTime { get; }
        int BreakType { get; }
        bool IsBreak { get; }
        int? TimeScheduleTypeId { get; }
        int? ShiftTypeId { get; }
        int? AccountId { get; }
        string Link { get; }
    }

    public interface IScheduleBlockAccounting
    {
        int? EmployeeId { get; }
        DateTime? Date { get; }
        int? AccountId { get; }
    }
}
