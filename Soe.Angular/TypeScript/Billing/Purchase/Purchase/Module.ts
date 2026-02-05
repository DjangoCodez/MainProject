import '../../Module';
import '../../../Shared/Billing/Module';

import { TermPartsLoaderProvider } from "../../../Core/Services/TermPartsLoader";
import { ITranslationService } from "../../../Core/Services/TranslationService"; 
import { PurchaseService } from '../../../Shared/Billing/Purchase/Purchase/PurchaseService';

angular.module("Soe.Billing.Purchase.Purchase.Module", ['Soe.Billing', 'Soe.Shared.Billing'])
    .service("purchaseService", PurchaseService)
    .config( /*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('economy');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });