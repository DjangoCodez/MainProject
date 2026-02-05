import '../../Module';

import { IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

angular.module("Soe.Manage.System.SysCompany.SysCompDB", ['Soe.Manage.System'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Manage/System/SysCompany/SysCompdb");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/{syscompdbId}",
                templateUrl: urlHelper.getCoreViewUrl("tabs1.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });

