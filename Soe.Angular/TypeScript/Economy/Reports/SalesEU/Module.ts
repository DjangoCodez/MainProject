import '../../Module';

import { TermPartsLoaderProvider } from "../../../Core/Services/TermPartsLoader";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { AccountingService } from '../../../Shared/Economy/Accounting/AccountingService';

angular.module("Soe.Economy.Reports.SalesEU.Module", ['Soe.Economy'])
    .service("accountingService", AccountingService)
    .config( /*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {

        termPartsLoaderProvider.addPart('billing');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });