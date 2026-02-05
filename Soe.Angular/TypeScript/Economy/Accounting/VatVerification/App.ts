import '../Module';
import '../Vouchers/Module';

import { TabsController } from "./TabsController";
import { IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";

angular.module("Soe.Economy.Accounting.VatVerification", ['Soe.Economy.Accounting', 'Soe.Economy.Accounting.Vouchers.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Economy/Accounting/VatVerification/");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "",
                templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
