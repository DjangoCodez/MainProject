import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SoeGridOptionsEvent, CustomerInvoiceGridButtonFunctions, SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize } from "../../../Util/Enumerations";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Feature, TermGroup, SoeInvoiceRowType, SoeInvoiceRowDiscountType, CompanySettingType, TermGroup_AttestEntity, SoeOriginStatusChange, TermGroup_InvoiceProductCalculationType, SoeProductRowType, TermGroup_TimeCodeRegistrationType, SettingMainType, UserSettingType, SoeEntityState } from "../../../Util/CommonEnumerations";
import { HandleBillingRowDTO } from "../../../Common/Models/HandleBillingRowDTO";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { Constants } from "../../../Util/Constants";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { NumberUtility } from "../../../Util/NumberUtility";
import { IOrderService } from "../../../Shared/Billing/Orders/OrderService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { IAttestTransitionDTO } from "../../../Scripts/TypeLite.Net4";
import { AttestStateDTO } from "../../../Common/Models/AttestStateDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { TimeColumnOptions } from "../../../Util/SoeGridOptionsAg";
import { ProjectTimeBlockDTO } from "../../../Common/Models/ProjectDTO";
import { EditNoteController } from "../../../Common/Directives/TimeProjectReport/EditNoteController";
import { IContextMenuHandlerFactory } from "../../../Core/Handlers/ContextMenuHandlerFactory";
import { TimeRowsHelper } from "../../../Shared/Billing/Helpers/TimeRowsHelper";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Permissions
    loadOnlyMine: boolean;
    onlyMineLocked: boolean;
    hasInvoiceRowPermission: boolean;
    hasStatusChangePermission: boolean;
    hasTransferToPreliminaryPermission: boolean;
    hasTransferToDefinitivePermission: boolean;
    hasSalesPricePermission = true;
    hasCurrencyPermission = false;
    hasPurchasePricePermission = true;
    hasEditProjectPermission = false;
    hasEditOrderPermission = false;
    hasInvoiceTimePermission = false;
    hasWorkTimePermission = false;
    hasTimeRowsPermission = false;

    // Settings
    transferAndPrint = false;
    defaultBillingInvoiceReportId: number = 0;
    useExtendedTimeRegistration = false;
    productGuaranteeId: number = 0;
    attestStateReadyId: number = 0;
    usePartialInvoicingOnOrderRow = false;

    // Data  
    handleBillingRows: HandleBillingRowDTO[];
    projects: any[];          
    orders: any[];
    customers: any[];
    attestTransitions: IAttestTransitionDTO[] = [];
    attestStates: AttestStateDTO[] = [];
    availableAttestStates: AttestStateDTO[] = [];
    availableAttestStateOptions: any[] = [];
    initialAttestState: AttestStateDTO;
    excludedAttestStates: number[] = [];
    attestStateTransferredOrderToInvoiceId: number = 0;
    attestStateTransferredOrderToContractId: number = 0;
    orderTypes: any[];
    orderContractTypes: any[];

    // Functions
    buttonFunctions: any = [];

    // Summaries
    filteredValidForInvoice: number = 0;
    filteredTotal: number = 0;

    toolbarInclude: any;
    gridFooterComponentUrl: any;

    // Columns
    private numbersColumn: any;
    private dateColumn: any;
    private orderTypeColumn: any;
    private orderCategoriesColumn: any;
    private contractCategoriesColumn: any;
    private attestStateColumn: any;
    private accountDim1ColumnName: string;

    // Terms
    terms: { [index: string]: string; };

    // Column labels
    numbersColumnHeader: string = "";
    dateColumnHeader: string = "";

    // Selection
    private onlyValidToTransfer: boolean;
    private selectedDateFrom: Date;
    private selectedDateTo: Date;
    private selectedProjects: any[] = [];
    private selectedOrders: any[] = [];
    private selectedCustomers: any[] = [];
    private selectedAttestState: any;
    private selectedOrderTypes: any[] = [];
    private selectedOrderContractTypes: any[] = [];

    // Flags
    private hasSelectedTimeRows = false;

    //@ngInject
    constructor(
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private invoiceService: IInvoiceService,
        private orderService: IOrderService,
        private commonCustomerService: ICommonCustomerService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private contextMenuHandlerFactory: IContextMenuHandlerFactory,
        private $scope: ng.IScope) {

        super(gridHandlerFactory, "billing.order.handlebilling.handlebilling", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.doLookup())
            .onSetUpGrid(() => this.setupGrid());

        this.doubleClickToEdit = false;
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.searchCustomerInvoiceRows(); });
        }

        // Set footer
        this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooter.html");

        this.flowHandler.start([
            { feature: Feature.Billing_Order_HandleBilling, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_OrdersAll, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_Orders, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_OrdersUser, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_Status, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_Orders_Edit_ProductRows, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_Status_OrderToInvoice, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Product_Products_ShowSalesPrice, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Product_Products_ShowPurchasePrice, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Status_DraftToOrigin, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Project_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_Orders_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Time_Project_Invoice_InvoicedTime, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Time_Project_Invoice_WorkedTime, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Time_Project_Invoice_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Customer_Invoice_Status_Foreign, loadReadPermissions: true, loadModifyPermissions: true },
        ]);


        this.$scope.$watch(() => this.loadOnlyMine, (newVal, oldVal) => {
            if (oldVal !== undefined) {
                this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.BillingHandleBillingOnlyMine, newVal);
                if(this.selectedOrders.length > 0 || this.selectedProjects.length > 0 || this.selectedCustomers.length > 0)
                    this.searchCustomerInvoiceRows();
            }
        });

        this.$scope.$watch(() => this.onlyValidToTransfer, (newVal, oldVal) => {
            if (oldVal !== undefined) {
                this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.BillingHandleBillingOnlyValid, newVal);
                if (this.selectedOrders.length > 0 || this.selectedProjects.length > 0 || this.selectedCustomers.length > 0)
                    this.searchCustomerInvoiceRows();
            }
        });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Order_HandleBilling].readPermission;
        this.modifyPermission = response[Feature.Billing_Order_HandleBilling].modifyPermission;
        
        this.loadOnlyMine = this.onlyMineLocked = response[Feature.Billing_Order_OrdersUser].readPermission;

        this.hasStatusChangePermission = response[Feature.Billing_Order_Status].modifyPermission;
        this.hasInvoiceRowPermission = response[Feature.Billing_Order_Orders_Edit_ProductRows].modifyPermission;
        this.hasTransferToPreliminaryPermission = response[Feature.Billing_Order_Status_OrderToInvoice].modifyPermission;
        this.hasTransferToDefinitivePermission = response[Feature.Billing_Order_Status_OrderToInvoice].modifyPermission && response[Feature.Billing_Invoice_Status_DraftToOrigin].modifyPermission;
        this.hasSalesPricePermission = response[Feature.Billing_Product_Products_ShowSalesPrice].modifyPermission;
        this.hasPurchasePricePermission = response[Feature.Billing_Product_Products_ShowPurchasePrice].modifyPermission;
        this.hasEditProjectPermission = response[Feature.Billing_Project_Edit].modifyPermission;
        this.hasEditOrderPermission = response[Feature.Billing_Order_Orders_Edit].modifyPermission;
        this.hasInvoiceTimePermission = response[Feature.Time_Project_Invoice_InvoicedTime].readPermission;
        this.hasWorkTimePermission = response[Feature.Time_Project_Invoice_WorkedTime].readPermission;
        this.hasTimeRowsPermission = response[Feature.Billing_Order_Orders_Edit].modifyPermission;
        this.hasCurrencyPermission = response[Feature.Economy_Customer_Invoice_Status_Foreign].readPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => {
            if (this.selectedDateFrom && this.selectedDateTo)
                this.searchCustomerInvoiceRows()
        });
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("searchHeader.html"));

        this.contextMenuHandler = this.contextMenuHandlerFactory.create();
        if (this.hasTimeRowsPermission) {
            this.contextMenuHandler.addContextMenuItem(this.terms["billing.productrows.functions.showconnectedtimerows"], 'fal fa-clock', ($itemScope, $event, modelValue) => { this.showTimeRows(); }, () => { return this.hasSelectedTimeRows; });
        }
    }

    private doLookup(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadUserSettings(),
            this.loadProjects(),
            this.loadOrders(),
            this.loadCustomer(),
            this.loadUserAttestTransitions(),
            this.loadOrderTypes(),
            this.loadOrderContractTypes(),
        ]).then(() => {
            // Set dates
            const today: Date = CalendarUtility.getDateToday();
            this.selectedDateTo = today;
            this.selectedDateFrom = new Date(today.getFullYear(), today.getMonth() - 1, 1);

            if (this.hasTransferToPreliminaryPermission)
                this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToPreliminarInvoice, name: this.terms["core.transfertopreliminaryinvoice"] });
            if (this.hasTransferToPreliminaryPermission)
                this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToInvoiceAndMergeOrders, name: this.terms["billing.contract.transfertopreliminaryandmerge"] });
            if (this.hasTransferToDefinitivePermission && this.transferAndPrint)
                this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.TransferToInvoiceAndPrint, name: this.terms["core.transfertoinvoiceandprint"] });
            if (this.hasTimeRowsPermission)
                this.buttonFunctions.push({ id: CustomerInvoiceGridButtonFunctions.SplitTimeRows, name: this.terms["billing.order.splittimerows"] });

            this.setData([]);
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.yes",
            "core.no",
            "core.continue",
            "core.warning",
            "core.verifyquestion",
            "billing.project.project",
            "common.quantity",
            "common.customer",
            "common.customer.invoices.ordernr",
            "common.customer.invoices.edi",
            "common.customer.invoices.productnr",
            "common.customer.invoices.productname",
            "common.customer.invoices.quantity",
            "common.customer.invoices.unit",
            "common.customer.invoices.price",
            "common.customer.invoices.discount",
            "billing.order.discounttype",
            "common.customer.invoices.sum",
            "billing.productrows.purchaseprice",
            "billing.productrows.purchasepricesum",
            "billing.productrows.marginalincome.short",
            "billing.productrows.marginalincomeratio.short",
            "core.transfertopreliminaryinvoice",
            "billing.contract.transfertopreliminaryandmerge",
            "core.transfertoinvoiceandprint",
            "billing.productrows.changeatteststate",
            "billing.project.project",
            "common.order",
            "billing.productrows.rownr",
            "common.date",
            "common.customer.invoice.willtransferorder2invoice",
            "billing.project.timesheet.employeenr",
            "common.employee",
            "common.yearweek",
            "common.weekday",
            "common.time.timedeviationcause",
            "billing.project.timesheet.chargingtype",
            "billing.project.timesheet.workedtime",
            "billing.project.timesheet.invoicedtime",
            "billing.productrows.functions.showconnectedtimerows",
            "billing.productrows.changeatteststate.errorlift",
            "billing.productrows.changeatteststate.errorstock",
            "common.customer.invoices.row",
            "common.customer.invoices.wrongstatetotransfer",
            "billing.productrows.changeatteststate.errortitle",
            "billing.order.invalidchangestatesingle",
            "billing.order.invalidchangestatesmultiple",
            "billing.order.validchangestatesingle",
            "billing.order.validchangestatemultiple",
            "billing.order.allselectedinvalid",
            "billing.order.changeatteststatefailed",
            "common.customer.invoices.amount",
            "common.customer.invoices.amountexvat",
            "common.customer.invoices.foreignamount",
            "common.customer.invoices.amounttoinvoice",
            "common.customer.invoices.currencyamounttotransfer",
            "billing.order.splittimerows",
            "billing.order.validsplitsingle",
            "billing.order.validsplitmultiple",
            "billing.order.invalidsplitstatesingle",
            "billing.order.invalidsplitstatemultiple",
            "billing.order.invalidstatefortransfertoinvoice",
            "billing.order.transferallrowsinfo",
            "billing.order.origindescription"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            CompanySettingType.BillingStatusTransferredOrderToInvoiceAndPrint,
            CompanySettingType.BillingDefaultInvoiceTemplate,
            CompanySettingType.BillingStatusTransferredOrderToInvoice,
            CompanySettingType.BillingStatusTransferredOrderToContract,
            CompanySettingType.ProjectUseExtendedTimeRegistration,
            CompanySettingType.ProductGuarantee,
            CompanySettingType.BillingStatusOrderReadyMobile,
            CompanySettingType.BillingUsePartialInvoicingOnOrderRow
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.transferAndPrint = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingStatusTransferredOrderToInvoiceAndPrint, false);
            this.defaultBillingInvoiceReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultInvoiceTemplate);
            this.attestStateTransferredOrderToInvoiceId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingStatusTransferredOrderToInvoice);
            if (this.attestStateTransferredOrderToInvoiceId !== 0 && !_.includes(this.excludedAttestStates, this.attestStateTransferredOrderToInvoiceId))
                this.excludedAttestStates.push(this.attestStateTransferredOrderToInvoiceId);
            this.attestStateTransferredOrderToContractId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingStatusTransferredOrderToContract);
            if (this.attestStateTransferredOrderToContractId !== 0 && !_.includes(this.excludedAttestStates, this.attestStateTransferredOrderToContractId))
                this.excludedAttestStates.push(this.attestStateTransferredOrderToContractId);
            this.useExtendedTimeRegistration = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectUseExtendedTimeRegistration, false);
            this.productGuaranteeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductGuarantee);
            this.attestStateReadyId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingStatusOrderReadyMobile);
            this.usePartialInvoicingOnOrderRow = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUsePartialInvoicingOnOrderRow);
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.BillingHandleBillingOnlyMine, UserSettingType.BillingHandleBillingOnlyValid];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.onlyValidToTransfer = SettingsUtility.getBoolUserSetting(x, UserSettingType.BillingHandleBillingOnlyValid, true);
            if (!this.onlyMineLocked)
                this.loadOnlyMine = SettingsUtility.getBoolUserSetting(x, UserSettingType.BillingHandleBillingOnlyMine, false);
        });
    }

    private loadProjects(): ng.IPromise<any> {
        this.projects = [];
        return this.invoiceService.getProjects(true, true, false, false, false, 0).then((x) => {
            _.forEach(x, (y) => {
                this.projects.push({ id: y.projectId, label: y.number + ' ' + y.name });
            });
        });
    }

    private loadOrders(): ng.IPromise<any> {
        this.orders = [];
        return this.orderService.getOpenOrdersDict().then((x) => {
            _.forEach(x, (y) => {
                this.orders.push({ id: y.id, label: y.name });
            });
        });
    }

    private loadCustomer(): ng.IPromise<any> {
        this.customers = [];
        return this.commonCustomerService.getCustomersDict(true, false, true).then((x) => {
            _.forEach(x, (y) => {
                this.customers.push({ id: y.id, label: y.name });
            });
        });
    }

    private loadUserAttestTransitions(startDate?: Date, stopDate?: Date): ng.IPromise<any> {
        return this.coreService.getUserAttestTransitions(TermGroup_AttestEntity.Order, startDate, stopDate).then(x => {
            this.attestTransitions = x;

            // Add states from returned transitions
            _.forEach(this.attestTransitions, (trans) => {
                if (_.filter(this.attestStates, a => a.attestStateId === trans.attestStateToId).length === 0)
                    this.attestStates.push(trans.attestStateTo);
            });

            // Sort states
            this.attestStates = _.orderBy(this.attestStates, 'sort');

            // Get initial state
            this.initialAttestState = _.find(this.attestStates, a => a.initial === true);
            if (!this.initialAttestState) {
                this.loadInitialAttestState();
            }

            // Setup available states (exclude finished states)
            this.availableAttestStates = [];
            _.forEach(this.attestStates, (attestState) => {
                if (!_.includes(this.excludedAttestStates, attestState.attestStateId)) {
                    // Map to correct type
                    var obj = new AttestStateDTO();
                    angular.extend(obj, attestState);
                    this.availableAttestStates.push(obj);
                }
            });

            // Setup available states for selector
            this.availableAttestStateOptions = [];
            this.availableAttestStateOptions.push({ id: 0, name: this.terms["billing.productrows.changeatteststate"] });
            _.forEach(this.availableAttestStates, (a: AttestStateDTO) => {
                this.availableAttestStateOptions.push({ id: a.attestStateId, name: a.name });
            });
            this.selectedAttestState = 0;
        });
    }

    private loadOrderTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.OrderType, false, false).then((x) => {
            this.orderTypes = [];
            _.forEach(x, (row) => {
                this.orderTypes.push({ id: row.id, label: row.name });
            });
        });
    }

    private loadOrderContractTypes() {
        return this.coreService.getTermGroupContent(TermGroup.OrderContractType, false, false).then((x) => {
            this.orderContractTypes = [];
            _.forEach(x, (row) => {
                this.orderContractTypes.push({ id: row.id, label: row.name });
            });
        });
    }

    private loadInitialAttestState() {
        this.coreService.getAttestStateInitial(TermGroup_AttestEntity.Order).then(x => {
            this.initialAttestState = x;

            if (!this.initialAttestState) {
                const keys: string[] = [
                    "billing.productrows.initialstatemissing.title",
                    "billing.productrows.initialstatemissing.message"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    this.notificationService.showDialog(terms["billing.productrows.initialstatemissing.title"], terms["billing.productrows.initialstatemissing.message"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Large);
                });
            } else {
                this.attestStates.push(this.initialAttestState);
                // Sort states
                this.attestStates = _.orderBy(this.attestStates, 'sort');
            }
        });
    }

    public setupGrid() {
        // Details
        this.gridAg.enableMasterDetail(true);
        this.gridAg.options.setDetailCellDataCallback((params) => {
            this.$timeout(() => {
                if (params.data.isTimeProjectRow) {
                    // Hide expense columns
                    this.gridAg.detailOptions.hideColumn("guantityFormatted");
                    this.gridAg.detailOptions.hideColumn("timeCodeName");
                    this.gridAg.detailOptions.hideColumn("from");
                    this.gridAg.detailOptions.hideColumn("amount");
                    this.gridAg.detailOptions.hideColumn("amountExVat");
                    this.gridAg.detailOptions.hideColumn("amountCurrency");
                    this.gridAg.detailOptions.hideColumn("payrollAttestStateColor");
                    this.gridAg.detailOptions.hideColumn("invoicedAmount");
                    this.gridAg.detailOptions.hideColumn("invoicedAmountCurrency");
                }
                else if (params.data.productRowType === SoeProductRowType.ExpenseRow) {
                    this.gridAg.detailOptions.hideColumn("date");
                    this.gridAg.detailOptions.hideColumn("yearWeek");
                    this.gridAg.detailOptions.hideColumn("weekDay");
                    this.gridAg.detailOptions.hideColumn("timeDeviationCauseName");
                    this.gridAg.detailOptions.hideColumn("timePayrollQuantityFormatted");
                    this.gridAg.detailOptions.hideColumn("timePayrollAttestStateColor");
                    this.gridAg.detailOptions.hideColumn("invoiceQuantityFormatted");
                    this.gridAg.detailOptions.hideColumn("customerInvoiceRowAttestStateColor");
                    this.gridAg.detailOptions.hideColumn("noteIcon");
                }
            });
            this.loadProjectTimeBlocks(params);
        });

        const timeColumnOptions: TimeColumnOptions = {
            enableHiding: true, enableRowGrouping: true,
            clearZero: false, alignLeft: false, minDigits: 5, cellClassRules: {
                "excelTime": () => true,
            }
        };

        const timePayrollColumnOptions: TimeColumnOptions = {
            enableHiding: true,
            clearZero: false, alignLeft: false, enableRowGrouping: true, minDigits: 5, cellClassRules: {
                "errorRow": (gridRow: any) => gridRow.data && (gridRow.data.timePayrollQuantity < gridRow.data.scheduledQuantity),
                "excelTime": () => true,
            }
        };

        this.gridAg.detailOptions.addColumnText("employeeNr", this.terms["billing.project.timesheet.employeenr"], null, { enableHiding: true, hide: true, enableRowGrouping: true });
        this.gridAg.detailOptions.addColumnText("employeeName", this.terms["common.employee"], null, { toolTipField: "columnNameTooltip", enableRowGrouping: true, cellClassRules: { "errorRow": (row: any) => row && row.data && row.data.employeeIsInactive } });

        this.gridAg.detailOptions.addColumnDate("date", this.terms["common.date"], null, true, null, null, {
            toolTipField: "dateFormatted", cellClassRules: {
                "excelDate": () => true,
            }
        });
        this.gridAg.detailOptions.addColumnText("yearWeek", this.terms["common.yearweek"], null, { enableHiding: true, hide: true, enableRowGrouping: true });
        this.gridAg.detailOptions.addColumnText("weekDay", this.terms["common.weekday"], null, { enableHiding: true, enableRowGrouping: true });

        if (this.useExtendedTimeRegistration) {
            this.gridAg.detailOptions.addColumnText("timeDeviationCauseName", this.terms["common.time.timedeviationcause"], null, { enableHiding: true, enableRowGrouping: true });
        }

        this.gridAg.detailOptions.addColumnText("timeCodeName", this.terms["billing.project.timesheet.chargingtype"], null, { enableRowGrouping: true });

        const quantityColumn = this.gridAg.detailOptions.addColumnText("guantityFormatted", this.terms["common.quantity"], null, {});
        quantityColumn.cellClass = "text-right";
        quantityColumn.cellStyle = { 'padding-right': '5px' };
        this.gridAg.detailOptions.addColumnDate("from", this.terms["common.date"], null, true, null, null, { toolTipField: "dateFormatted" });

        if (this.hasWorkTimePermission) {
            this.gridAg.detailOptions.addColumnTimeSpan("timePayrollQuantityFormatted", this.terms["billing.project.timesheet.workedtime"], null, timePayrollColumnOptions);
            this.gridAg.detailOptions.addColumnShape("timePayrollAttestStateColor", null, 40, { shape: Constants.SHAPE_CIRCLE, toolTipField: "timePayrollAttestStateName", showIconField: "timePayrollAttestStateColor" });
        }

        if (this.hasInvoiceTimePermission) {
            this.gridAg.detailOptions.addColumnTimeSpan("invoiceQuantityFormatted", this.terms["billing.project.timesheet.invoicedtime"], null, timeColumnOptions);
            this.gridAg.detailOptions.addColumnShape("customerInvoiceRowAttestStateColor", null, 40, { shape: Constants.SHAPE_CIRCLE, toolTipField: "customerInvoiceRowAttestStateName", showIconField: "customerInvoiceRowAttestStateColor" });
        }
        
        this.gridAg.detailOptions.addColumnNumber("amount", this.terms["common.customer.invoices.amount"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.detailOptions.addColumnNumber("amountExVat", this.terms["common.customer.invoices.amountexvat"], null, { enableHiding: true, decimals: 2 });

        if (this.hasCurrencyPermission)
            this.gridAg.detailOptions.addColumnNumber("amountCurrency", this.terms["common.customer.invoices.foreignamount"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.detailOptions.addColumnShape("payrollAttestStateColor", null, 40, { shape: Constants.SHAPE_CIRCLE, toolTipField: "payrollAttestStateName", showIconField: "payrollAttestStateColor", enableHiding: false, hide: false });
        this.gridAg.detailOptions.addColumnNumber("invoicedAmount", this.terms["common.customer.invoices.amounttoinvoice"], null, {
            enableHiding: true, decimals: 2,
            cellClassRules: {
                "text-right": () => true,
                "errorRow": (gridRow: any) => gridRow.data.invoicedAmount > 0 && gridRow.data.invoicedAmount < gridRow.data.amountExVat,
            }
        });
        if (this.hasCurrencyPermission)
            this.gridAg.detailOptions.addColumnNumber("invoicedAmountCurrency", this.terms["common.customer.invoices.currencyamounttotransfer"], null, {
                enableHiding: true, decimals: 2,
                cellClassRules: {
                    "text-right": () => true,
                    "errorRow": (gridRow: any) => gridRow.data.invoicedAmountCurrency > 0 && gridRow.data.invoicedAmountCurrency < gridRow.data.amountCurrency,
                }
            });

        this.gridAg.detailOptions.addColumnIcon("noteIcon", "", null, { onClick: this.showNote.bind(this), suppressExport: true });

        this.gridAg.addColumnNumber("rowNr", this.terms["billing.productrows.rownr"], 20, { pinned: "left", enableHiding: false, editable: false });
        this.gridAg.addColumnIcon("rowTypeIcon", null, 50, { pinned: "left", editable: false });
        this.gridAg.addColumnText("invoiceNr", this.terms["common.customer.invoices.ordernr"], 50, true, { pinned: "left", enableRowGrouping: true, enableHiding: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => row.invoiceId && this.hasEditOrderPermission, callback: this.openOrder.bind(this) } });
        this.gridAg.addColumnText("description", this.terms["billing.order.origindescription"], 50, true, { pinned: "left", enableRowGrouping: true,  enableHiding: true, hide: true });
        this.gridAg.addColumnText("project", this.terms["billing.project.project"], 50, true, { pinned: "left", enableRowGrouping: true, enableHiding: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => row.projectId && this.hasEditProjectPermission, callback: this.openProject.bind(this) } });
        this.gridAg.addColumnText("customer", this.terms["common.customer"], 50, true, { pinned: "left", enableRowGrouping: true, enableHiding: true });
        this.gridAg.addColumnText("ediTextValue", this.terms["common.customer.invoices.edi"], 20, true, { enableRowGrouping: true, enableHiding: true });
        this.gridAg.addColumnText("productNr", this.terms["common.customer.invoices.productnr"], 50, true, { enableRowGrouping: true, enableHiding: true });
        this.gridAg.addColumnText("text", this.terms["common.customer.invoices.productname"], 50, true, { enableRowGrouping: true, enableHiding: true });
        this.gridAg.addColumnNumber("quantity", this.terms["common.customer.invoices.quantity"], 50, { enableHiding: true, aggFuncOnGrouping: 'sum' });
        this.gridAg.addColumnText("productUnitCode", this.terms["common.customer.invoices.unit"], 50, true, { enableHiding: true });
        if (this.hasSalesPricePermission) {
            this.gridAg.addColumnNumber("amountCurrency", this.terms["common.customer.invoices.price"], 50, { enableHiding: true, decimals: 2, maxDecimals: 4, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("discountValue", this.terms["common.customer.invoices.discount"], 50, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnText("discountTypeText", this.terms["billing.order.discounttype"], 50, true, { enableHiding: true });
            this.gridAg.addColumnNumber("sumAmountCurrency", this.terms["common.customer.invoices.sum"], 50, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });
        }
        if (this.hasPurchasePricePermission) {
            this.gridAg.addColumnNumber("purchasePriceCurrency", this.terms["billing.productrows.purchaseprice"], 50, { enableHiding: true, decimals: 2, maxDecimals: 4, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("purchasePriceSum", this.terms["billing.productrows.purchasepricesum"], 50, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });

            if (this.hasSalesPricePermission) {
                this.gridAg.addColumnNumber("marginalIncomeCurrency", this.terms["billing.productrows.marginalincome.short"], 50, {
                    enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum',
                    cellClassRules: {
                        "text-right": () => true,
                        "errorRow": (gridRow: any) => gridRow && gridRow.data && gridRow.data.marginalIncomeLimit && gridRow.data.marginalIncomeLimit < 0 && gridRow.data.sumAmountCurrency > 0,
                        "deleted": () => false,
                    } });
                this.gridAg.addColumnNumber("marginalIncomeRatio", this.terms["billing.productrows.marginalincomeratio.short"], 50, {
                    enableHiding: true, decimals: 2,
                    cellClassRules: {
                        "text-right": () => true,
                        "errorRow": (gridRow: any) => gridRow && gridRow.data && gridRow.data.marginalIncomeLimit && gridRow.data.marginalIncomeLimit < 0 && gridRow.data.sumAmountCurrency > 0,
                        "deleted": () => false,
                    } });
            }
        }
        this.gridAg.addColumnDate("parsedDate", this.terms["common.date"], 50, true, null, { enableHiding: true, enableRowGrouping: true, pinned: 'right' });
        this.gridAg.addColumnSelect("attestStateNames", "", 20, {
            populateFilterFromGrid: true, pinned: 'right',
            toolTipField: "attestStateName", displayField: "attestStateName", selectOptions: this.attestStates, shape: Constants.SHAPE_CIRCLE, shapeValueField: "attestStateColor", colorField: "attestStateColor", ignoreTextInFilter: true, enableHiding: true
        });

        const defs = this.gridAg.options.getColumnDefs();
        _.forEach(defs, (colDef: uiGrid.IColumnDef) => {
            if (colDef.field !== "isModified" &&
                colDef.field !== "rowNr" &&
                colDef.field !== "text" &&
                colDef['soeType'] !== Constants.GRID_COLUMN_TYPE_ICON &&
                colDef['soeType'] !== Constants.GRID_COLUMN_TYPE_SHAPE) {
                colDef['collapseOnTextRow'] = true;
                colDef['collapseOnPageBreakRow'] = true;
                if (colDef.field !== "sumAmountCurrency")
                    colDef['collapseOnSubTotalRow'] = true;
            }
        });

        /*_.forEach(defs, (colDef: any) => {
            // Add strike through on deleted or processed rows
            // If a cell class function is alredy added, we can't add the class since it will break the added function,
            // therefore in all places above where functions are added, the strike through class must be added there also.
            if (!colDef.cellClass || !angular.isFunction(colDef.cellClass)) {
                var cellClass: string = colDef.cellClass ? colDef.cellClass.toString() : "";
                colDef.cellClass = (params) => {
                    const { data } = params;
                    return cellClass + (data.state == SoeEntityState.Deleted ? " deleted" : "");
                };
            }
        });*/

        // Setup events
        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: any) => {
            this.selectionChanged(row);
            this.summarize();
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: any) => {
            this.selectionChanged(row);
            this.summarize();
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: any[]) => {
            this.summarize();
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.IsRowMaster, (row) => {
            return row ? (row.isTimeProjectRow || row.productRowType === SoeProductRowType.ExpenseRow) : false;
        }));
        this.gridAg.options.subscribe(events);

        this.gridAg.options.useGrouping(true, true, {keepColumnsAfterGroup: false, selectChildren: true});

        this.gridAg.options.setSingelValueConfiguration([
            { field: "text", predicate: (data) => data ? data.type === SoeInvoiceRowType.TextRow : false, editable: false, spanTo: this.getSingleValueSpan()},
            { field: "text", predicate: (data) => data ? data.type === SoeInvoiceRowType.PageBreakRow : false, editable: false, cellClass: "bold", spanTo: this.getSingleValueSpan() },
            {
                field: "text",
                predicate: (data) => data ? data.type === SoeInvoiceRowType.SubTotalRow : false,
                editable: false,
                cellClass: "bold",
                cellRenderer: (data, value) => {
                    const sum = data["sumAmountCurrency"] || "";
                    return "<span class='pull-left' style='width:150px'>" + value + "</span><span class='pull-right' style='padding-left:5px;padding-right:2px;margin-right:-2px;background-color:#FFFFFF;'>" + NumberUtility.printDecimal(sum, 2) + "</span>";
                },
                spanTo: "attestStateNames"
            },
        ], true);

        this.gridAg.finalizeInitGrid("billing.order.handlebilling.periodinvoicing", true);
    }

    private getSingleValueSpan(): string {
        if (this.hasPurchasePricePermission) {
            return (this.hasSalesPricePermission ? "marginalIncomeRatio" : "purchasePriceSum");
        }
        else {
            return (this.hasSalesPricePermission ? "sumAmountCurrency" : "productUnitCode")
        }
    }

    private summarize() {
        this.$timeout(() => {
            this.filteredValidForInvoice = 0;
            this.filteredTotal = 0;
            _.forEach(this.gridAg.options.getFilteredRows(), (y: any) => {
                if (y) {
                    if (y.validForInvoice)
                        this.filteredValidForInvoice += y.sumAmountCurrency;
                    this.filteredTotal += y.sumAmountCurrency;
                }
            });
        });
    }

    private selectionChanged(row: any) {
        this.hasSelectedTimeRows = _.filter(this.gridAg.options.getSelectedRows(), (r) => r.isTimeProjectRow).length === 1;
    }

    private openProject(row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_EDITPROJECT, {
            id: row.projectId,
            name: this.terms["billing.project.project"] + " " + row.projectNr
        });
    }

    private openOrder(row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_ORDER, {
            id: row.invoiceId,
            name: this.terms["common.order"] + " " + row.invoiceNr
        });
    }

    private showNote(row: any) {

        // Show edit note dialog

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/TimeProjectReport/Views/editNote.html"),
            controller: EditNoteController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: this.useExtendedTimeRegistration ? 'xl' : 'lg',
            windowClass: '',
            resolve: {
                rows: () => { return null },
                row: () => { return row },
                isReadonly: () => { return false },
                workTimePermission: () => { return this.hasWorkTimePermission },
                invoiceTimePermission: () => { return this.hasInvoiceTimePermission },
                saveDirect: () => { return true }
            }
        }

        this.$uibModal.open(options);
    }

    private showTimeRows() {
        const rowsToShow = _.filter(this.gridAg.options.getSelectedRows(), (r) => r.isTimeProjectRow);
        const row = rowsToShow[0];

        if (row) {
            const isReadonly = (row.attestStateId !== this.initialAttestState.attestStateId);
            this.progress.startLoadingProgress([() => {
                return this.coreService.getCustomerInvoiceRows(row.invoiceId).then((x) => {
                    const timeRowsHelper = new TimeRowsHelper(this.guid, this.$q, this.$uibModal, this.$scope, this.messagingService, this.urlHelperService, this.translationService, this.orderService, this.coreService, row.invoiceId, row.customerInvoiceRowId);
                    timeRowsHelper.showTimeRowsAlternate(row.productId, isReadonly, x).then((result) => {
                        if (timeRowsHelper.reloadInvoiceAfterClose) {
                            this.searchCustomerInvoiceRows();
                        }
                    });
                });
            }]);
        }
    }

    private loadProjectTimeBlocks(params: any) {
        if (!params.data['rowsLoaded']) {
            if (params.data.isTimeProjectRow) {
                this.progress.startLoadingProgress([() => {
                    return this.orderService.getProjectTimeBlocksForInvoiceRow(params.data.invoiceId, params.data.customerInvoiceRowId, null, null).then((x) => {
                        const rows = x.map(dto => {
                            const obj = new ProjectTimeBlockDTO();
                            angular.extend(obj, dto);
                            if (obj.date)
                                obj.date = CalendarUtility.convertToDate(obj.date);
                            if (obj.startTime)
                                obj.startTime = CalendarUtility.convertToDate(obj.startTime);
                            if (obj.stopTime)
                                obj.stopTime = CalendarUtility.convertToDate(obj.stopTime);
                            return obj;
                        });

                        params.data['rows'] = rows;
                        params.data['rowsLoaded'] = true;

                    });
                }]).then(() => {
                    params.successCallback(params.data['rows']);
                });
            }
            else if (params.data.productRowType === SoeProductRowType.ExpenseRow) {
                // Hide time row columns
                this.gridAg.detailOptions.hideColumn("guantityFormatted");
                this.gridAg.detailOptions.hideColumn("timeCodeName");
                this.gridAg.detailOptions.hideColumn("from");
                this.gridAg.detailOptions.hideColumn("amount");
                this.gridAg.detailOptions.hideColumn("amountExVat");
                this.gridAg.detailOptions.hideColumn("amountCurrency");
                this.gridAg.detailOptions.hideColumn("payrollAttestStateColor");
                this.gridAg.detailOptions.hideColumn("invoicedAmount");
                this.gridAg.detailOptions.hideColumn("invoicedAmountCurrency");

                this.orderService.getExpenseRows(params.data.invoiceId, params.data.customerInvoiceRowId).then((rows) => {
                    var rows = rows;
                    _.forEach(rows, (r) => {
                        if (r.timeCodeRegistrationType === TermGroup_TimeCodeRegistrationType.Time)
                            r['guantityFormatted'] = CalendarUtility.minutesToTimeSpan(r.quantity);
                        else
                            r['guantityFormatted'] = NumberUtility.printDecimal(r.quantity, 2);
                    });

                    params.data['rows'] = rows;
                    params.data['rowsLoaded'] = true;
                }).then(() => {
                    params.successCallback(params.data['rows']);
                });
            }
        }
        else {
            params.successCallback(params.data['rows']);
        }
    }

    private searchCustomerInvoiceRows() {
        this.handleBillingRows = [];

        this.progress.startLoadingProgress([() => {
            return this.orderService.searchCustomerInvoiceRows(_.map(this.selectedProjects, 'id'), _.map(this.selectedOrders, 'id'), _.map(this.selectedCustomers, 'id'), _.map(this.selectedOrderTypes, 'id'), _.map(this.selectedOrderContractTypes, 'id'), this.selectedDateFrom, this.selectedDateTo, this.onlyValidToTransfer, this.loadOnlyMine).then((x: HandleBillingRowDTO[]) => {
                this.handleBillingRows = x;
                this.handleBillingRows.forEach((row) => {
                    row.ediTextValue = row.ediEntryId ? this.terms["core.yes"] : this.terms["core.no"];
                    row.ediTextValue = row.ediEntryId ? this.terms["core.yes"] : this.terms["core.no"];

                    if (row.discountType === SoeInvoiceRowDiscountType.Percent) {
                        row['discountTypeText'] = "%";
                        row['discountValue'] = row.discountPercent;
                    }
                    else {
                        row['discountTypeText'] = row.currencyCode;
                        row['discountValue'] = row.discountAmount;
                    }

                    if (row.productRowType === SoeProductRowType.TimeBillingRow) {
                        row['rowTypeIcon'] = 'fal fa-file-invoice-dollar';
                    }
                    else if (row.isTimeProjectRow) {
                        row['rowTypeIcon'] = 'fal fa-clock';
                    }
                    else if (row.productRowType === SoeProductRowType.ExpenseRow) {
                        row['rowTypeIcon'] = 'fal fa-wallet';
                    }
                    else {
                        switch (row.type) {
                            case SoeInvoiceRowType.ProductRow:
                                row['rowTypeIcon'] = 'fal fa-box-alt';
                                break;
                            case SoeInvoiceRowType.TextRow:
                                row['rowTypeIcon'] = 'fal fa-text';
                                break;
                            case SoeInvoiceRowType.PageBreakRow:
                                row['rowTypeIcon'] = 'fal fa-cut';
                                break;
                            case SoeInvoiceRowType.SubTotalRow:
                                row['rowTypeIcon'] = 'fal fa-calculator-alt';
                                break;
                        }
                    }

                    row['parsedDate'] = (row.date) ? new Date(<any>row.date).date() : row['parsedDate'] = undefined;
                    
                    row['expander'] = "";
                    row["isSelected"] = false;
                });

                // Reset visibility
                this.gridAg.options.showColumn('soe-ag-single-value-column');

                this.setData(this.handleBillingRows);
            });
        }]);
    }

    private initTransfer(option) {
        var originStatusChange: SoeOriginStatusChange;
        var merge: boolean = false;
        var checkPartialInvoicing: boolean = false;
        var validatedItems: any = [];
        var validMessage: string = "";
        var invalidMessage: string = "";
        var successMessage: string = "";
        var errorMessage: string = "";

        // Get selected rows
        var selectedItems = this.gridAg.options.getSelectedRows();

        switch (option.id) {
            case CustomerInvoiceGridButtonFunctions.TransferToPreliminarInvoice:
                _.forEach(selectedItems, (row: HandleBillingRowDTO) => {
                    if (row.validForInvoice && !_.includes(validatedItems, row.invoiceId)) 
                        validatedItems.push(row.invoiceId);
                });
                
                //MESSAGES MISSING
                validMessage += this.terms["common.customer.invoice.willtransferorder2invoice"]; 
                validMessage += "\n" + this.terms["billing.order.transferallrowsinfo"];
                invalidMessage += this.terms["billing.order.invalidstatefortransfertoinvoice"];
                errorMessage = "";
                originStatusChange = SoeOriginStatusChange.Billing_OrderToInvoice;
                break;
            case CustomerInvoiceGridButtonFunctions.TransferToInvoiceAndMergeOrders:
                _.forEach(selectedItems, (row: HandleBillingRowDTO) => {
                    if (row.validForInvoice && !_.includes(validatedItems, row.invoiceId))
                        validatedItems.push(row.invoiceId);
                });

                //MESSAGES MISSING
                validMessage += this.terms["common.customer.invoice.willtransferorder2invoice"];
                validMessage += "\n" + this.terms["billing.order.transferallrowsinfo"];
                invalidMessage += this.terms["billing.order.invalidstatefortransfertoinvoice"];
                errorMessage = "";
                originStatusChange = SoeOriginStatusChange.Billing_OrderToInvoice;
                merge = true;
                break;
            case CustomerInvoiceGridButtonFunctions.TransferToInvoiceAndPrint:
                _.forEach(selectedItems, (row: HandleBillingRowDTO) => {
                    if (row.validForInvoice && !_.includes(validatedItems, row.invoiceId))
                        validatedItems.push(row.invoiceId);
                });

                //MESSAGES MISSING
                validMessage += this.terms["common.customer.invoice.willtransferorder2invoice"];
                validMessage += "\n" + this.terms["billing.order.transferallrowsinfo"];
                invalidMessage += this.terms["billing.order.invalidstatefortransfertoinvoice"];
                errorMessage = "";
                originStatusChange = SoeOriginStatusChange.Billing_OrderToInvoiceAndPrint;
                break;
            case CustomerInvoiceGridButtonFunctions.SplitTimeRows:
                this.initSplitTimeRows();
                return;
        }

        if ((!validatedItems) || (validatedItems.length < 1))
            return;

        var title: string = "";
        var text: string = "";
        var image: SOEMessageBoxImage = SOEMessageBoxImage.None;
        var buttons: SOEMessageBoxButtons = SOEMessageBoxButtons.None;

        var noOfValid: number = validatedItems.length;
        var noOfInvalid = selectedItems.length - validatedItems.length;

        if (selectedItems.length === validatedItems.length) {

            title = this.terms["core.verifyquestion"];

            text += noOfValid.toString() + " " + validMessage + "<br\>";
            text += this.terms["core.continue"];

            image = SOEMessageBoxImage.Question;
            buttons = SOEMessageBoxButtons.OKCancel;
        }
        else if (selectedItems.length > validatedItems.length) {
            if (noOfValid === 0) {
                title = this.terms["core.warning"];

                text += noOfInvalid.toString() + " " + invalidMessage + "<br\>";

                image = SOEMessageBoxImage.Warning;
                buttons = SOEMessageBoxButtons.OK;
            }
            else {
                title = this.terms["core.verifyquestion"];

                text += noOfInvalid.toString() + " " + invalidMessage + "<br\>";
                text += noOfValid.toString() + " " + validMessage + "<br\>";
                text += this.terms["core.continue"];

                image = SOEMessageBoxImage.Question;
                buttons = SOEMessageBoxButtons.OKCancel;
            }
        }

        var modal = this.notificationService.showDialog(title, text, image, buttons);
        modal.result.then(val => {
            if (val != null && val === true) {
                if (errorMessage && errorMessage != "")
                    errorMessage = validatedItems.length.toString() + " " + errorMessage;
                this.transfer(validatedItems, originStatusChange, merge, errorMessage, successMessage);
            };
        });
    }

    private transfer(validatedItems: number[], originStatusChange: SoeOriginStatusChange, merge: boolean, errorMessage: string, succesMessage: string) {
        this.progress.startSaveProgress((completion) => {
            this.orderService.transferOrdersToInvoice(validatedItems, soeConfig.accountYearId, merge, false).then((result) => {
                if (result.success) {
                    completion.completed(null, null, true);
                }
                else {
                    if (result.errorNumber && result.errorNumber > 0) {
                        if (errorMessage)
                            completion.failed(errorMessage + "\n" + result.errorMessage);
                        else
                            completion.failed(result.errorMessage);
                    }
                    else
                        completion.failed(errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null)
            .then(data => {
                this.searchCustomerInvoiceRows();
            }, error => {
            });
    }

    private initSplitTimeRows() {
        var noOfValid: number = 0;
        var noOfInvalid: number = 0;
        var validItems: any = [];
        var selectedItems = this.gridAg.options.getSelectedRows();
        _.forEach(selectedItems, (row: HandleBillingRowDTO) => {
            if (row.isTimeProjectRow) {
                if (row.attestStateId === this.initialAttestState.attestStateId) {
                    validItems.push({ field1: row.invoiceId, field2: row.customerInvoiceRowId });
                    noOfValid++;
                }
                else {
                    noOfInvalid++;
                }
            }
        });

        var message = (noOfValid === 1 ? this.terms["billing.order.validsplitsingle"] : noOfValid + " " + this.terms["billing.order.validsplitmultiple"]) + "\n";
        if(noOfInvalid > 0)
            message += noOfInvalid === 1 ? this.terms["billing.order.invalidsplitstatesingle"] : noOfInvalid + " " + this.terms["billing.order.invalidsplitstatemultiple"];

        var modal = this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, noOfValid > 0 ? SOEMessageBoxButtons.OKCancel : SOEMessageBoxButtons.OK);
        modal.result.then(val => {
            if (val != null && val === true) {
                this.splitTimeRows(validItems, undefined);
            };
        });
    }

    private splitTimeRows(timeRows: any[], errorMessage: string) {
        this.progress.startSaveProgress((completion) => {
            this.orderService.batchSplitTimeRows(timeRows, this.selectedDateFrom, this.selectedDateTo).then((result) => {
                if (result.success) {
                    completion.completed(null, null, true);
                }
                else {
                    if (result.errorNumber && result.errorNumber > 0) {
                        if (errorMessage)
                            completion.failed(errorMessage + "\n" + result.errorMessage);
                        else
                            completion.failed(result.errorMessage);
                    }
                    else
                        completion.failed(errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null)
            .then(data => {
                this.searchCustomerInvoiceRows();
            }, error => {
            });
    }

    private initChangeAttestState() {
        if (!this.selectedAttestState)
            return;

        // Get selected attest state
        var attestState: AttestStateDTO = _.find(this.attestStates, a => a.attestStateId === this.selectedAttestState);

        var errorMessage: string = '';
        var guaranteeStockErrorFound: boolean = false;
        var validRows: HandleBillingRowDTO[] = [];

        var selectedRows: HandleBillingRowDTO[] = this.gridAg.options.getSelectedRows();
        var selectedIds: number[] = this.gridAg.options.getSelectedIds('customerInvoiceRowId');

        var groups = _.groupBy(selectedRows, 'invoiceId');
        var keys = Object.keys(groups);
        for (let i = 0; i < keys.length && !guaranteeStockErrorFound; i++) {
            _.forEach(groups[keys[i]], row => {
                var valid: boolean = true;
                if (row.productId === this.productGuaranteeId && attestState.attestStateId !== this.initialAttestState.attestStateId) {
                    var openLiftRows = _.filter(selectedRows, r => r.attestStateId === this.initialAttestState.attestStateId && r.productCalculationType === TermGroup_InvoiceProductCalculationType.Lift);
                    // If guarantee product, all lift products must have been transfered first (or being transferered now)
                    if (openLiftRows.length > 0) {
                        // If not all of the open lift rows are selected then show error
                        var openLiftRowIds: number[] = _.map(openLiftRows, r => r.customerInvoiceRowId);
                        // Remove selected ids from list of all open ids 
                        _.pullAll(openLiftRowIds, selectedIds);
                        // If any open ids remains, it means that not all open rows are selected
                        if (openLiftRowIds.length > 0) {
                            errorMessage += this.terms["billing.productrows.changeatteststate.errorlift"] + '\n';
                            guaranteeStockErrorFound = true;
                            valid = false;
                        }
                    }
                }

                if (!guaranteeStockErrorFound && attestState.attestStateId == this.attestStateReadyId && row.isStockRow && row.invoiceQuantity == 0 && this.usePartialInvoicingOnOrderRow) {
                    errorMessage += this.terms["billing.productrows.changeatteststate.errorstock"] + '\n';
                    guaranteeStockErrorFound = true;
                    valid = false;
                }

                if (!guaranteeStockErrorFound && _.filter(this.attestTransitions, (a) => a.attestStateFromId === row.attestStateId && attestState.attestStateId === a.attestStateToId).length === 0) {
                    errorMessage += this.terms["common.customer.invoices.row"] + " " + row.rowNr + " " + this.terms["common.customer.invoices.wrongstatetotransfer"] + " " + attestState.name + '\n';
                    valid = false;
                }

                if (valid)
                    validRows.push(row);
            });
        }

        if (guaranteeStockErrorFound) {
            this.notificationService.showDialogEx(this.terms["billing.productrows.changeatteststate.errortitle"], errorMessage, SOEMessageBoxImage.Error);
            return false;
        }
        else {
            var message = "";
            var modal = undefined;
            if (validRows.length === 0) {
                this.notificationService.showDialogEx(this.terms["billing.productrows.changeatteststate.errortitle"], this.terms["billing.order.allselectedinvalid"] + " " + attestState.name, SOEMessageBoxImage.Error);
                return false;
            }
            else if (validRows.length !== selectedRows.length) {
                message += ((selectedRows.length - validRows.length) === 1 ? this.terms["billing.order.invalidchangestatesingle"] : (selectedRows.length - validRows.length) + " " + this.terms["billing.order.invalidchangestatesmultiple"]) + " " + attestState.name + ".\n";
                message += (validRows.length === 1 ? this.terms["billing.order.validchangestatesingle"] : validRows.length + " " + this.terms["billing.order.validchangestatemultiple"]) + " " + attestState.name + ".\n";
                message += this.terms["core.continue"];

                modal = this.notificationService.showDialogEx(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val) {
                        var items: any[] = [];
                        _.forEach(validRows, (row) => {
                            items.push({ field1: row.invoiceId, field2: row.customerInvoiceRowId });
                        });

                        this.changeAttestState(items, attestState.attestStateId, this.terms["billing.order.changeatteststatefailed"].format(attestState.name));
                    }
                });
            }
            else {
                message += (validRows.length === 1 ? this.terms["billing.order.validchangestatesingle"] : validRows.length + " " + this.terms["billing.order.validchangestatemultiple"]) + " " + attestState.name + ".\n";
                message += this.terms["core.continue"];

                modal = this.notificationService.showDialogEx(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val) {
                        var items: any[] = [];
                        _.forEach(validRows, (row) => {
                            items.push({ field1: row.invoiceId, field2: row.customerInvoiceRowId });
                        });

                        this.changeAttestState(items, attestState.attestStateId, this.terms["billing.order.changeatteststatefailed"].format(attestState.name));
                    }
                });
            }
        }
    }

    private changeAttestState(items: any[], attestStateId: number, errorMessage: string) {
        this.progress.startSaveProgress((completion) => {
            this.orderService.changeAttestStateOnOrderRows(items, attestStateId).then((result) => {
                if (result.success) {
                    completion.completed(null, null, true);
                }
                else {
                    if (result.errorNumber && result.errorNumber > 0) {
                        if (errorMessage)
                            completion.failed(errorMessage + "\n" + result.errorMessage);
                        else
                            completion.failed(result.errorMessage);
                    }
                    else
                        completion.failed(errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null)
            .then(data => {
                this.searchCustomerInvoiceRows();
            }, error => {
            });
    }

    private orderSelectionComplete() {
        var selected: any[] = [];
        var notSelected: any[] = [];
        if (this.selectedOrders.length > 0) {
            for (let o of this.orders) {
                if (_.filter(this.selectedOrders, x => x.id === o.id).length > 0)
                    selected.push(o);
                else
                    notSelected.push(o);
            }
            this.orders = selected.concat(notSelected);
        }
    }

    private projectSelectionComplete() {
        var selected: any[] = [];
        var notSelected: any[] = [];
        if (this.selectedProjects.length > 0) {
            for (let o of this.projects) {
                if (_.filter(this.selectedProjects, x => x.id === o.id).length > 0)
                    selected.push(o);
                else
                    notSelected.push(o);
            }
            this.projects = selected.concat(notSelected);
        }
    }

    private customerSelectionComplete() {
        var selected: any[] = [];
        var notSelected: any[] = [];
        if (this.selectedCustomers.length > 0) {
            for (let o of this.customers) {
                if (_.filter(this.selectedCustomers, x => x.id === o.id).length > 0)
                    selected.push(o);
                else
                    notSelected.push(o);
            }
            this.customers = selected.concat(notSelected);
        }
    }

    private orderTypesSelectionComplete() {
        var selected: any[] = [];
        var notSelected: any[] = [];
        if (this.selectedOrderTypes.length > 0) {
            for (let o of this.orderTypes) {
                if (_.filter(this.selectedOrderTypes, x => x.id === o.id).length > 0)
                    selected.push(o);
                else
                    notSelected.push(o);
            }
            this.orderTypes = selected.concat(notSelected);
        }
    }

    private orderContractTypesSelectionComplete() {
        var selected: any[] = [];
        var notSelected: any[] = [];
        if (this.selectedOrderContractTypes.length > 0) {
            for (let o of this.orderContractTypes) {
                if (_.filter(this.selectedOrderContractTypes, x => x.id === o.id).length > 0)
                    selected.push(o);
                else
                    notSelected.push(o);
            }
            this.orderContractTypes = selected.concat(notSelected);
        }
    }

    private decreaseDate() {
        this.selectedDateFrom = this.selectedDateFrom.addDays(- 1);
    }

    private increaseDate() {
        this.selectedDateTo = this.selectedDateTo.addDays(1);
    }
}
