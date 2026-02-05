import '../Module';

import { TabsController } from "./TabsController";
import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";

angular.module("Soe.Economy.Export.SAFT", ['Soe.Economy.Export'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Economy/Export/SAFT/");
        const urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "",
                templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
