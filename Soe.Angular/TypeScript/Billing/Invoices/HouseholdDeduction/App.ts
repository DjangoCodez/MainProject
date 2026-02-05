import '../../../Core/Module';
import '../../../Shared/Billing/Module';
import '../../../Shared/Economy/Module';

import { TermPartsLoaderProvider } from "../../../Core/Services/TermPartsLoader";
import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";
import { ITranslationService } from '../../../Core/Services/TranslationService';

angular.module("Soe.Billing.Invoices.HouseholdDeduction", ['Soe.Core', 'Soe.Shared.Billing', 'Soe.Shared.Economy'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider, termPartsLoaderProvider: TermPartsLoaderProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Billing/Invoices/HouseholdDeduction");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/{customerInvoiceId}",
                templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            })
        $urlRouterProvider.otherwise("/");
        termPartsLoaderProvider.addPart('economy');
        termPartsLoaderProvider.addPart('billing');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });
