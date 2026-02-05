using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class GrossTimeInput
    {
        public int ActorCompanyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public List<int> EmployeeGroupIds { get; set; }
    }

    public class GrossTimeRule
    {
        public DateTime Date { get; set; }
        public int EmployeeeGroupId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public decimal Factor { get; set; }
        public string ExternalCode { get; set; }
        public int TimeRuleId { get; set; }
        public List<int?> TimeScheduleTypeIds { get; set; }

        public bool IsEqual(GrossTimeRule compareRule)
        {
            return this.CompareString.Equals(this);
        }

        public string CompareString
        {
            get
            {
                return $"{this.Date}#{this.EmployeeeGroupId}#{this.StartTime}#{this.StopTime}#{(this.TimeScheduleTypeIds != null ? String.Join("|", this.TimeScheduleTypeIds.Where(w => w.HasValue).ToList()) : string.Empty)}";
            }
        }

        public bool ValidForScheduleType(int? scheduleTypeId)
        {
            if (!scheduleTypeId.HasValue && (TimeScheduleTypeIds == null || !TimeScheduleTypeIds.Any(f => f.HasValue)))
                return true;

            if (scheduleTypeId.HasValue && scheduleTypeId != 0 && (TimeScheduleTypeIds == null || !TimeScheduleTypeIds.Any(f => f.HasValue)))
                return false;

            if (!scheduleTypeId.HasValue && (TimeScheduleTypeIds != null && TimeScheduleTypeIds.Any(f => f.HasValue)))
                return false;

            if (scheduleTypeId.HasValue && scheduleTypeId != 0)
            {
                if (TimeScheduleTypeIds.Any(f => f.HasValue))
                {
                    return TimeScheduleTypeIds.Any(a => a == scheduleTypeId);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public decimal GetGrossFactor(DateTime startTime, DateTime stopTime)
        {
            decimal minutes = 0;
            DateTime currentTime = startTime;

            if (this.StartTime > currentTime)
                currentTime = this.StartTime;

            while (currentTime < stopTime)
            {
                if (currentTime >= this.StartTime && currentTime < this.StopTime)
                    minutes += decimal.Multiply(1, this.Factor > 1 ? this.Factor - 1 : this.Factor);
                else if (minutes > 0)
                    currentTime = stopTime;

                currentTime = currentTime.AddMinutes(1);
            }
            if (minutes != 0)
                return decimal.Divide(minutes, Convert.ToInt32((stopTime - startTime).TotalMinutes));
            else
                return new decimal(0);
        }

        public decimal GetAddedGrossMinutes(DateTime startTime, DateTime stopTime)
        {
            if (startTime == stopTime)
                return 0;

            var factor = GetGrossFactor(startTime, stopTime);

            if (factor != 0)
                return decimal.Multiply(Convert.ToInt32((stopTime - startTime).TotalMinutes), factor);
            else
                return Convert.ToInt32((stopTime - StartTime).TotalMinutes);
        }

        public decimal GetAddedGrossQuantity(DateTime startTime, DateTime stopTime, decimal amount)
        {
            if (amount == 0)
                return 0;

            return decimal.Multiply(amount, GetGrossFactor(startTime, stopTime));
        }
    }
}
