import '../../Module';

import { IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

angular.module("Soe.Manage.System.Admin.Language", ['Soe.Manage.System'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Manage/System/Admin/Language");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider.state("home", {
            url: "/",
            templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
            controller: TabsController,
            controllerAs: "ctrl"
        });
        $urlRouterProvider.otherwise("/");
    });
