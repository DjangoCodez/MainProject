using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class WorkIntervalDTO
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public bool HasBilagaJ { get; set; }
        public List<int> ShiftTypeIds { get; set; }
        public bool HasAbsence()
        {
            return GrossNetCost != null && GrossNetCost.TimeDeviationCauseId.HasValue;
        }
        public GrossNetCostDTO GrossNetCost { get; set; }
        public int TotalMinutes
        {
            get
            {
                return (int)this.StopTime.Subtract(this.StartTime).TotalMinutes;
            }
        }
        public DateTime Date
        {
            get
            {
                return this.StartTime.Date;
            }
        }

        public WorkIntervalDTO(int id, DateTime startTime, DateTime stopTime, GrossNetCostDTO grossNetCost = null, params int[] shiftTypeIds)
        {
            this.Id = id;
            this.StartTime = startTime;
            this.StopTime = stopTime;
            this.GrossNetCost = grossNetCost;
            if (grossNetCost != null)
            {
                this.GrossNetCost.StartTime = startTime;
                this.GrossNetCost.StopTime = stopTime;
            }
            this.ShiftTypeIds = shiftTypeIds != null ? shiftTypeIds.ToList() : new List<int>();
        }
    }

    public static class WorkIntervalExtensions
    {
        public static (DateTime startTime, DateTime stopTime) GetStartAndStopTime(this List<WorkIntervalDTO> l, WorkIntervalDTO e, int shiftNr)
        {
            if (e == null)
                return (CalendarUtility.DATETIME_DEFAULT, CalendarUtility.DATETIME_DEFAULT);

            DateTime startTime = shiftNr > 1 ? e.StartTime : DateTime.MinValue;
            DateTime stopTime = shiftNr < l.Count ? e.StopTime : DateTime.MaxValue;
            return (startTime, stopTime);
        }
    }
}
