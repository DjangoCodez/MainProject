import './Module';

import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

angular.module("Soe.Economy.Accounting.AccountDims", ['Soe.Economy.Accounting.AccountDims.Module'])
    .config(/*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Economy/Accounting/AccountDims");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider.state("home", {
            url: "/{accountDimId}",
            templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
            controller: TabsController,
            controllerAs: "ctrl"
        })
        $urlRouterProvider.otherwise("/");
    });
