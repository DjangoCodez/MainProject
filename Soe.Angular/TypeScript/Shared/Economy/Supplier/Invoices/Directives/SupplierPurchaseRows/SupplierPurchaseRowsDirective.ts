import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../../Core/Services/UrlHelperService";
import { IGridHandlerFactory } from "../../../../../../Core/Handlers/gridhandlerfactory";
import { ICompositionGridController } from "../../../../../../Core/ICompositionGridController";
import { GridControllerBase2Ag } from "../../../../../../Core/Controllers/GridControllerBase2Ag";
import { IControllerFlowHandlerFactory } from "../../../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../../../Core/Handlers/messaginghandlerfactory";
import { IPermissionRetrievalResponse } from "../../../../../../Core/Handlers/ControllerFlowHandler";
import { ICoreService } from "../../../../../../Core/Services/CoreService";
import { ISupplierService } from "../../../SupplierService";
import { PurchaseRowSmallDTO, PurchaseSmallDTO } from "../../../../../../Common/Models/PurchaseDTO";
import { PurchaseDeliveryInvoiceDTO } from "../../../../../../Common/Models/PurchaseDeliveryDTO";
import { Feature } from "../../../../../../Util/CommonEnumerations";
import { SoeGridOptionsEvent } from "../../../../../../Util/Enumerations";
import { GridEvent } from "../../../../../../Util/SoeGridOptions";
import { Constants } from "../../../../../../Util/Constants";
import { IMessagingService } from "../../../../../../Core/Services/MessagingService";

