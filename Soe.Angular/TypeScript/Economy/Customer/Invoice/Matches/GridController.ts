import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { TabMessage } from "../../../../Core/Controllers/TabsControllerBase1";
import { EditController as CustomerInvoicesEditController } from "../../../../Common/Customer/Invoices/EditController";
import { EditController as CustomerPaymentsEditController } from "../../../../Common/Customer/Payments/EditController";
import { EditController as BillingInvoicesEditController } from "../../../../Shared/Billing/Invoices/EditController";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, InsecureDebtsButtonFunctions } from "../../../../Util/Enumerations";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { ICommonCustomerService } from "../../../../Common/Customer/CommonCustomerService";
import { Feature, SoeOriginType, TermGroup_BillingType, OrderInvoiceRegistrationType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    public gridHeaderComponentUrl;
    public gridFooterComponentUrl;
    public customers: ISmallGenericType[];
    public types: any[];

    public searchModel: any;
    public selectedCustomer: any;
    private terms: any;
    public selectedRow: any;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private commonCustomerService: ICommonCustomerService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService) {

        super(gridHandlerFactory, "economy.customer.invoice.matches.matches", progressHandlerFactory, messagingHandlerFactory);

        this.gridHeaderComponentUrl = this.urlHelperService.getViewUrl("searchHeader.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onDoLookUp(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))

        /*super("Soe.Economy.Customer.Invoice.Matches", "economy.customer.invoice.matches.matches", Feature.Economy_Customer_Invoice_Matches, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        this.gridHeaderComponentUrl = this.urlHelperService.getViewUrl("searchHeader.html");

        this.soeGridOptions.multiSelect = false;
        this.soeGridOptions.enableRowHeaderSelection = false;
        this.soeGridOptions.enableRowSelection = true;
        this.soeGridOptions.subscribe([
            new GridEvent(SoeGridOptionsEvent.RowSelectionChanged,
                row => {
                    this.selectedRow = row ? row.entity : null;
                })
        ]);*/
        this.setData("");
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.setData(""); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Customer_Invoice_Matches, loadReadPermissions: true, loadModifyPermissions: true });

        this.resetSearchModel();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.setData(""));
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([this.loadCustomers(), this.loadTypes()]);
    }

    private setupGrid() {
        this.gridAg.addColumnText("actorName", this.terms["common.customer"], null);
        this.gridAg.addColumnText("invoiceNr", this.terms["economy.customer.invoice.matches.invoicenr"], null);
        this.gridAg.addColumnText("paymentNr", this.terms["economy.customer.invoice.matches.paymentnr"], null);
        this.gridAg.addColumnNumber("amount", this.terms["economy.customer.invoice.matches.amount"], null, { decimals: 2});
        //this.gridAg.addColumnText("typeName", this.terms["common.type"], null);
        this.gridAg.addColumnSelect("typeName", this.terms["common.type"], null, { displayField: "typeName", selectOptions: null, populateFilterFromGrid: true });
        this.gridAg.addColumnDate("date", this.terms["economy.customer.invoice.matches.date"], null);
        this.gridAg.addColumnIcon(null, " ", null, { icon: "fal fa-file-search", toolTip: this.terms["economy.customer.invoice.matches.showinvoice"], onClick: this.openCustomerInvoice.bind(this) });

        this.gridAg.finalizeInitGrid("economy.supplier.invoice.matches.matches", true)
        this.setData("");
    }

    public openCustomerInvoice(row: any) {
        if (row.type === SoeOriginType.CustomerInvoice) {
            if (row.registrationType === OrderInvoiceRegistrationType.Ledger) {
                this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.typeName + " " + row.invoiceNr, row.invoiceId, CustomerInvoicesEditController, { id: row.invoiceId }, this.urlHelperService.getGlobalUrl('Common/Customer/Invoices/Views/edit.html')));
            }
            else {                
                this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.typeName + " " + row.invoiceNr, row.invoiceId, BillingInvoicesEditController, { id: row.invoiceId }, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html')));
            }            
        } else {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.typeName + " " + row.paymentNr, row.paymentRowId, CustomerPaymentsEditController, { paymentId: row.paymentRowId }, this.urlHelperService.getGlobalUrl('Common/Customer/Payments/Views/edit.html')));            
        }

    }

    private loadTypes() {
        var keys: string[] = [
            "common.type",
            "common.customer",
            "common.customer.invoices.invoicedate",
            "common.customer.invoices.paydate",
            "economy.customer.invoice.matches.date",
            "common.customer.invoices.customerinvoice",
            "economy.customer.invoice.matches.invoicenr",
            "economy.customer.invoice.matches.paymentnr",
            "economy.customer.invoice.matches.amount",
            "economy.customer.invoice.matches.showpayment",
            "economy.customer.invoice.matches.showinvoice",
            "economy.customer.invoice.matches.debitinvoice",
            "economy.customer.invoice.matches.creditinvoice",
            "economy.customer.invoice.matches.interestinvoice",
            "economy.customer.invoice.matches.demandinvoice",
            "economy.customer.invoice.matches.payment",
            "economy.customer.invoice.matches.paymentsuggestion"];

        this.types = [];
        return this.translationService.translateMany(keys)
            .then(terms => {
                var i = 0;
                this.types = [
                    { id: i++, name: "" },
                    { id: i++, name: terms["economy.customer.invoice.matches.debitinvoice"] },
                    { id: i++, name: terms["economy.customer.invoice.matches.creditinvoice"] },
                    { id: i++, name: terms["economy.customer.invoice.matches.interestinvoice"] },
                    { id: i++, name: terms["economy.customer.invoice.matches.demandinvoice"] },
                    { id: i++, name: terms["economy.customer.invoice.matches.payment"] }
                ];
                this.terms = terms;
                this.searchModel.type = 0;
            });
    }

    private loadCustomers() {
        return this.commonCustomerService.getCustomersDict(false, false, true)
            .then((customers: ISmallGenericType[]) => this.customers = customers);
    }

    public loadGridData() {
        this.search();
    }

    private setTypeName(row: any) {
        if (row.type === SoeOriginType.CustomerInvoice) {
            switch (row.billingType) {
                case TermGroup_BillingType.Credit: row.typeName = this.terms["economy.customer.invoice.matches.creditinvoice"]; break;
                case TermGroup_BillingType.Debit: row.typeName = this.terms["economy.customer.invoice.matches.debitinvoice"]; break;
                case TermGroup_BillingType.Interest: row.typeName = this.terms["economy.customer.invoice.matches.interestinvoice"]; break;
                case TermGroup_BillingType.Reminder: row.typeName = this.terms["economy.customer.invoice.matches.demandinvoice"]; break;
            }
            if (row.typeName && !row.isEditable) {
                row.typeName += ` (${this.terms["economy.customer.invoice.matches.paymentsuggestion"]})`;
            }
            return;
        }
        row.typeName = this.terms["economy.customer.invoice.matches.payment"];
    }
 
    public search() {
        this.gridAg.clearData();
        this.searchModel.actorId = this.selectedCustomer ? this.selectedCustomer.id : 0;

        this.progress.startLoadingProgress([() => {
            return this.commonCustomerService.getInvoicesPaymentsAndMatches(this.searchModel).then((x) => {
                x.forEach(row => {
                    this.setTypeName(row);
                    if (row.type === SoeOriginType.CustomerPayment)
                        row.amount = row.invoicePayedAmount;
                    else
                        row.amount = row.invoiceTotalAmount;
                });
                this.setData(x);
            });
        }]);
    }
    public resetSearchModel() {
        this.searchModel = {
            originType: SoeOriginType.CustomerInvoice
        };
    }
}
