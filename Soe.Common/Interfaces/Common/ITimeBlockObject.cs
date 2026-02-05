using System;

namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface ITimeBlockObject
    {
        int TimeBlockId { get; }
        int TimeBlockDateId { get; }
        int EmployeeId { get; }
        int? TimeDeviationCauseStartId { get; }
        int? TimeDeviationCauseStopId { get; }
        DateTime StartTime { get; }
        DateTime StopTime { get; }
    }
}
