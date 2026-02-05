import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { CustomerInvoiceIODTO } from "../../../../Common/Models/CustomerInvoiceIODTO";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../../Util/Enumerations";
import { Feature } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";

export class InvoiceImportDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Economy/Import/Automaster/Directives/InvoiceImport.html'),
            scope: {
                rows: '=',
                readOnly: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: InvoiceImportDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class InvoiceImportDirectiveController extends GridControllerBase {
    // Setup
    private rows: CustomerInvoiceIODTO[];
    private importHeadId: number;
    private readOnly: boolean;

    // Collections
    invoices: CustomerInvoiceIODTO[] = [];

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

        super("Soe.Economy.Import.Automaster.Directives.InvoiceImport", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

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
            "economy.import.automaster.number",
            "economy.import.automaster.invoicedate",
            "economy.import.automaster.duedate",
            "economy.import.automaster.totalamount",
            "economy.import.automaster.sumamount",
            "economy.import.automaster.invoicenr",
            "economy.import.automaster.claimaccount",
            "economy.import.automaster.acountdim2nr",
            "economy.import.automaster.seqnr",
            "economy.import.automaster.transfertype",
            "economy.import.automaster.errormessage"
        ];

        this.translationService.translateMany(keys).then(terms => {

            var colDef0 = this.soeGridOptions.addColumnText("statusName", terms["economy.import.automaster.state"], null);
            colDef0.enableCellEdit = true;
            var colDef1 = this.soeGridOptions.addColumnText("name", terms["common.name"], null);
            colDef1.enableCellEdit = true;
            var colDef2 = this.soeGridOptions.addColumnText("customerInvoiceNr", terms["economy.import.automaster.invoicenr"], null);
            colDef2.enableCellEdit = true;
            var colDef3 = this.soeGridOptions.addColumnDate("invoiceDate", terms["economy.import.automaster.invoicedate"], "15%", false);
            colDef3.enableCellEdit = true;
            var colDef4 = this.soeGridOptions.addColumnDate("dueDate", terms["economy.import.automaster.duedate"], "15%", false);
            colDef4.enableCellEdit = true;
            var colDef5 = this.soeGridOptions.addColumnNumber("totalAmountCurrency", terms["economy.import.automaster.totalamount"], null);
            colDef5.enableCellEdit = true;
            var colDef6 = this.soeGridOptions.addColumnText("customerNr", terms["economy.import.automaster.number"], null);
            colDef6.enableCellEdit = true;
            var colDef7 = this.soeGridOptions.addColumnText("claimAccountNr", terms["economy.import.automaster.claimaccount"], null);
            colDef7.enableCellEdit = true;
            var colDef8 = this.soeGridOptions.addColumnText("claimAccountNrDim2", terms["economy.import.automaster.acountdim2nr"], null);
            colDef8.enableCellEdit = true;
            var colDef9 = this.soeGridOptions.addColumnText("errorMessage", terms["economy.import.automaster.transfertype"], null);
            colDef9.enableCellEdit = true;
            var colDef10 = this.soeGridOptions.addColumnText("transferType", terms["economy.import.automaster.errormessage"], null);
            colDef10.enableCellEdit = true;

            super.gridDataLoaded(this.rows);

        });
    }

    protected allowNavigationFromTypeAhead(entity: CustomerInvoiceIODTO, colDef) {
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