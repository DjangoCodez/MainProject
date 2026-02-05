using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeEndReasonsItem
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<AccountInternalDTO> AccountInternals { get; internal set; }
        public string Gender { get; internal set; }
        public int BirthYear { get; internal set; }
        public string DefaultRole { get; internal set; }
        public string SSYKCode { get; internal set; }
        public string EmploymentTypeName { get; internal set; }
        public DateTime EmploymentDate { get; internal set; }
        public DateTime? EndDate { get; internal set; }
        public int EndReason { get; internal set; }
        public string Comment { get; internal set; }
        public string CategoryName { get; internal set; }
    }
}
