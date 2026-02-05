import { SupplierProductPriceDTO } from "../../../../../Common/Models/SupplierProductDTO";
import { GridControllerBase2Ag } from "../../../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { ICompositionGridController } from "../../../../../Core/ICompositionGridController";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { Feature, SoeEntityState } from "../../../../../Util/CommonEnumerations";
import { StandardRowFunctions, SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { GridEvent } from "../../../../../Util/SoeGridOptions";

class SupplierProductPricesController extends GridControllerBase2Ag implements ICompositionGridController {
    private supplierProductId: number;
    private parentGuid: any;
    private priceRows: SupplierProductPriceDTO[] = [];
    private visibleRows: SupplierProductPriceDTO[] = [];
    private useCurrency: boolean = false;
    private currencies: any[] = [];

    //gui
    private rowFunctions: any = [];

    // Flags
    private hasSelectedRows = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "billing.purchase.rows", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onDoLookUp(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.afterSetup());

        this.onInit({});
    }

    onInit(parameters: any) {
        this.parameters = parameters;

        this.flowHandler.start([
            { feature: Feature.Billing_Purchase_Purchase_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Preferences_Currency, loadReadPermissions: false, loadModifyPermissions: true },
        ]);

        this.gridAg.options.customTabToCellHandler = (params) => this.handleNavigateToNextCell(params);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Purchase_Purchase_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Purchase_Purchase_Edit].modifyPermission;
        this.useCurrency = response[Feature.Economy_Preferences_Currency].modifyPermission;
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadCurrencies(),
        ]);
    }

    private loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesSmall().then(x => {
            this.currencies = x;
        });
    }

    private setupGrid() {

        this.gridAg.options.setMinRowsToShow(6);
        this.gridAg.options.setAutoHeight(true);

        const keys: string[] = [
            "billing.product.purchaseprice",
            "billing.purchase.product.pricestartdate",
            "billing.purchase.product.priceqty",
            "billing.purchase.product.priceenddate",
            "common.newrow",
            "core.deleterow",
            "common.currency",
            "billing.purchase.product.linkedtopricelist"
        ];

        this.translationService.translateMany(keys).then(terms => {

            this.setupFunctions(terms);

            const isNotLinkedToPriceList = (data) => !data.supplierProductPriceListId;
            const styleRules = {
                "errorRow": ({ data }) => data.endDate && data.startDate && new Date(data.endDate) < new Date(data.startDate),
                "closedRow": ({ data }) => isNotLinkedToPriceList(data) === false,
            }


            this.gridAg.addColumnIsModified("isModified");
            this.gridAg.addColumnNumber("quantity", terms["billing.purchase.product.priceqty"], 60, { enableHiding: false, decimals: 2, editable: true, maxDecimals: 4 });
            this.gridAg.addColumnNumber("price", terms["billing.product.purchaseprice"], 60, { enableHiding: false, decimals: 2, editable: true, maxDecimals: 4 });
            if (this.useCurrency) {
                this.gridAg.addColumnSelect("currencyId", terms["common.currency"], null, {
                    selectOptions: this.currencies,
                    editable: isNotLinkedToPriceList,
                    cellClassRules: styleRules,
                    displayField: "currencyCode",
                    dropdownIdLabel: "currencyId",
                    dropdownValueLabel: "code",
                });
            }
            this.gridAg.addColumnDate("startDate", terms["billing.purchase.product.pricestartdate"], null, null, null, { enableHiding: false, editable: isNotLinkedToPriceList, cellClassRules: styleRules });
            this.gridAg.addColumnDate("endDate", terms["billing.purchase.product.priceenddate"], null, null, null, { enableHiding: false, editable: isNotLinkedToPriceList, cellClassRules: styleRules });
            this.gridAg.addColumnIcon("priceIcon", null, null, { toolTip: terms["billing.purchase.product.linkedtopricelist"] });

            const gridEvents: GridEvent[] = [];
            gridEvents.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
            gridEvents.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row) => {
                this.$timeout(() => {
                    this.hasSelectedRows = this.gridAg.options.getSelectedCount() > 0;
                });
            }));
            gridEvents.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row) => {
                this.$timeout(() => {
                    this.hasSelectedRows = this.gridAg.options.getSelectedCount() > 0;
                });
            }));
            this.gridAg.options.subscribe(gridEvents);

            this.gridAg.finalizeInitGrid("billing.purchase.rows", true);
        });
    }

    private setupFunctions(terms: { [index: string]: string }) {
        this.rowFunctions.push({ id: StandardRowFunctions.Add, name: terms["common.newrow"], icon: "fal fa-plus" });
        this.rowFunctions.push({ id: StandardRowFunctions.Delete, name: terms["core.deleterow"], icon: "fal fa-times iconDelete", disabled: () => { return !this.hasSelectedRows } });
    }

    private afterSetup() {
        this.setupWatchers();
    }

    private setupWatchers() {
        if (!this.priceRows)
            this.priceRows = [];

        this.$scope.$watch(() => this.priceRows, (newVal, oldVal) => {
            this.priceRowsUpdated();
        });
    }

    private priceRowsUpdated() {
        if (this.priceRows) {
            this.visibleRows = _.orderBy(_.filter(this.priceRows, r => r.state === SoeEntityState.Active), 'startDate');
            this.visibleRows.forEach(r => {
                if (r.supplierProductPriceListId) {
                    r["priceIcon"] = "fal fa-file-spreadsheet"
                }
            })
            this.gridAg.setData(this.visibleRows);
        }
    }

    private afterCellEdit(row: SupplierProductPriceDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        // afterCellEdit will always be called, even if just tabbing through the columns.
        // No need to perform anything if value has not been changed.
        if (newValue === oldValue)
            return;

        if (!row.isModified) {
            row.isModified = true;
        }

        this.setParentAsModified();
        this.$scope.$applyAsync(() => this.gridAg.options.refreshRows(row));
    }

    protected handleNavigateToNextCell(params: any): { rowIndex: number, column: any } {
        const { nextCellPosition } = params;
        let { rowIndex, column } = nextCellPosition;
        let row: SupplierProductPriceDTO = this.gridAg.options.getVisibleRowByIndex(rowIndex).data;
        if (column.colId === 'soe-grid-menu-column') {
            const nextRowResult = this.gridAg.findNextRowInfo(row);

            if (nextRowResult) {
                return {
                    rowIndex: nextRowResult.rowIndex,
                    column: this.gridAg.options.getColumnByField('quantity')
                };
            } else {
                this.gridAg.options.stopEditing(false);
                this.addRow();
                return null;
            }
        }
        else {
            return { rowIndex, column };
        }
    }

    private executeRowFunction(option) {
        switch (option.id) {
            case StandardRowFunctions.Add:
                this.addRow();
                break;
            case StandardRowFunctions.Delete:
                this.deleteRows();
                break;
        }
    }

    private deleteRows() {
        const keys = ["core.warning", "core.deleterowwarning"];

        this.translationService.translateMany(keys).then(terms => {
            const modal = this.notificationService.showDialog(terms["core.warning"], terms["core.deleterowwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    const selectedRows = this.gridAg.options.getSelectedRows();
                    if (selectedRows && selectedRows.length > 0) {
                        selectedRows.forEach((row: SupplierProductPriceDTO) => {
                            if (row.supplierProductPriceId) {
                                row.state = SoeEntityState.Deleted;
                                row.isModified = true;
                            }
                            else {
                                var index: number = this.priceRows.indexOf(row);
                                this.priceRows.splice(index, 1);
                            }
                        });
                        //this.gridAg.options.refreshRows(selectedRows);

                        this.priceRowsUpdated();
                        this.setParentAsModified();
                    }
                }
            });
        });
    }

    private addRow() {
        if (!this.priceRows) {
            this.priceRows = [];
        }

        const row = new SupplierProductPriceDTO();
        row.quantity = 0;
        row.isModified = true;
        row.state = SoeEntityState.Active;
        row.supplierProductId = this.supplierProductId;

        if (this.priceRows.length > 0) {
            const lastRow = this.priceRows.slice(-1)[0];
            row.currencyId = lastRow.currencyId;
            row.currencyCode = lastRow.currencyCode;
        }
        else if (this.currencies.length > 0) {
            row.currencyId = this.currencies[0].currencyId;
            row.currencyCode = this.currencies[0].code;
        }

        this.priceRows.push(row);

        this.priceRowsUpdated();

        this.$scope.$applyAsync(() => {
            this.gridAg.options.startEditingCell(row, this.getPriceColumn());
        });

        this.setParentAsModified();
    }

    private getPriceColumn() {
        return this.gridAg.options.getColumnByField('price');
    }

    private setParentAsModified() {
        this.$scope.$applyAsync(() => this.messagingHandler.publishSetDirty( this.parentGuid ));
    }
}

export class SupplierProductPricesDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Shared/Billing/Purchase/Directives/SupplierProductPrices/SupplierProductPrices.html"),
            scope: {
                parentGuid: "=",
                supplierProductId: "=",
                priceRows: '='
            },
            restrict: 'E',
            replace: true,
            controller: SupplierProductPricesController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}