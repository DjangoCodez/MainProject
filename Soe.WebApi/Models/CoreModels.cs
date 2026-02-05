using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Soe.WebApi.Models
{
    [TSInclude]
    public class CommodityCodeUploadDTO
    {
        public DateTime SelectedDate { get; set; }
        public int Year { get; set; }
        public string FileString { get; set; }
        public string FileName { get; set; }
    }

    [TSInclude]
    public class UpdateEntityStatesModel
    {
        [Required]
        public Dictionary<int, bool> Dict { get; set; }
        public bool? SkipStateValidation { get; set; }
    }

    [TSInclude]
    public class SaveUserGridStateModel
    {
        [Required]
        public string Grid { get; set; }

        [Required]
        public string GridState { get; set; }
    }

    [TSInclude]
    public class  SaveCustomerModel
    {
        [Required]
        public CustomerDTO Customer { get; set; }
        public List<HouseholdTaxDeductionApplicantDTO> HouseHoldTaxApplicants { get; set; }
        public List<ExtraFieldRecordDTO> ExtraFields { get; set; }
    }

    [TSInclude]
    public class UpdateIsPrivatePerson
    {
        [Required]
        public int id { get; set; }
        public bool isPrivatePerson { get; set; }
    }

    [TSInclude]
    public class CustomerUpdateGrid
    {
        public UpdateEntityStatesModel model { get; set; }

        public List<UpdateIsPrivatePerson> items { get; set; }
    }

    public class IntModel
    {
        [Required]
        public int Id { get; set; }
    }

    public class IntIntModel
    {
        [Required]
        public int Id1 { get; set; }
        [Required]
        public int Id2 { get; set; }
    }

    public class IntIntStringModel
    {
        [Required]
        public int Id1 { get; set; }
        [Required]
        public int Id2 { get; set; }
        [Required]
        public string Id3 { get; set; }
    }

    public class ListIntModel
    {
        [Required]
        public List<int> Numbers { get; set; }
    }

    public class DictIntDateModel
    {
        [Required]
        public Dictionary<int, DateTime> Dict { get; set; }
    }

    [TSInclude]
    public class SaveAttestWorkFlowForInvoicesModel
    {
        [Required]
        public List<int> IdsToTransfer { get; set; }
        [Required]
        public bool SendMessage { get; set; }
    }

    [TSInclude]
    public class SaveAttestWorkFlowForMultipleInvoicesModel
    {
        [Required]
        public AttestWorkFlowHeadDTO AttestWorkFlowHead { get; set; }
        [Required]
        public List<int> InvoiceIds { get; set; }
    }

    [TSInclude]
    public class CustomerSearchModel
    {
        public int ActorCustomerId { get; set; }
        public string CustomerNr { get; set; }
        public string Name { get; set; }
        public string BillingAddress { get; set; }
        public string DeliveryAddress { get; set; }
        public string Note { get; set; }
    }

    public class CustomerStatisticsModel
    {
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public int OriginType { get; set; }
        [Required]
        public TermGroup_ChangeStatusGridAllItemsSelection AllItemSelection { get; set; }
    }

    [TSInclude]
    public class GeneralProductStatisticsModel
    {
        [Required]
        public int OriginType { get; set; }
        [Required]
        public DateTime FromDate { get; set; }
        [Required]
        public DateTime ToDate { get; set; }
    }
    [TSInclude]
    public class TextBlockModel
    {
        [Required]
        public int Entity { get; set; }
        [Required]
        public TextblockDTO TextBlock { get; set; }
        public List<CompTermDTO> translations { get; set; }
    }

    public class ChecklistRowModel
    {
        [Required]
        public List<ChecklistExtendedRowDTO> rows { get; set; }
        [Required]
        public SoeEntityType entity { get; set; }
        [Required]
        public int recordId { get; set; }
    }

    [TSInclude]
    public class ExtraFieldRecordsModel
    {
        [Required]
        public List<ExtraFieldRecordDTO> records { get; set; }
        [Required]
        public int recordId { get; set; }
        [Required]
        public int entity { get; set; }
    }

    public class GetProjectTimeBlocksModel
    {
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public int RecordId { get; set; }
        [Required]
        public int RecordType { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public bool LoadOnlyForEmployee { get; set; }
        [Required]
        public List<int> MandatoryEmployeeIds { get; set; }
        [Required]
        public TermGroup_InvoiceVatType VatType { get; set; }
    }

    [TSInclude]
    public class MoveProjectTimeBlocksToOrderModel
    {
        [Required]
        public int CustomerInvoiceId { get; set; }
        
        public int CustomerInvoiceRowId { get; set; }
        [Required]
        public List<int> ProjectTimeBlockIds { get; set; }
    }

    [TSInclude]
    public class MoveProjectTimeBlocksToDateModel
    {
        [Required]
        public string SelectedDate { get; set; }
        
        [Required]
        public List<int> ProjectTimeBlockIds { get; set; }
    }

    [TSInclude]
    public class GetProjectTimeBlocksForTimesheetModel
    {
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public DateTime From { get; set; }
        [Required]
        public DateTime To { get; set; }
        [Required]
        public List<int> Employees { get; set; }
        [Required]
        public List<int> Projects { get; set; }
        [Required]
        public List<int> Orders { get; set; }
        public bool groupByDate { get; set; }
        public bool incPlannedAbsence { get; set; }
        public bool incInternOrderText { get; set; }

        public List<int> EmployeeCategories { get; set; }
        public List<int> TimeDeviationCauses { get; set; }
    }

    public class OriginUsersModel
    {
        [Required]
        public int OriginId { get; set; }
        [Required]
        public List<OriginUserSmallDTO> OriginUsers { get; set; }
        public bool SendXEMail { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
    }

    public class CreateEmployeePostModel
    {
        [Required]
        public List<int> Numbers { get; set; }
        public DateTime FromDate { get; set; }
    }

    public class ImportModel
    {        
        public int importId { get; set; }
        public List<int> dataStorageIds { get; set; }
        public int accountYearId { get; set; }
        public int voucherSeriesId { get; set; }
        public int importDefinitionId { get; set; }
    }
    [TSInclude]
    public class ImportIOModel
    {
        public TermGroup_IOImportHeadType importHeadType { get; set; }
        public List<int> ioIds { get; set; }
        public bool UseAccountDistribution { get; set; }
        public bool useAccoungDims { get; set; }
        public int defaultDim1AccountId { get; set; }
        public int defaultDim2AccountId { get; set; }
        public int defaultDim3AccountId { get; set; }
        public int defaultDim4AccountId { get; set; }
        public int defaultDim5AccountId { get; set; }
        public int defaultDim6AccountId { get; set; }
    }

    [TSInclude]
    public class SearchCustomerInvoiceModel
    {
        public string Number { get; set; }
        public string ExternalNr { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
        public string InternalText { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }
        [Required]
        public int OriginType { get; set; }
        public int? CustomerId { get; set; }
        public int? ProjectId { get; set; }
        public int? UserId { get; set; }
        public int? IgnoreInvoiceId { get; set; }
        [Required]
        public bool IgnoreChildren { get; set; }
        public bool? IncludePreliminary { get; set; }
        public bool? IncludeVoucher { get; set; }
        public bool? FullyPaid { get; set; }
    }

    public class ValidateTimeRuleStructureModel
    {
        public List<TimeRuleFormulaWidget> Widgets { get; set; }
    }

    public class TrackChangesSearchModel
    {
        public SoeEntityType EntityType { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<string> Users { get; set; }
    }

    [TSInclude]
    public class RefreshBatchUpdateOptionsModel
    {
        [Required]
        public SoeEntityType EntityType { get; set; }
        [Required]
        public BatchUpdateDTO BatchUpdate { get; set; }
    }

    [TSInclude]
    public class PerformBatchUpdateModel
    {
        [Required]
        public SoeEntityType EntityType { get; set; }
        [Required]
        public List<BatchUpdateDTO> BatchUpdates { get; set; }
        [Required]
        public List<int> Ids { get; set; }
        public List<int> FilterIds { get; set; }
    }

    public class SaveApiSettingsModel
    {
        public List<ApiSettingDTO> Settings { get; set; }
    }

    [TSInclude]
    public class ParseRowsModel
    {
        public List<ImportFieldDTO> Fields;
        public ImportOptionsDTO Options;
        public string[][] Data;
    }
}