using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface IPayrollType
    {
        int? SysPayrollTypeLevel1 { get; }
        int? SysPayrollTypeLevel2 { get; }
        int? SysPayrollTypeLevel3 { get; }
        int? SysPayrollTypeLevel4 { get; }
    }

    public interface IPayrollTransaction : IPayrollType
    {
        int ProductId { get; }
        int EmployeeId { get; }
        DateTime Date { get; }
        int? TimePeriodId { get; }
        int AttestStateId { get; }
        bool PayrollProductUseInPayroll { get; }
        bool IsAdded { get; }
        bool IsFixed { get; }
        bool IsCentRounding { get; }
        bool IsQuantityRounding { get; }
        int? UnionFeeId { get; }
        int? EmployeeVehicleId { get; }
    }

    public interface IPayrollScheduleTransaction : IPayrollType
    {
        int ProductId { get; }
        int EmployeeId { get; }
        DateTime Date { get; }
    }

    public interface IPayrollTransactionAccounting
    {
        int EmployeeId { get; }
        DateTime Date { get; }
        List<int> AccountInternalIds { get; }
    }

    public interface IPayrollTransactionProc : IPayrollTransaction
    {
        decimal? Amount { get; }
        decimal Quantity { get; }
        int? InvoiceQuantity { get; }
        bool PayrollProductPayed { get; }
        bool IsAdditionOrDeduction { get; }
    }

    public interface IPayrollScheduleTransactionProc
    {
        int ProductId { get; }
        decimal? Amount { get; }
        decimal Quantity { get; }
        bool PayrollProductPayed { get; }
        int? Type { get; }
    }

    public interface ITransactionProc
    {
        int Id { get; }
        int ProductId { get; }
        DateTime Date { get; }
        decimal? Amount { get; }
        decimal Quantity { get; }
    }
}
