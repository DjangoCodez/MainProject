import { IUrlHelperServiceProvider } from '../../../../Core/Services/UrlHelperService';
import './Module';

import { TabsController } from "./TabsController";

angular.module("Soe.Billing.Purchase.Products.Products", ['Soe.Billing.Purchase.Products.Products.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Billing/Purchase/Products/Products");
        const urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/{supplierProductId}",
                templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
