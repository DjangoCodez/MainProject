namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface IPayrollProduct
    {
        //Product
        int? ProductGroupId { get; set; }
        int? ProductUnitId { get; set; }
        int? SysPayrollProductId { get; set; }

        string Number { get; set; }
        string Name { get; set; }
        string ExternalNumber { get; set; }
        string Description { get; set; }
        string AccountingPrio { get; set; }
        int State { get; set; }

        //PayrollProduct
        int PayrollType { get; set; }
        int ResultType { get; set; }
        int? SysPayrollTypeLevel1 { get; set; }
        int? SysPayrollTypeLevel2 { get; set; }
        int? SysPayrollTypeLevel3 { get; set; }
        int? SysPayrollTypeLevel4 { get; set; }
        string ShortName { get; set; }
        decimal Factor { get; set; }
        bool AverageCalculated { get; set; }
        bool DontUseFixedAccounting { get; set; }
        bool Export { get; set; }
        bool ExcludeInWorkTimeSummary { get; set; }
        bool IncludeAmountInExport { get; set; }
        bool Payed { get; set; }
        bool UseInPayroll { get; set; }
    }
}
