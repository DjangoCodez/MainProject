using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.Leisure.PreFlight.AutomaticAllocation.Models
{
    public class AutomaticAllocationOutput
    {
        public List<AutomaticAllocationEmployeeOutput> Employees { get; set; } = new List<AutomaticAllocationEmployeeOutput>();

    }

    public class AutomaticAllocationEmployeeOutput
    {
        public string EmployeeName { get; set; }
        public int EmployeeId { get; set; }
        public List<AutomaticAllocationEmployeeDayOutput> AllocationEmployeeDayOutputs
        {
            get
            {
                return AllocationEmployeeWeekOutputs.SelectMany(o => o.EmployeeDays).ToList();
            }
        }
        public List<AutomaticAllocationEmployeeWeekOutput> AllocationEmployeeWeekOutputs { get; set; } = new List<AutomaticAllocationEmployeeWeekOutput>();
    }

    public class AutomaticAllocationEmployeeWeekOutput
    {
        public DateTime Monday { get; set; }
        public List<AutomaticAllocationEmployeeDayOutput> EmployeeDays { get; set; } = new List<AutomaticAllocationEmployeeDayOutput>();
        public List<LeisureCodeAllocationWeekInvalidation> WeekInvalidations { get; set; } = new List<LeisureCodeAllocationWeekInvalidation>();
        public bool Success => !WeekInvalidations.Any();
    }

    public class AutomaticAllocationEmployeeDayOutput
    {
        public DateTime Date { get; set; }
        public AutomaticAllocationLeisureCode LeisureCode { get; set; }
        public override string ToString()
        {
            return $"{Date.ToShortDateString()} {LeisureCode}";
        }
    }
}
