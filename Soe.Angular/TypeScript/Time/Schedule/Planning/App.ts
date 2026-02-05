import './Module';

import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";
import { TermPartsLoaderProvider } from '../../../Core/Services/TermPartsLoader';

angular.module("Soe.Time.Schedule.Planning", ['Soe.Time.Schedule.Planning.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider, termPartsLoaderProvider: TermPartsLoaderProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Time/Schedule/Planning");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/",
                templateUrl: urlHelper.getCoreViewUrl("tabs1.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
        // For the order planning
        termPartsLoaderProvider.addPart('billing');
    });
