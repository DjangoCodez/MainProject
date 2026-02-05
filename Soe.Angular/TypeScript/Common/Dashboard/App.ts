import './Module';

import { MainController } from "./MainController";
import { IUrlHelperServiceProvider } from "../../Core/Services/UrlHelperService";

angular.module("Soe.Common.Dashboard", ['Soe.Common.Dashboard.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Common/Dashboard");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/",
                templateUrl: urlHelper.getViewUrl("main.html"),
                controller: MainController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
