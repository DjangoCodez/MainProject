import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { EditController as VouchersEditController } from "../../../Shared/Economy/Accounting/Vouchers/EditController";
import { EditController as CustomerInvoicesEditController } from "../../../Common/Customer/Invoices/EditController";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { IDirtyHandler } from "../../../Core/Handlers/DirtyHandler";
import { Guid } from "../../../Util/StringUtility";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditController as SupplierInvoicesEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { EditController as SupplierPaymentsEditController } from "../../../Shared/Economy/Supplier/Payments/EditController";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";

export class EditController extends GridControllerBase2Ag implements ICompositionGridController {

    reconciliationPerAccount: any;
    terms: { [index: string]: string; };
    voucherSeries: {
        id: string,
        name: string,
    }[];

    public editPanelName: string;

    private accountId: number;
    private accountYearId: number;
    private fromDate: string;
    private toDate: string;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(gridHandlerFactory, "Economy.Accounting.Reconciliation.reconciliation", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onDoLookUp(() => this.onDoLookups());
    }

    public onInit(parameters: any) {

        this.accountId = parameters.accountId;
        this.accountYearId = parameters.accountYearId;
        this.fromDate = parameters.fromDate;
        this.toDate = parameters.toDate;
        this.voucherSeries = [];

        this.flowHandler.start({ feature: Feature.Economy_Accounting_Reconciliation, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    public loadGridData() {
        this.progress.startLoadingProgress([
            () => this.loadVoucherSeries(),
            () => this.load()]);
    }

    private onDoLookups(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        deferral.resolve();
        return deferral.promise;
    }

    private setupGrid() {
        
        var translationKeys = [
            "common.type",
            "common.date",
            "economy.accounting.vatverification.vouchernumber",
            "economy.accounting.voucherseriestype",
            "common.name",
            "common.amount",
            "common.state",
            "core.edit",
            "economy.accounting.voucher.voucher",
            "economy.supplier.invoice.invoice",
            "economy.accounting.reconciliation.paymentamount",
            "economy.accounting.accountdistribution.customerinvoice",
            "economy.accounting.reconciliation.paymentinvoice",
            "common.green",
            "common.yellow",
            "common.red"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            this.terms = terms;
            this.gridAg.addColumnText("type", terms["common.type"], null);
            this.gridAg.addColumnText("number", terms["economy.accounting.vatverification.vouchernumber"], null);
            this.gridAg.addColumnText("voucherSeriesTypeName", terms["economy.accounting.voucherseriestype"], null);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnNumber("customerAmount", terms["common.amount"], null, { decimals:2 }); //customerAmount using as amount in details grid (and therefor diffAmount, supplierAmount, paymentAmount is not used)                
            this.gridAg.addColumnDate("date", this.terms["common.date"], null);
            this.gridAg.addColumnShape("attestStateColor", null, 55, { shape: Constants.SHAPE_CIRCLE, showIconField: "attestStateColor" });            
            this.gridAg.addColumnEdit(terms["core.edit"], this.openEdit.bind(this));

            this.gridAg.addStandardMenuItems();
            this.gridAg.setExporterFilenamesAndHeader("economy.accounting.reconciliation.reconciliation");

            this.gridAg.options.finalizeInitGrid();
        });
    }

    private load(): ng.IPromise<any> {
        if (this.accountId <= 0) {
            return null;
        }

        return this.accountingService.getReconciliationPerAccount(this.accountId, new Date(this.fromDate), new Date(this.toDate)).then((x) => {
            
            this.reconciliationPerAccount = x;
            this.editPanelName = x[0].account;

            _.forEach(this.reconciliationPerAccount, (item: any) => {
                // Set type
                if (item.type == 1)
                    item.type = this.terms["economy.accounting.voucher.voucher"];
                else if (item.type == 2)
                    item.type = this.terms["economy.accounting.accountdistribution.customerinvoice"];
                else if (item.type == 3)
                    item.type = this.terms["economy.supplier.invoice.invoice"];
                else if (item.type == 4) {
                    item.type = this.terms["economy.accounting.reconciliation.paymentamount"];
                    item.name = this.terms["economy.accounting.reconciliation.paymentinvoice"] + " " + item.name;
                }
                else
                    item.type = "";
                item.voucherSeriesTypeName = "";
                _.forEach(this.voucherSeries, (voucherSeriesItem: any) => {
                    if (voucherSeriesItem.id == item.voucherSeriesId) {
                        item.voucherSeriesTypeName = voucherSeriesItem.name;
                    }
                });

                switch (item.rowStatus) {
                    case 1:
                        item.attestStateColor = "#2ACE2A";
                        item.attestStateName = this.terms["common.green"];
                        break;
                    case 2:
                        item.attestStateColor = "#FCFF00";
                        item.attestStateName = this.terms["common.yellow"];
                        break;
                    case 3:
                        item.attestStateColor = "#FF3D3D";  //"#FF0000";
                        item.attestStateName = this.terms["common.red"];
                        break;
                    default:
                        item.attestStateColor = "#2ACE2A";
                        item.attestStateName = this.terms["common.green"];
                        break;
                }
            });

            this.setData(x);
        });
    }

    private loadVoucherSeries(): ng.IPromise<any> {
        return this.accountingService.getVoucherSeriesByYear(this.accountYearId, false, true).then((x) => {
            _.forEach(x, (y: any) => {
                this.voucherSeries.push({ id: y.voucherSeriesId, name: y.voucherSeriesTypeName })
            });
        });
    }

    protected openEdit(row) {
        var controller: any;
        var templateUrl: any;
        var params = { id: row.associatedId, paymentId: undefined };
        if (row.type == this.terms["economy.accounting.voucher.voucher"]) {
            controller = VouchersEditController;
            templateUrl = this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html');
        }
        else if (row.type == this.terms["economy.accounting.accountdistribution.customerinvoice"]) {
            controller = CustomerInvoicesEditController;
            templateUrl = this.urlHelperService.getGlobalUrl('Common/Customer/Invoices/Views/edit.html');
        }
        else if (row.type == this.terms["economy.supplier.invoice.invoice"]) {
            controller = SupplierInvoicesEditController;
            templateUrl = this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html");
        }
        else if (row.type == this.terms["economy.accounting.reconciliation.paymentamount"]) {
            controller = SupplierPaymentsEditController;
            templateUrl = this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Payments/Views/edit.html");
            params.paymentId = row.associatedId;
        }

        if (controller && templateUrl) {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(`${row.type} ${row.number}`, row.associatedId, controller, params, templateUrl));
        }
    }
}