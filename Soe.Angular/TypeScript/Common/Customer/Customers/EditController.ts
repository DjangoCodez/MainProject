import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2"
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ICommonCustomerService } from "../CommonCustomerService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { Guid } from "../../../Util/StringUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { CustomerDTO, CustomerUserDTO } from "../../Models/CustomerDTO";
import { SmallGenericType } from "../../Models/smallgenerictype";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons, IconLibrary } from "../../../Util/Enumerations";
import { ContactAddressItemDTO } from "../../Models/ContactAddressDTOs";
import { HouseholdTaxDeductionApplicantDTO } from "../../Models/householdtaxdeductionapplicantdto";
import { IUserSmallDTO, IHouseholdTaxDeductionApplicantDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { SelectUsersController } from "../../Dialogs/SelectUsers/SelectUsersController";
import { HouseholdTaxDeductionController } from "../../Dialogs/HouseholdTaxDeduction/HouseholdTaxDeductionController";
import { Feature, TermGroup, SoeOriginType, CompanySettingType, CustomerAccountType, SoeReportTemplateType, SoeInvoiceDeliveryType, SoeEntityState, ActionResultSave, SoeEntityType, SoeEntityImageType, TermGroup_Languages, TermGroup_EInvoiceFormat } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { ExportUtility } from "../../../Util/ExportUtility";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { FilesHelper } from "../../Files/FilesHelper";
import { ExtraFieldGridDTO, ExtraFieldRecordDTO } from "../../Models/ExtraFieldDTO";
import { CoreUtility } from "../../../Util/CoreUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Modal
    modal: any;

    // Data
    customer: CustomerDTO;

    // Lookups 
    customers: SmallGenericType[] = [];
    countries: SmallGenericType[] = [];
    languages: SmallGenericType[] = [];
    currencies: SmallGenericType[] = [];
    vatTypes: SmallGenericType[] = [];
    paymentConditions: SmallGenericType[] = [];
    deliveryTypes: SmallGenericType[] = [];
    deliveryConditions: SmallGenericType[] = [];
    priceLists: SmallGenericType[] = [];
    wholesellers: SmallGenericType[] = [];
    agreementTemplates: SmallGenericType[] = [];
    offerTemplates: SmallGenericType[] = [];
    orderTemplates: SmallGenericType[] = [];
    billingTemplates: SmallGenericType[] = [];
    emails: SmallGenericType[] = [];
    invoiceDeliveryTypes: SmallGenericType[] = [];
    invoicePaymentServices: SmallGenericType[] = [];
    invoiceDeliveryProviders: ISmallGenericType[];
    originTypes: any[];
    defaultFilterOriginType: string;
    allItemsSelectionDict: any[];
    customerGLNs: SmallGenericType[] = [];
    eInvoiceFormat: number;
    private extraFieldRecords: ExtraFieldRecordDTO[];

    // Permissions
    modifyUsersPermission = false;
    finvoicePermission = false;
    eInvoicePermission = false;
    archivePermission = false;
    private modifyHHTDApplicantsPermission = false; // Edit Household Tax Deduction Applicants
    hasExtraFieldPermission = false;
    useInvoiceDeliveryProvider = false;
    hideTaxDeductionContacts = false;

    // CompanySettings
    defaultGracePeriodDays = 0;
    setOwnerAutomatically = false;
    useDeliveryCustomer = false;
    isAdditionalDiscount = false;

    settingTypes: SmallGenericType[];
    baseAccounts: SmallGenericType[];

    // Sub grids
    private rotGridOptions: ISoeGridOptionsAg;
    private rotGridButtonGroups = new Array<ToolBarButtonGroup>();
    private householdTaxApplicants: HouseholdTaxDeductionApplicantDTO[] = [];

    private statisticsGrid: EmbeddedGridController;
    private statisticsGridButtonGroups = new Array<ToolBarButtonGroup>();
    
    customerStatisticsGridFooterComponentUrl: any;
    filteredTotal = 0;
    selectedTotal = 0;


    // Files
    private filesHelper: FilesHelper;
    private documentExpanderIsOpen: boolean;

    //expander visiblity settings
    private productsExpanderRendered = false;
    private rotExpanderRendered = false;
    private statisticsExpanderRendered = false;
    private accountExpanderRendered = false;
    private contactExpanderRendered = false;

    // Properties
    private customerId: number;
    private _selectedPayingCustomer: any;
    get selectedPayingCustomer() {
        return this._selectedPayingCustomer;
    }
    set selectedPayingCustomer(item: any) {
        this._selectedPayingCustomer = item;
        if (this.customer)
            this.customer.payingCustomerId = item ? item.id : 0;
    }

    private _showAllApplicants: boolean = false;
    get showAllApplicants() {
        return this._showAllApplicants;
    }
    set showAllApplicants(item: boolean) {
        this._showAllApplicants = item;
        this.loadRotdata();
    }

    private _allItemsSelection: any;

    get allItemsSelection() {
        return this._allItemsSelection;
    }

    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
    }

    set hasConsent(value: any) {
        this.customer.hasConsent = value;
        if (this.customer.hasConsent && !this.customer.consentDate) {
            this.customer.consentDate = CalendarUtility.getDateToday();
        }
    }

    get hasConsent() {
        return this.customer.hasConsent;
    }

    private edit: ng.IFormController;
    private parameters: any;
    private isModal: any;

    // Flags
    private showNavigationButtons = true;
    private customerIds: number[];
    private permissionsLoaded = false;

    private isContactAddressesValid = true;
    private contactAddressesValidationErrors: string;
    private consentToolTip: string;
    extraFieldsExpanderRendered = false;

    // Extra fields
    private extraFields: ExtraFieldGridDTO[] = [];
    get showExtraFieldsExpander() {
        return this.hasExtraFieldPermission;
    }

    //@ngInject
    constructor(
        private $window,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private reportService: IReportService,
        private commonCustomerService: ICommonCustomerService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private focusService: IFocusService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.customerStatisticsGridFooterComponentUrl = urlHelperService.getGlobalUrl("Common/Customer/Customers/Views/customerStatisticsGridFooter.html");
        
        this.allItemsSelection = 1; //default                         

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.loadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        //Event from pages where controller is opened as dialog.
        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.onInit(parameters);
            this.modal = parameters.modal;
            this.focusService.focusByName(parameters.id ? "ctrl_customer_name" : "ctrl_customer_customerNr");
        });
    }

    // #region Setup
    public onInit(parameters: any) {
        this.customerId = parameters.id || 0;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.focusService.focusByName(this.customerId ? "ctrl_customer_name" : "ctrl_customer_customerNr");

        if (parameters.ids && parameters.ids.length > 0) {
            this.customerIds = parameters.ids;
        } else {
            this.showNavigationButtons = false;
        }

        this.flowHandler.start([
            { feature: Feature.Billing_Customer_Customers_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Customer_Customers_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Customer_Customers_Edit_Users, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Customer_Customers_Edit_Users, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Customer_Customers_Edit_Documents, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Customer_Customers_Edit_Documents, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Customer_Customers_Edit_HouseholdTaxDeductionApplicants, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_EInvoice, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Archive, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Common_ExtraFields_Customer, loadModifyPermissions: true }
        ]);
        
        this.filesHelper = new FilesHelper(this.coreService, this.$q, this.dirtyHandler, true, SoeEntityType.Customer, SoeEntityImageType.Customer, () => this.customerId);
    }

    public onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.modifyPermission = response[Feature.Billing_Customer_Customers_Edit].modifyPermission || response[Feature.Economy_Customer_Customers_Edit].modifyPermission;
        this.modifyUsersPermission = response[Feature.Billing_Customer_Customers_Edit_Users].modifyPermission || response[Feature.Economy_Customer_Customers_Edit_Users].modifyPermission;
        this.modifyHHTDApplicantsPermission = response[Feature.Billing_Customer_Customers_Edit_HouseholdTaxDeductionApplicants].modifyPermission;
        this.finvoicePermission = response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice].modifyPermission;
        this.eInvoicePermission = response[Feature.Billing_Invoice_Invoices_Edit_EInvoice].modifyPermission || response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice].modifyPermission || response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura].modifyPermission;
        this.archivePermission = response[Feature.Archive].modifyPermission;
        this.documentsPermission = response[Feature.Economy_Customer_Customers_Edit_Documents].modifyPermission || response[Feature.Billing_Customer_Customers_Edit_Documents].modifyPermission;
        this.hasExtraFieldPermission = response[Feature.Common_ExtraFields_Customer].modifyPermission;

        this.permissionsLoaded = true;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy());
        
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.download", "common.download", IconLibrary.FontAwesome, "fal fa-download", () => {
            this.exportCustomer();
        }, null, () => {
            return (!this.customerId || this.customerId === 0)
        })));

        if (CoreUtility.sysCountryId == TermGroup_Languages.Finnish) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("YTJ", "common.external.company.search.web.tooltip", IconLibrary.FontAwesome, "fal fa-globe-europe", () => {
                const url = this.customer.orgNr ?
                    `https://tietopalvelu.ytj.fi/yritys/${this.customer.orgNr}` :
                    `https://tietopalvelu.ytj.fi/?isCompanyValid=true&isCompanyTerminated=true&companyName=${this.customer.name}`;
                HtmlUtility.openInNewTab(this.$window, url);
            })));
        }

        if (this.showNavigationButtons) {
            this.toolbar.setupNavigationGroup(null, () => { return this.isNew }, (customerId) => {
                this.customerId = customerId;
                this.loadData();
            }, this.customerIds, this.customerId);
        }
    }

    public onDoLookups(): ng.IPromise<any> {
        //this.customerId = this.parameters.id || 0;
        return this.$q.all([
            this.loadOriginTypes(),
            this.loadSelectionTypes(),
            this.loadCompanySettings()]).then(() => {
                return this.$q.all([
                    this.loadSettingTypes(),
                    this.loadCustomers(),
                    this.loadCountries(),
                    this.loadLanguages(),
                    this.loadCurrencies(),
                    this.loadVatTypes(),
                    this.loadPaymentConditions(),
                    this.loadDeliveryTypes(),
                    this.loadDeliveryConditions(),
                    this.loadPriceLists(),
                    this.loadWholesellers(),
                    this.loadAgreementTemplates(),
                    this.loadOfferTemplates(),
                    this.loadOrderTemplates(),
                    this.loadBillingTemplates(),
                    this.loadInvoiceDeliveryTypes(),
                    this.loadInvoiceDeliveryProviders(),
                    this.loadInvoicePaymentServices()
                    ])
            });
    }

    private setupWatches() {
        this.$scope.$watch(() => this.customer.contactAddresses, (newValue, oldValue) => {
            if ((newValue && oldValue)) {
                this.loadEmails();
                this.loadCustomerGLNs();
            }
        });
    }
    // #endregion

    // #region expanderROT
    private openRotExpander() {
        if (!this.rotExpanderRendered) {
            this.rotExpanderRendered = true;
            this.setupRotExpander();
        }
    }
    private setupRotExpander() {
        this.rotGridOptions = new SoeGridOptionsAg("Common.Customer.Customers.Edit.Rot", this.$timeout);
        this.rotGridOptions.enableGridMenu = false;
        //this.rotGridOptions.showGridFooter = false;
        this.rotGridOptions.setMinRowsToShow(5);

        const keys: string[] = [
            "common.customer.customer.rot.socialsecnr",
            "common.customer.customer.rot.name",
            "common.customer.customer.rot.property",
            "common.customer.customer.rot.apartmentnr",
            "common.customer.customer.rot.cooperativeorgnr",
            "core.edit",
            "core.delete",
            "common.type",
            "common.invoicenr",
            "common.productnr",
            "common.name",
            "common.quantity",
            "common.amount",
            "common.purchaseprice",
            "common.price",
            "common.date",
            "common.customer.customer.marginalincome",
            "common.customer.customer.marginalincomeratioprocent"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.rotGridOptions.addColumnText("socialSecNr", terms["common.customer.customer.rot.socialsecnr"], null);
            this.rotGridOptions.addColumnText("name", terms["common.customer.customer.rot.name"], null);
            this.rotGridOptions.addColumnText("property", terms["common.customer.customer.rot.property"], null);
            this.rotGridOptions.addColumnText("apartmentNr", terms["common.customer.customer.rot.apartmentnr"], null);
            this.rotGridOptions.addColumnText("cooperativeOrgNr", terms["common.customer.customer.rot.cooperativeorgnr"], null);
            if (this.modifyPermission && this.modifyHHTDApplicantsPermission) {
                this.rotGridOptions.addColumnEdit(terms["core.edit"], this.editRot.bind(this), false, 'showButton');
                this.rotGridOptions.addColumnDelete(terms["core.delete"], this.deleteRot.bind(this), false, 'showButton');
            }
            this.rotGridOptions.finalizeInitGrid();

            this.$timeout(() => {
                this.rotGridOptions.setData(this.householdTaxApplicants);
            });
        });
    }

    private loadRotdata(setData = true) {
        this.coreService.getHouseholdTaxDeductionRowsByCustomer(this.customerId, false, this.showAllApplicants).then((x) => {
            this.householdTaxApplicants = x; 
            if(setData)
                this.rotGridOptions.setData(this.householdTaxApplicants);
        });
    }
    // #endregion

    // #region expanderStatistics
    private openStatisticsExpander() {
        if (!this.statisticsExpanderRendered) {
            this.statisticsExpanderRendered = true;
            this.setupStatisticsGrid();
        }
    }

    private setupStatisticsGrid() {
        this.statisticsGridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("core.search", "core.search", IconLibrary.FontAwesome, "fal fa-file-search", () => {
            this.loadCustomerStatistics();
        })));

        this.statisticsGrid = new EmbeddedGridController(this.gridHandlerFactory, "statisticsGrid");
        this.statisticsGrid.gridAg.options.setMinRowsToShow(8);

        this.statisticsGrid.gridAg.options.subscribe([new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows: any[]) => {
            this.selectedTotal = 0;
            rows.forEach(row => this.selectedTotal += row ? row.productSumAmount : 0);
            this.$scope.$applyAsync();
        }), new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: any[]) => {
            this.filteredTotal = 0;
            rows.forEach(row => this.filteredTotal += row ? row.productSumAmount : 0);
            this.$scope.$applyAsync();
        })]);

        const keys: string[] = [
            "common.date",
            "common.type",
            "common.invoicenr",
            "common.productnr",
            "common.name",
            "common.quantity",
            "common.price",
            "common.amount",
            "common.purchaseprice",
            "common.customer.customer.marginalincome",
            "common.customer.customer.marginalincomeratioprocent",
            "common.statistics"
        ];
        
        this.translationService.translateMany(keys).then((terms) => {
            this.statisticsGrid.gridAg.addColumnDate("date", terms["common.date"], null, false, null, {enableHiding:false, enableRowGrouping: true });
            this.statisticsGrid.gridAg.addColumnText("originType", terms["common.type"], null, false, { enableHiding: false, enableRowGrouping: true });
            this.statisticsGrid.gridAg.addColumnText("invoiceNr", terms["common.invoicenr"], null, null, { enableHiding: false, enableRowGrouping: true });
            this.statisticsGrid.gridAg.addColumnText("productNr", terms["common.productnr"], null, null, { enableRowGrouping: true });
            this.statisticsGrid.gridAg.addColumnText("productName", terms["common.name"], null, null, { enableRowGrouping: true });
            this.statisticsGrid.gridAg.addColumnNumber("productQuantity", terms["common.quantity"], null, { enableRowGrouping: true, aggFuncOnGrouping: "sum" });
            this.statisticsGrid.gridAg.addColumnNumber("productPurchasePrice", terms["common.purchaseprice"], null, { enableHiding: true, decimals: 2 });
            this.statisticsGrid.gridAg.addColumnNumber("productPrice", terms["common.price"], null, { enableHiding: true, decimals: 2, enableRowGrouping: true });
            this.statisticsGrid.gridAg.addColumnNumber("productSumAmount", terms["common.amount"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping:"sum" });
            this.statisticsGrid.gridAg.addColumnNumber("productMarginalIncome", terms["common.customer.customer.marginalincome"], null, { enableRowGrouping: true, enableHiding: true, decimals: 2 });
            this.statisticsGrid.gridAg.addColumnNumber("productMarginalRatio", terms["common.customer.customer.marginalincomeratioprocent"], null, { enableRowGrouping: true, enableHiding: true, decimals: 2 });

            this.statisticsGrid.gridAg.options.useGrouping();
            this.statisticsGrid.gridAg.finalizeInitGrid(terms["common.statistics"], true);
        });
    }
       
    protected addSumAggregationFooterToColumns(numberWithoutDecimals: boolean, ...args: uiGrid.IColumnDef[]) {
        args.forEach(col => {
            col.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            col.aggregationHideLabel = true;
            if (numberWithoutDecimals)
                col.footerCellFilter = 'number:0';
            else
                col.footerCellFilter = 'number:2';
            col.footerCellTemplate = '<div class="ui-grid-cell-contents" col-index="renderIndex">' +
                '<div class="pull-right">{{col.getAggregationText() + (col.getAggregationValue() CUSTOM_FILTERS )}}</div>' +
                '</div>';
        });
    }
    // #endregion

    // #region Lookups
    private loadOriginTypes(): ng.IPromise<any> {
        const deferral = this.$q.defer();
        this.coreService.getTermGroupContent(TermGroup.OriginType, false, false).then((x) => {
            this.originTypes = [];
            _.forEach(x, (row) => {
                if (row.id === SoeOriginType.CustomerInvoice ||
                    row.id === SoeOriginType.Order ||
                    row.id === SoeOriginType.Offer ||
                    row.id === SoeOriginType.Contract)
                    this.originTypes.push({ value: row.name, label: row.name });
                if (row.id === SoeOriginType.CustomerInvoice)
                    this.defaultFilterOriginType = row.name;
            });
            deferral.resolve();
        });

        return deferral.promise;
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
        });
    }

    private loadData(): ng.IPromise<any> {
        return this.loadCustomer();
    }

    private loadCustomer(): any {
        this.load().then(() => {
            this.setTabLabel()
            this.focusService.focusByName(this.customerId ? "ctrl_customer_name" : "ctrl_customer_customerNr");
            this.setupWatches();
        });
    }

    private setConsentToolTip() {
        if (this.customer.isPrivatePerson) {
            const keys: string[] = [
                "common.consentdescr",
                "common.modifiedby"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                this.consentToolTip = terms["common.consentdescr"] + "\n"
                if (this.customer.consentModifiedBy) {
                    this.consentToolTip = this.consentToolTip + terms["common.modifiedby"] + ": " + this.customer.consentModifiedBy + " " + CalendarUtility.toFormattedDate(this.customer.consentModified);
                }
            });
        }
        else {
            this.consentToolTip = "";
        }
    }

    private load(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.customerId > 0) {
            this.commonCustomerService.getCustomer(this.customerId, true, true, true, true, true, true).then((x) => {
                this.customer = x;

                this.loadRotdata(this.rotExpanderRendered);

                this.customer.consentDate = CalendarUtility.convertToDate(this.customer.consentDate);
                this.setConsentToolTip();

                this.customer.contactAddresses = this.customer.contactAddresses.map(ca => {
                    var obj = new ContactAddressItemDTO();
                    angular.extend(obj, ca);
                    return obj;
                });

                if (this.customer.payingCustomerId && this.customer.payingCustomerId !== 0)
                    this.selectedPayingCustomer = _.find(this.customers, { id: this.customer.payingCustomerId });

                this.isNew = false;

                if (this.extraFieldsExpanderRendered) {                    
                    this.$scope.$broadcast('reloadExtraFields', { guid: this.guid, recordId: this.customerId });
                }

                deferral.resolve();
            });
        }
        else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadCustomerStatistics() {
        if (this.customerId > 0) {

            const keys: string[] = [
                "common.customerinvoice",
                "common.order",
                "common.offer",
                "common.contract",
            ];

            this.translationService.translateMany(keys).then((terms) => {

                this.commonCustomerService.getCustomerStatistics(this.customerId, this.allItemsSelection).then((x:any[]) => {
                    _.forEach(x, (y) => {
                        switch (y.originType) {
                            case SoeOriginType.CustomerInvoice:
                                y.originType = terms["common.customerinvoice"];
                                break;
                            case SoeOriginType.Order:
                                y.originType = terms["common.order"];
                                break;
                            case SoeOriginType.Offer:
                                y.originType = terms["common.offer"];
                                break;
                            case SoeOriginType.Contract:
                                y.originType = terms["common.contract"];
                                break;
                        }
                    });

                    this.statisticsGrid.gridAg.setData(x);
                    if (x) {
                        if (x.length > 30)
                            this.statisticsGrid.gridAg.options.setMinRowsToShow(30);
                        else if (x.length > 6 )
                            this.statisticsGrid.gridAg.options.setMinRowsToShow(x.length+3);
                        else
                            this.statisticsGrid.gridAg.options.setMinRowsToShow(8);
                    }
                });
            });
        }
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            CompanySettingType.CustomerGracePeriodDays,
            CompanySettingType.BillingAutomaticCustomerOwner,
            CompanySettingType.CustomerInvoiceUseDeliveryCustomer,
            CompanySettingType.BillingEInvoiceFormat,
            CompanySettingType.BillingUseInvoiceDeliveryProvider,
            CompanySettingType.BillingCustomerHideTaxDeductionContacts,
            CompanySettingType.BillingUseAdditionalDiscount,

            // Base accounts
            CompanySettingType.AccountCustomerClaim,
            CompanySettingType.AccountCustomerSalesVat,
            CompanySettingType.AccountCommonVatPayable1
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultGracePeriodDays = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerGracePeriodDays);
            this.setOwnerAutomatically = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingAutomaticCustomerOwner);
            this.useDeliveryCustomer = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceUseDeliveryCustomer);
            this.eInvoiceFormat = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingEInvoiceFormat);
            this.useInvoiceDeliveryProvider = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseInvoiceDeliveryProvider);
            this.hideTaxDeductionContacts = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingCustomerHideTaxDeductionContacts);
            this.isAdditionalDiscount = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseAdditionalDiscount);

            // Base accounts
            this.baseAccounts = [];
            this.baseAccounts.push(new SmallGenericType(CustomerAccountType.Debit, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerClaim).toString()));
            this.baseAccounts.push(new SmallGenericType(CustomerAccountType.Credit, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerSalesVat).toString()));
            this.baseAccounts.push(new SmallGenericType(CustomerAccountType.VAT, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable1).toString()));
        });
    }

    private loadSettingTypes(): ng.IPromise<any> {
        const keys: string[] = [
            "common.customer.customer.accountingsettingtype.credit",
            "common.customer.customer.accountingsettingtype.debit",
            "common.customer.customer.accountingsettingtype.vat",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.settingTypes = [];
            this.settingTypes.push(new SmallGenericType(CustomerAccountType.Debit, terms["common.customer.customer.accountingsettingtype.debit"]));
            this.settingTypes.push(new SmallGenericType(CustomerAccountType.Credit, terms["common.customer.customer.accountingsettingtype.credit"]));
            this.settingTypes.push(new SmallGenericType(CustomerAccountType.VAT, terms["common.customer.customer.accountingsettingtype.vat"]));
        });
    }

    private loadCustomers(): ng.IPromise<any> {
        return this.commonCustomerService.getCustomersDict(true, true, true).then(x => {
            this.customers = x;
        });
    }

    private loadCountries(): ng.IPromise<any> {
        return this.coreService.getSysCountries(true, false).then(x => {
            this.countries = x;
        });
    }

    private loadLanguages(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.Language, true, false).then(x => {
            this.languages = x;
        });
    }

    private loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesDict(false).then(x => {
            this.currencies = x;
        });
    }

    private loadVatTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceVatType, true, false).then(x => {
            this.vatTypes = x;
        });
    }

    private loadPaymentConditions(): ng.IPromise<any> {
        return this.commonCustomerService.getPaymentConditionsDict(true).then(x => {
            this.paymentConditions = x;
        });
    }

    private loadDeliveryTypes(): ng.IPromise<any> {
        return this.commonCustomerService.getDeliveryTypesDict(true).then(x => {
            this.deliveryTypes = x;
        });
    }

    private loadDeliveryConditions(): ng.IPromise<any> {
        return this.commonCustomerService.getDeliveryConditionsDict(true).then(x => {
            this.deliveryConditions = x;
        });
    }

    private loadPriceLists(): ng.IPromise<any> {
        return this.commonCustomerService.getPriceListsDict(true, true).then(x => {
            this.priceLists = x;
        });
    }

    private loadWholesellers(): ng.IPromise<any> {
        return this.commonCustomerService.getSysWholesellersDict(true).then(x => {
            this.wholesellers = x;
        });
    }

    private loadAgreementTemplates(): ng.IPromise<any> {
        return this.reportService.getReportsDict(SoeReportTemplateType.BillingContract, false, false, true, false).then(x => {
            this.agreementTemplates = x;
        });
    }

    private loadOfferTemplates(): ng.IPromise<any> {
        return this.reportService.getReportsDict(SoeReportTemplateType.BillingOffer, false, false, true, false).then(x => {
            this.offerTemplates = x;
        });
    }

    private loadOrderTemplates(): ng.IPromise<any> {
        return this.reportService.getReportsDict(SoeReportTemplateType.BillingOrder, false, false, true, false).then(x => {
            this.orderTemplates = x;
        });
    }

    private loadBillingTemplates(): ng.IPromise<any> {
        return this.reportService.getReportsDict(SoeReportTemplateType.BillingInvoice, false, false, true, false).then(x => {
            this.billingTemplates = x;
        });
    }

    private loadEmails(): ng.IPromise<any> {
        return this.commonCustomerService.getCustomerEmails(this.customerId, true, true).then(x => {
            this.emails = x;
        });
    }

    private loadCustomerGLNs(): ng.IPromise<any> {        
        return this.commonCustomerService.getCustomerGLNs(this.customerId, true).then(x => {
                this.customerGLNs = x;
        });        
    }

    private loadInvoiceDeliveryTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceDeliveryType, true, false).then(x => {
            this.invoiceDeliveryTypes = x;

            if (this.eInvoiceFormat != TermGroup_EInvoiceFormat.Intrum) {
               this.invoiceDeliveryTypes = this.invoiceDeliveryTypes.filter(x => x.id !== SoeInvoiceDeliveryType.EDI);
            }

            if (!this.eInvoicePermission) {
                this.invoiceDeliveryTypes = this.invoiceDeliveryTypes.filter(y => (y.id !== SoeInvoiceDeliveryType.Electronic));
            }
        });
    }

    private loadInvoiceDeliveryProviders(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceDeliveryProvider, true, false).then(x => {
            if (this.useInvoiceDeliveryProvider) {
                this.invoiceDeliveryProviders = x;
            }
        });
    }

    private loadInvoicePaymentServices(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoicePaymentService, true, false).then(x => {
            this.invoicePaymentServices = x;
        });
    }

    private loadCurrentUser(): ng.IPromise<any> {
        return this.coreService.getCurrentUser().then((x) => {
            return x;
        });
    }

    private getNextCustomerNr() {
        this.commonCustomerService.getNextCustomerNr().then(x => {
            this.customer.customerNr = x;
        });
    }

    private getFinvoiceSearchUrl() {
        return "https://verkkolaskuosoite.fi/client/index.html#/?searchText=" + (this.customer.orgNr || this.customer.name);
    }

    // #endregion

    // #region Actions

    public closeModal(saved: boolean = false) {
        if (this.isModal) {
            if (this.customerId) {
                this.modal.close({ customerId: this.customerId, customerName: this.customer.name, saved: saved });
            } else {
                this.modal.dismiss();
            }
        }
    }

    private save(overrideCheck = false) {
        if (this.customer.active)
            this.customer.state = SoeEntityState.Active;
        else
            this.customer.state = SoeEntityState.Inactive;

        //Fix product rows
        this.customer.customerProducts = _.filter(this.customer.customerProducts, (p) => p.productId && p.productId > 0);

        // Applicants
        const applicants = _.filter(this.householdTaxApplicants, (a) => (a.customerInvoiceRowId > 0 && a.state === SoeEntityState.Deleted) || (a.state !== SoeEntityState.Deleted && ((a.householdTaxDeductionApplicantId > 0 && a.showButton === true) || a["new"])));

        // Files
        if (this.filesHelper.filesLoaded)
            this.customer.files = this.filesHelper.getAsDTOs();

        this.progress.startSaveProgress((completion) => {
            this.commonCustomerService.saveCustomer(this.customer, applicants, _.filter(this.extraFieldRecords, (r) => r.isModified === true)).then((result) => {
                if (result.success) {
                    if (!this.customerId)
                        this.commonCustomerService.getCustomersDict(true, true, false); // Clear Cache                                            
                    if (result.integerValue && result.integerValue > 0)
                        this.customerId = result.integerValue;
                    
                    if (this.extraFieldsExpanderRendered)
                        this.$scope.$broadcast('reloadExtraFields', { guid: this.guid, recordId: this.customerId });

                    completion.completed("", this.customer)

                    if(this.rotExpanderRendered)
                        this.loadRotdata(); // Reload rot/rut applicants

                    this.setTabLabel();
                    this.dirtyHandler.clean();
                    this.notifyProjectCentral();
                    if (this.isModal)
                        this.closeModal(true);
                    else
                        this.load();
                } else {
                    const keys: string[] = [
                        "common.customer.customer.customerexistserror",
                        "common.customer.customer.customernotsavederror",
                        "common.customer.customer.customernotupdatederror",
                        "common.customer.customer.customercontactsandtelecomnotsavederror",
                        "common.customer.customer.customeraccountsnotsavederror",
                        "common.customer.customer.customercompanycategorynotsavederror",
                        "common.customer.customer.contactpersonsavedwitherrorserror",
                        "common.customer.customer.customersavefailederror",
                    ];
                    this.translationService.translateMany(keys).then((terms) => {

                        let message: string = terms["common.customer.customer.customersavefailederror"] + ": " + result.errorMessage;
                        switch (result.errorNumber) {
                            case ActionResultSave.CustomerExists:
                                message = terms["common.customer.customer.customerexistserror"];
                                break;
                            case ActionResultSave.CustomerNotSaved:
                                message = terms["common.customer.customer.customernotsavederror"];
                                break;
                            case ActionResultSave.CustomerNotUpdated:
                                message = terms["common.customer.customer.customernotupdatederror"];
                                break;
                            case ActionResultSave.CustomerContactsAndTeleComNotSaved:
                                message = terms["common.customer.customer.customercontactsandtelecomnotsavederror"];
                                break;
                            case ActionResultSave.CustomerAccountsNotSaved:
                                message = terms["common.customer.customer.customeraccountsnotsavederror"];
                                break;
                            case ActionResultSave.CustomerCompanyCategoryNotSaved:
                                message = terms["common.customer.customer.customercompanycategorynotsavederror"];
                                break;
                            case ActionResultSave.ContactPersonSavedWithErrors:
                                message = terms["common.customer.customer.contactpersonsavedwitherrorserror"];
                                break;
                        }

                        completion.failed(message);

                    });

                }
            })
        }, this.guid)
    }

    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.commonCustomerService.deleteCustomer(this.customer.actorCustomerId).then((result) => {
                if (result.success) {
                    completion.completed(this.customer);
                    this.new();
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                    completion.failed(error.message);
            });
        })
    }

    protected copy() {
        this.isNew = true;
        this.customerId = 0;
        this.customer.actorCustomerId = undefined;
        this.getNextCustomerNr();

        _.forEach(this.customer.contactAddresses, (a) => {
            a.contactAddressId = undefined;
            a.contactEComId = undefined;
            a.contactId = undefined;
        });

        this.setAsDirty();
        this.messagingService.publish(Constants.EVENT_EDIT_NEW, { guid: this.guid });
    }

    private addRot() {
        var applicant: HouseholdTaxDeductionApplicantDTO = new HouseholdTaxDeductionApplicantDTO();
        applicant["new"] = true;
        this.showHouseholdApplicantDialog(applicant);
    }

    private editRot(row: HouseholdTaxDeductionApplicantDTO) {
        this.showHouseholdApplicantDialog(row);
    }

    private deleteRot(row: any) {
        row.state = SoeEntityState.Deleted;
        this.rotGridOptions.deleteRow(row);
        this.setAsDirty();
    }

    private showPayingCustomerNote() {
        if (this.selectedPayingCustomer) {
            this.commonCustomerService.getCustomer(this.selectedPayingCustomer.id, false, false, true, false, false, false).then(x => {
                const payingCustomer = x;
                this.translationService.translate("common.customer.customer.customernote").then((title) => {
                    this.notificationService.showDialog(title, payingCustomer.note ? payingCustomer.note : '', SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                });
            });
        }
    }

    // #endregion

    // #region Helpers

    private new() {
        this.isNew = true;
        this.customerId = 0;
        this.customer = new CustomerDTO();
        this.customer.active = true;
        this.getNextCustomerNr();
        this.customer.gracePeriodDays = this.defaultGracePeriodDays;
        this.customer.contactAddresses = [];
        this.customer.contactPersons = [];
        this.customer.customerProducts = [];
        this.householdTaxApplicants = [];
        this.customer.categoryIds = [];
        this.customer.accountingSettings = [];
        this.customer.addSupplierInvoicesToEInvoice = false;

        // Set current user as owner
        if (this.setOwnerAutomatically) {
            this.customer.customerUsers = [];

            this.loadCurrentUser().then((x) => {
                var currentUser: IUserSmallDTO = x;
                if (currentUser) {
                    var customerUser = new CustomerUserDTO();
                    customerUser.userId = currentUser.userId;
                    customerUser.name = currentUser.name;
                    customerUser.main = true;
                    this.customer.customerUsers.push(customerUser);
                }
            });
        }
        // default values
        if (this.currencies.length)
            this.customer.currencyId = this.currencies[0].id;
    }

    private setTabLabel() {
        let tabLabel: string;
        const keys: string[] = [
            "common.customer.customer.customer",
            "common.customer.customer.new"
        ]

        this.translationService.translateMany(keys).then((terms) => {
            if (this.customerId < 1) {
                tabLabel = terms["common.customer.customer.new"];
            }
            else
            {
                tabLabel = terms["common.customer.customer.customer"] + " " + this.customer.customerNr;
            }
            this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
                guid: this.guid,
                label: tabLabel
            });
        });
    }

    private notifyProjectCentral() {
        this.messagingService.publish(Constants.EVENT_REFRESH_PROJECTCENTRALDATA, { customerId: this.customerId });
    }

    private selectUsers() {
        this.translationService.translate("common.customer.customer.selectusers").then(title => {
            const modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectUsers", "SelectUsers.html"),
                controller: SelectUsersController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    translationService: () => { return this.translationService },
                    coreService: () => { return this.coreService },
                    title: () => { return title },
                    selectedUsers: () => { return this.customer.customerUsers },
                    showMain: () => { return true },
                    showParticipant: () => { return false },
                    showSendMessage: () => { return false },
                    showCategories: () => { return true }
                }
            });

            modal.result.then(x => {
                this.customer.customerUsers = x.selectedUsers;
            });

            return modal;
        });
    }

    private setAsDirty(dirty: boolean = true) {
        this.dirtyHandler.isDirty = dirty;
    }

    private showHouseholdApplicantDialog(applicant: IHouseholdTaxDeductionApplicantDTO) {
        var workingCopy = _.clone(applicant);
        this.translationService.translate("common.customer.customer.rot.register").then(title => {
            const modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/HouseholdTaxDeduction", "HouseholdTaxDeduction.html"),
                controller: HouseholdTaxDeductionController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    title: () => { return title },
                    applicant: () => { return workingCopy }
                }
            });

            modal.result.then((x: HouseholdTaxDeductionApplicantDTO) => {
                // Find existing
                var existing = _.find(this.householdTaxApplicants, { 'householdTaxDeductionApplicantId': x.householdTaxDeductionApplicantId });

                // If new applicant was created, add it to collection
                if (!existing)
                    this.householdTaxApplicants.push(x);
                else 
                    angular.extend(existing, x);
                
                this.customer.isPrivatePerson = true;
                this.setAsDirty();

                // Rebind
                this.rotGridOptions.setData(this.householdTaxApplicants);
            });

            return modal;
        });
    }

    private exportCustomer() {
        this.progress.startWorkProgress((completion) => {
            this.commonCustomerService.getCustomerForExport(this.customerId).then((customer) => {
                completion.completed("", true)
                ExportUtility.Export(customer, 'customer.json');
            });
        })
    }

    // #endregion

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            const errors = this['edit'].$error;
            if (this.customer) {
                if (!this.customer.customerNr)
                    mandatoryFieldKeys.push("common.customer.customer.customernr");
                if (!this.customer.name)
                    mandatoryFieldKeys.push("common.name");

                // Name contains comma
                if (errors['nameContainsComma'])
                    validationErrorKeys.push("common.customer.customer.namecontainscomma");

                if (errors['contactAddress'])
                    validationErrorStrings.push(this.contactAddressesValidationErrors);
            }
        });
    }
    public onExtraFieldsExpanderOpenClose() {
        this.extraFieldRecords = [];
        this.extraFieldsExpanderRendered = !this.extraFieldsExpanderRendered;
    }
}