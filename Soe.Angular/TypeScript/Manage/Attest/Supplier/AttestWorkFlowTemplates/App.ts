import './Module';

import { IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { TabsController } from "./TabsController";

angular.module("Soe.Manage.Attest.Supplier.AttestWorkFlowTemplates", ['Soe.Manage.Attest.Supplier.AttestWorkFlowTemplates.Module'])
    .config( /*@ngInject*/($stateProvider: angular.ui.IStateProvider, $urlRouterProvider: angular.ui.IUrlRouterProvider, urlHelperServiceProvider: IUrlHelperServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl, "/Manage/Attest/Supplier/AttestWorkFlowTemplates");
        var urlHelper = urlHelperServiceProvider.$get();
        $stateProvider
            .state("home", {
                url: "/{attestWorkFlowTemplateHeadId}",
                templateUrl: urlHelper.getCoreViewUrl("tabsComposition.html"),
                controller: TabsController,
                controllerAs: "ctrl"
            });
        $urlRouterProvider.otherwise("/");
    });
