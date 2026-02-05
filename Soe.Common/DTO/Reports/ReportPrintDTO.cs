namespace SoftOne.Soe.Common.DTO.Reports
{
    using SoftOne.Soe.Common.Attributes;
    using SoftOne.Soe.Common.Util;
    using System;
    using System.Collections.Generic;


    [TSInclude]
    public class ReportPrintDTO
    {
        public int ReportId { get; set; }
        public List<int> Ids { get; set; }
        public bool Queue { get; set; }
        public bool ReturnAsBinary { get; set; }
        public TermGroup_ReportExportType exportType { get; set; }
    }

    [TSInclude]
    public class ProjectPrintDTO : ReportPrintDTO
    {
        public SoeReportTemplateType SysReportTemplateTypeId { get; set; }
        public bool IncludeChildProjects { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    [TSInclude]
    public class ProjectTimeBookPrintDTO : ReportPrintDTO
    {
        public int ProjectId { get; set; }
        public int InvoiceId { get; set; }
        public bool IncludeOnlyInvoiced { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    [TSInclude]
    public class BalanceListPrintDTO : ReportPrintDTO
    {
        public CompanySettingType CompanySettingType { get; set; }

        public List<int> PaymentRowIds { get; set; }

    }

    [TSInclude]
    public class HouseholdTaxDeductionPrintDTO : ReportPrintDTO
    {
        public SoeReportTemplateType SysReportTemplateTypeId { get; set; }

        public int SequenceNumber { get; set; }

        public bool UseGreen { get; set; }

    }

    [TSInclude]
    public class CustomerInvoicePrintDTO : ReportPrintDTO
    {
        public SoeReportTemplateType SysReportTemplateTypeId { get; set; }
        public int[] AttachmentIds { get; set; }
        public int[] ChecklistIds { get; set; }
        public bool PrintTimeReport { get; set; }
        public bool IncludeOnlyInvoiced { get; set; }
        public OrderInvoiceRegistrationType OrderInvoiceRegistrationType { get; set; }
        public bool InvoiceCopy { get; set; }
        public bool AsReminder { get; set; }
        public bool MergePdfs { get; set; }
        public int? ReportLanguageId { get; set; }
    }
}