export class SupplierPurchaseRowsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Directives/SupplierPurchaseRows/SupplierPurchaseRows.html"),
            scope: {
                registerControl: '&',
                container: '@',
                progressBusy: '=?',
                supplierId: '=',
                supplierInvoiceId: '=',
                parentGuid: '=?',
                purchaseInvoiceRows : '=' 
            },
            restrict: 'E',
            replace: true,
            controller: SupplierPurchaseRowsController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class SupplierPurchaseRowsController extends GridControllerBase2Ag implements ICompositionGridController {

    // Init parameters
    private parentGuid: string;
    private supplierId: number;
    private supplierInvoiceId: number;
    private purchaseRows: { [purchaseId: number]: PurchaseRowSmallDTO[] } = {};
    private supplierPurchases: PurchaseSmallDTO[] = [];
    private purchaseInvoiceRows: PurchaseDeliveryInvoiceDTO[] = [];
    
    // Flags
    private purchasesIsLoaded: boolean = false;
    private linkToPurchaseSet: boolean = true;
    private readOnly = false;
    private hasSelectedRows = false;
    private getOnlyDelivered = true;
    private getAlreadyConnected = false;

    private filteredPurchaseDict: any[] = [];
    private selectedPurchaseDict: any[] = [];
    private buttonFunctions: any = [];
    private terms: { [index: string]: string; };

    //@ngInject
    constructor(
        private supplierService: ISupplierService,
        protected $uibModal,
        protected coreService: ICoreService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        protected messagingService: IMessagingService,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
        super(gridHandlerFactory, "common.Directives.SupplierPurchaseRows", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            //.onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())

        this.onInit({});

        this.$scope.$on('reloadPurchaseRows', (e, a) => {
            this.loadPurchaseInvoiceRows();
        });

        this.setupWatchers();
    }

    //SETUP
    onInit(parameters: any) {
        this.parameters = parameters;

        this.flowHandler.start([
            { feature: Feature.Economy_Supplier_Suppliers_Edit, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.modifyPermission = response[Feature.Economy_Supplier_Suppliers_Edit].modifyPermission;
        this.readPermission = response[Feature.Economy_Supplier_Suppliers_Edit].readPermission;
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.supplierInvoiceId, (newValue, oldValue) => {
            
            //if (newValue != oldValue) {
                this.loadPurchaseInvoiceRows();
            //}
        })
    }

    //GRID
    public setupGrid() {

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (val) => null));
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.selectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.selectionChanged(); }));
        this.gridAg.options.subscribe(events);
        this.gridAg.options.enableRowSelection = false;

        const getCellClassRules = (field, compareTo) => {
            return {
                "successRow": (gridRow: any) => gridRow.data[field] > gridRow.data[compareTo],
                "errorRow": (gridRow: any) => gridRow.data[field] < gridRow.data[compareTo],
            }
        }

        let keys: string[] = [
            "economy.supplier.invoice.purchasenr",
            "billing.purchaserows.deliveredquantity",
            "economy.supplier.invoice.pricefrominvoice",
            "billing.products.product",
            "economy.supplier.invoice.supplieritemno",
            "economy.supplier.invoice.invoiceqty", 
            "billing.purchase.delivery.purchaseqty",
            "economy.supplier.invoice.productnr",
            "economy.supplier.invoice.pricefrompurchase",
            "economy.supplier.invoice.purchaserownr",
            "economy.supplier.attestflowoverview.invoiceid",
            "common.sum",
            "economy.supplier.invoice.connectpurchaserowtooltip",
            "economy.supplier.invoice.invoice"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.gridAg.addColumnIsModified();
            this.gridAg.addColumnText("purchaseNr", terms["economy.supplier.invoice.purchasenr"], 20, true, {
                enableHiding: true, enableRowGrouping: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: () => true, callback: this.openPurchaseOrder.bind(this) }
            } );
            this.gridAg.addColumnNumber("purchaseRowNr", terms["economy.supplier.invoice.purchaserownr"], null, { alignLeft: true, formatAsText: true });

            this.gridAg.addColumnText("productName", terms["billing.products.product"], null);
            this.gridAg.addColumnText("productNumber", terms["economy.supplier.invoice.productnr"], null);
            this.gridAg.addColumnText("supplierProductNr", terms["economy.supplier.invoice.supplieritemno"], null);

            this.gridAg.addColumnNumber("deliveredQuantity", terms["billing.purchaserows.deliveredquantity"], null,
                {
                    cellClassRules: getCellClassRules("deliveredQuantity", "quantity"),
                    decimals: 2,
                    enableHiding: false,
                    editable: false,
                });

            this.gridAg.addColumnNumber("purchaseQuantity", terms["billing.purchase.delivery.purchaseqty"], null, {decimals: 2, enableHiding: false, editable: false });

            this.gridAg.addColumnNumber("askedPrice", terms["economy.supplier.invoice.pricefrompurchase"], null,
                {
                    cellClassRules: getCellClassRules("askedPrice", "price"),
                    decimals: 2,
                    enableHiding: false,
                    editable: false,
                });

            this.gridAg.addColumnNumber("price", terms["economy.supplier.invoice.pricefrominvoice"], null,
                {
                    decimals: 2,
                    enableHiding: false,
                    editable: (data) => this.isRowEditable(data)
                });

            this.gridAg.addColumnNumber("quantity", terms["economy.supplier.invoice.invoiceqty"], null,
                {
                    decimals: 2,
                    enableHiding: false,
                    editable: (data) => this.isRowEditable(data)
                });
            this.gridAg.addColumnNumber("invoiceRowSum", terms["common.sum"], null,
                {
                    decimals: 2,
                    enableHiding: false,
                    editable: false,
                });

            this.gridAg.addColumnText("supplierInvoiceSeqNr", terms["economy.supplier.attestflowoverview.invoiceid"], 20, true, { enableHiding: false, editable: false, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show:(row) => this.canEditSupplierInvoice(row), callback: this.openSupplierInvoice.bind(this) }});
            this.gridAg.addColumnIcon("statusIcon", null, 30, { toolTip: terms["economy.supplier.invoice.connectpurchaserowtooltip"], suppressSorting: false, enableHiding: false,onClick: this.statusIconClick.bind(this) });

            this.gridAg.finalizeInitGrid("common.directives.supplierPurchaseRows", true);
        });
    }

    private afterCellEdit(row: PurchaseDeliveryInvoiceDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            case 'price':
            case 'quantity':
                {
                    row.invoiceRowSum = row.price * row.quantity;
                    this.rowChanged(row);
                    break;
                }
        }
    }

    private openSupplierInvoice(row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_INVOICE, {
            row: {
                id: row.supplierinvoiceId,
                supplierInvoiceSeqNr: row.supplierInvoiceSeqNr,
                supplierinvoiceId: row.supplierinvoiceId,
                purchaseNr: row.purchaseNr,
                invoiceId: row.supplierinvoiceId,
                name: this.terms["economy.supplier.invoice.invoice"] + " " + row.supplierInvoiceSeqNr
            }
        });
    }

    private openPurchaseOrder(row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_PURCHASE, {
            row: {
                id: row.purchaseId,
                purchaseId: row.purchaseId,
                purchaseNr: row.purchaseNr,
                supplierinvoiceId: row.supplierinvoiceId,            
                name: this.terms["economy.supplier.invoice.purchasenr"] + " " + row.purchaseNr
            }
        });
    }

    private selectionChanged() {
        this.$timeout(() => {
            this.hasSelectedRows = this.gridAg.options.getSelectedCount() > 0;
        });
    }

    private isRowEditable(row: PurchaseDeliveryInvoiceDTO): boolean {
        if (this.modifyPermission && row.supplierinvoiceId == this.supplierInvoiceId)
            return true
        else
            return false;
    }

    private canEditSupplierInvoice(row: PurchaseDeliveryInvoiceDTO): boolean {        
        if (row.supplierinvoiceId != this.supplierInvoiceId) {
            if (row.supplierinvoiceId) {
                return true
            }
        }
        return false;
    }

    private statusIconClick(row: PurchaseDeliveryInvoiceDTO) {
        if (!row.supplierinvoiceId || row.supplierinvoiceId === this.supplierInvoiceId) {
            row.supplierinvoiceId = (row.supplierinvoiceId) ? 0 : this.supplierInvoiceId;
            //saved or not saved invoice
            if (row.supplierinvoiceId || (!this.supplierInvoiceId && !row.supplierinvoiceId) ) {
                row.linkToInvoice = row.linkToInvoice !== true;
            }
            this.setIcon(row);
            this.rowChanged(row);
        }
    }

    private setGridData() {
        const rows = this.purchaseInvoiceRows.filter(r => !r.isDeleted);
        this.gridAg.setData(rows);
    }

    private purchaseRowChanged(row: PurchaseDeliveryInvoiceDTO, val: string) {
        const [_, rows] = this.getPurchaseRows(row.purchaseId);

        const purchaseRow = rows.find(p => p.displayName === val);
        if (purchaseRow) {
            this.setValueFromPurchaseRow(row, purchaseRow);
            this.setGridData();
        }
    }

    private setValueFromPurchaseRow(row: PurchaseDeliveryInvoiceDTO, purchaseRow: PurchaseRowSmallDTO) {
        row.deliveredQuantity = purchaseRow?.deliveredQuantity || 0;
        row.productName = purchaseRow?.productName || "";
        row.productNumber = purchaseRow?.productNumber || "";
        row.askedPrice = purchaseRow?.price || 0;
        row.purchaseRowId = purchaseRow?.purchaseRowId || 0;
        row.purchaseRowDisplayName = purchaseRow?.displayName || "";
    }

    private loadPurchaseRows(): ng.IPromise<any> {
        const purchaseIds = this.selectedPurchaseDict.map(a => a.id);
        this.purchaseInvoiceRows = this.purchaseInvoiceRows.filter(r => r.supplierinvoiceId === this.supplierInvoiceId);

        return this.supplierService.getSupplierPurchaseRows(purchaseIds, this.getOnlyDelivered, this.getAlreadyConnected).then((rows: PurchaseDeliveryInvoiceDTO[]) => {
            if (this.supplierInvoiceId) {
                rows = rows.filter(r => !(r.supplierinvoiceId) || r.supplierinvoiceId !== this.supplierInvoiceId);
            }
            this.updateFieldsAfterLoad(rows);

            this.purchaseInvoiceRows = this.purchaseInvoiceRows.concat(rows);
            this.gridAg.setData(this.purchaseInvoiceRows);
        })
    }

    private loadPurchaseInvoiceRows(): ng.IPromise<any> {
        return this.supplierService.getSupplierPurchaseDeliveryInvoices(this.supplierInvoiceId).then((rows: PurchaseDeliveryInvoiceDTO[]) => {
            this.updateFieldsAfterLoad(rows);

            this.purchaseInvoiceRows = rows;
            this.gridAg.setData(this.purchaseInvoiceRows);
        });
    }

    private rowChanged(row: PurchaseDeliveryInvoiceDTO) {
        row.isModified = true;
        this.gridAg.options.refreshRows(row);
        this.setParentAsModified();
    }

    private updateFieldsAfterLoad(rows: PurchaseDeliveryInvoiceDTO[]) {
        rows.forEach(
            r => {
                this.setIcon(r);
                r.invoiceRowSum = r.quantity * r.price;
            }
        );
    }

    private setParentAsModified() {
        this.$scope.$applyAsync(() => this.messagingHandler.publishSetDirty(this.parentGuid));
    }

    private populatePurchases(): ng.IPromise<any> {
        if (this.filteredPurchaseDict.length == 0) {
            return this.supplierService.getSupplierPurchases(this.supplierId).then((data: any[]) => {
                data.forEach((c) => {
                    this.filteredPurchaseDict.push({ id: c.purchaseId, label: c.purchaseNr });
                })
            });
        }
    }

    private setIcon(row: PurchaseDeliveryInvoiceDTO) {
        row["statusIcon"] = row.supplierinvoiceId || row.linkToInvoice ? "fas fa-link okColor" : "fal fa-unlink warningColor";
    }

    // HELPERS
    private getPurchaseRows(purchaseId: number): [isDefined: boolean, arr: PurchaseRowSmallDTO[]] {
        return [!!this.purchaseRows[purchaseId], this.purchaseRows[purchaseId] || []];
    }
}
