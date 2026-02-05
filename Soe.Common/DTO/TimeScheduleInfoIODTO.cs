using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class TimeScheduleInfoIODTO
    {
        public TimeScheduleInfoIODTO()
        {
            Accounts = new List<AccountInfo>();
            TimeScheduleEmployees = new List<TimeScheduleEmployeeIO>();
            ProductInfos = new List<ProductInfo>();
            ShiftTypeInfos = new List<ShiftTypeInfo>();
            AttestStateInfos = new List<AttestStateInfo>();
        }

        /// <summary>
        /// Start of interval beeing fetched
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// End  of interval beeing fetched
        /// </summary>
        public DateTime StopDate { get; set; }
        /// <summary>
        /// All accounts, use this to connect transactions to the correct account
        /// </summary>
        public List<AccountInfo> Accounts { get; set; }
        /// <summary>
        /// List of all TimeScheduleEmployees. Contains all information about the employees.
        /// </summary>
        public List<TimeScheduleEmployeeIO> TimeScheduleEmployees { get; set; }
        /// <summary>
        /// All products, use this to connect transactions to the correct payrollproduct
        /// </summary>
        public List<ProductInfo> ProductInfos { get; set; }
        /// <summary>
        /// All shiftTypes, use this to connect transactions to the correct shifttype
        /// </summary>
        public List<ShiftTypeInfo> ShiftTypeInfos { get; set; }
        /// <summary>
        /// All attestStates, use this to connect transactions to the correct attestState
        /// </summary>  
        public List<AttestStateInfo> AttestStateInfos { get; set; }
    }

    /// <summary>
    /// ShiftType gives information about what the employee are supposed to do during a day.
    /// </summary>
    public class ShiftTypeInfo
    {
        /// <summary>
        /// ShiftTypeId is the identifier
        /// </summary>
        public int ShiftTypeId { get; set; }
        /// <summary>
        /// Name of ShiftType
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Hexadecimal colour code
        /// </summary>
        public string Color { get; set; }
    }

    /// <summary>
    /// ShiftType gives information about what kind of transactions that were generated during a day.
    /// </summary>
    public class ProductInfo
    {
        /// <summary>
        /// ProductId is the identifier
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// ProductNr is the code/number of the product
        /// </summary>
        public string ProductNr { get; set; }
        /// <summary>
        /// Name of Product
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// ProductNr is the external of the product, this can be used as an identifier for external systems.
        /// </summary>
        public string ExternalCode { get; set; }
        /// <summary>
        /// Product is marked as Absence
        /// </summary>
        public bool IsAbsence { get; set; }
    }

    public class AttestStateInfo
    {
        /// <summary>
        /// AttestStateId is the identifier
        /// </summary>
        public int AttestStateId { get; set; }
        /// <summary>
        /// Name of AttestState
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Sort order compared to other atteststates
        /// </summary>  
        public int Sort { get; set; }
        /// <summary>
        /// Is initial state
        /// </summary>  
        public bool IsInitial { get; set; }
    }

    /// <summary>
    /// AccountInfo contains information about the accounts. 
    /// </summary>
    public class AccountInfo
    {
        /// <summary>
        /// AccountId is the identifier
        /// </summary>
        public int AccountId { get; set; }
        /// <summary>
        /// Nr is the number/code of the Account
        /// </summary>
        public string Nr { get; set; }
        /// <summary>
        /// Name of Account
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Internal number in order to sort dimensions (for example costplaces and projects)
        /// </summary>
        public int DimNr { get; set; }
        /// <summary>
        /// Number according the SIE-standardspecification
        /// </summary>
        public int SieNr { get; set; }
        /// <summary>
        /// AccountParentId to parent account
        /// </summary>
        public int? ParentId { get; set; }
    }

    public class TimeScheduleEmployeeIO
    {
        public TimeScheduleEmployeeIO()
        {
            ScheduleInfos = new List<ScheduleInfo>();
            PayrollInfos = new List<PayrollInfo>();
            CalenderInfos = new List<CalenderInfo>();
        }

        public TimeScheduleEmployeeIO(string employeeNr, string name, string externalCode)
        {
            Name = name;
            EmployeeExternalCode = externalCode;
            EmployeeNr = employeeNr;
            ScheduleInfos = new List<ScheduleInfo>();
            PayrollInfos = new List<PayrollInfo>();
            CalenderInfos = new List<CalenderInfo>();
        }

        /// <summary>
        /// Name of Employee
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// EmployeeNumber
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// External code on employee
        /// </summary>
        public string EmployeeExternalCode { get; set; }
        /// <summary>
        /// Schedule information
        /// </summary>
        public List<ScheduleInfo> ScheduleInfos { get; set; }
        /// <summary>
        /// PayrollTransaction information
        /// </summary>
        public List<PayrollInfo> PayrollInfos { get; set; }
        /// <summary>
        /// Base information of each day in interval.
        /// </summary>
        public List<CalenderInfo> CalenderInfos { get; set; }
    }

    /// <summary>
    /// CalenderInfo contains information (connections to groups and employmentPercent) about the employee on a specific date.
    /// </summary>
    public class CalenderInfo
    {
        /// <summary>
        /// EmployeeNumber
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// External codes on employeeGroup
        /// </summary>
        public string EmployeeGroupExternalCodes { get; set; }
        /// <summary>
        /// External codes on payrollGroup
        /// </summary>
        public string PayrollGroupExternalCodes { get; set; }
        /// <summary>
        /// External codes on vacationGroup
        /// </summary>
        public string VacationGroupExternalCodes { get; set; }
        /// <summary>
        /// Employmentpercent on date
        /// </summary>
        public decimal EmploymentPercent { get; set; }
        /// <summary>
        /// Date
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Code to track to external system (if needed)
        /// </summary>
        public string DayExternalCode { get; set; }
    }

    public class ScheduleInfo
    {
        public ScheduleInfo()
        {
            AccountInfoLinks = new List<AccountInfoLink>();
        }

        /// <summary>
        /// EmpleeNumber
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// Date of schedule
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Start of scheduleblock
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// End of ScheduleBlock
        /// </summary>
        public DateTime StopTime { get; set; }
        /// <summary>
        /// Quantity in minutes
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Is a breakblock
        /// </summary>
        public bool IsBreak { get; set; }
        /// <summary>
        /// Has planned absence
        /// </summary>
        public bool PlannedAbsence { get; set; }
        /// <summary>
        /// Absence code
        /// </summary>
        public string TimeDeviationCause { get; set; }
        /// <summary>
        /// Calculated cest
        /// </summary>
        public decimal CalculatedCost { get; set; }
        /// <summary>
        /// Accounts on block
        /// </summary>
        public List<AccountInfoLink> AccountInfoLinks { get; set; }
        /// <summary>
        /// ShiftType on block, 0 == no shifttype
        /// </summary>
        public int ShiftTypeId { get; set; }
        /// <summary>
        /// TimeScheduleBlockType indicates if the block is a schedule, order, booking, standby or onduty
        /// </summary>
        public TimeScheduleBlockType TimeScheduleBlockType { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the current state is preliminary. null or false means not preliminary
        /// </summary>
        public bool? IsPreliminary { get; set; }
    }

    public class PayrollInfo
    {
        public PayrollInfo()
        {
            AccountInfoLinks = new List<AccountInfoLink>();
        }
        /// <summary>
        /// EmployeeNr
        /// </summary>
        public string EmployeeNr { get; set; }
        /// <summary>
        /// PayrollProductId
        /// </summary>
        public int Productid { get; set; }
        /// <summary>
        /// Date of transaction
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// StartTime of transaction, null if N/A
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// StopTime of transaction, null if N/A
        /// </summary>
        public DateTime? StopTime { get; set; }
        /// <summary>
        /// Quantity in minutes
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Amount is cost
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Current AttestState
        /// </summary>
        public int AttestStateId { get; set; }
        /// <summary>
        /// Accounts on block
        /// </summary>
        public List<AccountInfoLink> AccountInfoLinks { get; set; }
    }

    public class AccountInfoLink
    {
        /// <summary>
        /// Account according to Accounts on DTO.
        /// </summary>
        public int AccountId { get; set; }
    }

    public enum TimeScheduleBlockType
    {
        Schedule = 0,
        Order = 1,
        Booking = 2,
        Standby = 3,
        OnDuty = 4
    }
}

