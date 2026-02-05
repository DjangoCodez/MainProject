import './Module';

import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

    angular.module("Soe.Economy.Accounting.Budget", ['Soe.Economy.Accounting.Budget.Module'])
        .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
            urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Economy/Accounting/Budget");
            var urlHelper = urlHelperServiceProvider.$get();
            $stateProvider
                .state("home", {
                    url: "/{budgetHeadId}",
                    templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                    controller: TabsController,
                    controllerAs: "ctrl"
                });
            $urlRouterProvider.otherwise("/");
        });
