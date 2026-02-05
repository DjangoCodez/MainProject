import'../../Module'
import '../../../Module';
import '../../../../Common/Customer/Invoices/Module';

import { CommonCustomerService } from "../../../../Common/Customer/CommonCustomerService";

angular.module("Soe.Economy.Customer.Invoice.InvoiceMatching.Module", ['Soe.Economy', 'Soe.Economy.Customer', 'Soe.Common.Customer.Invoices.Module'])
    .service("commonCustomerService", CommonCustomerService);