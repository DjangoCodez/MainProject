import '../../Module';

import { IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";

import { TabsController } from "./TabsController";

angular.module("Soe.Time.Payroll.AccountProvision.AccountProvisionTransactions", ['Soe.Time.Payroll'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Time/Payroll/AccountProvision/AccountProvisionTransactions");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/",
                templateUrl: urlHelper.getCoreViewUrl("tabs.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
