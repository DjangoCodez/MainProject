using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;

namespace Soe.WebApi.Models
{
    [TSInclude]
    public class CopyInvoiceProductModel
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public decimal PurchasePrice { get; set; }
        [Required]
        public decimal SalesPrice { get; set; }
        [Required]
        public string ProductUnit { get; set; }
        [Required]
        public int PriceListTypeId { get; set; }
        [Required]
        public int PriceListHeadId { get; set; }
        [Required]
        public string SysWholesellerName { get; set; }
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public PriceListOrigin Origin { get; set; }
    }
    [TSInclude]
    public class ProductsInStockModel
    {
        [Required]
        public List<int> ProductIds { get; set; }
        [Required]
        public int StockId { get; set; }
    }

    [TSInclude]
    public class SearchProductPricesModel
    {
        [Required]
        public int PriceListTypeId { get; set; }
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public int CurrencyId { get; set; }
        [Required]
        public string Number { get; set; }
        [Required]
        public int ProviderType { get; set; }
    }

    public class GetStockInventoryRowsModel
    {
        [Required]
        public int StockId { get; set; }
        [Required]
        public string ProductNrFrom { get; set; }
        [Required]
        public string ProductNrTo { get; set; }
        [Required]
        public int ShelfIdFrom { get; set; }
        [Required]
        public int ShelfIdTo { get; set; }
    }

    [TSInclude]
    public class ProductStatisticsModel
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int OriginType { get; set; }
        [Required]
        public TermGroup_ChangeStatusGridAllItemsSelection AllItemSelection { get; set; }
    }

    [TSInclude]
    public class SaveInvoiceProductModel
    {
        [Required]
        public InvoiceProductDTO invoiceProduct { get; set; }
        [Required]
        public List<PriceListDTO> priceLists { get; set; }
        [Required]
        public List<CompanyCategoryRecordDTO> categoryRecords { get; set; }
        public List<StockDTO> stocks { get; set; }
        public List<CompTermDTO> translations { get; set; }
        public List<ExtraFieldRecordDTO> extrafields { get; set; }
    }

    public class SavePriceListsModel
    {
        [Required]
        public int priceListTypeId { get; set; }
        [Required]
        public List<PriceListDTO> priceLists { get; set; }
        [Required]
        public List<PriceListDTO> deletedPriceLists { get; set; }

    }
    public class CreateEInvoiceModel
    {
        [Required]
        public int InvoiceId { get; set; }
        [Required]
        public bool download { get; set; }
        public bool OverrideFinvoiceOperatorWarning { get; set; }
    }

    public class SaveNewStockInventory
    {
        [Required]
        public StockInventoryHeadDTO InventoryHead { get; set; }
        [Required]
        public List<StockInventoryRowDTO> InventoryRows { get; set; }
    }
    [TSInclude]
    public class ImportStockBalances
    {
        [Required]
        public int StockInventoryHeadId { get; set; }
        [Required]
        public int WholesellerId { get; set; }
        [Required]
        public int StockId { get; set; }
        [Required]
        public bool CreateVoucher { get; set; }
        [Required]
        public string FileName { get; set; }

        public string FileString { get; set; }
        [Required]
        public List<byte[]> FileData { get; set; }
    }

    public class SaveOrderModel
    {
        [Required]
        public ExpandoObject ModifiedFields { get; set; }
        public List<ProductRowDTO> NewRows { get; set; }
        public List<ExpandoObject> ModifiedRows { get; set; }
        public List<ChecklistHeadRecordCompactDTO> ChecklistHeads { get; set; }
        public List<ChecklistExtendedRowDTO> ChecklistRows { get; set; }
        public List<OriginUserDTO> OriginUsers { get; set; }
        public List<FileUploadDTO> Files { get; set; }
        public bool DiscardConcurrencyCheck { get; set; }
        public bool RegenerateAccounting { get; set; }
        public bool SendXEMail { get; set; }
        public bool Crediting { get; set; }
        public bool AutoSave { get; set; }
    }

    [TSInclude]
    public class ProductsSimpleModel
    {
        [Required]
        public List<int> ProductIds { get; set; }
    }

    [TSInclude]
    public class SaveInvoiceProjectModel
    {
        [Required]
        public TimeProjectDTO invoiceProject { get; set; }
        [Required]
        public List<DecimalKeyValue> priceLists { get; set; }
        [Required]
        public List<CompanyCategoryRecordDTO> categoryRecords { get; set; }
        [Required]
        public List<AccountingSettingDTO> accountSettings { get; set; }
        [Required]
        public List<ProjectUserDTO> projectUsers { get; set; }
        public bool newPricelist { get; set; }
        public string pricelistName { get; set; }
    }
    [TSInclude]
    public class SavePurchaseStatus
    {
        public int PurchaseId { get; set; }
        public SoeOriginStatus Status { get; set; }
    }
    [TSInclude]
    public class SavePurchaseModel
    {
        [Required]
        public ExpandoObject ModifiedFields { get; set; }
        public List<OriginUserDTO> OriginUsers { get; set; }
        public List<PurchaseRowDTO> NewRows { get; set; }
        public List<ExpandoObject> ModifiedRows { get; set; }
    }
    [TSInclude]
    public class SendPurchaseEmail
    {
        public int PurchaseId { get; set; }
        public List<int> PurchaseIds { get; set; }
        public int ReportId { get; set; }
        public int EmailTemplateId { get; set; }
        public int? LangId { get; set; }
        public List<int> Recipients { get; set; }
        public string SingleRecipient { get; set; }
    }

    [TSInclude]
    public class GetFilteredEDIEntrysModel
    {
        public int Classification { get; set; }
        public int OriginType { get; set; }
        public List<int> BillingTypes { get; set; }
        public string BuyerId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string OrderNr { get; set; }
        public List<int> OrderStatuses { get; set; }
        public string SellerOrderNr { get; set; }
        public List<int> EdiStatuses { get; set; }
        public decimal Sum { get; set; }
        public string SupplierNrName { get; set; }
        public TermGroup_ChangeStatusGridAllItemsSelection? AllItemsSelection { get; set; }
    }

    [TSInclude]
    public class SupplierAgreementModel
    {
        [Required]
        public int WholesellerId { get; set; }
        public int PriceListTypeId { get; set; }
        public decimal GeneralDiscount { get; set; }
        public List<FileDTO> Files { get; set; }
    }

    public class CustomerInvoiceRowImportModel
    {
        [Required]
        public int WholesellerId { get; set; }
        public TermGroup_InvoiceRowImportType TypeId { get; set; }
        public int InvoiceId { get; set; }
        public int ActorCustomerId { get; set; }
        public byte[] Bytes { get; set; }
    }


    public class CompanyWholesellerSettingModel
    {
        public CompanyWholesellerDTO CompanyWholesellerDTO { get; set; }
        public List<string> CustomerNbrs { get; set; }
        public int ActorSupplierId { get; set; }
    }

    public class UpdateContractPricesModel
    {
        public List<int> InvoiceIds { get; set; }
        public int Rounding { get; set; }
        public decimal Percent { get; set; }
        public decimal Amount { get; set; }
    }

    public class ExpenseRowsModel
    {
        public int CustomerInvoiceId { get; set; }
        public List<ExpenseRowDTO> ExpenseRows { get; set; }
    }

    [TSInclude]
    public class FilterExpensesModel
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

        public List<int> EmployeeCategories { get; set; }
    }

    [TSInclude]
    public class ProductUnitModel
    {
        public ProductUnitDTO ProductUnit { get; set; }
        public List<CompTermDTO> Translations { get; set; }
    }

    [TSInclude]
    public class ProductUnitFileModel
    {
        public List<int> ProductIds { get; set; }
        public List<byte[]> FileData { get; set; }
    }

    [TSInclude]
    public class SearchCustomerInvoiceRowModel
    {
        public List<int> projects { get; set; }
        public List<int> orders { get; set; }
        public List<int> customers { get; set; }
        public List<int> orderTypes { get; set; }
        public List<int> orderContractTypes { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public bool onlyValid { get; set; }
        public bool onlyMine { get; set; }
        public int? customerInvoiceRowId { get; set; }
    }

    [TSInclude]
    public class GetProjectEmployeesModel
    {
        public bool AddEmptyRow { get; set; }
        public bool GetHidden { get; set; }
        public bool AddNoReplacementEmployee { get; set; }
        public int? IncludeEmployeeId { get; set; }
        public string FromDateString { get; set; }
        public string ToDateString { get; set; }
        public List<int> EmployeeCategories { get; set; }
    }

    public class TransferOrdersToInvoiceModel
    {
        public List<int> Ids { get; set; }
        public bool Merge { get; set; }
        public bool SetStatusToOrigin { get; set; }
        public int AccountYearId { get; set; }
    }

    public class OrderRowChangeAttestStateModel
    {
        [Required]
        public List<GenericType<int, int>> Items { get; set; }
        [Required]
        public int AttestStateId { get; set; }
    }

    public class BatchSplitTimeRowsModel
    {
        [Required]
        public List<GenericType<int, int>> Items { get; set; }
        [Required]
        public DateTime From { get; set; }
        [Required]
        public DateTime To { get; set; }
    }

    [TSInclude]
    public class SaveIntrastatTransactionModel
    {
        [Required]
        public List<IntrastatTransactionDTO> Transactions { get; set; }
        [Required]
        public int OriginId { get; set; }
        [Required]
        public int OriginType { get; set; }
    }
    [TSInclude]
    public class SavePriceListTypeModel
    {
        [Required]
        public PriceListTypeDTO PriceListType { get; set; }

        public List<PriceListDTO> PriceLists { get; set; }
    }

    [TSInclude]
    public class PriceListUpdateModel
    {
        public PriceUpdateDTO PriceUpdate { get; set; }
        public List<int> PriceListTypeIds { get; set; }
        public bool UpdateExisting { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string ProductNrFrom { get; set; }
        public string ProductNrTo { get; set; }
        public int MaterialCodeId { get; set; }
        public int VatType { get; set; }
        public int ProductGroupId { get; set; }
        public DateTime PriceComparisonDate { get; set; }
        public decimal QuantityFrom { get; set; }
        public decimal? QuantityTo { get; set; }
    }

    [TSInclude]
    public class SupplierProductPriceUpdateModel
    {
        public PriceUpdateDTO PriceUpdate { get; set; }
        public List<int> SupplierProductIds { get; set; }
        public bool UpdateExisting { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime PriceComparisonDate { get; set; }
        public int CurrencyId { get; set; }
        public decimal? QuantityFrom { get; set; }
        public decimal? QuantityTo { get; set; }
    }

    [TSInclude]
    public class SupplierNetPricesDeleteModel
    {
        public List<int> WholsellerNetPriceRowIds { get; set; }
    }
}