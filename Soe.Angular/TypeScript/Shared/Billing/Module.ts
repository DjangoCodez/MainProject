import '../../Core/Module'
import '../../Common/Customer/Customers/Module'
import '../../Shared/Economy/Module';

import { TermPartsLoaderProvider } from "../../Core/Services/TermPartsLoader";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { OrderService } from "./Orders/OrderService";
import { ProductService } from "./Products/ProductService";
import { InvoiceService } from "./Invoices/InvoiceService";
import { StockService } from "./Stock/StockService";
import { ProjectService } from "./Projects/ProjectService";
import { ImportService } from "./Import/ImportService";
import { InventoryService } from '../Economy/Inventory/InventoryService';
import { SelectProjectService } from "../../Common/Dialogs/SelectProject/SelectProjectService";
import { AccountingRowsDirectiveFactory } from '../../Common/Directives/AccountingRows/AccountingRowsDirective';
import { AccountingRowsValidationDirectiveFactory } from '../../Common/Directives/AccountingRows/AccountingRowsValidationDirective';
import { ProductRowsDirectiveFactory } from "./Directives/ProductRows/ProductRowsDirective";
import { ProductRowsValidationDirectiveFactory } from "./Directives/ProductRows/ProductRowsValidationDirective";
import { EditProductRowValidationDirectiveFactory } from "./Directives/ProductRows/EditProductRowValidationDirective";
import { SplitAccountingValidationDirectiveFactory } from "./Directives/ProductRows/SplitAccountingValidationDirective";
import { TimeProjectReportDirectiveFactory } from "../../Common/Directives/TimeProjectReport/TimeProjectReportDirective";
import { CommonCustomerService } from '../../Common/Customer/CommonCustomerService';
import { ChecklistsDirective } from "./Orders/Directives/Checklists/ChecklistsDirective";
import { SysWholesellerPricesFactory } from "./Directives/SysWholesellerPrices/SysWholesellerPrices";
import { AccountingPriorityDirectiveFactory } from './Products/Products/Directives/AccountingPriorityDirective';
import { StocksDirectiveFactory } from './Products/Products/Directives/StocksDirective';
import { PriceListsDirectiveFactory } from './Products/Products/Directives/PriceListsDirective';
import { ProductUnitConvertDirectiveFactory } from './Products/Products/Directives/ProductUnitConvertDirective';
import { CategoriesDirectiveFactory } from '../../Common/Directives/Categories/CategoriesDirective';
import { ProjectPersonsDirectiveFactory } from "./Projects/Directives/ProjectPersonsDirective";
import { AccountDimsDirectiveFactory } from '../../Common/Directives/accountdims/accountdimsdirective';
import { ProjectProductsDirectiveFactory } from "./Projects/Directives/ProjectProductsDirective";
import { ProjectTimeCodesFactory } from "./Projects/Directives/ProjectTimeCodesDirective";
import { ProjectValidationDirectiveFactory } from "./Projects/Directives/ProjectValidationDirective";
import { traceRowsDirective } from "../../Common/Directives/TraceRows/TraceRows";
import { OrderValidationDirectiveFactory } from "./Orders/ordervalidationdirective";
import { InvoiceValidationDirectiveFactory } from "./Invoices/InvoiceValidationDirective";
import { plannedShiftsDirective } from './Orders/Directives/PlannedShiftsDirective';
import { TranslationsDirectiveFactory } from "../../Common/Directives/Translations/TranslationsDirective";
import { ExpenseRowsDirectiveFactory } from "../../Common/Directives/ExpenseRows/ExpenseRows";
import { OrderSupplierInvoicesDirectiveFactory } from "../../Shared/Billing/Directives/SupplierInvoices/SupplierInvoices";
import { DocumentsDirectiveControllerFactory } from "../../Common/Directives/Documents/DocumentsDirective";
import { ProjectBudgetDirectiveFactory } from './Projects/Directives/ProjectBudgetDirective';
import { ExtraFieldRecordsDirectiveFactory } from '../../Common/Directives/ExtraFields/ExtraFieldsDirective';
import { SupplierService } from '../Economy/Supplier/SupplierService';
import { PurchaseService } from './Purchase/Purchase/PurchaseService';
import { SupplierProductService } from './Purchase/Purchase/SupplierProductService';
import { PurchaseRowsDirectiveFactory } from './Purchase/Directives/PurchaseRows/PurchaseRows';
import { DeliveryRowsDirectiveFactory } from './Purchase/Directives/DeliveryRows/DeliveryRows';
import { SelectSupplierService } from '../../Common/Dialogs/SelectSupplier/SelectSupplierService';
import { PurchaseCustomerInvoiceRowsDirectiveFactory } from './Directives/PurchaseCustomerInvoiceRows/PurchaseCustomerInvoiceRows';
import { SupplierProductDirectiveFactory } from './Products/Products/Directives/SupplierProductsDirective';
import { SupplierProductPricesDirectiveFactory } from './Purchase/Directives/SupplierProductPrices/SupplierProductPrices';
import { AccountDimsValidationDirectiveFactory } from "../../Common/Directives/AccountDims/AccountDimsValidationDirective";

angular.module("Soe.Shared.Billing", ["Soe.Core", "Soe.Shared.Economy", "Soe.Common.Customer.Customers.Module", "ui.bootstrap.contextMenu"])
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
    .directive("accountDimsValidation", AccountDimsValidationDirectiveFactory.create)
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
    .directive("supplierProducts", SupplierProductDirectiveFactory.create)
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
    .directive("supplierProductPrices", SupplierProductPricesDirectiveFactory.create)
    .directive("deliveryRows", DeliveryRowsDirectiveFactory.create)

    .config( /*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('billing');
        termPartsLoaderProvider.addPart('economy');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });