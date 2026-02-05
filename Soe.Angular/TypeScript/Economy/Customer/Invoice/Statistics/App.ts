import '../../Module';
import '../../../../Common/Customer/Invoices/Module';

import { IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

angular.module("Soe.Economy.Customer.Invoice.Statistics", ['Soe.Economy.Customer', 'Soe.Common.Customer.Invoices.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Economy/Customer/Invoice/Statistics");
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