using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Status.Models
{
    public class VismaPayrollChangesItem
    {
        public int VismaPayrollChangeId { get; set; }
        public int VismaPayrollBatchId { get; set; }
        public int PersonId { get; set; }
        public int? VismaPayrollEmploymentId { get; set; }
        public string Entity { get; set; }
        public string Info { get; set; }
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string EmployerRegistrationNumber { get; set; }
        public string PersonName { get; set; }
        public DateTime Time { get; set; }
    }
}
