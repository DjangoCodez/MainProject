import '../../Module';

import { TermPartsLoaderProvider } from "../../../Core/Services/TermPartsLoader";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { CommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { OrderService } from '../../../Shared/Billing/Orders/OrderService';
import { ProductService } from '../../../Shared/Billing/Products/ProductService';
import { PurchaseService } from '../../../Shared/Billing/Purchase/Purchase/PurchaseService';
import { InvoiceService } from '../../../Shared/Billing/Invoices/InvoiceService';

import { StockService } from "../../../Shared/Billing/Stock/StockService";
import { InventoryService } from '../../../Shared/Economy/Inventory/InventoryService';
import { SupplierService } from '../../../Shared/Economy/Supplier/SupplierService';
import { SupplierProductService } from '../../../Shared/Billing/Purchase/Purchase/SupplierProductService';
import { ProductRowsDirectiveFactory } from '../../../Shared/Billing/Directives/ProductRows/ProductRowsDirective';
import { AccountingRowsDirectiveFactory } from '../../../Common/Directives/AccountingRows/AccountingRowsDirective';
import { traceRowsDirective } from '../../../Common/Directives/TraceRows/TraceRows';
import { DocumentsDirectiveControllerFactory } from '../../../Common/Directives/Documents/DocumentsDirective';
import { AccountDimsDirectiveFactory } from '../../../Common/Directives/accountdims/accountdimsdirective';
import { AccountingRowsValidationDirectiveFactory } from '../../../Common/Directives/AccountingRows/AccountingRowsValidationDirective';
import { ProductRowsValidationDirectiveFactory } from '../../../Shared/Billing/Directives/ProductRows/ProductRowsValidationDirective';
import { EditProductRowValidationDirectiveFactory } from '../../../Shared/Billing/Directives/ProductRows/EditProductRowValidationDirective';
import { SplitAccountingValidationDirectiveFactory } from '../../../Shared/Billing/Directives/ProductRows/SplitAccountingValidationDirective';
import { OrderValidationDirectiveFactory } from '../../../Shared/Billing/Orders/ordervalidationdirective';
import { InvoiceValidationDirectiveFactory } from '../../../Shared/Billing/Invoices/InvoiceValidationDirective';
import { TranslationsDirectiveFactory } from '../../../Common/Directives/Translations/TranslationsDirective';
import { PurchaseRowsDirectiveFactory } from '../../../Shared/Billing/Purchase/Directives/PurchaseRows/PurchaseRows';
import { DeliveryRowsDirectiveFactory } from '../../../Shared/Billing/Purchase/Directives/DeliveryRows/DeliveryRows';
import { ExtraFieldRecordsDirectiveFactory } from '../../../Common/Directives/ExtraFields/ExtraFieldsDirective';
import { ProjectBudgetDirectiveFactory } from '../../../Shared/Billing/Projects/Directives/ProjectBudgetDirective';
import { OrderSupplierInvoicesDirectiveFactory } from '../../../Shared/Billing/Directives/SupplierInvoices/SupplierInvoices';
import { PurchaseCustomerInvoiceRowsDirectiveFactory } from '../../../Shared/Billing/Directives/PurchaseCustomerInvoiceRows/PurchaseCustomerInvoiceRows';
import { ProjectService } from '../../../Shared/Billing/Projects/ProjectService';
import { ImportService } from '../../../Shared/Billing/Import/ImportService';
import { SelectProjectService } from '../../../Common/Dialogs/SelectProject/SelectProjectService';
import { SelectSupplierService } from '../../../Common/Dialogs/SelectSupplier/SelectSupplierService';
import { TimeProjectReportDirectiveFactory } from '../../../Common/Directives/TimeProjectReport/TimeProjectReportDirective';
import { ChecklistsDirective } from '../../../Shared/Billing/Orders/Directives/Checklists/ChecklistsDirective';
import { SysWholesellerPricesFactory } from '../../../Shared/Billing/Directives/SysWholesellerPrices/SysWholesellerPrices';
import { AccountingPriorityDirectiveFactory } from '../../../Shared/Billing/Products/Products/Directives/AccountingPriorityDirective';
import { StocksDirectiveFactory } from '../../../Shared/Billing/Products/Products/Directives/StocksDirective';
import { PriceListsDirectiveFactory } from '../../../Shared/Billing/Products/Products/Directives/PriceListsDirective';
import { ProductUnitConvertDirectiveFactory } from '../../../Shared/Billing/Products/Products/Directives/ProductUnitConvertDirective';
import { CategoriesDirectiveFactory } from '../../../Common/Directives/Categories/CategoriesDirective';
import { ProjectPersonsDirectiveFactory } from '../../../Shared/Billing/Projects/Directives/ProjectPersonsDirective';
import { ProjectProductsDirectiveFactory } from '../../../Shared/Billing/Projects/Directives/ProjectProductsDirective';
import { ProjectTimeCodesFactory } from '../../../Shared/Billing/Projects/Directives/ProjectTimeCodesDirective';
import { ProjectValidationDirectiveFactory } from '../../../Shared/Billing/Projects/Directives/ProjectValidationDirective';
import { plannedShiftsDirective } from '../../../Shared/Billing/Orders/Directives/PlannedShiftsDirective';
import { ExpenseRowsDirectiveFactory } from '../../../Common/Directives/ExpenseRows/ExpenseRows';
import { ReportMetaFieldsFactory } from '../../../Common/Directives/ReportMetaFields/ReportMetaFieldsDirective';
import { ReportFieldSettingFactory } from '../../../Common/Directives/ReportFieldSetting/ReportFieldSettingDirective';

angular.module("Soe.Billing.Reports.eDistribution.Module", ['Soe.Billing'])
    .service("orderService", OrderService)
    .service("productService", ProductService)
    .service("invoiceService", InvoiceService)
    .service("stockService", StockService)
    .service("projectService", ProjectService)
    .service("importService", ImportService)
    .service("inventoryService", InventoryService)
    .service("purchaseService", PurchaseService)
    .service("supplierProductService", SupplierProductService)
    .service("selectProjectService", SelectProjectService)
    .service("commonCustomerService", CommonCustomerService)
    .service("supplierService", SupplierService)
    .service("selectSupplierService", SelectSupplierService)
    .directive("accountingRows", AccountingRowsDirectiveFactory.create)
    .directive("accountDims", AccountDimsDirectiveFactory.create)
    .directive("accountingRowsValidation", AccountingRowsValidationDirectiveFactory.create)
    .directive("productRows", ProductRowsDirectiveFactory.create)
    .directive("productRowsValidation", ProductRowsValidationDirectiveFactory.create)
    .directive("editProductRowValidation", EditProductRowValidationDirectiveFactory.create)
    .directive("splitAccountingValidation", SplitAccountingValidationDirectiveFactory.create)
    .directive("timeProjectReport", TimeProjectReportDirectiveFactory.create)
    .directive("checklists", ChecklistsDirective.create)
    .directive("sysWholesellerPrices", SysWholesellerPricesFactory.create)
    .directive("accountingPriority", AccountingPriorityDirectiveFactory.create)
    .directive("priceLists", PriceListsDirectiveFactory.create)
    .directive("stocks", StocksDirectiveFactory.create)
    .directive("productUnitConvert", ProductUnitConvertDirectiveFactory.create)
    .directive("categories", CategoriesDirectiveFactory.create)
    .directive("projectPersons", ProjectPersonsDirectiveFactory.create)
    .directive("projectProducts", ProjectProductsDirectiveFactory.create)
    .directive("projectTimeCodes", ProjectTimeCodesFactory.create)
    .directive("projectValidation", ProjectValidationDirectiveFactory.create)
    .directive("traceRows", traceRowsDirective)
    .directive("plannedShifts", plannedShiftsDirective)
    .directive("orderValidation", OrderValidationDirectiveFactory.create)
    .directive("invoiceValidation", InvoiceValidationDirectiveFactory.create)
    .directive("compTerms", TranslationsDirectiveFactory.create)
    .directive("expenseRows", ExpenseRowsDirectiveFactory.create)
    .directive("orderSupplierInvoices", OrderSupplierInvoicesDirectiveFactory.create)
    .directive("purchaseCustomerInvoiceRows", PurchaseCustomerInvoiceRowsDirectiveFactory.create)
    .directive("soeDocuments", DocumentsDirectiveControllerFactory.create)
    .directive("projectBudget", ProjectBudgetDirectiveFactory.create)
    .directive("extraFields", ExtraFieldRecordsDirectiveFactory.create)
    .directive("purchaseRows", PurchaseRowsDirectiveFactory.create)
    .directive("deliveryRows", DeliveryRowsDirectiveFactory.create)
    .directive("soeReportMetaFields", ReportMetaFieldsFactory.create)
    .directive("reportFieldSetting", ReportFieldSettingFactory.create)
    .config( /*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {

        termPartsLoaderProvider.addPart('economy');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });