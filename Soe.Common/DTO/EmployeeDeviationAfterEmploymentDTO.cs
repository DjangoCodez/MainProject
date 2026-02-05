using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class EmployeeDeviationAfterEmploymentDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string Name { get; set; }
        public DateTime EmploymentStopDate { get; set; }
        public EmployeeDatesDTO EmployeeDates { get; set; }
        public List<int> TimePayrollTransactionIds { get; set; }
        public List<int> TimeSchedulePayrollTransactionIds { get; set; }

        public EmployeeDeviationAfterEmploymentDTO(int employeeId, string employeeNr, string firstName, string lastName, DateTime employmentStopDate, List<DateTime> dates, List<int> timePayrollTransactionIds, List<int> timePayrollScheduleTransactionIds)
        {
            this.EmployeeId = employeeId;
            this.EmployeeNr = employeeNr;
            this.Name = $"{firstName} {lastName}";
            this.EmploymentStopDate = employmentStopDate;
            this.EmployeeDates = new EmployeeDatesDTO(employeeId, dates);
            this.TimePayrollTransactionIds = timePayrollTransactionIds;
            this.TimeSchedulePayrollTransactionIds = timePayrollScheduleTransactionIds;
        }
    }
}
