import '../Customers/Module';
import '../../../Core/Module';
import "../../../Shared/Billing/Module";
import "../../../Shared/Economy/Module";

import { TermPartsLoaderProvider } from "../../../Core/Services/TermPartsLoader";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { CustomerPaymentValidationDirectiveFactory } from "./CustomerPaymentValidationDirective";
import { CommonCustomerService } from "../CommonCustomerService";

angular.module("Soe.Common.Customer.Payments.Module", ['Soe.Core', 'Soe.Shared.Billing', 'Soe.Shared.Economy', 'Soe.Common.Customer.Customers.Module'])
    .directive("customerPaymentValidation", CustomerPaymentValidationDirectiveFactory.create)
    .service("commonCustomerService", CommonCustomerService)
    .config( /*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {

        termPartsLoaderProvider.addPart('billing');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });      
