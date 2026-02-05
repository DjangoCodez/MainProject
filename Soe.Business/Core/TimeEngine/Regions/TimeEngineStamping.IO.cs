using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public abstract class SaveTimeStampsInputDTO : TimeEngineInputDTO
    {
        public List<TimeStampEntry> TimeStampEntryInputs { get; set; }
        public bool DiscardAttesteState { get; set; }
        public string TimeStampEntryInputsDetails
        {
            get
            {
                return this.TimeStampEntryInputs.GetInfo();
            }
        }
        public bool? DiscardBreakEvaluation { get; set; }
        protected SaveTimeStampsInputDTO()
        {

        }
        protected SaveTimeStampsInputDTO(List<TimeStampEntry> timeStampEntryInputs, bool? discardBreakEvaluation = null, bool discardAttesteState = false)
        {
            this.TimeStampEntryInputs = timeStampEntryInputs;
            this.DiscardBreakEvaluation = discardBreakEvaluation;
            this.DiscardAttesteState = discardAttesteState;
        }
        public override int? GetIdCount()
        {
            return TimeStampEntryInputs?.Select(i => i.EmployeeId).Distinct().Count();
        }
        public override int? GetIntervalCount()
        {
            return TimeStampEntryInputs?.Select(i => i.TimeBlockDateId).Distinct().Count();
        }
    }
    public class SynchTimeStampsInputDTO : SaveTimeStampsInputDTO
    {
        public List<TSTimeStampEntryItem> TSTimeStampEntryItems { get; set; }
        public int TimeTerminalId { get; set; }
        public int AccountDimId { get; set; }
        public SynchTimeStampsInputDTO(List<TSTimeStampEntryItem> timeStampEntryItems, int timeTerminalId, int accountDimId)
        {
            this.TSTimeStampEntryItems = timeStampEntryItems;
            this.TimeTerminalId = timeTerminalId;
            this.AccountDimId = accountDimId;
        }
        public override int? GetIdCount()
        {
            return TSTimeStampEntryItems?.Select(i => i.EmployeeId).Distinct().Count();
        }
        public override int? GetIntervalCount()
        {
            return TSTimeStampEntryItems?.Select(i => i.Time.Date).Distinct().Count();
        }
    }
    public class SynchGTSTimeStampsInputDTO : SaveTimeStampsInputDTO
    {
        public List<GoTimeStampTimeStamp> TimeStampEntryItems { get; set; }
        public int TimeTerminalId { get; set; }
        public SynchGTSTimeStampsInputDTO(List<GoTimeStampTimeStamp> timeStampEntryItems, int timeTerminalId)
        {
            this.TimeStampEntryItems = timeStampEntryItems;
            this.TimeTerminalId = timeTerminalId;
        }
        public override int? GetIdCount()
        {
            return TimeStampEntryItems?.Select(i => i.EmployeeId).Distinct().Count();
        }
        public override int? GetIntervalCount()
        {
            return TimeStampEntryItems?.Select(i => i.TimeStamp.Date).Distinct().Count();
        }
    }
    public class GenerateTimeStampsFromTimeBlocksInputDTO : TimeEngineInputDTO
    {
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public int SysScheduledJobId { get; set; }
        public int BatchNr { get; set; }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(StartDate, StopDate);
        }
    }
    public class SaveDeviationsFromStampingInputDTO : SaveTimeStampsInputDTO
    {
        public SaveDeviationsFromStampingInputDTO(List<TimeStampEntry> timeStampEntries, bool? discardBreakEvaluation = null, bool discardAttestState = false) : base(timeStampEntries, discardBreakEvaluation, discardAttestState) { }
    }
    public class SaveDeviationsFromStampingJobInputDTO : SaveTimeStampsInputDTO
    {
        public SaveDeviationsFromStampingJobInputDTO(List<TimeStampEntry> timeStampEntries, bool? discardBreakEvaluation = null) : base(timeStampEntries, discardBreakEvaluation) { }
    }

    #endregion

    #region Output

    public class SaveTimeStampsOutputDTO : TimeEngineOutputDTO 
    {
        public List<int> OvertimeTimeBlockDateIds { get; set; }
        public void AddOvertimeTimeBlockDateId(int timeBlockDateId)
        {
            if (this.OvertimeTimeBlockDateIds == null)
                this.OvertimeTimeBlockDateIds = new List<int>();
            if (!this.OvertimeTimeBlockDateIds.Contains(timeBlockDateId))
                this.OvertimeTimeBlockDateIds.Add(timeBlockDateId);
        }
    }
    public class SynchTimeStampsOutputDTO : SaveTimeStampsOutputDTO
    {
        public Dictionary<int, TimeStampEntry> UpdatedTimeStampEntries { get; set; }
        public Dictionary<int, List<DateTime>> SuccessfulAddedTimeStampEmployeeIds { get; set; }
        public SynchTimeStampsOutputDTO() : base()
        {
            this.UpdatedTimeStampEntries = new Dictionary<int, TimeStampEntry>();
            this.SuccessfulAddedTimeStampEmployeeIds = new Dictionary<int, List<DateTime>>();
        }
        public Dictionary<int, int> GetUpdatedTimeStampEntries()
        {
            Dictionary<int, int> updatedTimeStampEntries = new Dictionary<int, int>();
            if (!this.UpdatedTimeStampEntries.IsNullOrEmpty())
            {
                foreach (var pair in this.UpdatedTimeStampEntries)
                {
                    int timeStampEntryInternalId = pair.Key;
                    TimeStampEntry entry = pair.Value;
                    if (entry.TimeStampEntryId != 0)
                        updatedTimeStampEntries.Add(timeStampEntryInternalId, entry.TimeStampEntryId);
                }
            }
            return updatedTimeStampEntries;
        }
    }
    public class SynchGTSTimeStampsOutputDTO : SaveTimeStampsOutputDTO
    {
        public List<GoTimeStampEmployeeStampStatus> EmployeeStampStatuses { get; set; }
        public SynchGTSTimeStampsOutputDTO()
        {
            this.EmployeeStampStatuses = new List<GoTimeStampEmployeeStampStatus>();
        }
    }
    public class GenerateTimeStampsFromTimeBlocksOutputDTO : TimeEngineOutputDTO
    {
        public List<Tuple<Company, Employee, DateTime, List<TimeStampEntry>>> GeneratedTimeStampEntries { get; set; }
    }
    public class SaveDeviationsFromStampingOutputDTO : SaveTimeStampsOutputDTO { }
    public class SaveDeviationsFromStampingJobOutputDTO : SaveTimeStampsOutputDTO { }

    #endregion
}
