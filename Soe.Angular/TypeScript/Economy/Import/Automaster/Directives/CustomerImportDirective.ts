import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { CustomerIODTO } from "../../../../Common/Models/CustomerIODTO";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../../Util/Enumerations";
import { Feature } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";

export class CustomerImportDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Economy/Import/Automaster/Directives/CustomerImport.html'),
            scope: {
                rows: '=',
                readOnly: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: CustomerImportDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class CustomerImportDirectiveController extends GridControllerBase {
    // Setup
    private rows: CustomerIODTO[];
    private importHeadId: number;
    private readOnly: boolean;

    // Collections
    customers: CustomerIODTO[] = [];

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Soe.Economy.Import.Automaster.Directives.CustomerImport", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        if (!this.rows)
            this.rows = [];

        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.setMinRowsToShow(8);
        this.setupTypeAhead();
    }

    protected setupCustomToolBar() {
        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("time.time.timeperiod.newtimeperiod", "time.time.timeperiod.newtimeperiod", IconLibrary.FontAwesome, "fa-plus",
            () => { this.addRow(); },
            null,
            () => { return this.readOnly; })));
    }

    public setupGrid() {
        this.startLoad();
        this.setupGridColumns();
    }

    private setupGridColumns() {

        var keys: string[] = [
            "economy.import.automaster.state",
            "economy.import.automaster.isselected",
            "economy.import.automaster.number",
            "economy.import.automaster.name",
            "economy.import.automaster.address",
            "economy.import.automaster.postalcode",
            "economy.import.automaster.city",
            "economy.import.automaster.phonehome",
            "economy.import.automaster.phonejob",
            "economy.import.automaster.orgnr",
            "economy.import.automaster.errormessage"
        ];

        this.translationService.translateMany(keys).then(terms => {

            this.soeGridOptions.enableRowSelection = true;

            var colDef0 = this.soeGridOptions.addColumnText("statusName", terms["economy.import.automaster.state"], null);
            colDef0.enableCellEdit = true;
            var colDef1 = this.soeGridOptions.addColumnText("orgNr", terms["economy.import.automaster.orgnr"], null);
            colDef1.enableCellEdit = true;
            var colDef2 = this.soeGridOptions.addColumnText("customerNr", terms["economy.import.automaster.number"], null);
            colDef2.enableCellEdit = true;
            var colDef3 = this.soeGridOptions.addColumnText("name", terms["economy.import.automaster.name"], null);
            colDef3.enableCellEdit = true;
            var colDef4 = this.soeGridOptions.addColumnText("billingAddress", terms["economy.import.automaster.address"], null);
            colDef4.enableCellEdit = true;
            var colDef5 = this.soeGridOptions.addColumnText("billingPostalCode", terms["economy.import.automaster.postalcode"], null);
            colDef5.enableCellEdit = true;
            var colDef6 = this.soeGridOptions.addColumnText("billingPostalAddress", terms["economy.import.automaster.city"], null);
            colDef6.enableCellEdit = true;
            var colDef7 = this.soeGridOptions.addColumnText("phoneHome", terms["economy.import.automaster.phonehome"], null);
            colDef7.enableCellEdit = true;
            var colDef8 = this.soeGridOptions.addColumnText("phoneJob", terms["economy.import.automaster.phonejob"], null);
            colDef8.enableCellEdit = true;
            var colDef9 = this.soeGridOptions.addColumnText("errorMessage", terms["economy.import.automaster.errormessage"], null);
            colDef9.enableCellEdit = true;
            //this.soeGridOptions.addColumnDelete(terms["core.delete"], "deleteRow");

            super.gridDataLoaded(this.rows);

        });
    }


    protected allowNavigationFromTypeAhead(entity: CustomerIODTO, colDef) {
        return true;
    }

    protected onBlur(entity, colDef) {

    }

    // Lookups

    // Actions
    private addRow() {

    }

    protected initDeleteRow(row: any) {
        this.soeGridOptions.deleteRow(row);
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
    }
}