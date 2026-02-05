import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { ImportStockSaldoDialogController } from "../../Dialogs/ImportStockSaldo/ImportStockSaldo";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IStockService } from "../../../Shared/Billing/Stock/StockService";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";

export class EditController extends EditControllerBase2 implements ICompositionEditController {
    stockinventoryGrid: EmbeddedGridController;

    // Data
    private terms: any;
    stockInventoryHeadId: number;
    stockInventoryHead: any;
    stockInventoryRows: any[];
    stocksDict: any[];
    stockId: number;
    productNrFrom: string;
    productNrTo: string;
    shelfIdFrom: number;
    shelfIdTo: number;
    stockPlaces: any[];
    stockPlacesForStock: any[];
    private modalInstance: any;
    private afterChanged: boolean = false;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private stockService: IStockService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private gridHandlerFactory: IGridHandlerFactory,
        $uibModal) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookUp())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.stockinventoryGrid = new EmbeddedGridController(this.gridHandlerFactory, "stockinventoryGrid");
        this.stockinventoryGrid.gridAg.options.setMinRowsToShow(8);

        this.modalInstance = $uibModal;
    }

    $onInit() {
        this.createStockinventoryGrid();
    }

    public onInit(parameters: any) {
        this.stockInventoryHeadId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Stock, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Stock].readPermission;
        this.modifyPermission = response[Feature.Billing_Stock].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    private onDoLookUp() {
        return this.$q.all([
            this.loadStocks(),
            this.loadPlaces()]);
    }

    private onLoadData() {
        if (this.stockInventoryHeadId > 0) {
            return this.$q.all([
                this.loadStockInventory(),
                this.loadStockInventoryRows()]);
        } else {
            this.new();
        }
    }

    private createStockinventoryGrid() {

        this.$timeout(() => {
            const keys: string[] = [
                "billing.stock.stockinventory.productnr",
                "common.name",
                "billing.stock.stockinventory.shelfname",
                "billing.stock.stockinventory.startingsaldo",
                "billing.stock.stockinventory.inventorysaldo",
                "billing.stock.stockinventory.difference",
                "common.modified",
                "billing.stock.stocksaldo.ordered",
                "billing.stock.stocksaldo.reserved",
                "core.aggrid.totals.filtered",
                "core.aggrid.totals.total",
                "billing.stock.stockinventory.stockinventory",
                "billing.stock.stockinventory.transactiondate"
            ];

            return this.translationService.translateMany(keys).then((terms) => {
                this.stockinventoryGrid.gridAg.addColumnText("productNumber", terms['billing.stock.stockinventory.productnr'], null);
                this.stockinventoryGrid.gridAg.addColumnText("productName", terms['common.name'], null);
                this.stockinventoryGrid.gridAg.addColumnText("shelfName", terms['billing.stock.stockinventory.shelfname'], null);
                this.stockinventoryGrid.gridAg.addColumnNumber("startingSaldo", terms['billing.stock.stockinventory.startingsaldo'], null, { decimals: 2 });
                this.stockinventoryGrid.gridAg.addColumnNumber("orderedQuantity", terms['billing.stock.stocksaldo.ordered'], null, { decimals: 2, enableHiding: true, hide: true });
                this.stockinventoryGrid.gridAg.addColumnNumber("reservedQuantity", terms['billing.stock.stocksaldo.reserved'], null, { decimals: 2, enableHiding: true, hide: true });
                this.stockinventoryGrid.gridAg.addColumnNumber("inventorySaldo", terms["billing.stock.stockinventory.inventorysaldo"], null, { decimals: 2, editable: () => { return this.isRowEditable() } });
                this.stockinventoryGrid.gridAg.addColumnNumber("difference", terms['billing.stock.stockinventory.difference'], null, { decimals: 2 });
                this.stockinventoryGrid.gridAg.addColumnDate("transactionDate", terms['billing.stock.stockinventory.transactiondate'], null, true, null, { editable: () => { return this.isRowEditable() } });
                this.stockinventoryGrid.gridAg.addColumnDate("modified", terms['common.modified'], null);
                this.stockinventoryGrid.gridAg.finalizeInitGrid(terms['billing.stock.stockinventory.stockinventory'], true);
            });
        }, 200);

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        events.push(new GridEvent(SoeGridOptionsEvent.CellFocused, (row, column) => { this.cellFocused(row, column); }));
        this.stockinventoryGrid.gridAg.options.subscribe(events);
    }

    // LOOKUPS
    private loadStockInventoryRows(): ng.IPromise<any> {
        return this.stockService.getStockInventoryRows(this.stockInventoryHeadId).then((x) => {
            this.isNew = false;
            this.stockInventoryRows = x;
            this.stockinventoryGrid.gridAg.setData(this.stockInventoryRows);
        });
    }

    public loadStockInventory(): ng.IPromise<any> {
        return this.stockService.getStockInventory(this.stockInventoryHeadId).then((x) => {
            x.inventoryStart = CalendarUtility.toFormattedDateAndTime(x.inventoryStart);
            x.inventoryStop = CalendarUtility.toFormattedDateAndTime(x.inventoryStop);
            this.stockInventoryHead = x;
        });
    }

    public loadPlaces(): ng.IPromise<any> {
        // Load data
        return this.stockService.getStockPlaces(false, 0).then((x) => {
            this.stockPlaces = x;
        });
    }
    public loadStocks(): ng.IPromise<any> {
        // Load data
        return this.stockService.getStocks(false).then((x) => {
            this.stocksDict = x;
        });
    }

    private isRowEditable(): boolean {
        return (this.stockInventoryHeadId > 0) && (!this.stockInventoryHead.inventoryStop);
    }

    private afterCellEdit(row: any, colDef: uiGrid.IColumnDef, newValue, oldValue): any {
        // afterCellEdit will always be called, even if just tabbing through the columns.
        // No need to perform anything if value has not been changed.
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            case 'inventorySaldo':
            case 'transactionDate':
                this.dirtyHandler.setDirty();
                break;
        }
    }

    private cellFocused(row, colDef): any {
        if (colDef.colId === 'transactionDate' && row > 0) {
            const rowData = (this.stockinventoryGrid.gridAg.options as SoeGridOptionsAg).gridOptions.rowData;
            if (rowData[row - 1].transactionDate && rowData[row - 1].transactionDate != '') {
                if (!rowData[row].transactionDate)
                    rowData[row].transactionDate = rowData[row - 1].transactionDate
            }
            else {
                rowData[row].transactionDate = CalendarUtility.getDateToday();
            }
            this.stockinventoryGrid.gridAg.options.stopEditing(true);
            this.stockinventoryGrid.gridAg.options.refocusCell();
        }
    }

    public save() {
        if (this.isNew && this.stockInventoryRows.length == 0) {
            const keys: string[] = [
                "billing.stock.stockinventory.norows.message",
            ];

            this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms[""], terms["billing.stock.stockinventory.norows.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            });
        }
        else {
            this.progress.startSaveProgress((completion) => {
                this.stockService.saveStockInventoryRows(this.stockInventoryHead, this.stockInventoryRows).then((result) => {
                    if (result.success) {
                        if (result.integerValue && result.integerValue > 0)
                            this.stockInventoryHeadId = result.integerValue;

                        if (this.isNew) {
                            this.updateTabCaption();
                        }

                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.stockInventoryHead);
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }, this.guid)
                .then(data => {
                    this.dirtyHandler.clean();
                    this.onLoadData();
                }, error => {

                });
        }
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.stockService.deleteStockInventory(this.stockInventoryHeadId).then((result) => {
                if (result.success) {
                    completion.completed(this.stockInventoryHead);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    private generateNewInventory() {
        if (this.shelfIdFrom == null)
            this.shelfIdFrom = 0;
        if (this.shelfIdTo == null)
            this.shelfIdTo = 0;

        this.progress.startWorkProgress((completion) => {
            this.stockService.generateStockInventoryRows(this.stockInventoryHead.stockId, this.productNrFrom, this.productNrTo, this.shelfIdFrom, this.shelfIdTo).then((x) => {
                if (x && x.length > 0) {
                    this.stockInventoryRows = x;
                    this.stockinventoryGrid.gridAg.setData(this.stockInventoryRows);
                    completion.completed(x, true);
                }
                else {
                    this.translationService.translate("billing.stock.stockinventory.norowsgenerated").then((term) => {
                        completion.failed(term);
                    });
                }
            });
        });
    }

    private importInventoryFile() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Dialogs/ImportStockSaldo/ImportStockSaldo.html"),
            controller: ImportStockSaldoDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                stockService: () => { return this.stockService },
                commonCustomerService: () => { return null },
                notificationService: () => { return this.notificationService },
                stockInventoryHeadId: () => { return this.stockInventoryHeadId }
            }
        });

        modal.result.then((result: any) => {
            this.loadStockInventoryRows();
        });
    }

    private acceptInventory() {
        if (!this.isNew && this.isClosed()) {
            const keys: string[] = [
                "billing.stock.stockinventory.alreadyclosed.message",
            ];

            this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms[""], terms["billing.stock.stockinventory.alreadyclosed.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            });
        }
        else {
            this.stockInventoryHead.inventoryStop = 0;
            this.stockService.closeStockInventory(this.stockInventoryHeadId).then((x) => {
                this.loadStockInventory();
            });
        }
    }

    private isClosed(): boolean {
        return (this.stockInventoryHead && this.stockInventoryHead.inventoryStop != null);
    }

    // ACTIONS
    private stockChanged(item) {
        this.stockPlacesForStock = _.filter(this.stockPlaces, function (y: any) {
            if (y.stockId == item || y.stockShelfId == 0) return y
        });

    }
    private stockPlaceChanged(item) {

        var rows: any = [];
        var count = _.filter(this.stockPlacesForStock, function (y) { return (y.stockId == 0 && y.stockShelfId == 0) }).length;

        if (count == 1)
            this.stockPlacesForStock.shift();
        else
            rows.push({ stockId: 0, stockShelfId: 0, stockName: "", code: "", name: "" });

        _.filter(this.stockPlacesForStock, function (y: any) {
            rows.push(y);
        });

        this.afterChanged = true;
        this.stockPlacesForStock = rows;
    }

    private stockPlaceClicked(item) {

        var rows: any = [];
        const count = _.filter(this.stockPlacesForStock, function (y) { return (y.stockId == 0 && y.stockShelfId == 0) }).length;
        if (count == 1 && !this.afterChanged) {

            if (typeof (item) == "undefined") {
                this.stockPlacesForStock.shift();
            }
            _.filter(this.stockPlacesForStock, function (y: any) {
                rows.push(y);
            });
            this.stockPlacesForStock = rows;
        }
        else if (count == 0 && !this.afterChanged) {
            if (typeof (item) != "undefined" && item != null) {
                rows.push({ stockId: 0, stockShelfId: 0, stockName: "", code: "", name: "" });
            }

            _.filter(this.stockPlacesForStock, function (y: any) {
                rows.push(y);
            });
            this.stockPlacesForStock = rows;
        }
        else if (count == 1 && this.afterChanged) {
            this.stockPlacesForStock.shift();
        }
        this.afterChanged = false;

    }

    private updateTabCaption() {
        this.translationService.translate("billing.stock.stockinventory.stockinventory").then((term) => {
            this.messagingHandler.publishSetTabLabel(this.guid, term + " " + this.stockInventoryHead.headerText);
        });
    }

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.stockInventoryHeadId = 0;
        this.stockInventoryHead = {};
        this.stockInventoryRows = [];
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.stockInventoryHeadId == 0) {
                if (!this.stockInventoryHead.headerText) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.stockInventoryHead.stockId) {
                    mandatoryFieldKeys.push("billing.stock.stocks.stock");
                }
            }
        });
    }
}