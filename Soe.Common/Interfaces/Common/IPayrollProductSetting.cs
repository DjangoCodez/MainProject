namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface IPayrollProductSetting
    {
        int? PayrollGroupId { get; set; }
        int? ChildProductId { get; set; }

        int QuantityRoundingMinutes { get; set; }
        int QuantityRoundingType { get; set; }
        int CentRoundingType { get; set; }
        int CentRoundingLevel { get; set; }
        int TaxCalculationType { get; set; }
        int TimeUnit { get; set; }
        int PensionCompany { get; set; }
        string AccountingPrio { get; set; }
        bool CalculateSicknessSalary { get; set; }
        bool CalculateSupplementCharge { get; set; }
        bool DontPrintOnSalarySpecificationWhenZeroAmount { get; set; }
        bool DontIncludeInRetroactivePayroll { get; set; }
        bool DontIncludeInAbsenceCost { get; set; }
        bool PrintOnSalarySpecification { get; set; }
        bool PrintDate { get; set; }
        bool UnionFeePromoted { get; set; }
        bool VacationSalaryPromoted { get; set; }
        bool WorkingTimePromoted { get; set; }
    }
}
