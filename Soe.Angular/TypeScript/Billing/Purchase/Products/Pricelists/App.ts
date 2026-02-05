import { IUrlHelperServiceProvider } from '../../../../Core/Services/UrlHelperService';
import './Module';

import { TabsController } from "./TabsController";

angular.module("Soe.Billing.Purchase.Products.Pricelists", ['Soe.Billing.Purchase.Products.Pricelists.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Billing/Purchase/Products/Pricelists");
        const urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/{supplierPricelistId}",
                templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
