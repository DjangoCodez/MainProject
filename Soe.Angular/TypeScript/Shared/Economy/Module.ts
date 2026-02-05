import '../../Core/Module'
import '../../Common/GoogleMaps/Module';
import '../../Shared/Billing/Module';

import { AccountingService } from "./Accounting/AccountingService";
import { InventoryService } from "./Inventory/InventoryService";
import { SupplierService } from "./Supplier/SupplierService";
import { AddInvoiceToAttestFlowService } from '../../Common/Dialogs/addinvoicetoattestflow/addinvoicetoattestflowservice';
import { SupplierInvoiceValidationDirectiveFactory } from "./Supplier/Invoices/Directives/SupplierInvoiceValidationDirective";
import { expandableGridDirective } from "./Supplier/Invoices/Directives/ExpandableGridDirective";
import { linkToProjectDirective } from "./Supplier/Invoices/Directives/linkToProjectDirective";
import { linkToOrderDirective } from "./Supplier/Invoices/Directives/linkToOrderDirective";
import { InvoiceImageViewerDirective } from "./Supplier/Invoices/Directives/invoiceImageViewerDirective";
import { attestWorkflowDirective } from "./Supplier/Invoices/Directives/AttestWorkflowDirective";
import { attestHistoryDirective } from "./Supplier/Invoices/Directives/AttestHistoryDirective";
import { ContactAddressesDirectiveFactory } from '../../Common/Directives/ContactAddresses/ContactAddressesDirective';
import { DistributionRowsDirectiveFactory } from "../../Common/Directives/DistributionRows/distributionrowsdirective";
import { ContactPersonsDirecitve } from '../../Common/Directives/ContactPersons/ContactPersons';
import { SupplierPurchaseRowsDirectiveFactory } from './Supplier/Invoices/Directives/SupplierPurchaseRows/SupplierPurchaseRowsDirective';
import { SupplierInvoiceProductRowsDirectiveFactory } from './Supplier/Invoices/Directives/SupplierInvoiceProductRowsDirective';
import { AllocateCostsDirective, AllocateCostsDirectiveFactory } from './Supplier/Invoices/Directives/AllocateCostsDirective';

angular.module("Soe.Shared.Economy", ['Soe.Core', 'Soe.Shared.Billing', 'Soe.Common.GoogleMaps'])
    .service("addInvoiceToAttestFlowService", AddInvoiceToAttestFlowService)
    .service("accountingService", AccountingService)
    .service("inventoryService", InventoryService)
    .service("supplierService", SupplierService)
    .directive("supplierInvoiceValidation", SupplierInvoiceValidationDirectiveFactory.create)
    .directive("expandableGrid", expandableGridDirective)
    .directive("linkToProject", linkToProjectDirective)
    .directive("linkToOrder", linkToOrderDirective)
    .directive("invoiceImageViewer", InvoiceImageViewerDirective)
    .directive("contactAddresses", ContactAddressesDirectiveFactory.create)
    .directive("contactPersons", ContactPersonsDirecitve)
    .directive("attestHistory", attestHistoryDirective)
    .directive("attestWorkflow", attestWorkflowDirective)
    .directive("supplierInvoiceProductRows", SupplierInvoiceProductRowsDirectiveFactory.create)
    .directive("supplierPurchaseRows", SupplierPurchaseRowsDirectiveFactory.create)
    .directive("distributionRows", DistributionRowsDirectiveFactory.create)
    .directive("allocateCosts", AllocateCostsDirectiveFactory.create);