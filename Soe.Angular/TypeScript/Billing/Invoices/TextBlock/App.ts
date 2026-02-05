import '../../../Shared/Billing/Module';

import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

angular.module("Soe.Billing.Invoices.TextBlock", ['Soe.Shared.Billing'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Billing/Invoices/TextBlock");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/{textBlockId}",
                templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
