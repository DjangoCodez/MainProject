import '../../Module';

import { IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

angular.module("Soe.Time.Employee.Csr.Import", ['Soe.Time.Employee'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Time/Employee/Csr/Import");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/",
                templateUrl: urlHelper.getCoreViewUrl("tabs.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
