import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { CustomerInvoiceGridDTO } from "../../../Common/Models/InvoiceDTO";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SelectCustomerController } from "../../../Common/Dialogs/SelectCustomer/SelectCustomerController";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { FlaggedEnum } from "../../../Util/EnumerationsUtility";
import { Feature, CompanySettingType, TermGroup, SoeOriginType, TermGroup_AttestEntity, SoeModule, SoeOriginStatusClassification, InvoiceRowInfoFlag, SoeStatusIcon, SoeInvoiceRowType, SoeInvoiceRowDiscountType, SoeCategoryType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { NumberUtility } from "../../../Util/NumberUtility";
import { GridMenuBuilder } from "../../../Util/ag-grid/GridMenuBuilder";
import { CategoryDTO } from "../../../Common/Models/Category"
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4"
import { StringUtility } from "../../../Util/StringUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    actorCustomerId: number;
    private modalInstance: any;
    customer: any;

    // Permissions
    contractPermission = false;
    offerPermission = false;
    orderPermission = false;
    invoicePermission = false;
    offerForeignPermission = false;
    orderForeignPermission = false;
    invoiceForeignPermission = false;
    contractUserPermission = false;
    offerUserPermission = false;
    orderUserPermission = false;
    invoiceUserPermission = false;
    contractEditPermission = false;
    offerEditPermission = false;
    orderEditPermission = false;
    invoiceEditPermission = false;
    currencyPermission: boolean;
    productSalesPricePermission: boolean;
    planningPermission: boolean;
    exportPermission: boolean;

    // Collections
    attestStatesOffer = [];
    attestStatesFullOffer = [];
    attestStatesOrder = [];
    attestStatesFullOrder = [];
    originStatusOffer: any[];
    originStatusOrder: any[];
    originStatusInvoice: any[];
    orderTypes: any[];
    orderTypesDict: any[];
    invoiceBillingTypes: any[];

    // Grid collections
    contracts: CustomerInvoiceGridDTO[];
    offers: CustomerInvoiceGridDTO[];
    orders: CustomerInvoiceGridDTO[];
    invoices: CustomerInvoiceGridDTO[];
    filteredContracts: CustomerInvoiceGridDTO[];
    filteredOffers: CustomerInvoiceGridDTO[];
    filteredOrders: CustomerInvoiceGridDTO[];
    filteredInvoices: CustomerInvoiceGridDTO[];

    // Settings
    coreBaseCurrency: number;

    // Expanders
    contractExpanderLabelValue: string;
    offerExpanderLabelValue: string;
    orderExpanderLabelValue: string;
    invoiceExpanderLabelValue: string;

    // Grids
    private contractGridOptions: ISoeGridOptionsAg;
    private offerGridOptions: ISoeGridOptionsAg;
    private orderGridOptions: ISoeGridOptionsAg;
    private invoiceGridOptions: ISoeGridOptionsAg;

    // Detail grids
    private contractDetailsGridOptions: ISoeGridOptionsAg;
    private offerDetailsGridOptions: ISoeGridOptionsAg;
    private orderDetailsGridOptions: ISoeGridOptionsAg;
    private invoiceDetailsGridOptions: ISoeGridOptionsAg;

    // Data
    customerNumber: string;
    customerName: string;
    customerAddress: string;
    customerPhone: string;
    phoneLabel: string;

    // Values
    contractsIncVat: number;
    contractsExVat: number;
    offersIncVat: number;
    offersExVat: number;
    offersCurrencyIncVat: number;
    offersCurrencyExVat: number;
    ordersIncVat: number;
    ordersExVat: number;
    ordersCurrencyIncVat: number;
    ordersCurrencyExVat: number;
    invoicesIncVat: number;
    invoicesExVat: number;
    invoicesCurrencyIncVat: number;
    invoicesCurrencyExVat: number;
    unpaid: number;
    unpaidCurrency: number;

    // Flags 
    isCustomerSelected: boolean = false;
    contractsLoaded: boolean = false;
    offersLoaded: boolean = false;
    ordersLoaded: boolean = false;
    invoicesLoaded: boolean = false;
    contractsExpanderOpen: boolean = false;
    offersExpanderOpen: boolean = false;
    ordersExpanderOpen: boolean = false;
    invoicesExpanderOpen: boolean = false;

    // Sums
    offerSelectedTotal: number;
    offerFilteredTotal: number;
    orderSelectedTotal: number;
    orderFilteredTotal: number;
    orderSelectedToBeInvoicedTotal: number;
    orderFilteredToBeInvoicedTotal: number;
    invoiceSelectedTotal: number;
    invoiceFilteredTotal: number;
    invoiceSelectedToPay: number;
    invoiceFilteredToPay: number;

    // Properties
    private _onlyOpenContracts: boolean;
    private categories: CategoryDTO[];
    private invoiceDeliveryTypes: ISmallGenericType[];
    get onlyOpenContracts() {
        return this._onlyOpenContracts;
    }
    set onlyOpenContracts(value: boolean) {
        this._onlyOpenContracts = value;
        if (this.contractsLoaded) {
            if (!this.onlyOpenContracts)
                this.filteredContracts = this.contracts;
            else
                this.filteredContracts = _.filter(this.contracts, c => !c.useClosedStyle);

            this.contractGridOptions.setData(this.filteredContracts);
            this.contractGridOptions.refreshRows();
        }
    }

    private _onlyOpenOffers: boolean;
    get onlyOpenOffers() {
        return this._onlyOpenOffers;
    }
    set onlyOpenOffers(value: boolean) {
        this._onlyOpenOffers = value;
        if (this.offersLoaded) {
            if (!this.onlyOpenOffers)
                this.filteredOffers = this.offers;
            else
                this.filteredOffers = _.filter(this.offers, c => !c.useClosedStyle);

            this.offerGridOptions.setData(this.filteredOffers);
            this.offerGridOptions.refreshRows();
        }
    }

    private _onlyOpenOrders: boolean;
    get onlyOpenOrders() {
        return this._onlyOpenOrders;
    }
    set onlyOpenOrders(value: boolean) {
        this._onlyOpenOrders = value;
        if (this.ordersLoaded) {
            if (!this.onlyOpenOrders)
                this.filteredOrders = this.orders;
            else
                this.filteredOrders = _.filter(this.orders, c => !c.useClosedStyle);

            this.orderGridOptions.setData(this.filteredOrders);
            this.orderGridOptions.refreshRows();
        }
    }

    private _onlyOpenInvoices: boolean;
    get onlyOpenInvoices() {
        return this._onlyOpenInvoices;
    }
    set onlyOpenInvoices(value: boolean) {
        this._onlyOpenInvoices = value;
        if (this.invoicesLoaded) {
            if (!this.onlyOpenInvoices)
                this.filteredInvoices = this.invoices;
            else
                this.filteredInvoices = _.filter(this.invoices, c => !c.useClosedStyle);

            this.invoiceGridOptions.setData(this.filteredInvoices);
            this.invoiceGridOptions.refreshRows();
        }
    }

    //@ngInject
    constructor(
        $uibModal,
        private $timeout,
        private $q,
        private coreService: ICoreService,
        private commonCustomerService: ICommonCustomerService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $scope) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            //.onLoadData(() => this.onLoadData()) //this.doLookups())
            .onDoLookUp(() => this.doLookups()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));


        // Head grids
        this.contractGridOptions = SoeGridOptionsAg.create("contractGrid", this.$timeout);
        this.contractGridOptions.setMinRowsToShow(15);
        this.offerGridOptions = SoeGridOptionsAg.create("offerGrid", this.$timeout);
        this.offerGridOptions.setMinRowsToShow(15);
        this.orderGridOptions = SoeGridOptionsAg.create("orderGrid", this.$timeout);
        this.orderGridOptions.setMinRowsToShow(15);
        this.invoiceGridOptions = SoeGridOptionsAg.create("invoiceGrid", this.$timeout);
        this.invoiceGridOptions.setMinRowsToShow(15);

        //Details grids
        this.contractDetailsGridOptions = SoeGridOptionsAg.create("contractGrid", this.$timeout);
        this.offerDetailsGridOptions = SoeGridOptionsAg.create("offerGrid", this.$timeout);
        this.orderDetailsGridOptions = SoeGridOptionsAg.create("orderGrid", this.$timeout);
        this.invoiceDetailsGridOptions = SoeGridOptionsAg.create("invoiceGrid", this.$timeout);

        this.onlyOpenContracts = true;
        this.onlyOpenOffers = true;
        this.onlyOpenOrders = true;
        this.onlyOpenInvoices = true;

        this.modalInstance = $uibModal;
    }

    public onInit(parameters: any) {
        this.actorCustomerId = soeConfig.actorCustomerId ? soeConfig.actorCustomerId : (parameters.id || 0);
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([{ feature: Feature.Economy_Customer_Customers, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private doLookups() {
        if (this.actorCustomerId && this.actorCustomerId > 0) {
            return this.progress.startLoadingProgress([
                () => this.loadTerms(),
                () => this.loadCompanySettings(),
                () => this.loadOrderTypes(),
                () => this.loadInvoiceBillingTypes(),
                () => this.loadOriginStatusOffer(),
                () => this.loadOriginStatusOrder(),
                () => this.loadOriginStatusInvoice(),
                () => this.loadAttestStatesOffer(),
                () => this.loadAttestStatesOrder(),
                () => this.loadCategories(),
                () => this.loadInvoiceDeliveryTypes(),
                () => this.loadCustomer(),
            ]).then( () => {
                this.loadCustomerCountersAndBalances();
                this.setupGridColumns();
            });
        }
        else {
            return this.progress.startLoadingProgress([
                () => this.loadTerms(),
                () => this.loadCompanySettings(),
                () => this.loadOrderTypes(),
                () => this.loadInvoiceBillingTypes(),
                () => this.loadOriginStatusOffer(),
                () => this.loadOriginStatusOrder(),
                () => this.loadOriginStatusInvoice(),
                () => this.loadCategories(),
                () => this.loadInvoiceDeliveryTypes(),
                () => this.loadAttestStatesOffer(),
                () => this.loadAttestStatesOrder()
            ]).then( () => {
                this.setupGridColumns();
                this.searchCustomer();
            });
        }
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Customer_Customers].readPermission;
        this.modifyPermission = response[Feature.Economy_Customer_Customers].modifyPermission;

        //loadModifyPermissions
        const featureIds: number[] = [];
        featureIds.push(Feature.Billing_Offer_Offers);
        featureIds.push(Feature.Billing_Order_Orders);
        featureIds.push(Feature.Billing_Invoice_Invoices);
        featureIds.push(Feature.Billing_Contract_Contracts);
        featureIds.push(Feature.Billing_Offer_OffersUser);                     // User
        featureIds.push(Feature.Billing_Order_OrdersUser);                     // User
        featureIds.push(Feature.Billing_Invoice_InvoicesUser);                 // User
        featureIds.push(Feature.Billing_Contract_ContractsUser);               // User
        featureIds.push(Feature.Billing_Offer_Status_Foreign);                 // Foreign
        featureIds.push(Feature.Billing_Order_Status_Foreign);                 // Foreign
        featureIds.push(Feature.Billing_Invoice_Status_Foreign);               // Foreign
        featureIds.push(Feature.Billing_Offer_Offers_Edit);                    // Edit
        featureIds.push(Feature.Billing_Order_Orders_Edit);                    // Edit
        featureIds.push(Feature.Billing_Invoice_Invoices_Edit);                // Edit
        featureIds.push(Feature.Billing_Contract_Contracts_Edit);              // Editx[Feature.Billing_Product_Products_ShowSalesPrice]
        featureIds.push(Feature.Billing_Product_Products_ShowSalesPrice);
        featureIds.push(Feature.Billing_Order_Planning);
        featureIds.push(Feature.Economy_Customer_Invoice_Invoices_Edit_ExportSOP);
        featureIds.push(Feature.Economy_Customer_Invoice_Invoices_Edit_ExportUniMicro);
        featureIds.push(Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDIRegnskap);
        featureIds.push(Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDnBNor);

        this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.contractPermission = x[Feature.Billing_Contract_Contracts];
            this.offerPermission = x[Feature.Billing_Offer_Offers];
            this.orderPermission = x[Feature.Billing_Order_Orders];
            this.invoicePermission = x[Feature.Billing_Invoice_Invoices];
            this.offerForeignPermission = x[Feature.Billing_Offer_Status_Foreign];
            this.orderForeignPermission = x[Feature.Billing_Order_Status_Foreign];
            this.currencyPermission = this.invoiceForeignPermission = x[Feature.Billing_Invoice_Status_Foreign];
            this.contractUserPermission = x[Feature.Billing_Contract_ContractsUser];
            this.offerUserPermission = x[Feature.Billing_Offer_OffersUser];
            this.orderUserPermission = x[Feature.Billing_Order_OrdersUser];
            this.invoiceUserPermission = x[Feature.Billing_Invoice_InvoicesUser];
            this.contractEditPermission = x[Feature.Billing_Contract_Contracts_Edit];
            this.offerEditPermission = x[Feature.Billing_Offer_Offers_Edit];
            this.orderEditPermission = x[Feature.Billing_Order_Orders_Edit];
            this.invoiceEditPermission = x[Feature.Billing_Contract_Contracts_Edit];
            this.productSalesPricePermission = x[Feature.Billing_Product_Products_ShowSalesPrice];
            this.planningPermission = x[Feature.Billing_Order_Planning];
            this.exportPermission = x[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportSOP] || x[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportUniMicro] || x[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDIRegnskap] || x[Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDnBNor];
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null, () => this.isNew);
        if (this.toolbar) {
            var groupSearchCustomer = ToolBarUtility.createGroup(new ToolBarButton("economy.customer.customercentral.seekcustomerbutton", "economy.customer.customercentral.seekcustomerbutton", IconLibrary.FontAwesome, "fa-search",
                () => { this.searchCustomer(); },
                null,
                null,
                { buttonClass: "ngSoeMainButton pull-left" }));
            this.toolbar.addButtonGroup(groupSearchCustomer);

            var groupNewInvoice = ToolBarUtility.createGroup(new ToolBarButton("common.customer.invoices.newcustomerinvoice", "common.customer.invoices.newcustomerinvoice", IconLibrary.FontAwesome, "fa-plus", () => {
                if (this.customer)
                    this.messagingService.publish(Constants.EVENT_NEW_CUSTOMERINVOICE, { customerId: this.customer ? this.customer.actorCustomerId : 0 });
                else
                    this.messagingService.publish(Constants.EVENT_NEW_CUSTOMERINVOICE, null);
            }, null, () => {
                return !this.invoiceEditPermission;
            }));
            this.toolbar.addButtonGroup(groupNewInvoice);

            var groupNewOrder = ToolBarUtility.createGroup(new ToolBarButton("common.customer.invoices.neworder", "common.customer.invoices.neworder", IconLibrary.FontAwesome, "fa-plus", () => {
                if (this.customer)
                    this.messagingService.publish(Constants.EVENT_NEW_ORDER, { customerId: this.customer ? this.customer.actorCustomerId : 0 });
                else
                    this.messagingService.publish(Constants.EVENT_NEW_ORDER, null);
            }, null, () => {
                return !this.orderEditPermission;
            }));
            this.toolbar.addButtonGroup(groupNewOrder);

            // Temporarily hidden since we don´t have any editcontrollers for offer and contract
            /*var groupNewOffer = Soe.Util.ToolBarUtility.createGroup(new Soe.Util.ToolBarButton("common.customer.invoices.newoffers", "common.customer.invoices.newoffers", IconLibrary.FontAwesome, "fa-plus", () => {
                if (this.customer)
                    this.messagingService.publish(Constants.EVENT_NEW_OFFER, { customerId: this.customer ? this.customer.actorCustomerId : 0 });
                else
                    this.messagingService.publish(Constants.EVENT_NEW_OFFER, null);
            }, null, () => {
                return !this.offerEditPermission;
            }));
            this.toolbar.addButtonGroup(groupNewOffer);

            var groupNewContract = Soe.Util.ToolBarUtility.createGroup(new Soe.Util.ToolBarButton("common.customer.invoices.newcontract", "common.customer.invoices.newcontract", IconLibrary.FontAwesome, "fa-plus", () => {
                if (this.customer)
                    this.messagingService.publish(Constants.EVENT_NEW_CONTRACT, { customerId: this.customer ? this.customer.actorCustomerId : 0 });
                else
                    this.messagingService.publish(Constants.EVENT_NEW_CONTRACT, null);
            }, null, () => {
                    return !this.contractEditPermission;
                }));
            this.toolbar.addButtonGroup(groupNewContract);*/
        }
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.customer",
            "core.controlquestion",
            "core.continue",
            "core.warning",
            "core.showinfo",
            "core.yes",
            "core.no",
            "common.telephonenumber",
            "common.customer.invoices.invoicenr",
            "common.customer.invoices.invoiceseqnr",
            "common.customer.customer.invoicedeliverytypeshort",
            "common.customer.invoices.ordernr",
            "common.customer.invoices.type",
            "common.customer.invoices.status",
            "common.customer.invoices.customer",
            "common.customer.invoices.internaltext",
            "common.customer.invoices.payservice",
            "common.customer.invoices.amount",
            "common.customer.invoices.amountexvat",
            "common.customer.invoices.invoicedate",
            "common.customer.invoices.duedate",
            "common.customer.invoices.paydate",
            "common.customer.invoices.editinvoice",
            "common.customer.invoices.showinfo",
            "common.customer.invoices.haschecklists",
            "common.customer.invoices.hashousededuction",
            "common.customer.invoices.export",
            "common.customer.invoices.foreignamount",
            "common.customer.invoices.currencycode",
            "common.customer.invoices.projectnr",
            "common.customer.invoices.orderdate",
            "common.customer.invoices.deliveryaddress",
            "common.customer.invoices.customerinvoice",
            "common.customer.payment.paymentseqnr",
            "common.customer.invoices.reminder",
            "common.customer.invoices.payableamount",
            "common.customer.invoices.paidamount",
            "common.customer.payment.payment",
            "common.customer.invoices.offernr",
            "common.customer.invoices.offerdate",
            "common.customer.invoices.rowstatus",
            "economy.supplier.payment.validinvoice",
            "economy.supplier.payment.validinvoices",
            "common.customer.invoices.invoicedatesnotentered",
            "common.customer.invoices.invoicedatesnotwithincurrentaccountyear",
            "common.customer.invoices.duedatesnotentered",
            "common.customer.invoices.duedatesnotwithincurrentaccountyear",
            "common.customer.invoices.autotovoucher",
            "common.customer.invoices.einvoicealreadysent",
            "common.customer.invoices.einvoice",
            "common.customer.invoices.einvoices",
            "common.customer.invoices.paymentautotransfertovoucher",
            "common.customer.invoices.insecureinvoiceerror",
            "common.customer.invoices.paydatemustbeset",
            "common.customer.payment.payment",
            "common.customer.payment.payments",
            "common.customer.invoices.customerinvoice",
            "common.customer.invoices.customerinvoices",
            "common.customer.invoices.createpaymentinvalid",
            "economy.supplier.payment.changepaydate",
            "common.imported",
            "common.hasaattachedfiles",
            "common.hasattachedimages",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "common.customer.invoices.ordertype",
            "common.customer.invoices.remainingamount",
            "common.customer.invoices.remainingamountexvat",
            "common.customer.invoices.fixedpriceordertype",
            "common.customer.invoices.deliverydate",
            "common.customer.invoices.seqnr",
            "economy.supplier.invoice.paidlate",
            "economy.supplier.invoice.matches.totalamount",
            "economy.supplier.payment.paymentamount",
            "economy.supplier.invoice.amounttopay",
            "economy.supplier.invoice.partlypaid",
            "common.customer.invoice.multipleassetrows",
            "economy.supplier.invoice.manualadjustmentneeded",
            "common.customer.invoice.stopped",
            "common.customer.invoices.checkvatamount",
            "common.customer.contracts.contractnumbershort",
            "common.categories",
            "common.startdate",
            "common.stopdate",
            "common.customer.contracts.nextperiod",
            "common.customer.contracts.contractgroup",
            "common.customer.contract.editcontract",
            "common.customer.invoices.invoiceunpaid",
            "common.customer.invoices.invoicepartlypaid",
            "common.customer.invoices.invoicepaid",
            "common.customer.invoices.row",
            "common.customer.invoices.edi",
            "common.customer.invoices.productnr",
            "common.customer.invoices.productname",
            "common.customer.invoices.quantity",
            "common.customer.invoices.unit",
            "common.customer.invoices.price",
            "common.customer.invoices.discount",
            "common.customer.invoices.sum",
            "economy.supplier.invoice.amountincvat",
            "economy.import.payment.invoicetotalamount",
            "common.customer.invoices.payamountcurrency",
            "common.customer.invoices.responsible",
            "common.customer.invoices.participant",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.phoneLabel = terms["common.telephonenumber"];
        });
    }

    private loadCompanySettings() {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.CoreBaseCurrency);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.coreBaseCurrency = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CoreBaseCurrency, 0);
        });

    }

    private loadOrderTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.OrderType, false, false).then((x) => {
            this.orderTypes = x;
            this.orderTypesDict = [];
            _.forEach(x, (row) => {
                this.orderTypesDict.push({ value: row.name, label: row.name });
            });
        });
    }

    private loadInvoiceBillingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, false).then((x) => {
            this.invoiceBillingTypes = [];
            _.forEach(x, (row) => {
                this.invoiceBillingTypes.push({ value: row.name, label: row.name });
            });
        });
    }

    private loadOriginStatusOffer(): ng.IPromise<any> {
        return this.commonCustomerService.getInvoiceAndPaymentStatus(SoeOriginType.Offer, false).then((x) => {
            this.originStatusOffer = [];
            _.forEach(x, (row) => {
                this.originStatusOffer.push({ value: row.name, label: row.name });
            });
        });
    }

    private loadOriginStatusOrder(): ng.IPromise<any> {
        return this.commonCustomerService.getInvoiceAndPaymentStatus(SoeOriginType.Offer, false).then((x) => {
            this.originStatusOrder = [];
            _.forEach(x, (row) => {
                this.originStatusOrder.push({ value: row.name, label: row.name });
            });
        });
    }

    private loadOriginStatusInvoice(): ng.IPromise<any> {
        return this.commonCustomerService.getInvoiceAndPaymentStatus(SoeOriginType.Offer, false).then((x) => {
            this.originStatusInvoice = [];
            _.forEach(x, (row) => {
                this.originStatusInvoice.push({ value: row.name, label: row.name });
            });
        });
    }

    public loadAttestStatesOffer(): ng.IPromise<any> {
        return this.coreService.getAttestStates(TermGroup_AttestEntity.Offer, SoeModule.Billing, false).then((x) => {
            this.attestStatesFullOffer = x;
            this.attestStatesOffer.push({ value: this.terms["common.customer.invoice.norows"], label: this.terms["common.customer.invoice.norows"] });
            _.forEach(x, (y: any) => {
                this.attestStatesOffer.push({ value: y.name, label: y.name })
            });
            this.attestStatesOffer.push({ value: this.terms["common.customer.invoices.multiplestatuses"], label: this.terms["common.customer.invoices.multiplestatuses"] });
        });
    }

    public loadAttestStatesOrder(): ng.IPromise<any> {
        return this.coreService.getAttestStates(TermGroup_AttestEntity.Order, SoeModule.Billing, false).then((x) => {
            this.attestStatesFullOrder = x;
            this.attestStatesOrder.push({ value: this.terms["common.customer.invoice.norows"], label: this.terms["common.customer.invoice.norows"] });
            _.forEach(x, (y: any) => {
                this.attestStatesOrder.push({ value: y.name, label: y.name })
            });
            this.attestStatesOrder.push({ value: this.terms["common.customer.invoices.multiplestatuses"], label: this.terms["common.customer.invoices.multiplestatuses"] });
        });
    }

    public loadCategories(): ng.IPromise<any> {
        return this.coreService.getCategories(SoeCategoryType.Customer, false, false, false, false).then(categories => {
            this.categories = categories;
        })
    }

    public loadCustomer(): ng.IPromise<any> {
        return this.commonCustomerService.getCustomersBySearch("", "", "", "", "", this.actorCustomerId).then((result) => {
            if (result && result.length > 0)
                this.customer = result[0];
            this.loadCompleteCustomer(this.customer);
        });
    }

    private loadInvoiceDeliveryTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceDeliveryType, true, false).then(x => {
            this.invoiceDeliveryTypes = x;
        });
    }


    public loadCompleteCustomer(loadedData: any): ng.IPromise<any> {
        return this.commonCustomerService.getCustomer(this.actorCustomerId, false, false, true, false, true, true).then(data => {
            this.customer = { ...data, ...loadedData, categoryString: "", invoiceDeliveryTypeString: "", blockOrderString: "", phoneLabel: "" };
            this.customer.categoryIds.forEach((c, idx) => {
                this.customer.categoryString += `${idx === 0 ? "" : ", "}${this.categories.find(d => d.categoryId == c).name}`
            })

            var deliveryType = this.invoiceDeliveryTypes.find(t => this.customer.invoiceDeliveryType == t.id);
            if (deliveryType)
                this.customer.invoiceDeliveryTypeString = deliveryType.name;

            if (this.customer.blockOrder) {
                if (this.customer.blockNote)
                    this.customer.blockOrderString = `${this.terms["core.yes"]} (${this.customer.blockNote})`;
                else
                    this.customer.blockOrderString = this.terms["core.yes"];
            }
            else {
                this.customer.blockOrderString = this.terms["core.no"];
            }

            var phone = this.customer.contactAddresses.find(a => a.eComText == this.customer.phoneNumber);
            if (phone)
                this.phoneLabel = phone.name
        })
    }

    public setupGridColumns() {
        // Contracts grid
        let events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: uiGrid.IGridRow[]) => { this.summarizeFilteredContracts(rows); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.summarizeSelectedContracts(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.summarizeSelectedContracts(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.openContract(row); }));
        this.contractGridOptions.subscribe(events);

        this.contractGridOptions.addColumnText("invoiceNr", this.terms["common.customer.contracts.contractnumbershort"], null).cellRenderer = 'agGroupCellRenderer';
        this.contractGridOptions.addColumnText("mainUserName", this.terms["common.customer.invoices.responsible"], null, { enableHiding: false });
        this.contractGridOptions.addColumnText("users", this.terms["common.customer.invoices.participant"], null, { enableHiding: true });
        this.contractGridOptions.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, { enableHiding: false });
        this.contractGridOptions.addColumnText("categories", this.terms["common.categories"], null, { enableHiding: false });
        this.contractGridOptions.addColumnNumber("totalAmount", this.terms["economy.supplier.invoice.amountincvat"], null, { enableHiding: false, decimals: 2 });
        this.contractGridOptions.addColumnNumber("totalAmountExVat", this.terms["common.customer.invoices.amountexvat"], null, { enableHiding: false, decimals: 2 });
        this.contractGridOptions.addColumnDate("invoiceDate", this.terms["common.startdate"], null);
        this.contractGridOptions.addColumnDate("dueDate", this.terms["common.stopdate"], null);
        this.contractGridOptions.addColumnText("nextContractPeriod", this.terms["common.customer.contracts.nextperiod"], null, { enableHiding: false });
        this.contractGridOptions.addColumnText("contractGroupName", this.terms["common.customer.contracts.contractgroup"], null, { enableHiding: false });
        this.contractGridOptions.addColumnIcon(null, null, null, { icon: "fal fa-pencil iconEdit", onClick: this.openContract.bind(this) });

        // Offers grid
        events = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: uiGrid.IGridRow[]) => { this.summarizeFilteredOffers(rows); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.summarizeSelectedOffers(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.summarizeSelectedOffers(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.openOffer(row); }));
        this.offerGridOptions.subscribe(events);

        this.offerGridOptions.addColumnText("invoiceNr", this.terms["common.customer.invoices.offernr"], null).cellRenderer = 'agGroupCellRenderer';
        this.offerGridOptions.addColumnSelect("attestStateNames", this.terms["common.customer.invoices.rowstatus"], null, { selectOptions: this.attestStatesOffer, displayField: "attestStateNames" });
        this.offerGridOptions.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { selectOptions: this.originStatusOffer, displayField: "statusName" });
        this.offerGridOptions.addColumnText("mainUserName", this.terms["common.customer.invoices.responsible"], null, { enableHiding: true });
        this.offerGridOptions.addColumnText("users", this.terms["common.customer.invoices.participant"], null, { enableHiding: true });
        this.offerGridOptions.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, { enableHiding: true });
        this.offerGridOptions.addColumnText("deliveryAddress", this.terms["common.customer.invoices.deliveryaddress"], null, { enableHiding: true });
        this.offerGridOptions.addColumnNumber("totalAmount", this.terms["economy.supplier.invoice.amountincvat"], null, { enableHiding: true, decimals: 2 });
        this.offerGridOptions.addColumnNumber("totalAmountExVat", this.terms["common.customer.invoices.amountexvat"], null, { enableHiding: true, decimals: 2 });
        if (this.currencyPermission) {
            this.offerGridOptions.addColumnNumber("totalAmountCurrency", this.terms["common.customer.invoices.foreignamount"], null, { enableHiding: true, decimals: 2 });
            this.offerGridOptions.addColumnText("currencyCode", this.terms["common.customer.invoices.currencycode"], null, { enableHiding: true });
        }
        if (this.productSalesPricePermission)
            this.offerGridOptions.addColumnNumber("remainingAmount", this.terms["common.customer.invoices.remainingamount"], null, { enableHiding: true, decimals: 2 });
        this.offerGridOptions.addColumnDate("invoiceDate", this.terms["common.customer.invoices.offerdate"], null);
        this.offerGridOptions.addColumnIcon("statusIconValue", null, null, { onClick: this.showInformationMessage.bind(this), showIcon: this.isPropertyNull.bind(this, "statusIconValue"), toolTipField: "statusIconMessage" });
        this.offerGridOptions.addColumnIcon(null, null, null, { icon: "fal fa-pencil iconEdit", onClick: this.openOffer.bind(this) });

        // Orders grid
        events = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: uiGrid.IGridRow[]) => { this.summarizeFilteredOrders(rows); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.summarizeSelectedOrders(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.summarizeSelectedOrders(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.openOrder(row); }));
        this.orderGridOptions.subscribe(events);

        this.orderGridOptions.addColumnNumber("invoiceNr", this.terms["common.customer.invoices.ordernr"], null, { clearZero: true }).cellRenderer = 'agGroupCellRenderer';
        this.orderGridOptions.addColumnText("projectNr", this.terms["common.customer.invoices.projectnr"], null, { enableHiding: true });
        this.orderGridOptions.addColumnSelect("orderTypeName", this.terms["common.customer.invoices.ordertype"], null, { selectOptions: this.orderTypesDict, displayField: "orderTypeName", enableHiding: true });
        //this.addColumnShape("attestStateColor", null, null, "", Constants.SHAPE_CIRCLE, "attestStateNames", "", "attestStateColor", null, null, null, true);
        var colAttestStates = this.orderGridOptions.addColumnSelect("attestStateNames", this.terms["common.customer.invoices.rowstatus"], null, { displayField: "attestStateNames", selectOptions: this.attestStatesOrder, shapeValueField: "attestStateColor", shape: Constants.SHAPE_CIRCLE }); //, null, null, null, null, null, null, null, null, "attestStateColor", Constants.SHAPE_CIRCLE);
        (<any>colAttestStates).filters = [
            {
                condition: (term, value, row, column) => {
                    if (term === this.terms["common.customer.invoices.multiplestatuses"]) {
                        return (<string>value).indexOf(',') >= 0 ? true : false;
                    }
                    else {
                        return (<string>value).indexOf(term) >= 0 ? true : false;
                    }
                }
            }
        ];
        this.orderGridOptions.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { selectOptions: this.originStatusOrder, displayField: "statusName", enableHiding: true });
        this.orderGridOptions.addColumnText("mainUserName", this.terms["common.customer.invoices.responsible"], null, { enableHiding: true });
        this.orderGridOptions.addColumnText("users", this.terms["common.customer.invoices.participant"], null, { enableHiding: true });
        this.orderGridOptions.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, { enableHiding: true });
        this.orderGridOptions.addColumnText("invoicePaymentServiceName", this.terms["common.customer.invoices.payservice"], null, { enableHiding: true });
        this.orderGridOptions.addColumnText("deliveryAddress", this.terms["common.customer.invoices.deliveryaddress"], null, { enableHiding: true });
        if (this.productSalesPricePermission) {
            this.orderGridOptions.addColumnNumber("totalAmount", this.terms["economy.supplier.invoice.amountincvat"], null, { enableHiding: true, decimals: 2 });
            this.orderGridOptions.addColumnNumber("totalAmountExVat", this.terms["common.customer.invoices.amountexvat"], null, { enableHiding: true, decimals: 2 });
            if (this.currencyPermission) {
                this.orderGridOptions.addColumnNumber("totalAmountCurrency", this.terms["common.customer.invoices.foreignamount"], null, { enableHiding: true, decimals: 2 });
                this.orderGridOptions.addColumnText("currencyCode", this.terms["common.customer.invoices.currencycode"], null, { enableHiding: true });
            }
            this.orderGridOptions.addColumnNumber("remainingAmount", this.terms["common.customer.invoices.remainingamount"], null, { enableHiding: true, decimals: 2 });
            this.orderGridOptions.addColumnNumber("remainingAmountExVat", this.terms["common.customer.invoices.remainingamountexvat"], null, { enableHiding: true, decimals: 2 });
        }
        this.orderGridOptions.addColumnDate("invoiceDate", this.terms["common.customer.invoices.orderdate"], null, true);
        this.orderGridOptions.addColumnText("fixedPriceOrderName", this.terms["common.customer.invoices.fixedpriceordertype"], null, { enableHiding: true });
        this.orderGridOptions.addColumnDate("deliveryDate", this.terms["common.customer.invoices.deliverydate"], null, true);
        this.orderGridOptions.addColumnIcon(null, null, null, { icon: "fal fa-pencil iconEdit", onClick: this.openOrder.bind(this) });
        this.orderGridOptions.addColumnIcon("statusIconValue", null, null, { onClick: this.showInformationMessage.bind(this), showIcon: this.isPropertyNull.bind(this, "statusIconValue"), toolTipField: "statusIconMessage" });
        if (this.planningPermission)
            this.orderGridOptions.addColumnShape("shiftTypeColor", null, null, { shape: Constants.SHAPE_SQUARE, toolTipField: "shiftTypeName", showIconField: "shiftTypeColor" });
        this.orderGridOptions.addColumnNumber("seqNr", this.terms["common.customer.invoices.seqnr"], null, { alignLeft: true });

        // Invoices grid
        events = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: uiGrid.IGridRow[]) => { this.summarizeFilteredInvoices(rows); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.summarizeSelectedInvoices(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.summarizeSelectedInvoices(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.openInvoice(row); }));
        this.invoiceGridOptions.subscribe(events);

        this.invoiceGridOptions.addColumnNumber("seqNr", this.terms["common.customer.invoices.seqnr"], null, { alignLeft: true }).cellRenderer = 'agGroupCellRenderer';
        this.invoiceGridOptions.addColumnText("invoiceNr", this.terms["common.customer.invoices.invoicenr"], null);
        this.invoiceGridOptions.addColumnText("deliveryTypeName", this.terms["common.customer.customer.invoicedeliverytypeshort"], null, { enableHiding: true });
        
        this.invoiceGridOptions.addColumnText("orderNumbers", this.terms["common.customer.invoices.ordernr"], null, { enableHiding: true, hide:true });
        this.invoiceGridOptions.addColumnSelect("billingTypeName", this.terms["common.customer.invoices.type"], null, { selectOptions: this.invoiceBillingTypes, displayField: "billingTypeName" });
        this.invoiceGridOptions.addColumnSelect("statusName", this.terms["common.customer.invoices.status"], null, { selectOptions: this.originStatusInvoice, displayField: "statusName" });
        if (this.exportPermission)
            this.invoiceGridOptions.addColumnText("exportStatusName", this.terms["common.customer.invoices.export"], null, { enableHiding: true });
        this.invoiceGridOptions.addColumnText("mainUserName", this.terms["common.customer.invoices.responsible"], null, { enableHiding: true });
        this.invoiceGridOptions.addColumnText("users", this.terms["common.customer.invoices.participant"], null, { enableHiding: true });
        this.invoiceGridOptions.addColumnText("internalText", this.terms["common.customer.invoices.internaltext"], null, { enableHiding: true });
        this.invoiceGridOptions.addColumnText("deliveryAddress", this.terms["common.customer.invoices.deliveryaddress"], null, { enableHiding: true });

        this.invoiceGridOptions.addColumnText("invoicePaymentServiceName", this.terms["common.customer.invoices.payservice"], null, { enableHiding: true, hide:true });
        this.invoiceGridOptions.addColumnNumber("totalAmount", this.terms["economy.supplier.invoice.amountincvat"], null, { enableHiding: true, decimals: 2 });
        this.invoiceGridOptions.addColumnNumber("totalAmountExVat", this.terms["common.customer.invoices.amountexvat"], null, { enableHiding: true, decimals: 2 });
        this.invoiceGridOptions.addColumnNumber("totalAmount", this.terms["common.customer.invoices.amount"], null, { enableHiding: true, decimals: 2 });
        if (this.currencyPermission) {
            this.invoiceGridOptions.addColumnNumber("totalAmountCurrency", this.terms["common.customer.invoices.foreignamount"], null, { enableHiding: true, decimals: 2 });
            this.invoiceGridOptions.addColumnText("currencyCode", this.terms["common.customer.invoices.currencycode"], null, { enableHiding: false });
        }
        this.invoiceGridOptions.addColumnNumber("payAmount", this.terms["economy.import.payment.invoicetotalamount"], null, { enableHiding: true, decimals: 2 });
        if (this.currencyPermission)
            this.invoiceGridOptions.addColumnNumber("payAmountCurrency", this.terms["common.customer.invoices.payamountcurrency"], null, { enableHiding: true, decimals: 2 });
        this.invoiceGridOptions.addColumnText("projectNr", this.terms["common.customer.invoices.projectnr"], null, { enableHiding: true });
        this.invoiceGridOptions.addColumnDate("invoiceDate", this.terms["common.customer.invoices.invoicedate"], null, true);
        this.invoiceGridOptions.addColumnDate("dueDate", this.terms["common.customer.invoices.duedate"], null, true);
        this.invoiceGridOptions.addColumnDate("payDate", this.terms["common.customer.invoices.paydate"], null, true);
        this.invoiceGridOptions.addColumnIcon("billingIconValue", null, null, { showIcon: this.isPropertyNull.bind(this, "billingIconValue"), toolTipField: "billingIconMessage" });
        this.invoiceGridOptions.addColumnIcon(null, null, null, { icon: "fal fa-pencil iconEdit", onClick: this.openInvoice.bind(this) });
        this.invoiceGridOptions.addColumnIcon("statusIconValue", null, null, { onClick: this.showInformationMessage.bind(this), showIcon: this.isPropertyNull.bind(this, "statusIconValue"), toolTipField: "statusIconMessage" });
        this.invoiceGridOptions.addColumnShape("paidStatusColor", null, null, { shape: Constants.SHAPE_CIRCLE, toolTipField: "paidInfo", showIconField: "paidStatusColor" });

        this.addStandardMenuItems(this.contractGridOptions);
        this.addStandardMenuItems(this.orderGridOptions);
        this.addStandardMenuItems(this.offerGridOptions);
        this.addStandardMenuItems(this.invoiceGridOptions);

        this.contractGridOptions.finalizeInitGrid();
        this.orderGridOptions.finalizeInitGrid();
        this.offerGridOptions.finalizeInitGrid();
        this.invoiceGridOptions.finalizeInitGrid();

        this.$timeout(() => {
            if (this.contractPermission || this.contractUserPermission) {
                this.contractGridOptions.addTotalRow("#contract-totals-grid", {
                    filtered: this.terms["core.aggrid.totals.filtered"],
                    total: this.terms["core.aggrid.totals.total"]
                });
            }
            if (this.offerPermission || this.offerUserPermission) {
                this.offerGridOptions.addTotalRow("#offer-totals-grid", {
                    filtered: this.terms["core.aggrid.totals.filtered"],
                    total: this.terms["core.aggrid.totals.total"]
                });
            }
            if (this.orderPermission || this.orderUserPermission) {
                this.orderGridOptions.addTotalRow("#order-totals-grid", {
                    filtered: this.terms["core.aggrid.totals.filtered"],
                    total: this.terms["core.aggrid.totals.total"]
                });
            }
            if (this.invoicePermission || this.invoiceUserPermission) {
                this.invoiceGridOptions.addTotalRow("#invoice-totals-grid", {
                    filtered: this.terms["core.aggrid.totals.filtered"],
                    total: this.terms["core.aggrid.totals.total"]
                });
            }
        });

        this.contractGridOptions.getColumnDefs()
            .forEach(f => {
                // Append closedRow to cellClass
                var cellCls: string = f.cellClass ? f.cellClass.toString() : "";
                f.cellClass = (item: any) => {
                    return cellCls + (item.data.useClosedStyle ? " closedRow" : "");
                };
            });

        this.offerGridOptions.getColumnDefs()
            .forEach(f => {
                // Append closedRow to cellClass
                var cellCls: string = f.cellClass ? f.cellClass.toString() : "";
                f.cellClass = (item: any) => {
                    return cellCls + (item.data.useClosedStyle ? " closedRow" : "");
                };
            });

        this.orderGridOptions.getColumnDefs()
            .forEach(f => {
                // Append closedRow to cellClass
                var cellCls: string = f.cellClass ? f.cellClass.toString() : "";
                f.cellClass = (item: any) => {
                    return cellCls + (item.data.useClosedStyle ? " closedRow" : "");
                };
            });

        this.invoiceGridOptions.getColumnDefs()
            .forEach(f => {
                // Append closedRow to cellClass
                var cellCls: string = f.cellClass ? f.cellClass.toString() : "";
                f.cellClass = (item: any) => {
                    return cellCls + (item.data.useClosedStyle ? " closedRow" : "");
                };
            });

        // Setup detail grids
        this.contractGridOptions.enableMasterDetail(this.contractDetailsGridOptions);
        this.contractGridOptions.setDetailCellDataCallback((params) => {
            this.loadRows(params, SoeOriginType.Contract);
        });

        this.contractDetailsGridOptions.setSingelValueConfiguration([
            { field: "text", predicate: (data) => data.type === SoeInvoiceRowType.TextRow },
            { field: "text", predicate: (data) => data.type === SoeInvoiceRowType.PageBreakRow, editable: false, cellClass: "bold" },
            {
                field: "text",
                predicate: (data) => data.type === SoeInvoiceRowType.SubTotalRow,
                editable: true,
                cellClass: "bold",
                cellRenderer: (data, value) => {
                    const sum = data["sumAmountCurrency"] || "";
                    return "<span class='pull-left' style='width:150px'>" + value + "</span><span class='pull-right' style='padding-left:5px;padding-right:2px;margin-right:-2px;background-color:#FFFFFF;'>" + NumberUtility.printDecimal(sum, 2) + "</span>";
                },
                spanTo: "sumAmountCurrency"
            },
        ]);

        this.contractDetailsGridOptions.addColumnNumber("rowNr", this.terms["common.customer.invoices.row"], 100, { enableHiding: true, pinned: "left" });
        this.contractDetailsGridOptions.addColumnIcon("rowTypeIcon", null, null, { enableHiding: true, pinned: "left", editable: false });
        this.contractDetailsGridOptions.addColumnText("ediTextValue", this.terms["common.customer.invoices.edi"], null, { enableHiding: true });
        this.contractDetailsGridOptions.addColumnText("productNr", this.terms["common.customer.invoices.productnr"], null, { enableHiding: true });
        this.contractDetailsGridOptions.addColumnText("text", this.terms["common.customer.invoices.productname"], null, { enableHiding: true });
        this.contractDetailsGridOptions.addColumnNumber("quantity", this.terms["common.customer.invoices.quantity"], null, { enableHiding: true });
        this.contractDetailsGridOptions.addColumnText("productUnitCode", this.terms["common.customer.invoices.unit"], null, { enableHiding: true });
        if (this.productSalesPricePermission) {
            this.contractDetailsGridOptions.addColumnNumber("amountCurrency", this.terms["common.customer.invoices.price"], null, { enableHiding: true, decimals: 2 });
            this.contractDetailsGridOptions.addColumnNumber("discountValue", this.terms["common.customer.invoices.discount"], null, { enableHiding: true, decimals: 2 });
            this.contractDetailsGridOptions.addColumnText("discountTypeText", this.terms["common.customer.invoices.type"], null, { enableHiding: true });
            this.contractDetailsGridOptions.addColumnNumber("sumAmountCurrency", this.terms["common.customer.invoices.sum"], null, { enableHiding: true, decimals: 2 });
        }
        this.contractDetailsGridOptions.finalizeInitGrid();

        this.offerGridOptions.enableMasterDetail(this.offerDetailsGridOptions);
        this.offerGridOptions.setDetailCellDataCallback((params) => {
            this.loadRows(params, SoeOriginType.Offer);
        });

        this.offerDetailsGridOptions.setSingelValueConfiguration([
            { field: "text", predicate: (data) => data.type === SoeInvoiceRowType.TextRow },
            { field: "text", predicate: (data) => data.type === SoeInvoiceRowType.PageBreakRow, editable: false, cellClass: "bold" },
            {
                field: "text",
                predicate: (data) => data.type === SoeInvoiceRowType.SubTotalRow,
                editable: true,
                cellClass: "bold",
                cellRenderer: (data, value) => {
                    const sum = data["sumAmountCurrency"] || "";
                    return "<span class='pull-left' style='width:150px'>" + value + "</span><span class='pull-right' style='padding-left:5px;padding-right:2px;margin-right:-2px;background-color:#FFFFFF;'>" + NumberUtility.printDecimal(sum, 2) + "</span>";
                },
                spanTo: "sumAmountCurrency"
            },
        ]);

        this.offerDetailsGridOptions.addColumnNumber("rowNr", this.terms["common.customer.invoices.row"], 100, { enableHiding: true, pinned: "left" });
        this.offerDetailsGridOptions.addColumnIcon("rowTypeIcon", null, null, { enableHiding: true, pinned: "left", editable: false });
        this.offerDetailsGridOptions.addColumnText("ediTextValue", this.terms["common.customer.invoices.edi"], null, { enableHiding: true });
        this.offerDetailsGridOptions.addColumnText("productNr", this.terms["common.customer.invoices.productnr"], null, { enableHiding: true });
        this.offerDetailsGridOptions.addColumnText("text", this.terms["common.customer.invoices.productname"], null, { enableHiding: true });
        this.offerDetailsGridOptions.addColumnNumber("quantity", this.terms["common.customer.invoices.quantity"], null, { enableHiding: true });
        this.offerDetailsGridOptions.addColumnText("productUnitCode", this.terms["common.customer.invoices.unit"], null, { enableHiding: true });
        if (this.productSalesPricePermission) {
            this.offerDetailsGridOptions.addColumnNumber("amountCurrency", this.terms["common.customer.invoices.price"], null, { enableHiding: true, decimals: 2 });
            this.offerDetailsGridOptions.addColumnNumber("discountValue", this.terms["common.customer.invoices.discount"], null, { enableHiding: true, decimals: 2 });
            this.offerDetailsGridOptions.addColumnText("discountTypeText", this.terms["common.customer.invoices.type"], null, { enableHiding: true });
            this.offerDetailsGridOptions.addColumnNumber("sumAmountCurrency", this.terms["common.customer.invoices.sum"], null, { enableHiding: true, decimals: 2 });
        }
        this.offerDetailsGridOptions.addColumnShape("attestStateColor", null, null, { shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", showIconField: "attestStateColor" });
        this.offerDetailsGridOptions.finalizeInitGrid();

        this.orderGridOptions.enableMasterDetail(this.orderDetailsGridOptions);
        this.orderGridOptions.setDetailCellDataCallback((params) => {
            this.loadRows(params, SoeOriginType.Order);
        });

        this.orderDetailsGridOptions.setSingelValueConfiguration([
            { field: "text", predicate: (data) => data.type === SoeInvoiceRowType.TextRow },
            { field: "text", predicate: (data) => data.type === SoeInvoiceRowType.PageBreakRow, editable: false, cellClass: "bold" },
            {
                field: "text",
                predicate: (data) => data.type === SoeInvoiceRowType.SubTotalRow,
                editable: true,
                cellClass: "bold",
                cellRenderer: (data, value) => {
                    const sum = data["sumAmountCurrency"] || "";
                    return "<span class='pull-left' style='width:150px'>" + value + "</span><span class='pull-right' style='padding-left:5px;padding-right:2px;margin-right:-2px;background-color:#FFFFFF;'>" + NumberUtility.printDecimal(sum, 2) + "</span>";
                },
                spanTo: "sumAmountCurrency"
            },
        ]);

        this.orderDetailsGridOptions.addColumnNumber("rowNr", this.terms["common.customer.invoices.row"], 100, { enableHiding: true, pinned: "left" });
        this.orderDetailsGridOptions.addColumnIcon("rowTypeIcon", null, null, { enableHiding: true, pinned: "left", editable: false });
        this.orderDetailsGridOptions.addColumnText("ediTextValue", this.terms["common.customer.invoices.edi"], null, { enableHiding: true });
        this.orderDetailsGridOptions.addColumnText("productNr", this.terms["common.customer.invoices.productnr"], null, { enableHiding: true });
        this.orderDetailsGridOptions.addColumnText("text", this.terms["common.customer.invoices.productname"], null, { enableHiding: true });
        this.orderDetailsGridOptions.addColumnNumber("quantity", this.terms["common.customer.invoices.quantity"], null, { enableHiding: true });
        this.orderDetailsGridOptions.addColumnText("productUnitCode", this.terms["common.customer.invoices.unit"], null, { enableHiding: true });
        if (this.productSalesPricePermission) {
            this.orderDetailsGridOptions.addColumnNumber("amountCurrency", this.terms["common.customer.invoices.price"], null, { enableHiding: true, decimals: 2 });
            this.orderDetailsGridOptions.addColumnNumber("discountValue", this.terms["common.customer.invoices.discount"], null, { enableHiding: true, decimals: 2 });
            this.orderDetailsGridOptions.addColumnText("discountTypeText", this.terms["common.customer.invoices.type"], null, { enableHiding: true });
            this.orderDetailsGridOptions.addColumnNumber("sumAmountCurrency", this.terms["common.customer.invoices.sum"], null, { enableHiding: true, decimals: 2 });
        }
        this.orderDetailsGridOptions.addColumnShape("attestStateColor", null, 40, { shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", showIconField: "attestStateColor" });
        this.orderDetailsGridOptions.finalizeInitGrid();

        this.invoiceGridOptions.enableMasterDetail(this.invoiceDetailsGridOptions);
        this.invoiceGridOptions.setDetailCellDataCallback((params) => {
            this.loadRows(params, SoeOriginType.CustomerInvoice);
        });

        this.invoiceDetailsGridOptions.setSingelValueConfiguration([
            { field: "text", predicate: (data) => data.type === SoeInvoiceRowType.TextRow },
            { field: "text", predicate: (data) => data.type === SoeInvoiceRowType.PageBreakRow, editable: false, cellClass: "bold" },
            {
                field: "text",
                predicate: (data) => data.type === SoeInvoiceRowType.SubTotalRow,
                editable: true,
                cellClass: "bold",
                cellRenderer: (data, value) => {
                    const sum = data["sumAmountCurrency"] || "";
                    return "<span class='pull-left' style='width:150px'>" + value + "</span><span class='pull-right' style='padding-left:5px;padding-right:2px;margin-right:-2px;background-color:#FFFFFF;'>" + NumberUtility.printDecimal(sum, 2) + "</span>";
                },
                spanTo: "sumAmountCurrency"
            },
        ]);

        this.invoiceDetailsGridOptions.addColumnNumber("rowNr", this.terms["common.customer.invoices.row"], 100, { enableHiding: true, pinned: "left" });
        this.invoiceDetailsGridOptions.addColumnIcon("rowTypeIcon", null, null, { enableHiding: true, pinned: "left", editable: false });
        this.invoiceDetailsGridOptions.addColumnText("ediTextValue", this.terms["common.customer.invoices.edi"], null, { enableHiding: true });
        this.invoiceDetailsGridOptions.addColumnText("productNr", this.terms["common.customer.invoices.productnr"], null, { enableHiding: true });
        this.invoiceDetailsGridOptions.addColumnText("text", this.terms["common.customer.invoices.productname"], null, { enableHiding: true });
        this.invoiceDetailsGridOptions.addColumnNumber("quantity", this.terms["common.customer.invoices.quantity"], null, { enableHiding: true });
        this.invoiceDetailsGridOptions.addColumnText("productUnitCode", this.terms["common.customer.invoices.unit"], null, { enableHiding: true });
        if (this.productSalesPricePermission) {
            this.invoiceDetailsGridOptions.addColumnNumber("amountCurrency", this.terms["common.customer.invoices.price"], null, { enableHiding: true, decimals: 2 });
            this.invoiceDetailsGridOptions.addColumnNumber("discountValue", this.terms["common.customer.invoices.discount"], null, { enableHiding: true, decimals: 2 });
            this.invoiceDetailsGridOptions.addColumnText("discountTypeText", this.terms["common.customer.invoices.type"], null, { enableHiding: true });
            this.invoiceDetailsGridOptions.addColumnNumber("sumAmountCurrency", this.terms["common.customer.invoices.sum"], null, { enableHiding: true, decimals: 2 });
        }
        this.invoiceDetailsGridOptions.finalizeInitGrid();
    }

    private setBlockOrderString(blocked: boolean): string {
        return !blocked ? this.terms["core.no"] : !this.customer.blockNote ? this.terms["core.yes"] : `${this.terms["core.yes"]} (${this.customer.blockNote})`
    }

    public searchCustomer() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectCustomer", "selectcustomer.html"),
            controller: SelectCustomerController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                commonCustomerService: () => { return this.commonCustomerService }
            }
        });

        modal.result.then(item => {
            if (item) {
                if (!this.customer || this.customer.actorCustomerId != item.actorCustomerId) {
                    // Reset
                    this.contractsExpanderOpen = false;
                    this.contractsLoaded = false;
                    this.contracts = [];
                    this.offersExpanderOpen = false;
                    this.offersLoaded = false;
                    this.offers = [];
                    this.ordersExpanderOpen = false;
                    this.ordersLoaded = false;
                    this.orders = [];
                    this.invoicesExpanderOpen = false;
                    this.invoicesLoaded = false;
                    this.invoices = [];

                    this.isCustomerSelected = true;
                    this.actorCustomerId = item.actorCustomerId;
                    this.loadCompleteCustomer(item)
                    this.loadCustomerCountersAndBalances();
                }
            }
        });

        return modal;
    }

    public addStandardMenuItems(gridOptions: ISoeGridOptionsAg) {
        const gridMenuBuilder = new GridMenuBuilder(gridOptions, this.translationService, this.coreService, this.notificationService);
        gridMenuBuilder.buildDefaultMenu();
        gridOptions.restoreState((name) => this.coreService.getUserGridState(name), true);
    }

    public openCustomer() {
        if (!this.customer)
            return;

        this.messagingService.publish(Constants.EVENT_OPEN_EDITCUSTOMER, {
            id: this.customer.actorCustomerId,
            name: this.terms["common.customer"] + " " + this.customer.name
        });
    }

    private loadCustomerCountersAndBalances() {
        const counterTypes: number[] = [];
        const isUserInvoice = !this.invoicePermission && this.invoiceUserPermission;
        const isUserOrder = !this.orderPermission && this.orderUserPermission;
        const isUserOffer = !this.offerPermission && this.offerUserPermission;
        const isUserContract = !this.contractPermission && this.contractUserPermission;

        counterTypes.push(isUserInvoice ? SoeOriginStatusClassification.CustomerInvoicesOpenUser : SoeOriginStatusClassification.CustomerInvoicesOpen);
        if (this.invoiceForeignPermission) {
            counterTypes.push(isUserInvoice ? SoeOriginStatusClassification.CustomerInvoicesOpenUserForeign : SoeOriginStatusClassification.CustomerInvoicesOpenForeign);
        }
        counterTypes.push(isUserOrder ? SoeOriginStatusClassification.OrdersOpenUser : SoeOriginStatusClassification.OrdersOpen);
        if (this.orderForeignPermission) {
            counterTypes.push(isUserOrder ? SoeOriginStatusClassification.OrdersOpenUserForeign : SoeOriginStatusClassification.OrdersOpenForeign);
        }
        
        counterTypes.push(isUserOffer ? SoeOriginStatusClassification.OffersOpenUser : SoeOriginStatusClassification.OffersOpen);
        if (this.offerForeignPermission) {
            counterTypes.push(isUserOffer ? SoeOriginStatusClassification.OffersOpenUserForeign : SoeOriginStatusClassification.OffersOpenForeign);
        }
        counterTypes.push(isUserContract ? SoeOriginStatusClassification.ContractsOpenUser : SoeOriginStatusClassification.ContractsOpen);

        this.commonCustomerService.getCustomerCentralCountersAndBalance(counterTypes, this.actorCustomerId, null, null).then((items) => {
            for (let v of items) {
                switch (v.classification) {
                    case SoeOriginStatusClassification.ContractsOpenUser:
                    case SoeOriginStatusClassification.ContractsOpen:
                        this.contractsIncVat = v.balanceTotal;
                        this.contractsExVat = v.balanceExVat;
                        break;
                    case SoeOriginStatusClassification.OffersOpenUser:
                    case SoeOriginStatusClassification.OffersOpen:
                        this.offersIncVat = v.balanceTotal;
                        this.offersExVat = v.balanceExVat;
                        break;
                    case SoeOriginStatusClassification.OffersOpenUserForeign:
                    case SoeOriginStatusClassification.OffersOpenForeign:
                        this.offersCurrencyIncVat = v.balanceTotal;
                        this.offersCurrencyExVat = v.balanceExVat;
                        break;
                    case SoeOriginStatusClassification.OrdersOpen:
                    case SoeOriginStatusClassification.OrdersOpenUser:
                        this.ordersIncVat = v.balanceTotal;
                        this.ordersExVat = v.balanceExVat;
                        break;
                    case SoeOriginStatusClassification.OrdersOpenForeign:
                    case SoeOriginStatusClassification.OrdersOpenUserForeign:
                        this.ordersCurrencyIncVat = v.balanceTotal;
                        this.ordersCurrencyExVat = v.balanceExVat;
                        break;
                    case SoeOriginStatusClassification.CustomerInvoicesOpen:
                    case SoeOriginStatusClassification.CustomerInvoicesOpenUser:
                        this.invoicesIncVat = v.balanceTotal;
                        this.invoicesExVat = v.balanceExVat;
                        break;
                    case SoeOriginStatusClassification.CustomerInvoicesOpenForeign:
                    case SoeOriginStatusClassification.CustomerInvoicesOpenUserForeign:
                        this.invoicesCurrencyIncVat = v.balanceTotal;
                        this.invoicesCurrencyExVat = v.balanceExVat;
                        break;
                    case SoeOriginStatusClassification.CustomerPaymentsUnpayed:
                        this.unpaid = v.balanceTotal;
                        break;
                    case SoeOriginStatusClassification.CustomerPaymentsUnpayedForeign:
                        this.unpaidCurrency = v.balanceTotal;
                        break;
                }
            }

        }, error => {

        });

    }

    private loadContracts() {
        this.progress.startLoadingProgress([() => {
            // Load data
            // KOM IHÅG ATT FIXA HEADER OCH MINE
            var deferral = this.$q.defer();
            this.commonCustomerService.getCustomerInvoicesForCustomerCentral(SoeOriginStatusClassification.ContractsAll, SoeOriginType.Contract, this.customer.actorCustomerId, false).then((x) => {
                this.contracts = x;
                // Post process
                this.postProcessRows(this.contracts);

                this.filteredContracts = _.filter(this.contracts, c => !c.useClosedStyle);
                this.contractGridOptions.setData(this.filteredContracts);
                this.contractsLoaded = true;
                this.contractGridOptions.refreshRows();
                deferral.resolve();
            });
            return deferral.promise;
        }]);
    }

    private loadOffers() {
        this.progress.startLoadingProgress([() => {
            // Load data
            // KOM IHÅG ATT FIXA HEADER OCH MINE
            var deferral = this.$q.defer();
            this.commonCustomerService.getCustomerInvoicesForCustomerCentral(SoeOriginStatusClassification.OffersAll, SoeOriginType.Offer, this.customer.actorCustomerId, false).then((x) => {
                this.offers = x;

                // Post process
                this.postProcessRows(this.offers);

                this.filteredOffers = _.filter(this.offers, c => !c.useClosedStyle);
                this.offerGridOptions.setData(this.filteredOffers);
                this.offersLoaded = true;
                this.offerGridOptions.refreshRows();
                deferral.resolve();
            });
            return deferral.promise;
        }]);
    }

    private loadRows(params: any, originType: SoeOriginType) {
        if (!params.data['rowsLoaded']) {
            this.coreService.getCustomerInvoiceRowsSmall(params.data.customerInvoiceId).then((x) => {
                var rows = [];
                _.forEach(_.filter(x, r => (r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.TextRow || r.type === SoeInvoiceRowType.PageBreakRow || r.type === SoeInvoiceRowType.SubTotalRow)), (y) => {
                    y.ediTextValue = y.ediEntryId ? this.terms["core.yes"] : this.terms["core.no"];
                    y['discountTypeText'] = y.discountType === SoeInvoiceRowDiscountType.Percent ? "%" : params.data.currencyCode;
                    if (y.attestStateId != null) {
                        if (originType === SoeOriginType.Offer) {
                            var attestStateOffer = _.find(this.attestStatesFullOffer, { attestStateId: y.attestStateId });
                            if (attestStateOffer) {
                                y.attestStateName = attestStateOffer.name;
                                y.attestStateColor = attestStateOffer.color;
                            }
                        }
                        else if (originType === SoeOriginType.Order) {
                            var attestStateOrder = _.find(this.attestStatesFullOrder, { attestStateId: y.attestStateId });
                            if (attestStateOrder) {
                                y.attestStateName = attestStateOrder.name;
                                y.attestStateColor = attestStateOrder.color;
                            }
                        }
                    }

                    if (y.isTimeProjectRow) {
                        y['rowTypeIcon'] = 'fal fa-clock';
                    }
                    else {
                        switch (y.type) {
                            case SoeInvoiceRowType.ProductRow:
                                y['rowTypeIcon'] = 'fal fa-box-alt';
                                break;
                            case SoeInvoiceRowType.TextRow:
                                y['rowTypeIcon'] = 'fal fa-text';
                                break;
                            case SoeInvoiceRowType.PageBreakRow:
                                y['rowTypeIcon'] = 'fal fa-cut';
                                break;
                            case SoeInvoiceRowType.SubTotalRow:
                                y['rowTypeIcon'] = 'fal fa-calculator-alt';
                                break;
                        }
                    }

                    rows.push(y);
                });

                params.data['rows'] = rows;
                params.data['rowsLoaded'] = true;
                params.successCallback(params.data['rows']);
            });
        }
        else {
            params.successCallback(params.data['rows']);
        }
    }

    private loadOrders() {
        this.progress.startLoadingProgress([() => {
            // Load data
            // KOM IHÅG ATT FIXA HEADER OCH MINE
            var deferral = this.$q.defer();
            this.commonCustomerService.getCustomerInvoicesForCustomerCentral(SoeOriginStatusClassification.OrdersAll, SoeOriginType.Order, this.customer.actorCustomerId, false).then((x) => {
                this.orders = x;
                // Post process
                this.postProcessRows(this.orders);

                this.filteredOrders = _.filter(this.orders, c => !c.useClosedStyle);
                this.orderGridOptions.setData(this.filteredOrders);
                this.ordersLoaded = true;
                this.orderGridOptions.refreshRows();
                deferral.resolve();
            });
            return deferral.promise;
        }]);
    }

    private loadInvoices() {
        this.progress.startLoadingProgress([() => {
            // Load data
            // KOM IHÅG ATT FIXA HEADER OCH MINE
            var deferral = this.$q.defer();
            this.commonCustomerService.getCustomerInvoicesForCustomerCentral(SoeOriginStatusClassification.CustomerInvoicesAll, SoeOriginType.CustomerInvoice, this.customer.actorCustomerId, false).then((x) => {
                this.invoices = x;

                // Post process
                this.postProcessRows(this.invoices);
                this.filteredInvoices = _.filter(this.invoices, c => !c.useClosedStyle);
                this.invoiceGridOptions.setData(this.filteredInvoices);
                this.invoicesLoaded = true;
                this.invoiceGridOptions.refreshRows();
                deferral.resolve();
            });
            return deferral.promise;
        }]);
    }

    public postProcessRows(items: CustomerInvoiceGridDTO[]) {
        _.forEach(items, (invoice: CustomerInvoiceGridDTO) => {
            // Convert to dates
            invoice.payDate = CalendarUtility.convertToDate(invoice.payDate);
            invoice.invoiceDate = CalendarUtility.convertToDate(invoice.invoiceDate);
            invoice.dueDate = CalendarUtility.convertToDate(invoice.dueDate);

            invoice.expandableDataIsLoaded = false;

            if (invoice.paidAmount === 0)
                invoice.payDate = null;

            if (invoice.exportStatus) {
                invoice.exportStatus = 1;
                invoice.exportStatusName = this.terms["common.customer.invoices.export"];
            }

            if (invoice.orderType) {
                var orderType = _.find(this.orderTypes, o => o.id === invoice.orderType);
                if (orderType)
                    invoice.orderTypeName = orderType.name;
            }
            else {
                invoice.orderTypeName = this.terms["common.customer.invoices.notspecified"];
            }

            if (!invoice.attestStates || invoice.attestStates.length === 0) {
                invoice.useGradient = false;
                invoice.attestStateNames = this.terms["common.customer.invoice.norows"];
            }
            else if (invoice.attestStates.length === 1) {
                invoice.useGradient = false;
                invoice.attestStateColor = invoice.attestStates[0].color;
            }
            else {
                invoice.useGradient = true;
                invoice.attestStateColor = undefined;
            }

            if (invoice.fullyPaid) {
                invoice['paidInfo'] = this.terms["common.customer.invoices.invoicepaid"];
                invoice['paidStatusColor'] = "#98EF5D";
            }
            else {
                if (invoice.paidAmount > 0) {
                    invoice['paidInfo'] = this.terms["common.customer.invoices.invoicepartlypaid"];
                    invoice['paidStatusColor'] = "#EAF055";
                }
                else {
                    invoice['paidInfo'] = this.terms["common.customer.invoices.invoiceunpaid"];
                    invoice['paidStatusColor'] = "#ED8D6C";
                }
            }

            this.setInformationIconAndTooltip(invoice);

            if (invoice.useClosedStyle) {
                invoice.showCreatePayment = !invoice.useClosedStyle;
                invoice.payAmount = invoice.payAmountCurrency = 0;
            }
            else {
                invoice.showCreatePayment = true;
            }
        });
    }

    public setInformationIconAndTooltip(item: CustomerInvoiceGridDTO) {
        var hasInfo: boolean = ((item.infoIcon & Number(InvoiceRowInfoFlag.Info)) == Number(InvoiceRowInfoFlag.Info));
        var hasError: boolean = ((item.infoIcon & Number(InvoiceRowInfoFlag.Error)) == Number(InvoiceRowInfoFlag.Error));
        var hasHousehold: boolean = ((item.infoIcon & Number(InvoiceRowInfoFlag.HouseHold)) == Number(InvoiceRowInfoFlag.HouseHold));

        // Get status icons
        var flaggedEnum: FlaggedEnum.IFlaggedEnum = FlaggedEnum.create(SoeStatusIcon, SoeStatusIcon.ElectronicallyDistributed);
        var statusIcons: FlaggedEnum.IFlaggedEnum = new flaggedEnum(item.statusIcon);

        //Printing - distribution
        if (item.billingInvoicePrinted) {
            item.billingIconValue = "fal fa-print";
            item.billingIconMessage = this.terms["common.customer.invoices.printed"];
        }

        if (statusIcons.contains(SoeStatusIcon.ElectronicallyDistributed)) {
            item.billingIconValue = "fal fa-paper-plane";
            item.billingIconMessage = this.terms["common.customer.invoices.einvoiced"];
        }
        else if (statusIcons.contains(SoeStatusIcon.Email)) {
            item.billingIconValue = "fal fa-envelope";
            item.billingIconMessage = this.terms["common.customer.invoices.emailsent"];
        }
        else if (statusIcons.contains(SoeStatusIcon.EmailError)) {
            item.billingIconValue = "fal fa-envelope errorColor";
            item.billingIconMessage = this.terms["common.customer.invoices.sendemailfailed"];
        }

        if (hasError || hasInfo || hasHousehold || (item.statusIcon != SoeStatusIcon.None)) {
            if (statusIcons.contains(SoeStatusIcon.Imported)) {
                item.statusIconValue = "fal fa-download";
            } else if (hasError) {
                item.statusIconValue = "fal fa-exclamation-triangle errorColor";
                item.statusIconMessage = this.terms["core.showinfo"];
            } else if (hasInfo && hasHousehold) {
                item.statusIconValue = "fal fa-home";
                item.statusIconMessage = this.terms["core.showinfo"] + " - " + this.terms["common.customer.invoices.hashousededuction"];
            } else if (hasInfo && !hasHousehold) {
                item.statusIconValue = "fal fa-info-circle infoColor";
                item.statusIconMessage = this.terms["core.showinfo"];
            } else if (!hasInfo && hasHousehold) {
                item.statusIconValue = "fal fa-home";
                item.statusIconMessage = this.terms["common.customer.invoices.hashousededuction"];
            }
            else if (item.statusIcon != SoeStatusIcon.None) {
                if (!statusIcons.contains(SoeStatusIcon.Email) && !statusIcons.contains(SoeStatusIcon.EmailError) && !statusIcons.contains(SoeStatusIcon.ElectronicallyDistributed)) {
                    item.statusIconValue = "fal fa-paperclip";

                    if (statusIcons.contains(SoeStatusIcon.Imported))
                        item.statusIconMessage = item.statusIconMessage && item.statusIconMessage != "" ? "<br/>" + this.terms["common.imported"] : this.terms["common.imported"];
                    if (statusIcons.contains(SoeStatusIcon.Attachment))
                        item.statusIconMessage = item.statusIconMessage && item.statusIconMessage != "" ? "<br/>" + this.terms["common.hasaattachedfiles"] : this.terms["common.hasaattachedfiles"];
                    if (statusIcons.contains(SoeStatusIcon.Image))
                        item.statusIconMessage = item.statusIconMessage && item.statusIconMessage != "" ? "<br/>" + this.terms["common.hasattachedimages"] : this.terms["common.hasattachedimages"];
                    if (statusIcons.contains(SoeStatusIcon.Checklist))
                        item.statusIconMessage = item.statusIconMessage && item.statusIconMessage != "" ? "<br/>" + this.terms["common.customer.invoices.haschecklists"] : this.terms["common.customer.invoices.haschecklists"];
                }
            }
        }
    }

    // Sums
    private summarizeFilteredContracts(x) {

    }

    private summarizeSelectedContracts() {

    }


    private summarizeFilteredOffers(x) {
        this.$scope.$applyAsync(() => {
            this.offerFilteredTotal = 0;
            _.forEach(x, (o: any) => {
                this.offerFilteredTotal += o.totalAmount;
            });
        });
    }

    private summarizeSelectedOffers() {
        this.$scope.$applyAsync(() => {
            this.offerSelectedTotal = 0;
            var rows = this.offerGridOptions.getSelectedRows();
            _.forEach(rows, (y: any) => {
                this.offerSelectedTotal += y.totalAmount;
            });
        });
    }

    private summarizeFilteredOrders(x) {
        this.$scope.$applyAsync(() => {
            this.orderFilteredTotal = 0;
            this.orderFilteredToBeInvoicedTotal = 0;
            _.forEach(x, (o: any) => {
                this.orderFilteredTotal += o.totalAmount;
                this.orderFilteredToBeInvoicedTotal += o.remainingAmount;
            });
        });
    }

    private summarizeSelectedOrders() {
        this.$scope.$applyAsync(() => {
            this.orderSelectedTotal = 0;
            this.orderSelectedToBeInvoicedTotal = 0;
            var rows = this.orderGridOptions.getSelectedRows();
            _.forEach(rows, (y: any) => {
                this.orderSelectedTotal += y.totalAmount;
                this.orderSelectedToBeInvoicedTotal += y.remainingAmount;
            });
        });
    }

    private summarizeFilteredInvoices(x) {
        this.$scope.$applyAsync(() => {
            this.invoiceFilteredTotal = 0;
            this.invoiceFilteredToPay = 0;
            _.forEach(x, (o: any) => {
                this.invoiceFilteredTotal += o.totalAmount;
                this.invoiceFilteredToPay += o.payAmount ? o.payAmount : 0;
            });
        });
    }

    private summarizeSelectedInvoices() {
        this.$scope.$applyAsync(() => {
            this.invoiceSelectedTotal = 0;
            this.invoiceSelectedToPay = 0;
            var rows = this.invoiceGridOptions.getSelectedRows();
            _.forEach(rows, (y: any) => {
                this.invoiceSelectedTotal += y.totalAmount;
                this.invoiceSelectedToPay += y.payAmount ? y.payAmount : 0;
            });
        });
    }

    //Events
    private contractExpanderOpened() {
        if (!this.contractsLoaded) {
            this.loadContracts();
        }
    }

    private offerExpanderOpened() {
        if (!this.offersLoaded) {
            this.loadOffers();
        }
    }

    private orderExpanderOpened() {
        if (!this.ordersLoaded) {
            this.loadOrders();
        }
    }

    private invoiceExpanderOpened() {
        if (!this.invoicesLoaded) {
            this.loadInvoices();
        }
    }

    private openContract(row: any) {
        if (row)
            this.messagingService.publish(Constants.EVENT_OPEN_CONTRACT, { customerInvoiceId: row.customerInvoiceId, invoiceNr: row.invoiceNr });
        else
            this.messagingService.publish(Constants.EVENT_NEW_CONTRACT, null);

    }

    private openOffer(row: any) {
        if (row)
            this.messagingService.publish(Constants.EVENT_OPEN_OFFER, { customerInvoiceId: row.customerInvoiceId, invoiceNr: row.invoiceNr });
        else
            this.messagingService.publish(Constants.EVENT_NEW_OFFER, null);
    }

    private openOrder(row: any) {
        if (row)
            this.messagingService.publish(Constants.EVENT_OPEN_ORDER, { customerInvoiceId: row.customerInvoiceId, invoiceNr: row.invoiceNr });
        else
            this.messagingService.publish(Constants.EVENT_NEW_ORDER, null);
    }

    private openInvoice(row: any) {
        if (row)
            this.messagingService.publish(Constants.EVENT_OPEN_CUSTOMERINVOICE, { customerInvoiceId: row.customerInvoiceId, invoiceNr: row.invoiceNr, registrationType: row.registrationType });
        else
            this.messagingService.publish(Constants.EVENT_NEW_CUSTOMERINVOICE, null);
    }

    private showInformationMessage(row: any) {
        var message: string = "";
        if (!row.fullyPaid) {
            var isTotalAmountPaid: boolean = false;
            var partlyPaid: boolean = false;
            var partlyPaidForeign: boolean = false;
            if (row.paidAmount >= 0) {
                isTotalAmountPaid = row.paidAmount != 0 && row.paidAmount >= row.totalAmount;
                partlyPaid = row.paidAmount != 0 && row.paidAmount < row.totalAmount;
                partlyPaidForeign = row.paidAmountCurrency != 0 && row.paidAmountCurrency < row.totalAmountCurrency;
            }
            else {
                isTotalAmountPaid = row.paidAmount != 0 && row.paidAmount <= row.totalAmount;
                partlyPaid = row.paidAmount != 0 && row.paidAmount > row.totalAmount;
                partlyPaidForeign = row.paidAmountCurrency != 0 && row.paidAmountCurrency > row.totalAmountCurrency;
            }

            if (isTotalAmountPaid) {
                message = message + this.terms["economy.supplier.invoice.paidlate"] + "<br/>";
                message = message + this.terms["economy.supplier.invoice.matches.totalamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency.toString() : row.totalAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.payment.paymentamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.paidAmountCurrency.toString() : row.paidAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.invoice.amounttopay"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency.toString() : row.payAmount.toString()) + "<br/>";
            }
            else if (partlyPaid || partlyPaidForeign) {
                message = message + this.terms["economy.supplier.invoice.partlypaid"] + "<br/>";
                message = message + this.terms["economy.supplier.invoice.matches.totalamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency.toString() : row.totalAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.payment.paymentamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.paidAmountCurrency.toString() : row.paidAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.invoice.amounttopay"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency.toString() : row.payAmount.toString()) + "<br/>";
            }
        }

        if (row.multipleAssetRows) {
            if (message != "")
                message = message + "---<br/>";

            message = message + this.terms["common.customer.invoice.multipleassetrows"] + "<br/>";
            message = message + this.terms["economy.supplier.invoice.manualadjustmentneeded"] + "<br/>";
        }

        if (row.insecureDebt) {
            if (message != "")
                message = message + "---<br/>";

            message += this.terms["common.customer.invoice.stopped"] + "<br/>";
        }

        if (((row.infoIcon & Number(InvoiceRowInfoFlag.Error)) == Number(InvoiceRowInfoFlag.Error)) && row.vatRate > 0) {

            message = message + this.terms["common.customer.invoices.checkvatamount"].format(row.vatRate.toString()) + "<br/>";
        }

        if (message !== "")
            this.notificationService.showDialog(this.terms["core.information"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
    }

    private isPropertyNull(row: any, property: string): boolean {
        return !row[property];
    }

    private getPropertyValue(row: any, property: string): string {
        return row[property] ? row[property] : "";
    }

    terms: { [index: string]: string; };
}