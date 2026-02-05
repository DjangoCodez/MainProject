using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class PayrollGroupCopyItem
    {
        public string Name { get; set; }
        public int? TimePeriodHeadId { get; set; }
        public int? OneTimeTaxFormulaId { get; set; }
        public int PayrollGroupId { get; set; }

        public List<PayrollGroupSettingCopyItem> PayrollGroupSettings { get; set; } = new List<PayrollGroupSettingCopyItem>();
        public List<PayrollGroupAccountStdCopyItem> PayrollGroupAccountStds { get; set; } = new List<PayrollGroupAccountStdCopyItem>();
        public List<PayrollGroupPriceTypeCopyItem> PayrollGroupPriceTypes { get; set; } = new List<PayrollGroupPriceTypeCopyItem>();
        public List<PayrollGroupPriceFormulaCopyItem> PayrollGroupPriceFormulas { get; set; } = new List<PayrollGroupPriceFormulaCopyItem>();
        public List<PayrollGroupVacationGroupCopyItem> PayrollGroupVacationGroups { get; set; } = new List<PayrollGroupVacationGroupCopyItem>();
        public List<PayrollGroupPayrollProductCopyItem> PayrollGroupPayrollProducts { get; set; } = new List<PayrollGroupPayrollProductCopyItem>();
        public List<PayrollGroupReportCopyItem> PayrollGroupReports { get; set; } = new List<PayrollGroupReportCopyItem>();
    }

    public class PayrollGroupSettingCopyItem
    {
        public int Type { get; set; }
        public int DataType { get; set; }
        public string Name { get; set; }
        public string StrData { get; set; }
        public int? IntData { get; set; }
        public decimal? DecimalData { get; set; }
        public bool? BoolData { get; set; }
        public DateTime? DateData { get; set; }
        public DateTime? TimeData { get; set; }
    }
    public class PayrollGroupAccountStdCopyItem
    {
        public int Type { get; set; }
        public decimal? Percent { get; set; }
        public decimal? FromInterval { get; set; }
        public decimal? ToInterval { get; set; }
        public int AccountId { get; set; }
    }

    public class PayrollGroupPriceTypeCopyItem
    {
        public int Sort { get; set; }
        public bool ShowOnEmployee { get; set; }
        public bool ReadOnlyOnEmployee { get; set; }
        public int PayrollPriceTypeId { get; set; }
        public List<PayrollGroupPriceTypePeriodCopyItem> PriceTypePeriods { get; set; } = new List<PayrollGroupPriceTypePeriodCopyItem>();
    }

    public class PayrollGroupPriceTypePeriodCopyItem
    {
        public decimal Amount { get; set; }
        public DateTime? FromDate { get; set; }
    }

    public class PayrollGroupPriceFormulaCopyItem
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool ShowOnEmployee { get; set; }
        public int PayrollPriceFormulaId { get; set; }
    }

    public class PayrollGroupVacationGroupCopyItem
    {
        public bool Default { get; set; }
        public int VacationGroupId { get; set; }
    }

    public class PayrollGroupPayrollProductCopyItem
    {
        public bool Distribute { get; set; }
        public int ProductId { get; set; }
    }

    public class PayrollGroupReportCopyItem
    {
        public int SysReportTemplateTypeId { get; set; }
        public int ReportId { get; set; }
    }
}
