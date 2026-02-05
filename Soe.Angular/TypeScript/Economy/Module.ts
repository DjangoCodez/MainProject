import '../Core/Module';
import '../Shared/Economy/Module';

import { TermPartsLoaderProvider } from "../Core/Services/TermPartsLoader";
import { ITranslationService } from "../Core/Services/TranslationService";
import { SupplierService } from "../Shared/Economy/Supplier/SupplierService";
import { InventoryService } from "../Shared/Economy/Inventory/InventoryService";

angular.module("Soe.Economy", ['Soe.Economy.Common.Module'])
    .config( /*@ngInject*/(termPartsLoaderProvider: TermPartsLoaderProvider) => {

        termPartsLoaderProvider.addPart('economy');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });

//TODO: do we want a Soe.economy.common, or do we want people to use the actuall module needed, like in this case soe.economy.accounting?
angular.module("Soe.Economy.Common.Module", ['Soe.Shared.Economy'])