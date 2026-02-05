using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class GrossNetCostDTO
    {
        //General
        public string UniqueId { get; set; }
        public int EmployeeId { get; set; }

        // Schedule
        public int TimeScheduleTemplateBlockId { get; set; }
        public int? TimeScheduleTypeId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? PayrollGroupId { get; set; }
        public int? EmployeeGroupId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }

        // Schedule breaks (all breaks on day, not only this shift)
        public DateTime Break1StartTime { get; set; }
        public int Break1Minutes { get; set; }
        public DateTime Break2StartTime { get; set; }
        public int Break2Minutes { get; set; }
        public DateTime Break3StartTime { get; set; }
        public int Break3Minutes { get; set; }
        public DateTime Break4StartTime { get; set; }
        public int Break4Minutes { get; set; }

        // Calculated
        public int? CalculatedDayTypeId { get; set; }

        // Gross/Net/Cost
        public TimeSpan NetTime { get; set; }
        public TimeSpan GrossTime { get; set; }
        public TimeSpan BreakTime { get; set; }
        public TimeSpan IwhTime { get; set; }
        public TimeSpan GrossNetDiff { get; set; }
        public decimal CostPerHour { get; set; }
        public decimal EmploymentTaxCost { get; set; }
        public decimal SupplementChargeCost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalCostIncEmpTaxAndSuppCharge { get; set; }
        public bool IsAbsenceCost { get; set; }
        #region Static methods

        public static GrossNetCostDTO Create(TimeSchedulePlanningDayDTO dto)
        {
            return Create(
                    // General
                    dto.EmployeeId,
                    // Schedule
                    dto.TimeScheduleTemplateBlockId > 0 ? dto.TimeScheduleTemplateBlockId : dto.OriginalBlockId,
                    dto.TimeScheduleTypeId,
                    dto.TimeDeviationCauseId,
                    dto.StartTime,
                    dto.StopTime,
                    // Schedule breaks
                    dto.Break1StartTime,
                    dto.Break1Minutes,
                    dto.Break2StartTime,
                    dto.Break2Minutes,
                    dto.Break3StartTime,
                    dto.Break3Minutes,
                    dto.Break4StartTime,
                    dto.Break4Minutes);
        }

        public static GrossNetCostDTO Create(int employeeId, int timeScheduleTemplateBlockId, int? timeScheduleTypeId, int? timeDeviationCauseId, DateTime date, DateTime? startTime = null, DateTime? stopTime = null)
        {
            var dateStart = date;
            var dateStop = date;

            // start previous day
            if (startTime != null && startTime.Value.Date < CalendarUtility.DATETIME_DEFAULT.Date)
                dateStart = date.AddDays(-1);
            // start next day
            else if (startTime != null && startTime.Value.Date > CalendarUtility.DATETIME_DEFAULT.Date)
                dateStart = date.AddDays(1);

            // stop previous day
            if (stopTime != null && stopTime.Value.Date < CalendarUtility.DATETIME_DEFAULT.Date)
                dateStop = date.AddDays(-1);
            // stop next day
            else if (stopTime != null && stopTime.Value.Date > CalendarUtility.DATETIME_DEFAULT.Date)
                dateStop = date.AddDays(1);

            return Create(
                    // General
                    employeeId,
                    // Schedule
                    timeScheduleTemplateBlockId,
                    timeScheduleTypeId,
                    timeDeviationCauseId,
                    startTime.HasValue ? CalendarUtility.MergeDateAndDefaultTime(dateStart, startTime.Value) : CalendarUtility.DATETIME_DEFAULT,
                    stopTime.HasValue ? CalendarUtility.MergeDateAndDefaultTime(dateStop, stopTime.Value) : CalendarUtility.DATETIME_DEFAULT,
                    // Schedule breaks
                    CalendarUtility.DATETIME_DEFAULT,
                    0,
                    CalendarUtility.DATETIME_DEFAULT,
                    0,
                    CalendarUtility.DATETIME_DEFAULT,
                    0,
                    CalendarUtility.DATETIME_DEFAULT,
                    0);
        }

        public static GrossNetCostDTO Create(int employeeId, int timeScheduleTemplateBlockId, int? timeScheduleTypeId, int? timeDeviationCauseId, DateTime startTime, DateTime stopTime, DateTime break1StartTime, int break1Minutes, DateTime break2StartTime, int break2Minutes, DateTime break3StartTime, int break3Minutes, DateTime break4StartTime, int break4Minutes)
        {
            return new GrossNetCostDTO()
            {
                // General
                EmployeeId = employeeId,
                // Schedule
                TimeScheduleTemplateBlockId = timeScheduleTemplateBlockId,
                TimeScheduleTypeId = timeScheduleTypeId,
                TimeDeviationCauseId = timeDeviationCauseId,
                StartTime = startTime,
                StopTime = stopTime,
                // Schedule breaks
                Break1StartTime = break1StartTime,
                Break1Minutes = break1Minutes,
                Break2StartTime = break2StartTime,
                Break2Minutes = break2Minutes,
                Break3StartTime = break3StartTime,
                Break3Minutes = break3Minutes,
                Break4StartTime = break4StartTime,
                Break4Minutes = break4Minutes,
            };
        }

        public static GrossNetCostDTO FindIdentical(GrossNetCostDTO grossNetCost, List<GrossNetCostDTO> otherGrossNetCosts)
        {
            if (grossNetCost == null || otherGrossNetCosts == null)
                return null;

            foreach (GrossNetCostDTO otherGrossNetCost in otherGrossNetCosts)
            {
                if (grossNetCost.IsIdentical(otherGrossNetCost))
                    return otherGrossNetCost;
            }
            return null;
        }

        #endregion

        #region Public methods

        public void SetBreaks(DateTime break1StartTime, int break1Minutes, DateTime break2StartTime, int break2Minutes, DateTime break3StartTime, int break3Minutes, DateTime break4StartTime, int break4Minutes)
        {
            // Schedule breaks
            this.Break1StartTime = CalendarUtility.GetDateTime(this.StartTime, break1StartTime).AddDays((break1StartTime.Date - CalendarUtility.DATETIME_DEFAULT).Days);
            this.Break1Minutes = break1Minutes;
            this.Break2StartTime = CalendarUtility.GetDateTime(this.StartTime, break2StartTime).AddDays((break2StartTime.Date - CalendarUtility.DATETIME_DEFAULT).Days);
            this.Break2Minutes = break2Minutes;
            this.Break3StartTime = CalendarUtility.GetDateTime(this.StartTime, break3StartTime).AddDays((break3StartTime.Date - CalendarUtility.DATETIME_DEFAULT).Days);
            this.Break3Minutes = break3Minutes;
            this.Break4StartTime = CalendarUtility.GetDateTime(this.StartTime, break4StartTime).AddDays((break1StartTime.Date - CalendarUtility.DATETIME_DEFAULT).Days);
            this.Break4Minutes = break4Minutes;
        }

        public void SynchTimeAndCost(IEnumerable<TimeSchedulePlanningDayDTO> dtos)
        {
            if (this.TimeScheduleTemplateBlockId == 0)
                return;

            var dtosForBlock = dtos.Where(i => (i.TimeScheduleTemplateBlockId == this.TimeScheduleTemplateBlockId && i.OriginalBlockId == 0) || (i.OriginalBlockId == this.TimeScheduleTemplateBlockId && i.TimeScheduleTemplateBlockId == 0)).ToList();
            var dto = dtosForBlock.Count == 1 ? dtosForBlock.FirstOrDefault() : dtosForBlock.FirstOrDefault(i => i.StartTime.Date == this.StartTime.Date);
            if (dto != null)
            {
                // Gross/Net/Cost
                dto.NetTime = (int)this.NetTime.TotalMinutes;
                dto.GrossTime = (int)this.GrossTime.TotalMinutes;
                dto.GrossTimeDecimal = (decimal)this.GrossTime.TotalMinutes;
                dto.BreakTime = this.BreakTime;
                dto.IwhTime = this.IwhTime;
                dto.GrossNetDiff = this.GrossNetDiff;
                dto.CostPerHour = this.CostPerHour;
                dto.EmploymentTaxCost = this.EmploymentTaxCost;
                dto.SupplementChargeCost = this.SupplementChargeCost;
                dto.TotalCost = this.TotalCost;
                dto.TotalCostIncEmpTaxAndSuppCharge = this.TotalCostIncEmpTaxAndSuppCharge;
            }
        }

        public bool IsIdentical(GrossNetCostDTO dto)
        {
            if (dto == null)
                return false;
            if (this.EmployeeId != dto.EmployeeId)
                return false;
            if (!NumberUtility.IsEqual(this.PayrollGroupId, dto.PayrollGroupId))
                return false;
            if (this.StartTime != dto.StartTime)
                return false;
            if (this.StopTime != dto.StopTime)
                return false;
            if (!NumberUtility.IsEqual(this.TimeDeviationCauseId, dto.TimeDeviationCauseId))
                return false;
            if (!NumberUtility.IsEqual(this.TimeScheduleTypeId, dto.TimeScheduleTypeId))
                return false;
            if (!NumberUtility.IsEqual(this.CalculatedDayTypeId, dto.CalculatedDayTypeId))
                return false;
            if (this.Break1Minutes != dto.Break1Minutes || this.Break1StartTime != dto.Break1StartTime)
                return false;
            if (this.Break2Minutes != dto.Break2Minutes || this.Break2StartTime != dto.Break2StartTime)
                return false;
            if (this.Break3Minutes != dto.Break3Minutes || this.Break3StartTime != dto.Break3StartTime)
                return false;
            if (this.Break4Minutes != dto.Break4Minutes || this.Break4StartTime != dto.Break4StartTime)
                return false;
            return true;
        }

        public void Update(GrossNetCostDTO dto)
        {
            if (dto == null)
                return;

            NetTime = dto.NetTime;
            GrossTime = dto.GrossTime;
            BreakTime = dto.BreakTime;
            IwhTime = dto.IwhTime;
            GrossNetDiff = dto.GrossNetDiff;
            CostPerHour = dto.CostPerHour;
            EmploymentTaxCost = dto.EmploymentTaxCost;
            SupplementChargeCost = dto.SupplementChargeCost;
            TotalCost = dto.TotalCost;
            TotalCostIncEmpTaxAndSuppCharge = dto.TotalCostIncEmpTaxAndSuppCharge;
        }

        #endregion
    }
}
