using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class TimeSalaryExportSelectionDTO
    {
        public TimeSalaryExportSelectionDTO()
        {
            this.TimeSalaryExportSelectionGroups = new List<TimeSalaryExportSelectionGroupDTO>();
        }

        public int ActorCompanyId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public bool EntirePeriodValidForExport { get; set; }
        public List<TimeSalaryExportSelectionGroupDTO> TimeSalaryExportSelectionGroups { get; set; }
    }

    public class TimeSalaryExportSelectionGroupDTO
    {
        public TimeSalaryExportSelectionGroupDTO()
        {
            this.TimeSalaryExportSelectionEmployees = new List<TimeSalaryExportSelectionEmployeeDTO>();
            this.TimeSalaryExportSelectionSubGroups = new List<TimeSalaryExportSelectionGroupDTO>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public List<TimeSalaryExportSelectionGroupDTO> TimeSalaryExportSelectionSubGroups { get; set; }
        public List<TimeSalaryExportSelectionEmployeeDTO> TimeSalaryExportSelectionEmployees { get; set; }
        public bool EntirePeriodValidForExport { get; set; }
    }

    public class TimeSalaryExportSelectionEmployeeDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string Name { get; set; }
        public int AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public bool EntirePeriodValidForExport { get; set; }
    }

    public class TimeSalaryExportSelectionTransactionDTO
    {
        public TimeSalaryExportSelectionTransactionDTO()
        {
            this.AccountInternals = new List<AccountInternalDTO>();
        }
        public string Name { get; set; }
        public int AttestStateId { get; set; }
        public List<AccountInternalDTO> AccountInternals { get; set; }
        public List<int> AccountInternalIds { get; set; }
        public int EmployeeId { get; set; }
        public int PayrollProductId { get; set; }
        public int Id { get; set; }
        public bool AssignedToGroup { get; set; }

        public bool ContainsAnyAccount(List<int> accountIds)
        {
            return this.AccountInternalIds != null && accountIds.ContainsAny(this.AccountInternalIds);
        }
    }
}
