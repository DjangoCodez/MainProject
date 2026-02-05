import '../../Module';

import { CommonCustomerService } from "../../../Common/Customer/CommonCustomerService";

angular.module("Soe.Billing.Orders.HandleBilling.Module", ['Soe.Billing'])
    .service("commonCustomerService", CommonCustomerService);
