using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeDocumentItem
    {
        public EmployeeDocumentItem()
        {
            ExtraFieldAnalysisFields = new List<ExtraFieldAnalysisField>();
            AccountAnalysisFields = new List<AccountAnalysisField>();
        }

        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Description { get; set; }
        public DateTime? Created { get; set; }
        public bool NeedsConfirmation { get; set; }
        public bool Confirmed { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? Read { get; set; }
        public DateTime? Answered { get; set; }
        public string AnswerType { get; set; }
        public bool ByMessage { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string AttestStatus { get; set; }
        public string AttestState { get; set; }
        public string CurrentAttestUsers { get; set; }
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
