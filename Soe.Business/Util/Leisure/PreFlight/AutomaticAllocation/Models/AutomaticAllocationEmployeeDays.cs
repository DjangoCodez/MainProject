using Microsoft.Owin.Security.Provider;
using SoftOne.Soe.Business.Altinn.PreFillEUSExternalBasic;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.Leisure.PreFlight.AutomaticAllocation.Models
{
    public class AutomaticAllocationEmployee
    {
        public string EmployeeName { get; set; }
        public int EmployeeGroupId { get; set; }
        public int EmployeeId { get; set; }
        public List<AutomaticAllocationEmployeeWeek> AllocationEmployeeWeeks { get; set; } = new List<AutomaticAllocationEmployeeWeek>();

        public List<AutomaticAllocationEmployeeWeek> GetAllocationEmployeeWeeksInNumberOfWorkDaysOrder()
        {
            return AllocationEmployeeWeeks.OrderByDescending(w => w.WorkDays.Count).ToList();
        }

        public List<AutomaticAllocationUnAssignedRestDay> UnAssignedRestDays { get; set; } = new List<AutomaticAllocationUnAssignedRestDay>();
        public AutomaticAllocationEmployeeOutput Output
        {
            get
            {
                return new AutomaticAllocationEmployeeOutput
                {
                    EmployeeName = EmployeeName,
                    EmployeeId = EmployeeId,
                    AllocationEmployeeWeekOutputs = AllocationEmployeeWeeks.Select(w => w.Output).ToList()
                };
            }
        }

        public bool HasAssignedDay(DateTime date, LeisureCodeType type)
        {
            return AllocationEmployeeWeeks
             .SelectMany(w => w.EmployeeDays)
             .Any(d => d.Date == date && d.Output != null && d.Output.LeisureCode.Type == type);
        }
    }

    public class AutomaticAllocationUnAssignedRestDay
    {
        public DateTime Monday { get; set; }
        public AutomaticAllocationLeisureCode LeisureCode { get; set; }
        public bool HasBeenAssigned { get; set; } = false;

        public bool MoveableToOtherWeek
        {
            get
            {
                return LeisureCode.MoveableToOtherWeek && !HasBeenAssigned;
            }
        }
    }

    public class AutomaticAllocationEmployeeWeek
    {
        public DateTime Monday { get; set; }
        public List<AutomaticAllocationEmployeeDay> EmployeeDays { get; set; } = new List<AutomaticAllocationEmployeeDay>();
        public List<LeisureCodeAllocationWeekInvalidation> WeekInvalidations { get; set; } = new List<LeisureCodeAllocationWeekInvalidation>();
        public AutomaticAllocationEmployeeWeekOutput Output
        {
            get
            {
                return new AutomaticAllocationEmployeeWeekOutput
                {
                    WeekInvalidations = WeekInvalidations,
                    Monday = Monday,
                    EmployeeDays = EmployeeDays.Select(d => d.Output).ToList()
                };
            }
        }

        public List<AutomaticAllocationEmployeeDay> WorkDays
        {
            get
            {
                return EmployeeDays.Where(x => x.IsWorkDay())
                    .ToList();
            }
        }

        public List<AutomaticAllocationEmployeeDay> RestDays
        {
            get
            {
                return EmployeeDays.Where(x => x.Output != null)
                    .ToList();
            }
        }

        public int GetNumberOfRestDaysOfType(LeisureCodeType leisureCodeType)
        {
            return EmployeeDays.Count(x => x.Output != null && x.Output.LeisureCode.Type == leisureCodeType);
        }

        public DateTime? GetFirstPossibleLeisureDay(List<AutomaticAllocationEmployee> otherEmployees, LeisureCodeType type)
        {
            var dates = GetPossibleLeisureDayDates();
            if (dates != null && dates.Count() > 1 && type == LeisureCodeType.V)
            {
                List<Tuple<DateTime, int>> tuples = new List<Tuple<DateTime, int>>();
                foreach (var date in dates)
                {
                    var count = otherEmployees.Count(e => e.HasAssignedDay(date, type));
                    tuples.Add(new Tuple<DateTime, int>(date, count));
                }

                var min = tuples.OrderBy(x => x.Item2).FirstOrDefault();
                return min.Item1;
            }

            if (dates.Any())
                return dates.FirstOrDefault();
            return null;
        }

        public int GetLeisureDaysCount()
        {
            return RestDays.Count;
        }

        public List<AutomaticAllocationEmployeeDay> LeisureDays
        {
            get
            {
                return EmployeeDays.Where(x => x.IsPossibleLeisureDay()).ToList();
            }
        }

        public List<DateTime> GetPossibleLeisureDayDates()
        {
            return EmployeeDays.Where(x => x.IsPossibleLeisureDay())
                .Select(x => x.Date)
                .ToList();
        }

        public bool WeekHasEnoughWorkDays(LeisureCodeType leisureCodeType, int numberOfUnAssignedRestDaysFromPreviousWeek)
        {
            if (leisureCodeType == LeisureCodeType.V)
            {
                //five or more workdays or X days
                return (numberOfUnAssignedRestDaysFromPreviousWeek + WorkDays.Count + GetNumberOfRestDaysOfType(LeisureCodeType.X)) >= 5;
            }
            else if (leisureCodeType == LeisureCodeType.X)
            {
                return (numberOfUnAssignedRestDaysFromPreviousWeek + WorkDays.Count + GetNumberOfRestDaysOfType(LeisureCodeType.V)) >= 5;
            }

            return false;
        }

        public void CleanWeek(LeisureCodeType leisureCodeType)
        {
            EmployeeDays.Where(w => w.Output != null && w.Output.LeisureCode.Type == leisureCodeType).ToList().ForEach(x => x.Output = null);
        }

        public List<AutomaticAllocationUnAssignedRestDay> UnAssignedRestDays { get; set; } = new List<AutomaticAllocationUnAssignedRestDay>();
        public List<AutomaticAllocationUnAssignedRestDay> GetMoveableUnAssignedRestDays()
        {
            return UnAssignedRestDays.Where(w => w.MoveableToOtherWeek).ToList();
        }

        public void AllocateLeisureDays(AutomaticAllocationLeisureCode leisureCode, List<AutomaticAllocationUnAssignedRestDay> unAssignedRestDaysFromPreviousWeek, List<AutomaticAllocationEmployee> otherEmployees)
        {
            bool allocatedV = false;

            // Check if the week is valid for the leisure code type
            if (!WeekHasEnoughWorkDays(leisureCode.Type, unAssignedRestDaysFromPreviousWeek.Count))
            {
                //TODO Log on week
                this.WeekInvalidations.Add(LeisureCodeAllocationWeekInvalidation.WeekHasToFewWorkDays);
                return;
            }

            if (leisureCode.Type == LeisureCodeType.V && TryAllocateLeisureDays(leisureCode, otherEmployees))
                allocatedV = true;

            foreach (var unAssignedRestDay in unAssignedRestDaysFromPreviousWeek)
            {
                if (TryAllocateLeisureDays(unAssignedRestDay.LeisureCode, otherEmployees))
                    unAssignedRestDay.HasBeenAssigned = true;
            }

            if (leisureCode.Type == LeisureCodeType.X)
                TryAllocateLeisureDays(leisureCode, otherEmployees);
            else if (!allocatedV)
            {
                TryAllocateLeisureDays(leisureCode, otherEmployees);
            }

            if (unAssignedRestDaysFromPreviousWeek.Any(a => !a.HasBeenAssigned))
                this.UnAssignedRestDays.AddRange(unAssignedRestDaysFromPreviousWeek.Where(a => !a.HasBeenAssigned));
        }
        public bool TryAllocateLeisureDays(AutomaticAllocationLeisureCode leisureCode, List<AutomaticAllocationEmployee> otherEmployees)
        {
            if (!TryAllocateLeisureDay(this, leisureCode, otherEmployees))
            {
                UnAssignedRestDays.Add(new AutomaticAllocationUnAssignedRestDay
                {
                    Monday = Monday,
                    LeisureCode = leisureCode
                });
                return false;
            }

            return true;
        }

        private bool TryAllocateLeisureDay(AutomaticAllocationEmployeeWeek week, AutomaticAllocationLeisureCode leisureCode, List<AutomaticAllocationEmployee> otherEmployees)
        {

            var preferredDay = week.EmployeeDays.FirstOrDefault(d => d.Date.DayOfWeek == leisureCode.PreferredDay && d.IsPossibleLeisureDay());
            if (preferredDay != null)
            {
                preferredDay.Output = new AutomaticAllocationEmployeeDayOutput { Date = preferredDay.Date, LeisureCode = leisureCode };
                return true;
            }

            // Assign the off day tag to the first available day
            var firstAvailableDay = week.GetFirstPossibleLeisureDay(otherEmployees, leisureCode.Type);
            if (firstAvailableDay != null)
            {
                var day = week.EmployeeDays.FirstOrDefault(d => d.Date == firstAvailableDay);
                day.Output = new AutomaticAllocationEmployeeDayOutput { Date = firstAvailableDay.Value, LeisureCode = leisureCode };
                return true;
            }

            if (leisureCode.Type == LeisureCodeType.V)
                week.WeekInvalidations.Add(LeisureCodeAllocationWeekInvalidation.WeekHasNoDaysToAllocateTo);

            return false;
        }
    }

    public class AutomaticAllocationEmployeeDay
    {
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public bool IsWorkDay() => StopTime > StartTime;
        public bool IsRestDay() => Output != null;
        public bool IsPossibleLeisureDay() => !IsWorkDay() && !IsRestDay() && !HasOtherAbsence;
        public bool HasOtherAbsence { get; set; }

        public int MinutesOfFreeTime(AutomaticAllocationEmployee allocationEmployee)
        {
            if (!IsPossibleLeisureDay())
                return 0;

            var dayBefore = allocationEmployee.AllocationEmployeeWeeks
                .SelectMany(w => w.EmployeeDays)
                .FirstOrDefault(d => d.Date == Date.AddDays(-1));
            var dayAfter = allocationEmployee.AllocationEmployeeWeeks
                .SelectMany(w => w.EmployeeDays)
                .FirstOrDefault(d => d.Date == Date.AddDays(1));

            return MinutesOfFreeTime(dayBefore, dayAfter);
        }

        public int MinutesOfFreeTime(AutomaticAllocationEmployeeDay dayBefore, AutomaticAllocationEmployeeDay dayAfter)
        {
            if (!IsPossibleLeisureDay())
                return 0;

            var minutes = 0;
            if (dayBefore != null && dayAfter != null)
            {
                var startTime = dayBefore.StopTime;
                var stopTime = dayAfter.StartTime;
                if (startTime < stopTime)
                {
                    minutes = (int)(stopTime - startTime).TotalMinutes;
                }
            }
            return minutes;
        }

        public AutomaticAllocationEmployeeDayOutput Output { get; set; }


        public override string ToString()
        {
            return $"IsWorkDay {IsWorkDay()} IsRestDay {IsRestDay()} IsPossibleLeisureDay {IsPossibleLeisureDay()} output: {Output}";
        }
    }
}
