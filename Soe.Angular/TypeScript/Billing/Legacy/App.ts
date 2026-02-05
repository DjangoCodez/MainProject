import '../Module';

import { IUrlHelperServiceProvider } from "../../Core/Services/UrlHelperService";
import { LegacyController } from "./LegacyController";

angular.module("Soe.Billing.Legacy", ['Soe.Billing'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Billing/Legacy");
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
