import '../Module';

import { TabsController } from "./TabsController";
import { IUrlHelperServiceProvider } from '../../../Core/Services/UrlHelperService';

angular.module("Soe.Manage.System.VolymInvoicing", ['Soe.Manage.System'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Manage/System/VolymInvoicing");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider.state("home", {
            url: "/",
            templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
            controller: TabsController,
            controllerAs: "ctrl"
        });
        $urlRouterProvider.otherwise("/");
    });
