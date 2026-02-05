import '../Module';
import '../../../Shared/Billing/Module';
import '../../../Common/Customer/Customers/Module';

import { CommonCustomerService } from "../../../Common/Customer/CommonCustomerService";

angular.module("Soe.Billing.Stock.StockSaldo.Module", ['Soe.Billing', 'Soe.Shared.Billing', 'Soe.Common.Customer.Customers.Module'])
    .service("commonCustomerService", CommonCustomerService)
