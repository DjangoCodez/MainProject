import './Module';

import { TabsController } from "./TabsController";
import { IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";

angular.module("Soe.Manage.Attest.Time.Rules", ['Soe.Manage.Attest.Time.Rules.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Manage/Attest/Time/Rules");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/{attestRuleHeadId}",
                templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
