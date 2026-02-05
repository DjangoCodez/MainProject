import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ProductRowDTO } from "../../../../Common/Models/InvoiceDTO";
import { ProductRowsRowFunctions, SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { StringUtility } from "../../../../Util/StringUtility";
import { SoeOriginStatusClassification, SoeOriginType, Feature, OrderInvoiceRegistrationType, SoeEntityState } from "../../../../Util/CommonEnumerations";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { Constants } from "../../../../Util/Constants";

export class CopyProductRowsController {

    // Terms
    private terms: any;

    // Values
    private classification: SoeOriginStatusClassification;
    private selectedCustomerInvoice: number;
    private selectedPosition: number;
    private functionName: string;
    private functionNameDefinitive: string;
    private functionNameVerb: string;
    private moveSelection: string;
    private toolbarTitle: string;
    private selectLabel: string;
    private workingMessage: string;
    private recalculateSalesPrices: boolean = false;

    // Collections
    private numbersDict: ISmallGenericType[] = [];
    private positionDict: ISmallGenericType[] = [];
    private copiedRows: any[] = [];

    // Flags
    private working = false;
    private showStatus = false;
    private showNumbersSelect: boolean;
    private showPositionSelect: boolean;
    private rowsCopied = false;

    // Grid
    private soeGridOptions: ISoeGridOptionsAg;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private $q: ng.IQService,
        private productRows: ProductRowDTO[],
        private selectedProductRows: number[],
        private originType: SoeOriginType,
        private buttonFunction: ProductRowsRowFunctions,
        private move: boolean,
        private within: boolean,
        private toContract: boolean,
        private invoiceId: number) {
    }

    private $onInit() {
        this.soeGridOptions = new SoeGridOptionsAg("Billing.Dialogs.CopyProductRows.ProductRows", this.$timeout);
        this.soeGridOptions.enableGridMenu = false;
        
        if (this.within)
            this.soeGridOptions.enableSingleSelection();
        
        this.soeGridOptions.setMinRowsToShow(8);

        if (this.buttonFunction === ProductRowsRowFunctions.MoveRowsWithinOrder)
            this.showPositionSelect = true;
        else
            this.showNumbersSelect = true;

        this.$q.all([
            this.loadReadOnlyPermissions(),
            this.loadTerms()]).then(() => {
                this.getTargetNumbers();
            }).then(() => {
                this.setTerms();
                this.setupGrid();
            });
    }

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        const featureIds = [
            Feature.Billing_Order_Orders,
            Feature.Billing_Order_Status_OrderToInvoice,
            Feature.Billing_Order_OrdersUser,
            Feature.Billing_Invoice_Invoices,
            Feature.Billing_Invoice_InvoicesUser,
            Feature.Billing_Offer_Offers,
            Feature.Billing_Offer_OffersUser
        ];

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            if (this.originType == SoeOriginType.Order) {
                if (x[Feature.Billing_Order_Orders] || x[Feature.Billing_Order_Status_OrderToInvoice])
                    this.classification = SoeOriginStatusClassification.OrdersOpen;
                else if (x[Feature.Billing_Order_OrdersUser])
                    this.classification = SoeOriginStatusClassification.OrdersOpenUser;
                else
                    this.classification = SoeOriginStatusClassification.None;
            } else if (this.originType == SoeOriginType.CustomerInvoice) {
                if (x[Feature.Billing_Invoice_Invoices])
                    this.classification = SoeOriginStatusClassification.CustomerInvoicesOpen;
                else if (x[Feature.Billing_Invoice_InvoicesUser])
                    this.classification = SoeOriginStatusClassification.CustomerInvoicesOpenUser;
                else
                    this.classification = SoeOriginStatusClassification.None;
            } else if (this.originType == SoeOriginType.Offer) {
                if (x[Feature.Billing_Offer_Offers])
                    this.classification = SoeOriginStatusClassification.OffersOpen;
                else if (x[Feature.Billing_Offer_OffersUser])
                    this.classification = SoeOriginStatusClassification.OffersOpenUser;
                else
                    this.classification = SoeOriginStatusClassification.None;
            }else {
                this.classification = SoeOriginStatusClassification.None;
            }
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.rownr",
            "common.name",
            "billing.productrows.productnr",
            "billing.productrows.text",
            "billing.productrows.quantity",
            "core.copy",
            "core.thecopying",
            "core.copying",
            "core.move",
            "core.themove",
            "core.moving",
            "billing.productrows.copyrows.choosetargetrow",
            "billing.productrows.copyrows.movedrowstarget",
            "billing.productrows.copyrows.beforeselected",
            "billing.productrows.copyrows.afterselected",
            "billing.order.productrows",
            "billing.productrows.copyrows.chooseinvoice",
            "billing.productrows.copyrows.choosecontract",
            "billing.productrows.copyrows.chooseorder",
            "core.succeeded",
            "core.failed",
            "billing.offer.chooseoffer",
            "billing.contract.choosecontracttomoveto",
            "billing.contract.choosecontracttocopyto"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private getTargetNumbers(): ng.IPromise<any> {
        if (!this.within) {
            if (this.buttonFunction === ProductRowsRowFunctions.CopyRowsToContract || this.originType === SoeOriginType.Contract) {
                return this.coreService.getCustomerInvoiceNumbersDict(0, SoeOriginType.Contract, SoeOriginStatusClassification.ContractsRunning, OrderInvoiceRegistrationType.Contract).then((x) => {
                    this.numbersDict = x;
                });
            } else {
                return this.coreService.getCustomerInvoiceNumbersDict(0, this.originType, this.classification, this.originType == SoeOriginType.Order ? OrderInvoiceRegistrationType.Order : OrderInvoiceRegistrationType.Invoice, true).then((x) => {
                    this.numbersDict = x;
                });
            }
        } else {
            return null;
        }
    }

    private setupGrid() {
        if (this.buttonFunction === ProductRowsRowFunctions.MoveRowsWithinOrder) {
            const numberCol = this.soeGridOptions.addColumnText("rowNr", this.terms["common.rownr"], 100);
            numberCol.name = "numberCol";
        }
        const productCol = this.soeGridOptions.addColumnText("productNr", this.terms["billing.productrows.productnr"], null);
        productCol.name = "productCol";
        const textCol = this.soeGridOptions.addColumnText("text", this.terms["common.name"], null);
        textCol.name = "textCol";
        this.soeGridOptions.addColumnNumber("quantity", this.terms["billing.productrows.quantity"], 100, {
            editable: true, cellClassRules: {
                "text-right": () => true,
                "errorRow": (gridRow: any) => gridRow.data.timeManuallyChanged,
                "deleted": (gridRow: any) => gridRow.data.state === SoeEntityState.Deleted,
            }
        });

        this.soeGridOptions.addColumnShape("attestStateColor", null, null, { shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", showIconField: "attestStateColor" }).pinned = "right";

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row) => {
            this.$timeout(() => {
                this.buttonEnabled();
            });
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row) => {
            this.$timeout(() => {
                this.buttonEnabled();
            });
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.IsRowSelectable, (rowNode) => {
            return rowNode.data && !rowNode.data.isReadOnly;
        }));

        this.soeGridOptions.subscribe(events);

        this.soeGridOptions.finalizeInitGrid();

        let sortedCollection = _.filter(this.productRows, (r) => _.includes(this.selectedProductRows, r.tempRowId));
        _.forEach(this.productRows, (row) => {
            if (!_.find(sortedCollection, (rr) => rr.tempRowId === row.tempRowId))
                sortedCollection.push(row);
        });
        this.soeGridOptions.setData(sortedCollection);

        this.$timeout(() => _.forEach(this.selectedProductRows, (n) => {
            const row = _.find(this.productRows, { tempRowId: n });
            if (row) {
                this.soeGridOptions.selectRow(row, true);
            }
        }), null);
    }

    private setTerms() {
        // Functions
        if (this.buttonFunction === ProductRowsRowFunctions.CopyRows || this.buttonFunction === ProductRowsRowFunctions.CopyRowsToContract) {
            this.functionName = this.terms["core.copy"];
            this.functionNameDefinitive = this.terms["core.thecopying"];
            this.functionNameVerb = this.terms["core.copying"];
        } else {
            this.functionName = this.terms["core.move"];
            this.functionNameDefinitive = this.terms["core.themove"];
            this.functionNameVerb = this.terms["core.moving"];
        }

        // Title
        if (this.buttonFunction === ProductRowsRowFunctions.MoveRowsWithinOrder) {
            this.toolbarTitle = this.terms["billing.productrows.copyrows.choosetargetrow"];
            this.moveSelection = this.terms["billing.productrows.copyrows.movedrowstarget"];
            this.positionDict.push({ id: 1, name: this.terms["billing.productrows.copyrows.beforeselected"] });
            this.positionDict.push({ id: 2, name: this.terms["billing.productrows.copyrows.afterselected"] });
            this.selectedPosition = 1;
        } else {
            this.toolbarTitle = this.functionName + " " + this.terms["billing.order.productrows"].toLowerCase();

            // Selections
            if (this.originType == SoeOriginType.CustomerInvoice) {
                this.selectLabel = this.terms["billing.productrows.copyrows.chooseinvoice"].format(StringUtility.nullToEmpty(this.functionName.toLowerCase()));
            }
            else if (this.originType === SoeOriginType.Contract) {
                if (this.buttonFunction == ProductRowsRowFunctions.CopyRows)
                    this.selectLabel = this.terms["billing.contract.choosecontracttocopyto"].format(StringUtility.nullToEmpty(this.functionName.toLowerCase()));
                else
                    this.selectLabel = this.terms["billing.contract.choosecontracttomoveto"].format(StringUtility.nullToEmpty(this.functionName.toLowerCase()));
            }
            else if (this.originType === SoeOriginType.Offer) {
                this.selectLabel = this.terms["billing.offer.chooseoffer"].format(StringUtility.nullToEmpty(this.functionName.toLowerCase()));
            }else {
                if (this.buttonFunction == ProductRowsRowFunctions.CopyRowsToContract)
                    this.selectLabel = this.terms["billing.productrows.copyrows.choosecontract"].format(StringUtility.nullToEmpty(this.functionName.toLowerCase()));
                else
                    this.selectLabel = this.terms["billing.productrows.copyrows.chooseorder"].format(StringUtility.nullToEmpty(this.functionName.toLowerCase()));
            }
        }
    }

    buttonCancelClick() {
        this.close(null);
    }

    buttonOkClick() {
        if (this.working)
            return;

        this.soeGridOptions.stopEditing(false);

        this.$timeout(() => {
            if (this.rowsCopied) {
                this.close({ rows: this.buttonFunction === ProductRowsRowFunctions.MoveRows ? this.copiedRows : null, targetInvoiceId: this.selectedCustomerInvoice });
            } else {
                if (this.buttonFunction === ProductRowsRowFunctions.MoveRowsWithinOrder) {
                    const row = _.first(this.soeGridOptions.getSelectedRows());
                    this.close({ moveTo: row ? row.rowNr : null, position: this.selectedPosition });
                } else {
                    const rowsToCopy = this.soeGridOptions.getSelectedRows();
                    if (rowsToCopy && rowsToCopy.length > 0 && this.selectedCustomerInvoice) {
                        this.working = this.showStatus = true;
                        this.coreService.copyCustomerInvoiceRows(rowsToCopy, this.buttonFunction === ProductRowsRowFunctions.CopyRowsToContract ? SoeOriginType.Contract : this.originType, this.selectedCustomerInvoice, this.invoiceId, (this.move && !this.within), this.recalculateSalesPrices).then((result) => {
                            this.working = false;
                            if (result.success) {
                                this.workingMessage = this.functionNameDefinitive + " " + this.terms["core.succeeded"];
                                this.functionName = "OK";
                                this.rowsCopied = true;
                                _.forEach(rowsToCopy, (r) => {
                                    this.copiedRows.push({ id: r.tempRowId, quantity: r.quantity ? r.quantity : 0 });
                                });
                            } else {
                                this.workingMessage = this.functionNameDefinitive + " " + this.terms["core.failed"];
                            }
                        });
                    }
                }
            }
        });
    }

    close(result: any) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        } else {
            this.$uibModalInstance.close(result);
        }
    }

    buttonEnabled() {
        if (this.buttonFunction === ProductRowsRowFunctions.MoveRowsWithinOrder) {
            return this.selectedPosition && this.selectedPosition > 0 && this.soeGridOptions.getSelectedRows().length > 0 && !this.working;
        } else {
            return this.selectedCustomerInvoice && this.selectedCustomerInvoice > 0 && this.soeGridOptions.getSelectedRows().length > 0 && !this.working;
        }
    }
}