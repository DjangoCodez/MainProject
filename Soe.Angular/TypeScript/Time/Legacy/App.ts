import '../Module';

import { IUrlHelperServiceProvider } from "../../Core/Services/UrlHelperService";
import { LegacyController } from "./LegacyController";

angular.module("Soe.Time.Legacy", ['Soe.Time'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Time/Legacy");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/",
                templateUrl: urlHelper.getViewUrl("legacy.html"),
                controller: LegacyController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
