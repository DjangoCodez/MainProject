import '../Module';

import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

    angular.module("Soe.Time.Payroll.Retroactive", ['Soe.Time.Payroll'])
        .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
            urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Time/Payroll/Retroactive");
            var urlHelper = urlHelperServiceProvider.$get();
            $stateProvider
                .state("home", {
                    url: "/{retroactivePayrollId}",
                    templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                    controller: TabsController,
                    controllerAs: "ctrl"
                });
            $urlRouterProvider.otherwise("/");
        });
