import './Module';

import { IUrlHelperServiceProvider } from '../../../../Core/Services/UrlHelperService';
import { TabsController } from "./TabsController";

angular.module("Soe.Manage.Attest.Customer.Roles", ['Soe.Manage.Attest.Customer.Roles.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Manage/Attest/Customer/Roles");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/",
                templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
