import '../../Module';

import { CommonCustomerService } from "../../../Common/Customer/CommonCustomerService";

angular.module("Soe.Billing.Statistics.Sales.Module", ['Soe.Billing'])
    .service("commonCustomerService", CommonCustomerService);
