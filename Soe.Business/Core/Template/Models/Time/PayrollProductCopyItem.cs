using SoftOne.Soe.Common.Interfaces.Common;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class PayrollProductCopyItem : IPayrollProduct
    {
        public int PayrollProductId { get; set; }
        public int? SysPayrollProductId { get; set; }
        public int? ProductGroupId { get; set; }
        public int? ProductUnitId { get; set; }
        public int State { get; set; }

        public int PayrollType { get; set; }
        public int ResultType { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string ExternalNumber { get; set; }
        public string AccountingPrio { get; set; }
        public decimal Factor { get; set; }
        public bool AverageCalculated { get; set; }
        public bool DontUseFixedAccounting { get; set; }
        public bool Export { get; set; }
        public bool ExcludeInWorkTimeSummary { get; set; }
        public bool IncludeAmountInExport { get; set; }
        public bool Payed { get; set; }
        public bool UseInPayroll { get; set; }

        public List<PayrollProductSettingCopyItem> PayrollProductSettings { get; set; } = new List<PayrollProductSettingCopyItem>();
    }

    public class PayrollProductSettingCopyItem : IPayrollProductSetting
    {
        public int? ChildProductId { get; set; }
        public int? PayrollGroupId { get; set; }

        public int QuantityRoundingMinutes { get; set; }
        public int QuantityRoundingType { get; set; }
        public int CentRoundingType { get; set; }
        public int CentRoundingLevel { get; set; }
        public int TaxCalculationType { get; set; }
        public int TimeUnit { get; set; }
        public int PensionCompany { get; set; }
        public string AccountingPrio { get; set; }
        public bool CalculateSicknessSalary { get; set; }
        public bool CalculateSupplementCharge { get; set; }
        public bool DontPrintOnSalarySpecificationWhenZeroAmount { get; set; }
        public bool DontIncludeInRetroactivePayroll { get; set; }
        public bool DontIncludeInAbsenceCost { get; set; }
        public bool PrintOnSalarySpecification { get; set; }
        public bool PrintDate { get; set; }
        public bool UnionFeePromoted { get; set; }
        public bool VacationSalaryPromoted { get; set; }
        public bool WorkingTimePromoted { get; set; }

        public List<PayrollProductAccountStdCopyItem> PayrollProductAccountStdCopyItems { get; set; } = new List<PayrollProductAccountStdCopyItem>();
        public List<PayrollProductPriceFormulaCopyItem> PayrollProductPriceFormulaCopyItems { get; set; } = new List<PayrollProductPriceFormulaCopyItem>();
        public List<PayrollProductPriceTypeCopyItem> PayrollProductPriceTypeCopyItems { get; set; } = new List<PayrollProductPriceTypeCopyItem>();
    }

    public class PayrollProductAccountStdCopyItem
    {
        public int Type { get; set; }
        public int? Percent { get; set; }
        public int? AccountId { get; set; }
        public List<int> AccountInternalIds { get; set; }
    }

    public class PayrollProductPriceFormulaCopyItem
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PayrollPriceFormulaId { get; set; }
    }

    public class PayrollProductPriceTypeCopyItem
    {
        public int PayrollPriceTypeId { get; set; }
        public List<PayrollProductPriceTypePeriodCopyItem> PayrollProductPriceTypePeriodCopyItems { get; set; } = new List<PayrollProductPriceTypePeriodCopyItem>();
    }

    public class PayrollProductPriceTypePeriodCopyItem
    {
        public decimal? Amount { get; set; }
        public DateTime? FromDate { get; set; }
    }

}
