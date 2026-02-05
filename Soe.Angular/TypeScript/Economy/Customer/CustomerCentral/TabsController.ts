import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IReportService } from "../../../Core/Services/ReportService";
import { TabsControllerBase1 } from "../../../Core/Controllers/TabsControllerBase1";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { EditController } from "./EditController";
import { EditController as BillingInvoicesEditController } from "../../../Shared/Billing/Invoices/EditController";
import { EditController as BillingOrdersEditController } from "../../../Shared/Billing/Orders/EditController";
import { EditController as EconomyInvoicesEditController } from "../../../Common/Customer/Invoices/EditController";
import { EditController as CommonCustomersEditController } from "../../../Common/Customer/Customers/EditController";
import { Constants } from "../../../Util/Constants";
import { Feature, OrderInvoiceRegistrationType } from "../../../Util/CommonEnumerations";

export class TabsController extends TabsControllerBase1 {

    terms: any;
    hasOfferPermission: boolean;
    hasOrderPermission: boolean;
    hasContractPermission: boolean;
    hasCustomerInvoicePermission: boolean;

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
        private reportService: IReportService,
        private commonCustomerService: ICommonCustomerService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        protected $scope: ng.IScope,
        private $injector: ng.auto.IInjectorService) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService, $scope)

        // Setup base class
        var part: string = "economy.customer.";
        super.initialize(part + "customercentral", part + "customercentral", part + "customercentral");

        this.messagingService.subscribe(Constants.EVENT_NEW_OFFER, (x) => {
            this.addOffer(x);
        }, this.$scope);

        this.messagingService.subscribe(Constants.EVENT_OPEN_OFFER, (x) => {
            this.editOffer(x);
        }, this.$scope);

        this.messagingService.subscribe(Constants.EVENT_NEW_ORDER, (x) => {
            this.addOrder(x);
        }, this.$scope);

        this.messagingService.subscribe(Constants.EVENT_OPEN_ORDER, (x) => {
            this.editOrder(x);
        }, this.$scope);

        this.messagingService.subscribe(Constants.EVENT_NEW_CUSTOMERINVOICE, (x) => {
            this.addInvoice(x);
        }, this.$scope);

        this.messagingService.subscribe(Constants.EVENT_OPEN_CUSTOMERINVOICE, (x) => {
            this.editInvoice(x);
        }, this.$scope);

        this.messagingService.subscribe(Constants.EVENT_NEW_CONTRACT, (x) => {
            this.addContract(x);
        }, this.$scope);

        this.messagingService.subscribe(Constants.EVENT_OPEN_CONTRACT, (x) => {
            this.editContract(x);
        }, this.$scope);

        this.messagingService.subscribe(Constants.EVENT_OPEN_EDITCUSTOMER, (x) => {
            this.editCustomer(x);
        }, this.$scope);
    }

    protected setupTabs() {
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions()
        ]).then(() => {
            this.addHomeTab(EditController, null, this.urlHelperService.getViewUrl("edit.html"));
        });
    }

    protected add() {
        this.addCreateNewTab(EditController);
    }

    protected addOffer(x: any) {
        this.addEditTab(this.terms["common.customer.invoices.newoffers"], "common.customer.invoices.newoffers", BillingOrdersEditController, x, this.urlHelperService.getGlobalUrl("billing/offers/views/editOffer.html"), true);
    }

    protected addOrder(x: any) {
        this.addEditTab(this.terms["common.customer.invoices.neworder"], "common.customer.invoices.neworder", BillingOrdersEditController, x, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/edit.html"), true);
    }

    protected addInvoice(x: any) {
        this.addEditTab(this.terms["common.customer.invoices.newcustomerinvoice"], "common.customer.invoices.newcustomerinvoice", BillingInvoicesEditController, x, this.urlHelperService.getGlobalUrl("shared/billing/invoices/views/edit.html"), true);
    }

    protected addContract(x: any) {
        this.addEditTab(this.terms["common.customer.invoices.newcontract"], "common.customer.invoices.newcontract", BillingOrdersEditController, x, this.urlHelperService.getGlobalUrl("billing/contracts/views/editContract.html"), true);
    }

    protected editOffer(row: any, useAssociated: boolean = false) {
        var activeTab = _.find(this.tabs, tab => tab.identifier === this.getEditIdentifier(row, useAssociated));
        if (activeTab) {
            this.setActiveTabIndex(activeTab.index);
        } else {
            this.addEditTab(this.terms["common.offer"] + " " + row.invoiceNr, this.getEditIdentifier(row, useAssociated), BillingOrdersEditController, { id: useAssociated ? row.associatedId : row.customerInvoiceId, feature: Feature.Billing_Offer_Status }, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/editOffer.html"), true);
        }
    }

    protected editOrder(row: any, useAssociated: boolean = false) {
        var activeTab = _.find(this.tabs, tab => tab.identifier === this.getEditIdentifier(row, useAssociated));
        if (activeTab) {
            this.setActiveTabIndex(activeTab.index);
        } else {
            this.addEditTab(this.terms["common.order"] + " " + row.invoiceNr, this.getEditIdentifier(row, useAssociated), BillingOrdersEditController, { id: useAssociated ? row.associatedId : row.customerInvoiceId }, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/edit.html"), true);
        }
    }

    protected editInvoice(row: any, useAssociated: boolean = false) {
        var activeTab = _.find(this.tabs, tab => tab.identifier === this.getEditIdentifier(row, useAssociated));
        if (activeTab) {
            this.setActiveTabIndex(activeTab.index);
        } else {
            if (row.registrationType === OrderInvoiceRegistrationType.Ledger)
                this.addEditTab(this.terms["common.customerinvoice"] + " " + row.invoiceNr, this.getEditIdentifier(row, useAssociated), EconomyInvoicesEditController, { id: useAssociated ? row.associatedId : row.customerInvoiceId }, this.urlHelperService.getGlobalUrl('Common/Customer/Invoices/Views/edit.html'), true);
            else
                this.addEditTab(this.terms["common.customerinvoice"] + " " + row.invoiceNr, this.getEditIdentifier(row, useAssociated), BillingInvoicesEditController, { id: useAssociated ? row.associatedId : row.customerInvoiceId }, this.urlHelperService.getGlobalUrl("shared/billing/invoices/views/edit.html"), true);
        }
    }

    protected editContract(row: any, useAssociated: boolean = false) {
        var activeTab = _.find(this.tabs, tab => tab.identifier === this.getEditIdentifier(row, useAssociated));
        if (activeTab) {
            this.setActiveTabIndex(activeTab.index);
        } else {
            this.addEditTab(this.terms["common.contract"] + " " + row.invoiceNr, this.getEditIdentifier(row, useAssociated), BillingOrdersEditController, { id: useAssociated ? row.associatedId : row.customerInvoiceId, feature: Feature.Billing_Contract_Status }, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/editContract.html"), true);
        }
    }

    protected editCustomer(row: any) {
        var activeTab = _.find(this.tabs, tab => tab.identifier === "customer_" + row.id);
        if (activeTab) {
            this.setActiveTabIndex(activeTab.index);
        } else {
            this.addEditTab(row.name, "customer_" + row.id, CommonCustomersEditController, { id: row.id }, this.urlHelperService.getGlobalUrl("common/customer/customers/views/edit.html"), true);
        }
    }

    protected edit(row: any) {

    }

    protected getEditIdentifier(row: any, useAssociated: boolean = false): any {
        return useAssociated ? row.associatedId : row.customerInvoiceId;
    }

    protected getEditName(data: any): string {
        return data.number ? data.number : "";
    }

    //LOOKUPS
    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "billing.project.central.projectcentral",
            "common.customer.invoices.orders",
            "common.customer.invoices.customerinvoices",
            "common.customer.invoices.newoffers",
            "common.customer.invoices.newoffers",
            "common.customer.invoices.neworder",
            "common.customer.invoices.newcustomerinvoice",
            "common.offer",
            "common.order",
            "common.customerinvoice",
            "common.contract",
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions() {
        var featureIds: number[] = [];
        featureIds.push(Feature.Billing_Order_Status);
        featureIds.push(Feature.Billing_Invoice_Status);
        featureIds.push(Feature.Billing_Offer_Status);
        featureIds.push(Feature.Billing_Contract_Status);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.hasOfferPermission = x[Feature.Billing_Offer_Status];
            this.hasOrderPermission = x[Feature.Billing_Order_Status];
            this.hasCustomerInvoicePermission = x[Feature.Billing_Invoice_Status];
            this.hasContractPermission = x[Feature.Billing_Contract_Status];
        });
    }
}