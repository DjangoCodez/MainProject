using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeAccountItem
    {
        public EmployeeAccountItem()
        {
            ExtraFieldAnalysisFields = new List<ExtraFieldAnalysisField>();
            AccountAnalysisFields = new List<AccountAnalysisField>();
        }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateFrom { get; set; }
        public decimal? Percent { get; set; }
        public bool FixedAccounting { get; set; }
        public string Type { get; set; }
        public string AccountInternalStd { get; set; }
        public string AccountInternalName1 { get; set; }
        public string AccountInternalName2 { get; set; }
        public string AccountInternalName3 { get; set; }
        public string AccountInternalName4 { get; set; }
        public string AccountInternalName5 { get; set; }
        public string CategoryName { get; set; }
        public string AccountStdName { get; set; }
        public List<ExtraFieldAnalysisField> ExtraFieldAnalysisFields { get; set; }
        public List<AccountAnalysisField> AccountAnalysisFields { get; set; }
    }
}
