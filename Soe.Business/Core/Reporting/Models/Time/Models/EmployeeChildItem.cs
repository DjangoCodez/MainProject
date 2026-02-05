using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeChildItem
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string ChildFirstName { get; set; }
        public string ChildLastName { get; set; }
        public DateTime? ChildDateOfBirth { get; set; }
        public bool ChildSingelCustody { get; set; }
        public int AmountOfDays { get; set; }
        public int AmountOfDaysUsed { get; set; }
        public int AmountOfDaysLeft { get; set; }
        public int Openingbalanceuseddays { get; set; }

    }
}
