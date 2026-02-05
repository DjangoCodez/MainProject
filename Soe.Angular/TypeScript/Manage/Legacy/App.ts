import '../Module';

import { IUrlHelperServiceProvider } from "../../Core/Services/UrlHelperService";
import { LegacyController } from "./LegacyController";

angular.module("Soe.Manage.Legacy", ['Soe.Manage'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Manage/Legacy");
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
