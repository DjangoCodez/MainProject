import '../../Module';
import '../../../Shared/Billing/Module';

import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";
import { ITranslationService } from '../../../Core/Services/TranslationService';
import { TermPartsLoaderProvider } from '../../../Core/Services/TermPartsLoader';

angular.module("Soe.Billing.Contracts.ContractGroups", ['Soe.Billing', 'Soe.Shared.Billing'])
    .config(/*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider, termPartsLoaderProvider: TermPartsLoaderProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Billing/Contracts/ContractGroups");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/{contractGroupId}",
                templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
        termPartsLoaderProvider.addPart('billing');
    })
    .run(/*@ngInject*/(translationService: ITranslationService) => {
        // Need to refresh here to load the added part
        translationService.refresh();
    });
