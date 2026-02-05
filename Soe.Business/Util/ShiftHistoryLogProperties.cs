using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util
{
    public class ShiftHistoryLogCallStackProperties
    {

        public ShiftHistoryLogCallStackProperties()
        {
            DeletedBreaks = new List<DeletedBreakLogData>();
        }

        public ShiftHistoryLogCallStackProperties(Guid batchId, int originalShiftId, TermGroup_ShiftHistoryType historyType, int? recordId, bool skipXEMailOnChanges, bool isStandByView = false)
        {
            this.BatchId = batchId;
            this.OriginalShiftId = originalShiftId;
            this.HistoryType = historyType;
            this.RecordId = recordId;
            this.DeletedBreaks = new List<DeletedBreakLogData>();
            this.SkipXEMailOnChanges = skipXEMailOnChanges;
            this.IsStandByView = isStandByView;
        }

        public Guid BatchId;
        public int OriginalShiftId;
        public int NewShiftId;
        public TermGroup_ShiftHistoryType HistoryType;
        public int? RecordId = null;        
        public List<DeletedBreakLogData> DeletedBreaks;
        public int? AbsenceForEmployeeId = null;
        public TimeScheduleTemplateBlockHistory HistoryEntry = null;
        public bool SkipXEMailOnChanges = false;
        public bool IsStandByView = false;
        
        public bool IsCreatingScenario()
        {
            return this.HistoryType.IsCreatingScenario();
        }
        public bool IsActivatingScenario()
        {
            return this.HistoryType.IsActivatingScenario();
        }
        public bool IsSaveTimeScheduleShift()
        {
            return this.HistoryType.IsSaveTimeScheduleShift();
        }
        public bool IsApplyingAbsence()
        {
            return this.HistoryType.IsApplyingAbsence();
        }
    }

    public class DeletedBreakLogData
    {
        public DateTime OriginStartTime;
        public DateTime OriginStopTime;
        public TimeScheduleTemplateBlock DeletedBreak;

        public DeletedBreakLogData(DateTime originStart, DateTime originStop, TimeScheduleTemplateBlock deletedBreak)
        {
            this.OriginStartTime = originStart;
            this.OriginStopTime = originStop;
            this.DeletedBreak = deletedBreak;
        }
    }
}
