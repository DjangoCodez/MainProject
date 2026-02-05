import './Module';
import '../../Module';

import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

angular.module("Soe.Manage.Gdpr.Logs", ['Soe.Manage.Gdpr.Logs.Module', 'Soe.Manage'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Manage/Gdpr/Logs");
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