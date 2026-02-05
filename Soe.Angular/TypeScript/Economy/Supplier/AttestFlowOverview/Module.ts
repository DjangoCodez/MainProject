import '../Module';
import '../Invoices/Module';

import { supplierInvoiceDirective } from "./SupplierInvoiceDirective";

angular.module("Soe.Economy.Supplier.AttestFlowOverview.Module", ['Soe.Economy.Supplier', 'Soe.Economy.Supplier.Invoices.Module'])
    .directive("supplierInvoice", supplierInvoiceDirective);
