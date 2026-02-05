using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Shared.DTO
{
    /// <summary>
    /// Input class for ordering information about long term absence.
    /// </summary>
    public class LongTermAbsenceInput
    {
        /// <summary>
        /// List of employee numbers to be processed. If empty, all employees (null) will be processed.
        /// </summary>
        public List<string> EmployeeNrs { get; set; }
        /// <summary>
        /// List of product types to be processed. If empty, all products (null) will be processed.
        /// </summary>
        public List<LongTermAbsencePayrollProductInput> PayrollProductInputs { get; set; }
        /// <summary>
        /// First date of interval containing long term absence.
        /// </summary>
        public DateTime DateFrom { get; set; }
        /// <summary>
        /// Last date of interval containing long term absence.
        /// </summary>
        public DateTime DateTo { get; set; }
        /// <summary>
        /// If true, the system will calculate the ratio of the long term absence. If false, the system will not calculate the ratio. The is performance intensive and should be used only when really needed.
        /// </summary>
        public bool CalculateRatio { get; set; } = false;
    }

    /// <summary>
    /// Set the valid type of payroll product for the long term absence.
    /// </summary>
    public class LongTermAbsencePayrollProductInput
    {
        public TermGroup_SysPayrollType SysPayrollTypeLevel1 { get; set; }
        public TermGroup_SysPayrollType SysPayrollTypeLevel2 { get; set; }
        public TermGroup_SysPayrollType SysPayrollTypeLevel3 { get; set; }
        public TermGroup_SysPayrollType SysPayrollTypeLevel4 { get; set; }
    }

    /// <summary> 
    /// The output header class for the long term absence.
    /// </summary>
    public class LongTermAbsenceOutput
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<LongTermAbsenceOutputRow> Rows { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// The output row class for the long term absence. Contains on interval, each employee can have multiple rows
    /// </summary>
    public class LongTermAbsenceOutputRow
    {
        /// <summary>
        /// Employee number.
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// Employee name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Employee first name.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Employee last name.
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Payroll type level 1.
        /// </summary>
        public int PayrollTypeLevel1 { get; set; }
        /// <summary>
        /// Payroll type level 2.
        /// </summary>
        public int PayrollTypeLevel2 { get; set; }
        /// <summary>
        /// Payroll type level 3.
        /// </summary>
        public int PayrollTypeLevel3 { get; set; }
        /// <summary>
        /// Payroll type level 4.
        /// </summary>
        public int PayrollTypeLevel4 { get; set; }
        /// <summary>
        /// Start date of the absence within the selected interval.
        /// </summary>
        public DateTime StartDateInInterval { get; set; }
        /// <summary>
        /// Stop date of the absence within the selected interval.
        /// </summary>
        public DateTime StopDateInInterval { get; set; }
        /// <summary>
        /// Total number of days for the absence.
        /// </summary>
        public double NumberOfDaysTotal { get; set; }
        /// <summary>
        /// Indicates if the absence covers the entire selected period.
        /// </summary>
        public bool EntireSelectedPeriod { get; set; }
        /// <summary>
        /// Number of days of absence within the interval.
        /// </summary>
        public int NumberOfDaysInInterval { get; set; }
        /// <summary>
        /// Number of days of absence before the interval.
        /// </summary>
        public int NumberOfDaysBeforeInterval { get; set; }
        /// <summary>
        /// Number of days of absence after the interval.
        /// </summary>
        public int NumberOfDaysAfterInterval { get; set; }
        /// <summary>
        /// Start date of the absence.Can be inside or outside of selected interval
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Stop date of the absence. Can be inside or outside of selected interval
        /// </summary>
        public DateTime StopDate { get; set; }
        /// <summary>
        /// Employee social security number. 
        /// </summary>
        public string SocialSec { get; set; }
        /// <summary>
        /// Ratio of the absence, if calculated.
        /// </summary>
        public decimal? Ratio { get; set; }
        /// <summary>
        /// Date and time when the record was created.
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// Date and time when the record was last modified.
        /// </summary>
        public DateTime? Modified { get; set; }
        /// <summary>
        /// List of transaction accounts related to the absence.
        /// </summary>
        public List<LongTermAbsenceAccount> TransactionAccounts { get; set; } = new List<LongTermAbsenceAccount>();
        /// <summary>
        /// List of employee accounts related to the absence.
        /// </summary>
        public List<LongTermAbsenceAccount> EmployeeAccounts { get; set; } = new List<LongTermAbsenceAccount>();
    }

    public class LongTermAbsenceAccount
    {
        /// <summary>
        /// The dimension number of the account.
        /// </summary>
        public int AccountDimNr { get; set; }
        /// <summary>
        /// The account number.
        /// </summary>
        public string AccountNr { get; set; }
        /// <summary>
        /// The account name.
        /// </summary>
        public string AccountName { get; set; }
        /// <summary>
        /// Unit code for the account.
        /// </summary>
        public string UnitExternalCode { get; set; }
        /// <summary>
        /// Company code for the account.
        /// </summary>
        public string ExportExternalCode { get; set; }
    }
}
