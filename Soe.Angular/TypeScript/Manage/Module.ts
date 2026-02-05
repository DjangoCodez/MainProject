import '../Core/Module';

import { TermPartsLoaderProvider } from "../Core/Services/TermPartsLoader";
import { ITranslationService } from "../Core/Services/TranslationService";

angular.module("Soe.Manage", ['Soe.Core'])
    .config( /*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {
        termPartsLoaderProvider.addPart('manage');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });
