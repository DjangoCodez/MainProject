import { ICoreService } from "../../../Core/Services/CoreService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { Feature, TermGroup, TermGroup_BillingType } from "../../../Util/CommonEnumerations";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { GridEvent } from "../../../Util/SoeGridOptions";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Grid
    private soeGridOptions: ISoeGridOptionsAg;

    private paymentServices: any;
    private paymentServiceId: any;
    public invoices: any;
    public batchId: string;
    public exportPaymentId: number;
    public selectedTotal: number = 0;

    private terms: any;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $scope) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        //super("Economy.Export.PaymentServices.Edit", Feature.Economy_Export_Payments, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);
        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            //.onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.exportPaymentId = parameters.id || 0;
        this.batchId = parameters.batchId;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Export_Payments, loadReadPermissions: true, loadModifyPermissions: true }]);

        this.soeGridOptions = new SoeGridOptionsAg("InvoicesGrid", this.$timeout);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Export_Payments].readPermission;
        this.modifyPermission = response[Feature.Economy_Export_Payments].modifyPermission;
    }

    private onDoLookups() {
        return this.$q.all([
            this.loadPaymentService(),
            this.loadTerms()]).then(() => {
                this.setupGrid();
                this.loadData();
            });
    }

    // LOOKUPS

    private loadPaymentService() {
        return this.coreService.getTermGroupContent(TermGroup.InvoicePaymentService, true, true)
            .then(x => {
                this.paymentServices = x;
                this.paymentServiceId = this.paymentServices[0].id;
            });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "economy.export.payments.bankaccountmissing",
            "economy.export.payments.bankaccountmissingincompany",
            "economy.export.payments.customernrmissing",
            "economy.export.payments.failedtocreatefile",
            "common.type",
            "common.report.selection.invoicenr",
            "common.customer",
            "economy.export.payments.invoiceamount",
            "economy.export.payments.invoicedate",
            "economy.export.payments.paydate",
            "economy.export.payments.bankaccount",
            "common.send",
            "common.debit",
            "common.credit",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "economy.customer.invoice.matches.demandinvoice",
            "economy.customer.invoice.matches.interestinvoice",
        ];
        return this.translationService.translateMany(keys)
            .then(terms => this.terms = terms);
    }

    // ACTIONS
    public setupGrid() {
        this.soeGridOptions.addColumnText("invoiceTypeName", this.terms["common.type"], null);
        this.soeGridOptions.addColumnNumber("invoiceNr", this.terms["common.report.selection.invoicenr"], null);
        this.soeGridOptions.addColumnText("customerName", this.terms["common.customer"], null);
        this.soeGridOptions.addColumnNumber("invoiceAmount", this.terms["economy.export.payments.invoiceamount"], null, { decimals: 2 });
        this.soeGridOptions.addColumnDate("invoiceDate", this.terms["economy.export.payments.invoicedate"], null);
        this.soeGridOptions.addColumnDate("dueDate", this.terms["economy.export.payments.paydate"], null);
        this.soeGridOptions.addColumnText("bankAccount", this.terms["economy.export.payments.bankaccount"], null);

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.summarizeSelected(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.summarizeSelected(); }));
        this.soeGridOptions.subscribe(events);

        this.soeGridOptions.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"]
        });

        this.soeGridOptions.finalizeInitGrid();
    }

    private summarizeSelected() {
        this.$scope.$applyAsync(() => {
            this.selectedTotal = 0;
            _.forEach(this.soeGridOptions.getSelectedRows(), (y: any) => {
                this.selectedTotal += y.invoiceAmount;
            });
        });
    }

    public paymentServiceChanged() {
        this.invoices = [];
    }

    public createTemplate() {
        if (!this.paymentServiceId)
            this.invoices = [];
        else
            this.accountingService.getInvoicesForPaymentService(this.paymentServiceId)
                .then(x => {
                    this.invoices = x;

                    this.setInvoicesType();

                    this.soeGridOptions.setData(this.invoices);
                });
    }
    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.exportPaymentId = 0;
    }

    public export() {
        var invoices = this.soeGridOptions.getSelectedRows();
        if (invoices.filter(r => !r.bankAccount).length > 0) {
            var message = this.terms["economy.export.payments.bankaccountmissing"];
            this.notificationService.showDialog(message, message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
        } else {
            this.progress.startWorkProgress((completion) => {
                return this.accountingService.saveCustomerInvoicePaymentService(this.paymentServiceId, invoices).then(x => {
                    if (x.success) {
                        completion.completed(x, true);
                        var url: string = "?c=" + CoreUtility.actorCompanyId + "&r=" + CoreUtility.roleId + "&filename=" + x.stringValue;
                        window.open(url, '_blank');

                    } else {
                        completion.failed(x.errorMessage);
                    }
                });
            });
        }
    }

    private loadData(): ng.IPromise<any> {
        if (this.exportPaymentId) {
            return this.accountingService.getExportedIOInvoices(this.exportPaymentId).then((result) => {
                this.invoices = result;

                this.setInvoicesType();

                this.soeGridOptions.setData(this.invoices);
            });
        }
        else {
            this.new();
        }
    }

    private setInvoicesType() {
        this.invoices.forEach(invoice => {
            switch (invoice.invoiceType) {
                case TermGroup_BillingType.Debit:
                    invoice.invoiceTypeName = this.terms["common.debit"];
                    break;
                case TermGroup_BillingType.Credit:
                    invoice.invoiceTypeName = this.terms["common.credit"];
                    break;
                case TermGroup_BillingType.Interest:
                    invoice.invoiceTypeName = this.terms["economy.customer.invoice.matches.interestinvoice"];
                    break;
                case TermGroup_BillingType.Reminder:
                    invoice.invoiceTypeName = this.terms["economy.customer.invoice.matches.demandinvoice"];
                    break;
            }
        });
    }
}