import { TabsControllerBase } from "../../../Core/Controllers/TabsControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IGridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { IEditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { GridController } from "./GridController";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { IExportService } from "../ExportService";

export class TabsController extends TabsControllerBase {
    private type: number;

    //@ngInject
    constructor($state: angular.ui.IStateService,
        $stateParams: angular.ui.IStateParamsService,
        $window: ng.IWindowService,
        $timeout: ng.ITimeoutService,
        private $http,
        private $templateCache,
        private $uibModal,
        private $filter,
        translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private coreService: ICoreService,
        private accountingService: IAccountingService,
        private exportService: IExportService,
        private uiGridConstants: uiGrid.IUiGridConstants) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService)

        // Setup base class
        var part: string = "economy.export.invoice.";
        super.initialize("dataStorageId", "name", part + "invoiceexport", part + "invoiceexports", part + "new_invoiceexport");
        this.type = soeConfig.exportType;
    }

    protected setupTabs() {
        // Create the tabs
        this.tabs = [
            super.createHomeTab(this.createGridController()),
        ];
    }

    protected edit(id: number, label: string) {
        // Open edit page
        if (id && id != 0) {
            HtmlUtility.openInSameTab(this.$window, "edit/?paymentId=" + id);
        }
        else {
            HtmlUtility.openInSameTab(this.$window, "edit/");
        }
    }

    private createGridController(): IGridControllerBase {
        return new GridController(this.$http, this.$templateCache, this.$timeout, this.$uibModal, this.$filter, this.type, this.coreService, this.accountingService, this.exportService, this.translationService, this.messagingService, this.notificationService, this.urlHelperService, this.uiGridConstants);
    }
}