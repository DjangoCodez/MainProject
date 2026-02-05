using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Core
{
    public class ReportCopyItem
    {
        public int ReportId { get; set; }
        public int ReportNr { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ReportTemplateId { get; set; }
        public bool Standard { get; set; }
        public bool Original { get; set; }
        public SoeModule Module { get; set; }
        public bool IncludeAllHistoricalData { get; set; }
        public bool GetDetailedInformation { get; set; }
        public int NoOfYearsBackinPreviousYear { get; set; }
        public bool IncludeBudget { get; set; }
        public bool ShowInAccountingReports { get; set; }
        public int ExportType { get; set; }
        public int FileType { get; set; }
        public int GroupByLevel1 { get; set; }
        public int GroupByLevel2 { get; set; }
        public int GroupByLevel3 { get; set; }
        public int GroupByLevel4 { get; set; }
        public int SortByLevel1 { get; set; }
        public int SortByLevel2 { get; set; }
        public int SortByLevel3 { get; set; }
        public int SortByLevel4 { get; set; }
        public bool IsSortAscending { get; set; }
        public string Special { get; set; }
        public int? NrOfDecimals { get; set; }
        public ReportSelectionCopyItem ReportSelection { get; set; }
        public List<RoleCopyItem> ReportRolePermission { get; set; } = new List<RoleCopyItem>();

    }

    public class ReportSelectionCopyItem
    {
        public string ReportSelectionText { get; set; }
        public List<ReportSelectionValueCopyItem> ReportSelectionValueCopyItems { get; set; } = new List<ReportSelectionValueCopyItem>();
    }

    public class RoleCopyItem
    {
        public int RoleId { get; set; }
    }

    public class ReportSelectionValueCopyItem
    {
        public int ReportSelectionType { get; set; }
        public int? SelectFromInt { get; set; }
        public int? SelectToInt { get; set; }
        public string SelectFromStr { get; set; }
        public string SelectToStr { get; set; }
        public DateTime? SelectFromDate { get; set; }
        public DateTime? SelectToDate { get; set; }
    }
    public class ReportTemplateCopyItem
    {
        public int ReportTemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string Template { get; set; }
        public int SysReportTypeId { get; set; }
        public int SysTemplateTypeId { get; set; }
        public SoeModule Module { get; set; }
        public int GroupByLevel1 { get; set; }
        public int GroupByLevel2 { get; set; }
        public int GroupByLevel3 { get; set; }
        public int GroupByLevel4 { get; set; }
        public int SortByLevel1 { get; set; }
        public int SortByLevel2 { get; set; }
        public int SortByLevel3 { get; set; }
        public int SortByLevel4 { get; set; }
        public bool IsSortAscending { get; set; }
        public string Special { get; set; }
        public int? ReportNr { get; set; }
        public bool ShowOnlyTotals { get; set; }
        public bool ShowGroupingAndSorting { get; set; }
        public string ValidExportTypes { get; set; }
    }
}
