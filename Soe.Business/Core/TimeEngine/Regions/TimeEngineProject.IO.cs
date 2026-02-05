using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class SaveOrderShiftInputDTO : TimeEngineInputDTO
    {
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public bool SkipXEMailOnChanges { get; set; }
        public SaveOrderShiftInputDTO(List<TimeSchedulePlanningDayDTO> shifts, bool skipXEMailOnChanges) : base()
        {
            this.Shifts = shifts;
            this.SkipXEMailOnChanges = skipXEMailOnChanges;
        }
        public override int? GetIdCount()
        {
            return Shifts?.Select(i => i.EmployeeId).Distinct().Count();
        }
        public override int? GetIntervalCount()
        {
            return Shifts?.Select(i => i.ActualDate).Distinct().Count();
        }
    }
    public class SaveOrderAssignmentInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public int OrderId { get; set; }
        public int? ShiftTypeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public TermGroup_AssignmentTimeAdjustmentType AssignmentTimeAdjustmentType { get; set; }
        public bool SkipXEMailOnChanges { get; set; }
        public SaveOrderAssignmentInputDTO(int employeeId, int orderId, int? shiftTypeId, DateTime startTime, DateTime? stopTime, TermGroup_AssignmentTimeAdjustmentType assignmentTimeAdjustmentType, bool skipXEMailOnChanges)
        {
            this.EmployeeId = employeeId;
            this.OrderId = orderId;
            this.ShiftTypeId = shiftTypeId;
            this.StartTime = startTime;
            this.StopTime = stopTime;
            this.AssignmentTimeAdjustmentType = assignmentTimeAdjustmentType;
            this.SkipXEMailOnChanges = skipXEMailOnChanges;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(StartTime, StopTime);
        }
    }
    public abstract class SaveProjectTimeBlocksInputDTO : TimeEngineInputDTO
    {
        public List<ProjectTimeBlock> ProjectTimeBlocks { get; set; }
        public override int? GetIdCount()
        {
            return ProjectTimeBlocks?.Select(i => i.EmployeeId).Distinct().Count();
        }
        public override int? GetIntervalCount()
        {
            return ProjectTimeBlocks?.Select(i => i.TimeBlockDateId).Distinct().Count();
        }
    }
    public class SaveTimeBlocksFromProjectTimeBlockInputDTO : SaveProjectTimeBlocksInputDTO
    {
        public bool AutoGenTimeAndBreakForProject { get; set; }
        public SaveTimeBlocksFromProjectTimeBlockInputDTO(List<ProjectTimeBlock> projectTimeBlocks, bool autoGenTimeAndBreakForProject)
        {
            this.ProjectTimeBlocks = projectTimeBlocks;
            this.AutoGenTimeAndBreakForProject = autoGenTimeAndBreakForProject;
        }
    }

    #endregion

    #region Output

    public class SaveOrderShiftOutputDTO : TimeEngineOutputDTO { }
    public class SaveOrderAssignmentOutputDTO : TimeEngineOutputDTO { }
    public class SaveTimeBlocksFromProjectTimeBlocksOutputDTO : TimeEngineOutputDTO
    {
        public List<int> TimeCodeTransactionIds { get; set; }
    }

    #endregion
}
