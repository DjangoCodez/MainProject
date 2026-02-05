import './Module';

import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

angular.module("Soe.Manage.Attest.Time.States", ['Soe.Manage.Attest.Time.States.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Manage/Attest/Time/States");
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
