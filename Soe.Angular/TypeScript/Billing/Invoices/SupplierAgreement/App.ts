import '../../../Shared/Billing/Module';

import { TabsController } from "./TabsController";
import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";

angular.module("Soe.Billing.Invoices.SupplierAgreement", ['Soe.Shared.Billing'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Billing/Invoices/SupplierAgreement");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/",
                templateUrl: urlHelper.getCoreViewUrl("tabs1.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
