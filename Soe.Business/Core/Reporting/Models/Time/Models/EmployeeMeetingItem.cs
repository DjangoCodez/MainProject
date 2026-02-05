using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeMeetingItem
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string EmploymentType { get; set; }
        public string Position { get; set; }
        public string SSYKCode { get; set; }
        public DateTime? StartTime { get; set; }
        public string Participants { get; set; }
        public string OtherParticipants { get; set; }
        public bool Completed { get; set; }
        public string MeetingType { get; set; }
        public bool Reminder { get; set; }
        public string CategoryName { get; set; }
        public string AccountInternalName1 { get; set; }
        public string AccountInternalName2 { get; set; }
        public string AccountInternalName3 { get; set; }
        public string AccountInternalName4 { get; set; }
        public string AccountInternalName5 { get; set; }
        public List<ExtraFieldAnalysisField> ExtraFieldAnalysisFields { get; set; }
        public List<AccountAnalysisField> AccountAnalysisFields { get; set; }
    }
}
