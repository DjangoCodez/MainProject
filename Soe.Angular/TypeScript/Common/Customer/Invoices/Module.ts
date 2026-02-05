import '../../../Core/Module';
import "../../../Shared/Billing/Module";
import "../../../Shared/Economy/Module";

import { TermPartsLoaderProvider } from "../../../Core/Services/TermPartsLoader";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ContactPersonService } from "../../../Common/Directives/ContactPersons/ContactPersonService";
import { CustomerInvoiceValidationDirectiveFactory } from "./CustomerInvoiceValidationDirective";
import { DistributionRowsDirectiveFactory } from "../../../Common/Directives/DistributionRows/distributionrowsdirective";

angular.module("Soe.Common.Customer.Invoices.Module", ['Soe.Core', 'Soe.Shared.Billing', 'Soe.Shared.Economy'])
    .service("contactPersonService", ContactPersonService)
    .directive("customerInvoiceValidation", CustomerInvoiceValidationDirectiveFactory.create)
    .config( /*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('billing');
        termPartsLoaderProvider.addPart('economy');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });

