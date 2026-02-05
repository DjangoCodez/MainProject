import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { GridController } from "../../../Common/Directives/ContactPersons/ContactPersonsExtended";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { TabsControllerBase1 } from "../../../Core/Controllers/TabsControllerBase1";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ICoreService } from "../../../Core/Services/CoreService";

export class TabsController extends TabsControllerBase1 {
    //@ngInject
    constructor($state: angular.ui.IStateService,
        $stateParams: angular.ui.IStateParamsService,
        $window: ng.IWindowService,
        $timeout: ng.ITimeoutService,
        translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private coreService: ICoreService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        $scope: ng.IScope) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService, $scope)

        // Setup base class
        var part: string = "manage.contactperson.contactpersons.";
        super.initialize(part + "contactperson", part + "contactpersons", part + "newcontactperson");
    }

    protected setupTabs() {
        return this.translationService.translate("manage.contactperson.contactpersons.contactpersons").then((term) => {
            this.addNewTab(term, null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), null, false, true)
        });
    }

    protected add() {
    }

    protected edit(row: any) {
        //Edit is taken care of in gridcontroller
    }

    protected getEditIdentifier(row: any): any {
        return null;
    }

    protected getEditName(data: any): string {
        return "";
    }
}