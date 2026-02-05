import '../../Module';
import '../../../Common/GoogleMaps/Module';

import { ContactAddressesDirectiveFactory } from '../../../Common/Directives/ContactAddresses/ContactAddressesDirective';

import { CompanyService } from "../CompanyService";
import { PaymentInformationDirectiveFactory } from './Directives/PaymentInformationDirective';
import { TermPartsLoaderProvider } from '../../../Core/Services/termpartsloader';
import { ITranslationService } from '../../../Core/Services/TranslationService';

angular.module("Soe.Manage.Company.Company.Module", ['Soe.Manage', 'Soe.Core', 'Soe.Common.GoogleMaps'])
    .service("companyService", CompanyService)
    .directive("paymentInformation", PaymentInformationDirectiveFactory.create)
    .directive("contactAddresses", ContactAddressesDirectiveFactory.create)
    .config(/*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('economy');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });
