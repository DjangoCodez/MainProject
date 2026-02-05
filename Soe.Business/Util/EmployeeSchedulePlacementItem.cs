using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class SaveEmployeeSchedulePlacementItem
    {
        #region Variables

        //From view
        public int ExistingTimeScheduleTemplateHeadId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeInfo { get; set; }
        public int EmployeeScheduleId { get; set; }
        public DateTime? EmployeeScheduleStartDate { get; set; }
        public DateTime? EmployeeScheduleStopDate { get; set; }

        //Input
        public int TimeScheduleTemplateHeadId { get; set; }
        public int TimeScheduleTemplatePeriodId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public int TotalDays
        {
            get
            {
                return CalendarUtility.GetTotalDays(this.StartDate ?? EmployeeScheduleStopDate, this.StopDate);
            }
        }
        public bool Preliminary { get; set; }
        public int? ShiftTypeId { get; set; }
        public List<int> AccountIds { get; set; }
        public bool CreateTimeBlocksAndTransactionsAsync { get; set; }

        //Calculated
        public bool Validated { get; set; }
        public Guid UniqueId { get; set; }
        public bool IsPersonalTemplate { get; set; }
        public bool NewPlacement { get; set; }
        private bool changeStopDate = false;
        public bool ChangeStopDate
        {
            get
            {
                return changeStopDate;
            }
        }
        public bool ShortSchedule
        {
            get
            {
                return this.ChangeStopDate && this.StopDate < this.EmployeeScheduleStopDate;
            }
        }
        public bool ExtendSchedule
        {
            get
            {
                return this.ChangeStopDate && this.StopDate >= this.EmployeeScheduleStopDate;
            }
        }

        #endregion

        #region Ctor

        public SaveEmployeeSchedulePlacementItem() : base()
        {
            Validated = false;
        }

        #endregion

        #region Static methods

        public static SaveEmployeeSchedulePlacementItem Create(TimeScheduleTemplateHeadRangeDTO headRange, bool preliminary, bool createTimeBlocksAndTransactionsAsync = true)
        {
            return new SaveEmployeeSchedulePlacementItem()
            {
                EmployeeId = headRange.EmployeeId,
                TimeScheduleTemplateHeadId = headRange.TimeScheduleTemplateHeadId,
                TimeScheduleTemplatePeriodId = 0,
                StartDate = headRange.StartDate,
                StopDate = headRange.StopDate.Value,
                Preliminary = preliminary,
                CreateTimeBlocksAndTransactionsAsync = createTimeBlocksAndTransactionsAsync,
                IsPersonalTemplate = !headRange.TimeScheduleTemplateGroupId.HasValue,
                changeStopDate = headRange.EmployeeScheduleId.HasValue && headRange.StopDate != headRange.EmployeeScheduleStopDate,
                NewPlacement = !headRange.EmployeeScheduleId.HasValue,
                ExistingTimeScheduleTemplateHeadId = headRange.TimeScheduleTemplateHeadId,
                EmployeeInfo = String.Empty,
                EmployeeScheduleStartDate = headRange.EmployeeScheduleStartDate,
                EmployeeScheduleStopDate = headRange.EmployeeScheduleStopDate,
                EmployeeScheduleId = headRange.EmployeeScheduleId ?? 0,
            };
        }

        public static SaveEmployeeSchedulePlacementItem Create(EmployeeSchedulePlacementGridViewDTO item, DateTime? startDate, DateTime stopDate, bool preliminary, bool isPersonalTemplate, bool changeStopDate, int employeeId, int timeScheduleTemplateHeadId = 0, int timeScheduleTemplatePeriodId = 0, int? shiftTypeId = null, List<int> accountIds = null, bool createTimeBlocksAndTransactionsAsync = true)
        {
            var placement = new SaveEmployeeSchedulePlacementItem()
            {
                EmployeeId = employeeId,
                TimeScheduleTemplateHeadId = timeScheduleTemplateHeadId,
                TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId,
                StartDate = startDate,
                StopDate = stopDate,
                Preliminary = preliminary,
                ShiftTypeId = shiftTypeId,
                AccountIds = accountIds,
                CreateTimeBlocksAndTransactionsAsync = createTimeBlocksAndTransactionsAsync,
                IsPersonalTemplate = isPersonalTemplate,
                changeStopDate = changeStopDate,
                NewPlacement = startDate.HasValue,
            };
            if (item != null)
            {
                placement.ExistingTimeScheduleTemplateHeadId = item.TimeScheduleTemplateHeadId;
                placement.EmployeeInfo = item.EmployeeInfo;
                placement.EmployeeScheduleStartDate = item.EmployeeScheduleStartDate;
                placement.EmployeeScheduleStopDate = item.EmployeeScheduleStopDate;
                if (!placement.NewPlacement)
                    placement.EmployeeScheduleId = item.EmployeeScheduleId;
            }
            return placement;
        }

        public static List<SaveEmployeeSchedulePlacementItem> Create(IEnumerable<ActivateScheduleGridDTO> items, TermGroup_TemplateScheduleActivateFunctions function, int timeScheduleTemplateHeadId, int timeScheduleTemplatePeriodId, DateTime? startDate, DateTime stopDate, bool preliminary, bool createTimeBlocksAndTransactionsAsync = true)
        {
            var placements = new List<SaveEmployeeSchedulePlacementItem>();
            foreach (var item in items)
            {
                placements.Add(Create(item, function, timeScheduleTemplateHeadId, timeScheduleTemplatePeriodId, startDate, stopDate, preliminary, item.EmployeeId, createTimeBlocksAndTransactionsAsync));
            }
            return placements;
        }

        public static SaveEmployeeSchedulePlacementItem Create(ActivateScheduleGridDTO item, TermGroup_TemplateScheduleActivateFunctions function, int timeScheduleTemplateHeadId, int timeScheduleTemplatePeriodId, DateTime? startDate, DateTime stopDate, bool preliminary, int employeeId, bool createTimeBlocksAndTransactionsAsync = true)
        {
            var placement = new SaveEmployeeSchedulePlacementItem()
            {
                EmployeeId = employeeId,
                TimeScheduleTemplateHeadId = timeScheduleTemplateHeadId,
                TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId,
                StartDate = startDate,
                StopDate = stopDate,
                Preliminary = preliminary,
                CreateTimeBlocksAndTransactionsAsync = createTimeBlocksAndTransactionsAsync,
                changeStopDate = (function == TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate),
                NewPlacement = (function == TermGroup_TemplateScheduleActivateFunctions.NewPlacement),

            };
            if (placement.NewPlacement && placement.TimeScheduleTemplateHeadId > 0)
                placement.IsPersonalTemplate = false;
            else
                placement.IsPersonalTemplate = (item != null && item.IsPersonalTemplate) || (placement.TimeScheduleTemplateHeadId == 0 && placement.NewPlacement);

            if (item != null)
            {
                placement.ExistingTimeScheduleTemplateHeadId = item.TimeScheduleTemplateHeadId;
                placement.EmployeeInfo = item.EmployeeInfo;
                placement.EmployeeScheduleStartDate = item.EmployeeScheduleStartDate;
                placement.EmployeeScheduleStopDate = item.EmployeeScheduleStopDate;
                if (!placement.NewPlacement)
                    placement.EmployeeScheduleId = item.EmployeeScheduleId;
            }
            return placement;
        }

        public static DateRangeDTO GetExtensionInterval(SaveEmployeeSchedulePlacementItem item)
        {
            DateTime? startDate = item.EmployeeScheduleStopDate ?? item.StartDate;
            if (!startDate.HasValue)
                return null;

            return item.ExtendSchedule ? new DateRangeDTO(startDate.Value.AddDays(1), item.StopDate) : null;
        }

        public static List<DateRangeDTO> GetShortenIntervals(List<SaveEmployeeSchedulePlacementItem> items)
        {
            List<DateRangeDTO> dateRanges = new List<DateRangeDTO>();
            foreach (var item in items.Where(i => i.ShortSchedule))
            {
                dateRanges.Add(new DateRangeDTO(item.StopDate, item.EmployeeScheduleStopDate.Value));
            }
            return dateRanges;
        }

        #endregion

        #region Public methods

        public SaveEmployeeSchedulePlacementItem Copy()
        {
            return new SaveEmployeeSchedulePlacementItem()
            {
                EmployeeId = this.EmployeeId,
                TimeScheduleTemplateHeadId = this.TimeScheduleTemplateHeadId,
                TimeScheduleTemplatePeriodId = this.TimeScheduleTemplatePeriodId,
                StartDate = this.StartDate,
                StopDate = this.StopDate,
                Preliminary = this.Preliminary,
                ShiftTypeId = this.ShiftTypeId,
                AccountIds = this.AccountIds,

                //Flags
                IsPersonalTemplate = this.IsPersonalTemplate,
                changeStopDate = this.ChangeStopDate,
                NewPlacement = this.NewPlacement,

                //From placement
                ExistingTimeScheduleTemplateHeadId = this.ExistingTimeScheduleTemplateHeadId,
                EmployeeInfo = this.EmployeeInfo,
                EmployeeScheduleStartDate = this.EmployeeScheduleStartDate,
                EmployeeScheduleStopDate = this.EmployeeScheduleStopDate,
                EmployeeScheduleId = this.EmployeeScheduleId,
            };
        }

        public void MarkAsNewPlacement()
        {
            this.NewPlacement = true;
            this.changeStopDate = false;
        }

        public override string ToString()
        {
            return $"UniqueId:{UniqueId},EmployeeId:{EmployeeId},EmployeeInfo:{EmployeeInfo},EmployeeScheduleId:{EmployeeScheduleId},TimeScheduleTemplateHeadId:{TimeScheduleTemplateHeadId}";
        }

        #endregion
    }

    public class EmployeeSchedulePlacementValidationResult
    {
        public ActionResult Result { get; set; }
        public List<SaveEmployeeSchedulePlacementItem> ValidPlacements { get; set; }
        public List<SaveEmployeeSchedulePlacementItem> Batch { get; set; }
        public RecalculateTimeHead RecalculateTimeHead { get; set; }
        public Dictionary<int, List<TimeBlock>> ShortenScheduleEmployeeTimeBlocksDict { get; set; }
        public List<string> EmployeesWithoutOrInvalidPersonalSchedules { get; set; }
        public Dictionary<int, List<TimeScheduleTemplateBlock>> ShortenScheduleEmployeeTemplateBlocksDict { get; set; }

        public EmployeeSchedulePlacementValidationResult()
        {
            Result = new ActionResult();
            ShortenScheduleEmployeeTimeBlocksDict = new Dictionary<int, List<TimeBlock>>();
            ShortenScheduleEmployeeTemplateBlocksDict = new Dictionary<int, List<TimeScheduleTemplateBlock>>();
            EmployeesWithoutOrInvalidPersonalSchedules = new List<string>();
            ValidPlacements = new List<SaveEmployeeSchedulePlacementItem>();
        }
        public EmployeeSchedulePlacementValidationResult(ActionResult result)
        {
            Result = result;
        }

        public List<int> GetBatchEmployeeIds()
        {
            return this.Batch?.Select(b => b.EmployeeId).ToList() ?? new List<int>();
        }

        public string GetBatchInfo()
        {
            return this.Batch?.Select(b => b.ToString()).ToCommaSeparated();
        }
    }
}
