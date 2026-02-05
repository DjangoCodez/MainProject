import '../../Module';
import '../../Invoices/Module';
import '../../Payments/Module';

import { invoiceMatchMatchingsDirective } from "./Directives/MatchingsDirective";

angular.module("Soe.Economy.Supplier.Invoice.Matches.Module", ['Soe.Economy.Supplier', 'Soe.Economy.Supplier.Invoices.Module', 'Soe.Economy.Supplier.Payments.Module'])
    .directive("invoiceMatching", invoiceMatchMatchingsDirective);
