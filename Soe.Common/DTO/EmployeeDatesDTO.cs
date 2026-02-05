using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class EmployeeDatesDTO
    {
        public int EmployeeId { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime StopDate { get; private set; }
        public List<DateTime> Dates { get; private set; }
        public string DateRangeText { get; private set; }

        public EmployeeDatesDTO(int employeeId, List<DateTime> dates)
        {
            this.EmployeeId = employeeId;
            this.Dates = dates?.Distinct().ToList() ?? new List<DateTime>();
            this.StartDate = this.Dates.Any() ? this.Dates.Min() : CalendarUtility.DATETIME_DEFAULT;
            this.StopDate = this.Dates.Any() ? this.Dates.Max() : CalendarUtility.DATETIME_DEFAULT;
            this.Init();
        }
        public EmployeeDatesDTO(int employeeId, DateTime startDate, DateTime stopDate, string employeeNr = null, string name = null)
        {
            this.EmployeeId = employeeId;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.Dates = CalendarUtility.GetDates(this.StartDate, this.StopDate);
            this.Init();
        }
        private void Init()
        {
            this.DateRangeText = this.Dates.GetCoherentDateRangeText();
        }
    }

    public static class EmployeeDatesExtension
    {
        public static List<EmployeeDatesDTO> CreateEmployeeDates(this List<int> l, DateTime startDate, DateTime stopDate)
        {
            List<EmployeeDatesDTO> employeeDates = new List<EmployeeDatesDTO>();
            foreach (int employeeId in l.Distinct())
            {
                employeeDates.Add(new EmployeeDatesDTO(employeeId, startDate, stopDate));
            }
            return employeeDates;
        }
        public static List<EmployeeDatesDTO> ToEmployeeDates(this Dictionary<int, List<DateTime>> dict)
        {
            List<EmployeeDatesDTO> dtos = new List<EmployeeDatesDTO>();
            foreach (var pair in dict)
            {
                dtos.Add(new EmployeeDatesDTO(pair.Key, pair.Value));
            }
            return dtos;
        }
        public static bool HasDates(this EmployeeDatesDTO e)
        {
            return e != null && !e.Dates.IsNullOrEmpty();
        }
    }
}
