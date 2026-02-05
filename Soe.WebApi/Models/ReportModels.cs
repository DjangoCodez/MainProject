using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Soe.WebApi.Models
{
    public class SaveReportUrlModel
    {
        [Required]
        public string Guid { get; set; }
        [Required]
        public string Url { get; set; }
        [Required]
        public int ReportId { get; set; }
        [Required]
        public int SysReportTemplateTypeId { get; set; }
    }

    public class SaveReportTemplateModel
    {
        [Required]
        public ReportTemplateDTO ReportTemplate { get; set; }
        public byte[] TemplateData { get; set; }
        [Required]
        public bool IsSystem { get; set; }
    }

    public class DeleteReportTemplateModel
    {
        [Required]
        public ReportTemplateDTO ReportTemplate { get; set; }
    }
    [TSInclude]
    public class GetReportsForTypesModel
    {
        [Required]
        public List<int> ReportTemplateTypeIds { get; set; }
        [Required]
        public bool OnlyOriginal { get; set; }
        [Required]
        public bool OnlyStandard { get; set; }
        public SoeModule? Module { get; set; }
    }

    public class GetFilteredEmployeesModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<int> EmployeeGroupIds { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<int> VacationGroupIds { get; set; }
        public List<int> PayrollGroupIds { get; set; }
        public List<int> AccountIds { get; set; }
        public List<int> TimePeriodIds { get; set; }

        public bool IncludeInactive { get; set; }
        public bool OnlyInactive { get; set; }
        public bool IncludeEnded { get; set; }
        public bool IncludeVacant { get; set; }
        public bool IncludeHidden { get; set; }
        public bool IncludeSecondary { get; set; }
        public TermGroup_EmployeeSelectionAccountingType AccountingType { get; set; }

        public SoeReportTemplateType SoeReportTemplateType { get; set; }
    }

    public class GetOrderPrintUrlModel
    {
        [Required]
        public List<int> InvoiceIds { get; set; }

        [Required]
        public List<int> EmailRecipients { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public int LanguageId { get; set; }

        [Required]
        public string InvoiceNr { get; set; }

        [Required]
        public int ActorCustomerId { get; set; }

        [Required]
        public OrderInvoiceRegistrationType RegistrationType { get; set; }

        [Required]
        public bool InvoiceCopy { get; set; }
    }
    [TSInclude]
    public class GetPurchasePrintUrlModel
    {
        [Required]
        public List<int> PurchaseIds { get; set; }

        [Required]
        public List<int> EmailRecipients { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public int LanguageId { get; set; }
    }
    public class GetIOPrintUrlModel
    {
        [Required]
        public List<int> IoIds { get; set; }
        [Required]
        public int ReportId { get; set; }
        [Required]
        public int SysReportTemplateTypeId { get; set; }
    }

    public class GetProductListPrintUrlModel
    {
        [Required]
        public List<int> productIds { get; set; }
        [Required]
        public int ReportId { get; set; }
        [Required]
        public int SysReportTemplateTypeId { get; set; }
    }

    public class GetStockInventoryPrintUrlModel
    {
        [Required]
        public List<int> StockInventoryIds { get; set; }
        [Required]
        public int ReportId { get; set; }
    }

    public class GetOrderPrintUrlSingleModel
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public List<int> EmailRecipients { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public int LanguageId { get; set; }

        [Required]
        public string InvoiceNr { get; set; }

        [Required]
        public int ActorCustomerId { get; set; }

        [Required]
        public bool PrintTimeReport { get; set; }

        [Required]
        public bool IncludeOnlyInvoicedTime { get; set; }

        [Required]
        public OrderInvoiceRegistrationType RegistrationType { get; set; }

        [Required]
        public bool InvoiceCopy { get; set; }
        public bool AsReminder { get; set; }
        public int EmailTemplateId { get; set; }
        public bool AddAttachmentsToEinvoice { get; set; }
        public List<int> AttachmentIds { get; set; }
        public List<int> ChecklistIds { get; set; }
        public bool MergePdfs { get; set; }
        public string SingleRecipient { get; set; }
    }

    public class GetProjectTransactionsPrintUrlModel
    {
        [Required]
        public int ReportId { get; set; }

        [Required]
        public int SysReportTemplateTypeId { get; set; }

        [Required]
        public int ExportType { get; set; }

        public string Dim2From { get; set; }
        public int Dim2Id { get; set; }
        public string Dim2To { get; set; }
        public string Dim3From { get; set; }
        public int Dim3Id { get; set; }
        public string Dim3To { get; set; }
        public string Dim4From { get; set; }
        public int Dim4Id { get; set; }
        public string Dim4To { get; set; }
        public string Dim5From { get; set; }
        public int Dim5Id { get; set; }
        public string Dim5To { get; set; }
        public string Dim6From { get; set; }
        public int Dim6Id { get; set; }
        public string Dim6To { get; set; }
        public string EmployeeNrFrom { get; set; }
        public string EmployeeNrTo { get; set; }
        public bool IncludeChildProjects { get; set; }
        public string InvoiceNrFrom { get; set; }
        public string InvoiceNrTo { get; set; }
        public string InvoiceProductNrFrom { get; set; }
        public string InvoiceProductNrTo { get; set; }
        public DateTime? InvoiceTransactionDateFrom { get; set; }
        public DateTime? InvoiceTransactionDateTo { get; set; }
        public string OfferNrFrom { get; set; }
        public string OfferNrTo { get; set; }
        public string OrderNrFrom { get; set; }
        public string OrderNrTo { get; set; }
        public string PayrollProductNrFrom { get; set; }
        public string PayrollProductNrTo { get; set; }
        public DateTime? PayrollTransactionDateFrom { get; set; }
        public DateTime? PayrollTransactionDateTo { get; set; }
        public List<int> ProjectIds { get; set; }
    }

    public class DeleteReportGroupsModel
    {
        public List<int> ReportGroupIds { get; set; }
    }

    public class DeleteReportHeadersModel
    {
        public List<int> ReportHeaderIds { get; set; }
    }

    public class GetTimeEmployeeSchedulePrintUrlModel
    {
        public List<int> EmployeeIds { get; set; }
        public List<int> ShiftTypeIds { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int ReportId { get; set; }
        public SoeReportTemplateType ReportTemplateType { get; set; }
    }

    public class GetTimeScheduleTasksAndDeliverysReportPrintUrlModel
    {
        public List<int> TimeScheduleTaskIds { get; set; }
        public List<int> TimeScheduleDeliveryHeadIds { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool IsDayView { get; set; }
    }

    public class ReportProjectSearchModel
    {
        public List<int> StatusIds { get; set; }
        public List<int> CategoryIds { get; set; }
        public bool SetStatusName { get; set; }
        public DateTime? StopDate { get; set; }
        public bool WithoutStopDate { get; set; }
    }

    public class HouseholdTaxDeductionPrintUrlModel
    {
        [Required]
        public int ReportId { get; set; }

        [Required]
        public int SysReportTemplateTypeId { get; set; }

        [Required]
        public int NextSequenceNumber { get; set; }

        [Required]
        public bool UseGreen { get; set; }

        [Required]
        public List<int> CustomerInvoiceRowIds { get; set; }
    }

    [TSInclude]
    public class GenericPrintUrlModel
    {
        [Required]
        public List<int> ItemIds { get; set; }
        [Required]
        public int ReportId { get; set; }
        [Required]
        public int SysReportTemplateTypeId { get; set; }
        public List<int> SecondaryItemIds { get; set; }
    }
}