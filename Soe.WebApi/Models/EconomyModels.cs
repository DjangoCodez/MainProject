using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Soe.WebApi.Models
{
    [TSInclude]
    public class SaveAccountModel
    {
        [Required]
        public AccountEditDTO Account { get; set; }
        [Required]
        public List<CompTermDTO> Translations { get; set; }
        [Required]
        public List<AccountMappingDTO> AccountMappings { get; set; }
        [Required]
        public List<CategoryAccountDTO> CategoryAccounts { get; set; }
        public List<ExtraFieldRecordDTO> ExtraFields { get; set; }
        public bool? SkipStateValidation { get; set; }
    }
    [TSInclude]
    public class SaveAccountSmallModel
    {
        [Required]
        public string AccountNr { get; set; }
        [Required]
        public string Name { get; set; }
        public int AccountTypeId { get; set; }
        public int VatAccountId { get; set; }
        public int SruCode1Id { get; set; }
    }

    public class SaveSupplierInvoiceModel
    {
        [Required]
        public SupplierInvoiceDTO Invoice { get; set; }
        [Required]
        public List<AccountingRowDTO> AccountingRows { get; set; }
        public List<SupplierInvoiceProjectRowDTO> ProjectRows { get; set; }
        public List<SupplierInvoiceOrderRowDTO> OrderRows { get; set; }
        public List<PurchaseDeliveryInvoiceDTO> PurchaseInvoiceRows { get; set; }
        public List<SupplierInvoiceCostAllocationDTO> CostAllocationRows { get; set; }
        [Required]
        public bool CreateAttestVoucher { get; set; }
        public bool SkipInvoiceNrCheck { get; set; }
        public bool DisregardConcurrencyCheck { get; set; }
    }

    public class TransferSupplierInvoiceModel
    {
        [Required]
        public List<SupplierInvoiceGridDTO> Invoices { get; set; }
        [Required]
        public int AccountYearId { get; set; }
        [Required]
        public SoeOriginStatusChange OriginStatusChange { get; set; }
        public DateTime? BulkInvoiceDate { get; set; }
        public DateTime? BulkPayDate { get; set; }
    }

    public class SaveSupplierPaymentModel
    {
        [Required]
        public PaymentRowSaveDTO Payment { get; set; }
        [Required]
        public List<AccountingRowDTO> AccountingRows { get; set; }

        public int? MatchCodeId { get; set; }
    }

    public class TransferSupplierPaymentModel
    {
        [Required]
        public List<SupplierPaymentGridDTO> Payments { get; set; }
        [Required]
        public int AccountYearId { get; set; }
        [Required]
        public SoeOriginStatusChange originStatusChange { get; set; }
        public int PaymentMethodId { get; set; }
        public DateTime? bulkPayDate { get; set; }
        public bool SendPaymentFile { get; set; }
    }

    public class SendPaymentNotificationModel
    {
        [Required]
        public int PaymentMethodId{ get; set; }
        public string PageUrl { get; set; }
        [Required]
        public SoeOriginStatusClassification Classification { get; set; }
    }

    [TSInclude]
    public class SaveVoucherModel
    {
        [Required]
        public VoucherHeadDTO VoucherHead { get; set; }
        [Required]
        public List<AccountingRowDTO> AccountingRows { get; set; }

        public List<int> HouseholdRowIds { get; set; }
        public int? RevertVatVoucherId { get; set; }
        public List<FileUploadDTO> Files { get; set; }
    }
    [TSInclude]
    public class EditVoucherNrModel
    {
        [Required]
        public int VoucherHeadId { get; set; }
        [Required]
        public int NewVoucherNr { get; set; }
    }

    [TSInclude]
    public class SaveAccountDistributionModel
    {
        [Required]
        public AccountDistributionHeadDTO AccountDistributionHead { get; set; }
        [Required]
        public List<AccountDistributionRowDTO> AccountDistributionRows { get; set; }
    }

    [TSInclude]
    public class SaveAccountDimModel
    {
        [Required]
        public AccountDimDTO AccountDim { get; set; }
      
        public bool Reset { get; set; }
    }

    [TSInclude]
    public class TransferToAccountDistributionEntryModel
    {
        [Required]
        public List<AccountDistributionEntryDTO> AccountDistributionEntryDTOs { get; set; }
        [Required]
        public DateTime PeriodDate { get; set; }
        [Required]
        public int AccountDistributionType { get; set; }
    }

    [TSInclude]
    public class TransferAccountDistributionEntryToVoucherModel
    {
        [Required]
        public List<AccountDistributionEntryDTO> AccountDistributionEntryDTOs { get; set; }
        [Required]
        public int AccountDistributionType { get; set; }
    }

    [TSInclude]
    public class ReverseAccountDistributionEntryModel
    {
        [Required]
        public List<AccountDistributionEntryDTO> AccountDistributionEntryDTOs { get; set; }
        [Required]
        public int AccountDistributionType { get; set; }
    } 

    public class RestoreAccountDistributionEntryModel
    {
        [Required]
        public AccountDistributionEntryDTO AccountDistributionEntryDTO { get; set; }
        [Required]
        public int AccountDistributionType { get; set; }
    }

    [TSInclude]
    public class DeleteDistributionEntryModel
    {
        [Required]
        public List<AccountDistributionEntryDTO> AccountDistributionEntryDTOs { get; set; }
        [Required]
        public int AccountDistributionType { get; set; }
    }

    public class DeletePermanentlyDistributionEntryModel
    {
        [Required]
        public AccountDistributionEntryDTO AccountDistributionEntryDTO { get; set; }
        [Required]
        public int AccountDistributionType { get; set; }
    }

    [TSInclude]
    public class SaveAccountYearBalanceModel
    {
        [Required]
        public int AccountYearId { get; set; }
        [Required]
        public List<AccountYearBalanceFlatDTO> items { get; set; }
    }

    [TSInclude]
    public class SaveAccountYearModel
    {
        [Required]
        public AccountYearDTO AccountYear { get; set; }
        [Required]
        public List<VoucherSeriesDTO> VoucherSeries { get; set; }
        [Required]
        public bool KeepNumbers { get; set; }
    }

    [TSInclude]
    public class SaveSupplierModel
    {
        [Required]
        public SupplierDTO Supplier { get; set; }
        public List<FileUploadDTO> Files { get; set; }
        public List<ExtraFieldRecordDTO> ExtraFields { get; set; }
    }

    public class ChangeInvoiceSeqNrStatesModel
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public int SeqNr { get; set; }
    }

    public class SupplierInvoiceNrAlreadyExistModel
    {
        [Required]
        public int ActorId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public string InvoiceNr { get; set; }
    }

    public class SaveAnswersToAttestFlowRowModel
    {
        [Required]
        public int RowId { get; set; }
        public string Comment { get; set; }
        [Required]
        public bool Answer { get; set; }
        [Required]
        public int AccountYearId { get; set; }
    }

    public class SaveAnswersToAttestFlowModel
    {
        [Required]
        public List<int> InvoiceIds { get; set; }
        public string Comment { get; set; }
        [Required]
        public bool Answer { get; set; }
        [Required]
        public int AccountYearId { get; set; }

        public List<FileUploadDTO> Attachments { get; set; } = new List<FileUploadDTO>();
    }

    public class ReplaceAttestWorkFlowUserModel
    {
        [Required]
        public AttestFlow_ReplaceUserReason Reason { get; set; }
        [Required]
        public int DeletedWorkFlowRowId { get; set; }
        public string Comment { get; set; }
        [Required]
        public int ReplacementUserId { get; set; }
        [Required]
        public int InvoiceId { get; set; }
        [Required]
        public bool SendMail { get; set; }
    }

    public class TransferModel
    {
        [Required]
        public List<int> IdsToTransfer { get; set; }
        [Required]
        public int AccountYearId { get; set; }
        public string Guid { get; set; }
    }
    [TSInclude]
    public class TransferEdiStateModel
    {
        [Required]
        public List<int> IdsToTransfer { get; set; }
        [Required]
        public int StateTo { get; set; }
    }

    [TSInclude]
    public class TransferSupplierInvoiceRowsToOrderModel
    {
        [Required]
        public int CustomerInvoiceId { get; set; }
        [Required]
        public int SupplierInvoiceId { get; set; }
        [Required]
        public List<int> SupplierInvoiceProductRowIds { get; set; }
        [Required]
        public int WholesellerId { get; set; }
    }

    public class TransferSupplierInvoicesToOrderModel
    {
        [Required]
        public List<GenericType<int, decimal>> Items { get; set; }
        [Required]
        public bool TransferSupplierInvoiceRows { get; set; }
        public bool UseMiscProduct { get; set; }
    }

    public class HideUnhandledInvoicesModel
    {
        [Required]
        public List<int> InvoiceIds { get; set; }
    }

    public class SaveSupplierInvoiceAccountingRows
    {
        [Required]
        public List<AccountingRowDTO> accountingRows { get; set; }
        public Dictionary<string,string> currentDimIds { get; set; }
    }

    [TSInclude]
    public class GetResultPerPeriodModel
    {
        [Required]
        public string Key { get; set; }
        [Required]
        public int NoOfPeriods { get; set; }
        [Required]
        public int AccountYearId { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required]
        public bool GetPrevious { get; set; }
        public List<int> Dims { get; set; }

    }

    public class GetCustomerCentralCountersAndBalanceModel
    {
        [Required]
        public List<int> CounterTypes { get; set; }
        [Required]
        public int CustomerId { get; set; }

        public int AccountYearId { get; set; }
        public int BaseSysCurrencyId { get; set; }

    }

    [TSInclude]
    public class BlockPaymentModel
    {
        [Required]
        public int InvoiceId { get; set; }
        [Required]
        public bool Block { get; set; }
        public string Reason { get; set; }
    }

    [TSInclude]
    public class InvoiceTextActionModel 
    {
        [Required]
        public InvoiceTextType Type { get; set; }
        public int? InvoiceId { get; set; }
        public int? EdiEntryId { get; set; }

        [Required]
        public bool ApplyAction { get; set; }
        public string Reason { get; set; }
    }
    public class SupplierInvoiceCostAllocationModel
    {
        [Required]
        public int InvoiceId { get; set; }
        [Required]
        public List<SupplierInvoiceCostAllocationDTO> CostAllocationRows { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public int CustomerInvoiceId { get; set; }
        [Required]
        public int OrderSeqNr { get; set; }
    }

    [TSInclude]
    public class GetSupplierCentralCountersAndBalanceModel
    {
        [Required]
        public List<int> CounterTypes { get; set; }
        [Required]
        public int SupplierId { get; set; }
    }

    public class GetAttestGroupSuggestion
    {
        [Required]
        public int SupplierId { get; set; }

        public int ProjectId { get; set; }
        public int CostplaceAccountId { get; set; }
        public string ReferenceOur { get; set; }

    }
    [TSInclude]
    public class SavePaymentImportIODTOModel
    {
        public List<PaymentImportIODTO> items { get; set; }
        public DateTime bulkPayDate { get; set; }
        public int accountYearId { get; set; }
    }
    [TSInclude]
    public class SaveCustomerPaymentImportIODTOModel
    {
        public List<PaymentImportIODTO> items { get; set; }
        public DateTime bulkPayDate { get; set; }
        public int accountYearId { get; set; }
        public int paymentMethodId { get; set; }
    }

    [TSInclude]
    public class PaymentMethodsGetModel
    {
        public int[] OriginTypeIds { get; set; }
        public bool AddEmptyRow { get; set; }
    }

    public class SaveCustomerLedgerModel
    {
        [Required]
        public CustomerLedgerSaveDTO invoice { get; set; }

        [Required]
        public List<AccountingRowDTO> accountingRows { get; set; }
        public List<FileUploadDTO> files { get; set; }

    }

    public class SaveCustomerInvoiceModel
    {
        [Required]
        public CustomerInvoiceSaveDTO invoice { get; set; }

        [Required]
        public List<CustomerInvoiceRowDTO> invoiceRows { get; set; }

        [Required]
        public List<AccountingRowDTO> accountingRows { get; set; }
    }

    [TSInclude]
    public class CustomerInvoicesGridModel
    {
        [Required]
        public int Classification { get; set; }
        [Required]
        public int AllItemsSelection { get; set; }
        [Required]
        public int OriginType { get; set; }
        [Required]
        public bool LoadOpen { get; set; }
        [Required]
        public bool LoadClosed { get; set; }
        [Required]
        public bool OnlyMine { get; set; }
        [Required]
        public bool LoadActive { get; set; }
        [Required]
        public bool Billing { get; set; }
        public List<int> ModifiedIds { get; set; }
    }

    [TSInclude]
    public class InvoicesForProjectCentralModel
    {
        [Required]
        public int Classification { get; set; }
        [Required]
        public int OriginType { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public bool LoadChildProjects { get; set; }
        public DateTime? FromDate { get; set; } 
        public DateTime? ToDate { get; set; }   
        public List<int> InvoiceIds { get; set; }
    }

    public class InvoicesForCustomerCentralModel
    {
        [Required]
        public int Classification { get; set; }
        [Required]
        public int OriginType { get; set; }
        [Required]
        public int ActorCustomerId { get; set; }
        public bool OnlyMine { get; set; }
    }

    public class TransferCustomerInvoiceAndPaymentModel
    {//p.KeepFixedPriceOrderOpen, p.CheckPartianInvoicing, p.alInvoicingDeductLiftRows, p.CreateCopiesOfTransferedContractRows
        [Required]
        public List<CustomerInvoiceGridDTO> Items { get; set; }
        [Required]
        public int AccountYearId { get; set; }
        [Required]
        public SoeOriginStatusChange originStatusChange { get; set; }
        public int PaymentMethodId { get; set; }
        public int ClaimLevel { get; set; }
        public int? EmailTemplateId { get; set; }
        public int? ReportId { get; set; }
        public int? LanguageId { get; set; }
        public bool MergeInvoices { get; set; }
        public bool KeepFixedPriceOrderOpen { get; set; }
        public bool CheckPartialInvoicing { get; set; }        
        public bool CreateCopiesOfTransferedContractRows { get; set; }
        public DateTime? bulkPayDate { get; set; }
        public DateTime? bulkInvoiceDate { get; set; }
        public DateTime? bulkDueDate { get; set; }
        public DateTime? bulkVoucherDate { get; set; }
        public bool SetStatusToOrigin { get; set; }
        public bool MergePdfs { get; set; }
        public bool OverrideFinvoiceOperatorWarning { get; set; }
    }

    public class AutomaticDistributionModel
    {
        [Required]
        public List<CustomerInvoiceGridDTO> Items { get; set; }
    }

    public class UpdateHouseholdDeductionModel
    {
        [Required]
        public List<int> idsToUpdate { get; set; }
        public DateTime? bulkDate { get; set; }
        public int customerInvoiceId { get; set; }
        public int customerInvoiceRowId { get; set; }
        public decimal amount { get; set; }
    }

    public class GetHouseholdTaxDeductionFileModel
    {
        [Required]
        public List<HouseholdTaxDeductionFileRowDTO> Applications { get; set; }
        [Required]
        public int SeqNr { get; set; }
        [Required]
        public TermGroup_HouseHoldTaxDeductionType Type { get; set; }
    }

    public class ChangeProjectStatusModel
    {
        [Required]
        public List<int> Ids { get; set; }
        [Required]
        public int NewState { get; set; }
    }
    [TSInclude]
    public class ProjectSearchModel
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
        public string ManagerName { get; set; }
        public string OrderNr { get; set; }
        [Required]
        public bool OnlyActive { get; set; }
        [Required]
        public bool Hidden { get; set; }
        [Required]
        public bool ShowWithoutCustomer { get; set; }
        public bool LoadMine { get; set; }
        public int? CustomerId { get; set; }
        public bool ShowAllProjects { get; set; } = false;
    }
    [TSInclude]
    public class CalculateAccountBalanceForAccountsFromVoucherModel
    {
        public int accountYearId { get; set; }
    }

    [TSInclude]
    public class SaveInventoryWriteOffTemplateModel
    {
        [Required]
        public InventoryWriteOffTemplateDTO inventoryWriteOffTemplate { get; set; }
        public List<AccountingSettingDTO> accountSettings { get; set; }

    }

    [TSInclude]
    public class GetLiquidityPlanningModel
    {
        
        [Required]
        public DateTime From { get; set; }
        [Required]
        public DateTime To { get; set; }
        [Required]
        public DateTime? Exclusion { get; set; }
        [Required]
        public decimal Balance { get; set; }
        [Required]
        public bool Unpaid { get; set; }
        [Required]
        public bool PaidUnchecked { get; set; }
        [Required]
        public bool PaidChecked { get; set; }

    }

    public class EditedEdiModel
    {
        [Required]
        public int EdiEntryId { get; set; }
        [Required]
        public int SupplierId { get; set; }
        [Required]
        public int AttestGroupId { get; set; }
    }

    public class ScanningFeedbackModel
    {
        [Required]
        public string DocumentId { get; set; }
        [Required]
        public bool UnidentifiedSupplier { get; set; }
        [Required]
        public bool IncorrectFieldProposal { get; set; }
        [Required]
        public bool ValidationQuestions { get; set; }
        [Required]
        public bool Other { get; set; }
        [Required]
        public string Comment { get; set; }
    }
    
    [TSInclude]
    public class GetCSRResponseModel
    {
        [Required]
        public List<int> IdsToTransfer { get; set; }
        public int year { get; set; }
    }
    public class GetCSRReportModel
    {
        [Required]
        public List<int> IdsToTransfer { get; set; }
        public int year { get; set; }
    }

    public class SaveInventoryModel
    {
        [Required]
        public InventoryDTO inventory { get; set; }        
        [Required]
        public List<CompanyCategoryRecordDTO> categoryRecords { get; set; }
        
        public List<AccountingSettingDTO> accountSettings { get; set; }
        public int debtAccountId { get; set; }        
    }
    [TSInclude]
    public class SaveAdjustmentModel
    {
        [Required]
        public int inventoryId { get; set; }
        [Required]
        public TermGroup_InventoryLogType type { get; set; }
        [Required]
        public int voucherSeriesTypeId { get; set; }
        [Required]
        public decimal amount { get; set; }
        [Required]
        public DateTime date { get; set; }
        public string note { get; set; }
        [Required]
        public List<AccountingRowDTO> accountRowItems { get; set; }
    }
    [TSInclude]
    public class SaveInventoryNotesModel
    {
        [Required]
        public int InventoryId { get; set; }
        public string Description { get; set; } = "";
        public string Notes { get; set; } = "";

    }

    public class CopyCustomerInvoiceRowsModel
    {
        [Required]
        public int TargetId { get; set; }
        [Required]
        public SoeOriginType OriginType { get; set; }
        [Required]
        public List<CustomerInvoiceRowDTO> RowsToCopy { get; set; }
        public int? OriginId { get; set; }
        public bool? UpdateOrigin { get; set; }
        public bool? Recalculate { get; set; }
    }

    [TSInclude]
    public class CompanyGroupTransferModel
    {
        [Required]
        public CompanyGroupTransferType TransferType { get; set; }
        [Required]
        public int AccountYearId { get; set; }
        public int VoucherSeriesId { get; set; }
        public int PeriodFrom { get; set; }
        public int PeriodTo { get; set; }
        public bool IncludeIB { get; set; }
        public int? BudgetCompanyFrom { get; set; }
        public int? BudgetCompanyGroup { get; set; }
        public int? BudgetChild { get; set; }
    }

    public class CashPaymentModel
    {
        [Required]
        public int InvoiceId { get; set; }
        public List<CashPaymentDTO> Payments { get; set; }
        public int? MatchCodeId { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool SendEmail { get; set; }
        public string Email { get; set; }
        public bool UseRounding { get; set; }
    }
    [TSInclude]
    public class FInvoiceModel
    {
        [Required]
        public string FileName { get; set; }

        [Required]
        public string FileString { get; set; }

        [Required]
        public string Extention { get; set; }

        public SoeEntityType Entity { get; set; }

    }
}