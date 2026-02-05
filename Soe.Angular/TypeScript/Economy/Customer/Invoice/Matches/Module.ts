import '../../Module';
import '../../../../Common/Customer/Invoices/Module';
import '../../../../Common/Customer/Payments/Module';

import { CommonCustomerService } from "../../../../Common/Customer/CommonCustomerService";
import { invoiceMatchMatchingsDirective } from "./Directives/MatchingsDirective";

angular.module("Soe.Economy.Customer.Invoice.Matches.Module", ['Soe.Economy.Customer', 'Soe.Common.Customer.Invoices.Module', 'Soe.Common.Customer.Payments.Module'])
    .service("commonCustomerService", CommonCustomerService)
    .directive("invoiceMatching", invoiceMatchMatchingsDirective);
