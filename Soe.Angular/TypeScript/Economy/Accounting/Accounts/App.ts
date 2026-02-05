import '../Module';

import { TabsController } from "./TabsController";
import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";

angular.module("Soe.Economy.Accounting.Accounts", ['Soe.Economy.Accounting'])
    .config(/*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Economy/Accounting/Accounts");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider.state("home", {
            url: "/{accountId}",
            templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
            controller: TabsController,
            controllerAs: "ctrl"
        })
        $urlRouterProvider.otherwise("/");
    })
