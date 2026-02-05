import './Module';

import { IUrlHelperServiceProvider } from "../../Core/Services/UrlHelperService";
import { StartController } from "./StartController";

angular.module("Soe.Common.Start", ['Soe.Common.Start.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Common/Start");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/",
                templateUrl: urlHelper.getViewUrl("start.html"),
                controller: StartController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
