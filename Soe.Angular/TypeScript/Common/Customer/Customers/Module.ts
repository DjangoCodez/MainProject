import '../../../Common/GoogleMaps/Module';
import '../../../Shared/Economy/Module';

import { CustomerProductsDirectiveFactory } from "./Directives/CustomerProductsDirective";
import { CustomerValidationDirectiveFactory } from "./Directives/CustomerValidationDirective";
import { AccountingSettingsDirectiveFactory } from '../../Directives/AccountingSettings/AccountingSettingsDirective';
import { CommonCustomerService } from '../CommonCustomerService';

angular.module("Soe.Common.Customer.Customers.Module", ['Soe.Shared.Economy', 'Soe.Common.GoogleMaps'])
    .service("commonCustomerService", CommonCustomerService)
    .directive("customerProducts", CustomerProductsDirectiveFactory.create)
    .directive("customerValidation", CustomerValidationDirectiveFactory.create)
    .directive("accountingSettings", AccountingSettingsDirectiveFactory.create);
