import './Module';

import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

angular.module("Soe.Manage.Preferences.FieldSettings", ['Soe.Manage.Preferences.FieldSettings.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Manage/Preferences/FieldSettings");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider.state("home", {
            url: "/{fieldId}",
            templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
            controller: TabsController,
            controllerAs: "ctrl"
        });
        $urlRouterProvider.otherwise("/");
    });
