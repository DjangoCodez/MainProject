import '../Module';

import { TabsController } from "./TabsController";
import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";

angular.module("Soe.Billing.Products.IntrastatExport", ['Soe.Billing.Products'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Billing/Products/IntrastatExport/");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "",
                templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
