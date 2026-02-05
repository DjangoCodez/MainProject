import './Module';

import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

angular.module("Soe.Economy.Export.Payments", ['Soe.Economy.Export.Payments.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Economy/Export/Payments");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/{paymentExportId}",
                templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });