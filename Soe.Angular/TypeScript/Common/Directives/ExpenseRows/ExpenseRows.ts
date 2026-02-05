import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup, ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { SoeGridOptionsEvent, IconLibrary, TimeProjectSearchFunctions, SOEMessageBoxSize } from "../../../Util/Enumerations";
import { Feature, TermGroup_TimeCodeRegistrationType, TermGroup_ExpenseType, CompanySettingType, TermGroup, SoeCategoryType, TermGroup_AttestEntity, UserSettingType, SettingMainType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Guid } from "../../../Util/StringUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { AddExpenseDialogController } from "../../../Common/Directives/AddExpense/AddExpenseDialogController";
import { IOrderService } from "../../../Shared/Billing/Orders/OrderService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IProjectService } from "../../../Shared/Billing/Projects/ProjectService";
import { IActionResult, IEmployeeTimeCodeDTO } from "../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { ExpenseRowGridDTO } from "../../../Common/Models/ExpenseDTO";
import { IColumnAggregations } from "../../../Util/SoeGridOptionsAg";
import { TimeCodeAdditionDeductionDTO } from "../../../Common/Models/TimeCode";
import { NumberUtility } from "../../../Util/NumberUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { SmallGenericType } from "../../Models/SmallGenericType";
import { AttestStateDTO } from "../../Models/AttestStateDTO";
import { AttestPayrollTransactionDTO } from "../../Models/AttestPayrollTransactionDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { TimePayrollUtility } from "../../../Util/TimePayrollUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";

class ExpenseRowController extends GridControllerBase2Ag implements ICompositionGridController {
    guid: Guid;
    readOnly: boolean;
    projectId?: number;
    employeeId?: number;
    customerInvoiceId: number;
    currencyCode: string;
    isBaseCurrency: boolean = false;
    priceListTypeInclusiveVat: boolean = false;

    terms: { [index: string]: string; };

    // Sums
    sumExpenses: number = 0;
    sumExpensesInvoiced: number = 0;

    // Lookups
    private fromDate: Date;
    private toDate: Date;
    private employee: IEmployeeTimeCodeDTO;
    private employees: IEmployeeTimeCodeDTO[] = [];
    private employeesDict: any[] = [];
    private timeCodes: TimeCodeAdditionDeductionDTO[] = [];
    private timeCodesDict: any[] = [];
    private customers: SmallGenericType[] = [];
    private weekdays: SmallGenericType[] = [];
    private allProjects: any[] = [];
    private projectInvoices: any[] = [];
    private allProjectsAndInvoices: any[] = [];
    private allOrders: any[] = [];
    private includeExpenseInReportItems: any[] = [];

    // settings
    private expenseReportId: 0;
    private projectLimitOrderToProjectUsers = false;
    private defaultTimeCodeId = 0;
    private attestStateTransferredOrderToInvoiceId = 0;
    private useExtendedTimeRegistration = false;
    private userSettingTimeAttestDisableSaveAttestWarning = false;

    // Permissions
    private hasCurrencyPermission: boolean = false;
    private editProjectPermission = false;
    private editOrderPermission = false;
    private editCustomerPermission = false;
    private modifyOtherEmployeesPermission = false;

    //Attest
    private userValidPayrollAttestStates: AttestStateDTO[] = [];
    private userValidPayrollAttestStatesOptions: any = [];

    // Rows
    expenseRows: ExpenseRowGridDTO[];
    private includeExpenseInReport: number;

    // ToolBar
    private toolbarInclude: any;
    protected buttonGroups = new Array<ToolBarButtonGroup>();

    // Properties
    get isOrderMode() {
        return (!!(this.customerInvoiceId && this.customerInvoiceId > 0));
    }

    get isAttestDisabled() {
        return !(this.expenseRows && (this.expenseRows.length > 0) &&(this.gridAg.options.getSelectedRows()) &&
            this.userValidPayrollAttestStatesOptions &&
            this.userValidPayrollAttestStatesOptions.length > 0);
    }

    // Filters
    private filteredEmployeeCategoriesDict: any[] = [];
    private selectedEmployeeCategoryDict: any[] = [];
    private selectedEmployeesDict: any[] = [];
    private filteredProjectsDict: any[] = [];
    private selectedProjectsDict: any[] = [];
    private selectedOrdersDict: any[] = [];
    private filteredOrdersDict: any[] = [];

    //Functions
    searchButtonFunctions: any = [];
    buttonFunctions: any = [];

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModal,
        private $window: ng.IWindowService,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private orderService: IOrderService,
        private projectService: IProjectService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $scope: ng.IScope) {

        super(gridHandlerFactory, "Common.Directives.ExpenseRows", progressHandlerFactory, messagingHandlerFactory);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => {
                if(this.isOrderMode)
                    this.loadGridData()
            });

        this.onInit({});
    }

    onInit(parameters: any) {
        this.parameters = parameters;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start([
            { feature: Feature.Billing_Order_Orders_Edit_Expenses, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Time_Time_TimeSheetUser_OtherEmployees, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Customer_Invoice_Status_Foreign, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Project_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_Orders_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Customer_Customers_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Project_TimeSheetUser_OtherEmployees, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Time_Time_TimeSheetUser_OtherEmployees, loadReadPermissions: true, loadModifyPermissions: true },
        ]);

        // Set intial values
        this.employeeId = soeConfig.employeeId;
        this.fromDate = CalendarUtility.getDateToday().beginningOfWeek();
        this.toDate = CalendarUtility.getDateToday().endOfWeek();
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Order_Orders_Edit_Expenses].readPermission;
        this.modifyPermission = response[Feature.Billing_Order_Orders_Edit_Expenses].modifyPermission;
        this.hasCurrencyPermission = response[Feature.Economy_Customer_Invoice_Status_Foreign].readPermission;
        this.modifyOtherEmployeesPermission = (response[Feature.Billing_Project_TimeSheetUser_OtherEmployees].modifyPermission || response[Feature.Time_Time_TimeSheetUser_OtherEmployees].modifyPermission);
        this.editProjectPermission = response[Feature.Billing_Project_Edit].modifyPermission;
        this.editOrderPermission = response[Feature.Billing_Order_Orders_Edit].modifyPermission;
        this.editCustomerPermission = response[Feature.Billing_Customer_Customers_Edit].modifyPermission;

        if (this.modifyPermission) {
            // Send messages to TabsController
            this.messagingHandler.publishActivateAddTab();
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.$timeout(() => {
            if (this.isOrderMode) {
                this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.customer.invoices.newexpenserow", "common.customer.invoices.newexpenserowtooltip", IconLibrary.FontAwesome, "fa-plus",
                    () => { this.edit(null); },
                    null,
                    () => { return this.readOnly; })));

                this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("", "core.print", IconLibrary.FontAwesome, "fa-print",
                    () => { this.print(); },
                    () => { return !this.expenseReportId },
                    () => { return false; })));
            }
            else {
                this.buttonGroups.push(ToolBarUtility.createGroup(ToolBarUtility.createClearFiltersButton(() => {
                    this.gridAg.options.clearFilters();
                })));

                this.toolbarInclude = this.urlHelperService.getGlobalUrl("Common/Directives/ExpenseRows/Views/gridHeader.html");
            }
        });
    }

    private doLookups(): ng.IPromise<any> {
        if (this.isOrderMode) {
            return this.$q.all([
                this.loadCompanySettings(),
                this.loadIncludeExpenseInReportItems()
            ]);
        }
        else {
            return this.$q.all([
                this.loadCompanySettings(),
                this.loadUserSettings(),
                this.loadCustomers(),
                this.loadEmployee().then(() => {
                    this.$q.all([
                        this.loadTimeCodes(),
                        this.loadWeekdays(),]).then(() => {
                            this.$q.all([this.loadEmployees()]).then(() => {
                                this.setButtonFunctions();
                                //this.gridAndDataIsReady();
                            });
                        });
                })]);
        }
    }

    private doLookupsForEdit(): ng.IPromise<any> {
        return this.$q.all([
            this.loadEmployee(),
            this.loadTimeCodes(),
            this.loadEmployees()
        ]);
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.BillingDefaultExpenseReportTemplate);
        settingTypes.push(CompanySettingType.ProjectLimitOrderToProjectUsers);
        settingTypes.push(CompanySettingType.TimeDefaultTimeCode);
        settingTypes.push(CompanySettingType.BillingStatusTransferredOrderToInvoice);
        settingTypes.push(CompanySettingType.ProjectUseExtendedTimeRegistration);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.expenseReportId = x[CompanySettingType.BillingDefaultExpenseReportTemplate];
            this.projectLimitOrderToProjectUsers = x[CompanySettingType.ProjectLimitOrderToProjectUsers];
            this.defaultTimeCodeId = x[CompanySettingType.TimeDefaultTimeCode];
            this.attestStateTransferredOrderToInvoiceId = x[CompanySettingType.BillingStatusTransferredOrderToInvoice];
            this.useExtendedTimeRegistration = x[CompanySettingType.ProjectUseExtendedTimeRegistration];
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.TimeDisableApplySaveAttestWarning];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.userSettingTimeAttestDisableSaveAttestWarning = SettingsUtility.getBoolUserSetting(x, UserSettingType.TimeDisableApplySaveAttestWarning);
        });
    }

    private loadCustomers(): ng.IPromise<any> {
        return this.projectService.getCustomersDict(true, true).then((x) => {
            this.customers = x;
        });
    }

    private loadEmployee(): ng.IPromise<any> {
        return this.projectService.getEmployeeForUserWithTimeCode(CalendarUtility.getDateToday()).then(x => {
            this.employee = x;
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.coreService.getAdditionDeductionTimeCodes(this.isOrderMode).then(x => {
            this.timeCodes = this.isOrderMode ? x.filter(t => t.expenseType !== TermGroup_ExpenseType.Time) : x;
            _.forEach(x, (t) => {
                this.timeCodesDict.push({ value: t.timeCodeId, label: t.name });
            });
        });
    }

    private loadWeekdays(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.StandardDayOfWeek, false, false).then(x => {
            this.weekdays = x;
            _.forEach(this.weekdays, (y) => {
                y.name = y.name.toLowerCase();
            });
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        const deferral = this.$q.defer();
        this.employees = [];
        this.employeesDict = [];

        if (this.isOrderMode) {
            return this.projectService.getEmployeesForTimeProjectRegistrationSmall(this.projectId, null, null).then(x => {
                this.employees = x;
                _.forEach(x, (e) => {
                    this.employeesDict.push({ id: e.employeeId, label: e.name + " (" + e.employeeNr + ")" });
                });
            });
        } else {
            if (this.modifyOtherEmployeesPermission) {
                const categories = this.selectedEmployeeCategoryDict.map(a => a.id);
                return this.projectService.getEmployeesForProjectTimeCode(false, false, false, this.employeeId, this.fromDate, this.toDate, categories).then((x: IEmployeeTimeCodeDTO[]) => {
                    this.employees = x;
                    x.forEach((e) => {
                        this.employeesDict.push({ id: e.employeeId, label: e.name + " (" + e.employeeNr + ")" });
                    });
                });
            } else {
                if (this.employee) {
                    this.employees.push({ employeeId: this.employee.employeeId, name: this.employee.name, employeeNr: this.employee.employeeNr, defaultTimeCodeId: this.employee.defaultTimeCodeId, timeDeviationCauseId: this.employee.timeDeviationCauseId, employeeGroupId: this.employee.employeeGroupId, autoGenTimeAndBreakForProject: this.employee.autoGenTimeAndBreakForProject });
                    this.employeesDict.push({ id: this.employee.employeeId, label: this.employee.name });
                }
                deferral.resolve();
            }
        }

        return deferral.promise;
    }

    private loadProjectInvoices(employeeIds: number[]): ng.IPromise<any[]> {
        const deferral = this.$q.defer<any[]>();

        this.projectService.getProjectsForTimeSheetEmployees(employeeIds, this.projectId).then((result: any[]) => {
            this.projectInvoices = this.projectInvoices.concat(result);

            this.allProjectsAndInvoices = [];
            this.allProjects = [];
            this.filteredProjectsDict = [];
            this.allOrders = [];
            this.filteredOrdersDict = [];

            for (let e of this.projectInvoices) {
                //Filter projects
                for (let p of e.projects) {
                    if (_.filter(this.allProjects, x => x.id === p.projectId).length === 0) {
                        this.allProjectsAndInvoices.push(p);
                        this.allProjects.push({ id: p.projectId, label: p.numberName });
                        this.filteredProjectsDict.push({ id: p.projectId, label: p.numberName })
                    }
                }
                //Filter invoices
                for (let i of e.invoices) {
                    if (_.filter(this.allOrders, x => x.id === i.invoiceId).length === 0) {
                        this.allOrders.push({ id: i.invoiceId, label: i.numberName });
                        this.filteredOrdersDict.push({ id: i.invoiceId, label: i.numberName })
                    }
                }
            }

            deferral.resolve(this.projectInvoices);
        });

        return deferral.promise;
    }

    private loadIncludeExpenseInReportItems() {
        return this.coreService.getTermGroupContent(TermGroup.IncludeExpenseInReportType, true, false).then((x: any[]) => {
            this.includeExpenseInReportItems = x.sort(function (a, b) {
                return a.id - b.id;
            });
        });
    }

    private loadAttestStates(forceUseEmployeeGroup: boolean) {
        this.userValidPayrollAttestStates = [];

        return this.coreService.getUserValidAttestStates(TermGroup_AttestEntity.PayrollTime, this.fromDate, this.toDate, true, this.modifyOtherEmployeesPermission && !forceUseEmployeeGroup ? undefined : this.employee.employeeGroupId).then((result) => {
            this.userValidPayrollAttestStates = result;
            this.userValidPayrollAttestStatesOptions.length = 0;
            _.forEach(this.userValidPayrollAttestStates, (attestState: AttestStateDTO) => {
                this.userValidPayrollAttestStatesOptions.push({ id: attestState.attestStateId, name: attestState.name });
            });
        });
    }

    private setButtonFunctions() {
        const keys: string[] = [
            "core.loading",
            "billing.order.timeproject.searchintervall",
            "billing.order.timeproject.getall",
            "core.search",
            "billing.project.timesheet.groupbydateincplannedabsence",
            "billing.project.timesheet.groupbydate",
            "common.newrow",
            "core.deleterow",
            "billing.project.timesheet.changeorder",
            "billing.project.timesheet.movetonewproductrow",
            "billing.project.timesheet.movetoexistingproductrow",
            "billing.project.timesheet.searchincplannedabsence",
            "billing.project.timesheet.changedate"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            if (!this.isOrderMode) {
                this.searchButtonFunctions.push({ id: TimeProjectSearchFunctions.SearchIntervall, name: terms["core.search"] });
            }

            // Commented out for now
            /*if (!this.readOnly && !this.isOrderRows) {
                this.buttonFunctions.push({ id: TimeProjectButtonFunctions.AddRow, name: terms["common.newrow"], icon: 'fal fa-plus' });
                this.buttonFunctions.push({ id: TimeProjectButtonFunctions.DeleteRow, name: terms["core.deleterow"], icon: 'fal fa-times iconDelete', disabled: () => { return this.readOnly || this.groupByDate || !this.hasSelectedRows } });
                this.buttonFunctions.push({ id: TimeProjectButtonFunctions.MoveRow, name: terms["billing.project.timesheet.changeorder"], icon: 'fal fa-arrow-right', disabled: () => { return this.readOnly || this.groupByDate || !this.hasSelectedRows } });
                if (this.useExtendedTimeRegistration) {
                    this.buttonFunctions.push({ id: TimeProjectButtonFunctions.ChangeDate, name: terms["billing.project.timesheet.changedate"], icon: 'fal fa-arrow-right', disabled: () => { return this.readOnly || this.groupByDate || !this.hasSelectedRows } });
                }
            }

            if (!this.readOnly && this.isOrderRows && this.splitTimeProductRowsPermission) {
                this.buttonFunctions.push({ id: TimeProjectButtonFunctions.MoveRowToNewInvoiceRow, name: terms["billing.project.timesheet.movetonewproductrow"], icon: 'fal fa-file-invoice-dollar', disabled: () => { return this.readOnly || this.groupByDate || !this.hasSelectedRows } });
                this.buttonFunctions.push({ id: TimeProjectButtonFunctions.MoveRowToExistingInvoiceRow, name: terms["billing.project.timesheet.movetoexistingproductrow"], icon: 'fal fa-file-invoice-dollar', disabled: () => { return this.readOnly || this.groupByDate || !this.hasSelectedRows } });
            }*/
        });
    }

    // Grid
    public setupGrid(): void {
        const translationKeys: string[] = [
            "common.employee",
            "billing.project.timesheet.chargingtype",
            "common.date",
            "common.timecode",
            "common.quantity",
            "common.customer.invoices.amount",
            "common.customer.invoices.amounttoinvoice",
            "common.customer.invoices.foreignamount",
            "common.customer.invoices.currencyamounttotransfer",
            "common.customer.invoices.amount",
            "common.expensetype",
            "core.edit",
            "core.delete",
            "billing.project.timesheet.employeenr",
            "common.customer.invoices.specifiedunitprice",
            "common.customer.invoices.amountexvat",
            "common.customer.invoices.order",
            "common.customer.customer.customer",
            "billing.project.projectnr",
            "common.customer.customer.orderproject",
            "common.order",
            "common.sum"
        ];

        this.translationService.translateMany(translationKeys).then(terms => {

            // Set name
            this.gridAg.options.setName("expenseRowsGrid");

            if (!this.isOrderMode) {
                this.gridAg.addColumnText("orderNr", terms["common.customer.invoices.order"], null, false, { enableRowGrouping: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => this.editOrderPermission && row.orderId > 0, callback: this.openOrder.bind(this) } });
                this.gridAg.addColumnText("customerName", terms["common.customer.customer.customer"], null, false, {
                    enableRowGrouping: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => this.editCustomerPermission && row.actorCustomerId > 0, callback: this.openCustomer.bind(this) }
                });
                this.gridAg.addColumnText("projectNr", terms["billing.project.projectnr"], null, true, { hide: true, enableRowGrouping: true });
                this.gridAg.addColumnText("projectName", terms["common.customer.customer.orderproject"], null, false, {
                    enableRowGrouping: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => this.editProjectPermission && row.projectId > 0, callback: this.openProject.bind(this) }
                });
            }
            else {
                this.gridAg.options.setMinRowsToShow(10);
            }
            this.gridAg.addColumnText("employeeNumber", terms["billing.project.timesheet.employeenr"], null, true, { hide: true, enableRowGrouping: true });
            this.gridAg.addColumnText("employeeName", terms["common.employee"], null, false, { enableRowGrouping: true, toolTipField: "columnNameTooltip"/*, cellClassRules: { "errorRow": (row: any) => row.data.employeeIsInactive }*/ });
            this.gridAg.addColumnText("timeCodeName", terms["common.expensetype"], null, false, { enableRowGrouping: true });
            var quantityColumn = this.gridAg.addColumnText("guantityFormatted", terms["common.quantity"], null, true, {});
            quantityColumn.cellClass = "text-right";
            quantityColumn.cellStyle = { 'padding-right': '5px' };
            this.gridAg.addColumnDate("from", terms["common.date"], null, true, null, { enableRowGrouping: true, toolTipField: "dateFormatted" });

            this.gridAg.addColumnNumber("amount", terms["common.customer.invoices.amount"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("amountExVat", terms["common.customer.invoices.amountexvat"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });

            if (this.hasCurrencyPermission && !this.isBaseCurrency)
                this.gridAg.addColumnNumber("amountCurrency", terms["common.customer.invoices.foreignamount"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnShape("payrollAttestStateColor", null, 40, { shape: Constants.SHAPE_CIRCLE, toolTipField: "payrollAttestStateName", showIconField: "payrollAttestStateColor", enableHiding: false, hide: false });
            this.gridAg.addColumnNumber("invoicedAmount", terms["common.customer.invoices.amounttoinvoice"], null, {
                enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum',
                cellClassRules: {
                    "text-right": () => true,
                    "errorRow": (gridRow: any) => gridRow.data && gridRow.data.invoiceAmount && gridRow.data.invoicedAmount > 0 && gridRow.data.invoicedAmount < gridRow.data.amountExVat,
                }
            });
            if (this.hasCurrencyPermission && !this.isBaseCurrency)
                this.gridAg.addColumnNumber("invoicedAmountCurrency", terms["common.customer.invoices.currencyamounttotransfer"], null, {
                    enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum',
                    cellClassRules: {
                        "text-right": () => true,
                        "errorRow": (gridRow: any) => gridRow.data && gridRow.data.invoicedAmountCurrency && gridRow.data.invoicedAmountCurrency > 0 && gridRow.data.invoicedAmountCurrency < gridRow.data.amountCurrency,
                    }
                });
            this.gridAg.addColumnShape("invoiceRowAttestStateColor", null, 40, { shape: Constants.SHAPE_CIRCLE, toolTipField: "invoiceRowAttestStateName", showIconField: "invoiceRowAttestStateColor", enableHiding: false, hide: false });

            this.gridAg.addColumnBoolEx("isSpecifiedUnitPrice", terms["common.customer.invoices.specifiedunitprice"], null, { hide: true, enableHiding: true });

            this.gridAg.addColumnIcon(null, null, 40, { icon: "fal fa-paperclip", showIcon: (row) => row && row.hasFiles });

            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));
            this.gridAg.addColumnDelete(terms["core.delete"], this.deleteRow.bind(this), null, () => this.isOrderMode);

            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.edit(row); }));

            if (!this.isOrderMode) {
                events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows) => {
                    const localhasSelectedMyOwnRows = (rows.filter(x => x.employeeId === this.employeeId).length > 0);
                    this.loadAttestStates(localhasSelectedMyOwnRows && this.employees.length > 0);
                    this.$scope.$applyAsync();
                }));
            }

            this.gridAg.options.subscribe(events);

            if (!this.isOrderMode) {
                this.gridAg.options.useGrouping(true, true, {
                    keepColumnsAfterGroup: false, selectChildren: true, keepGroupState: true, groupSelectsFiltered: true, totalTerm: terms["common.sum"]
                });
            }

            this.gridAg.finalizeInitGrid("billing.projects.list.budgetcost", true);

            this.$timeout(() => {
                this.gridAg.options.addFooterRow("#expense-row-grid-sum-footer", {
                    "quantity": "sum",
                    "amount": "sum",
                    "amountExVat": "sum",
                    "amountCurrency": "sum",
                    "invoicedAmount": "sum",
                    "invoicedAmountCurrency": "sum",
                } as IColumnAggregations);
            });
        });
    }

    public loadGridData() {
        this.orderService.getExpenseRows(this.customerInvoiceId).then((rows) => {
            this.expenseRows = rows;
            _.forEach(this.expenseRows, (r) => {
                if (r.timeCodeRegistrationType === TermGroup_TimeCodeRegistrationType.Time)
                    r['guantityFormatted'] = CalendarUtility.minutesToTimeSpan(r.quantity);
                else
                    r['guantityFormatted'] = NumberUtility.printDecimal(r.quantity, 2);
            });
            this.sumExpenses = _.sum(_.map(this.expenseRows, 'amountCurrency'));
            this.sumExpensesInvoiced = _.sum(_.map(this.expenseRows, 'invoicedAmountCurrency'));

            this.gridAg.setData(this.expenseRows);
        });
    }

    public loadFilteredGridData() {
        return this.progress.startLoadingProgress([() => {
                const employees = this.getSelectedEmployees();

                const categories: number[] = [];
                this.selectedEmployeeCategoryDict.forEach(o => {
                    categories.push(o.id);
                });

                const projects: number[] = [];
                this.selectedProjectsDict.forEach(p => {
                    projects.push(p.id);
                });
                const orders: number[] = [];
                this.selectedOrdersDict.forEach(o => {
                    orders.push(o.id);
                });

                return this.orderService.getExpenseRowsFiltered(this.employeeId, this.fromDate, this.toDate, employees, categories, projects, orders).then((rows) => {
                    this.expenseRows = rows;
                    _.forEach(this.expenseRows, (r) => {
                        if (r.timeCodeRegistrationType === TermGroup_TimeCodeRegistrationType.Time)
                            r['guantityFormatted'] = CalendarUtility.minutesToTimeSpan(r.quantity);
                        else
                            r['guantityFormatted'] = NumberUtility.printDecimal(r.quantity, 2);
                    });

                    this.gridAg.setData(this.expenseRows);

                    this.$timeout(() => this.messagingService.publish(Constants.EVENT_RESIZE_WINDOW, null), 500);
                });
            }
        ]);
    }

    private openOrder(row: any) {
        this.translationService.translate("common.order").then((term) => {
            this.messagingService.publish(Constants.EVENT_OPEN_ORDER, {
                id: row.orderId,
                name: term + " " + row.invoiceNr
            });
        });
    }

    private openCustomer(row: any) {
        this.translationService.translate("common.customer.customer.customer").then((term) => {
            this.messagingService.publish(Constants.EVENT_OPEN_EDITCUSTOMER, {
                id: row.actorCustomerId,
                name: term + " " + row.customerName
            });
        });
    }

    private openProject(row: any) {
        this.translationService.translate("billing.project.project").then((term) => {
            this.messagingService.publish(Constants.EVENT_OPEN_EDITPROJECT, {
                id: row.projectId,
                name: term + " " + row.projectNr
            });
        });
        
    }

    private print() {
        var employeeIds: any[] = [];

        _.forEach(this.employees, (row: IEmployeeTimeCodeDTO) => {
            employeeIds.push(row.employeeId);
        });

        this.projectService.getProjectExpenseReportUrl(this.customerInvoiceId, this.projectId).then((x) => {
            HtmlUtility.openInSameTab(this.$window, x);
        });
    }

    public edit(row: ExpenseRowGridDTO) {

        this.doLookupsForEdit().then(() => {
            var options: angular.ui.bootstrap.IModalSettings = {
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/AddExpense/AddExpenseDialog.html"),
                controller: AddExpenseDialogController,
                controllerAs: "ctrl",
                bindToController: true,
                backdrop: 'static',
                size: 'lg',
                windowClass: 'fullsize-modal',
                resolve: {
                    isMySelf: () => { return false },
                    settings: () => { return [] },
                    settingTypes: () => { return [] },
                    employeeId: () => { return undefined },
                    standsOnDate: () => { return undefined },
                    isProjectMode: () => { return true },
                    timePeriodId: () => { return undefined },
                    readOnly: () => { return this.readOnly || !this.isOrderMode },
                    customerInvoiceId: () => { return this.customerInvoiceId },
                    projectId: () => { return this.projectId },
                    expenseRowId: () => { return row ? row.expenseRowId : undefined },
                    timeCodes: () => { return this.timeCodes },
                    employees: () => { return this.employeesDict },
                    currencyCode: () => { return this.currencyCode },
                    priceListTypeInclusiveVat: () => { return this.priceListTypeInclusiveVat },
                    hasFiles: () => { return row ? row.hasFiles : false }
                }
            }
            this.$uibModal.open(options).result.then((result: any) => {
                if (result && result.rowToSave) {
                    // Add invoice id to new rows
                    if (!result.rowToSave.customerInvoiceId)
                        result.rowToSave.customerInvoiceId = this.customerInvoiceId;

                    this.save(result.rowToSave);
                }
            }, (result: any) => {
            });
        });
    }

    // Server calls
    public save(row: any) {
        this.progress.startSaveProgress((completion) => {
            this.coreService.saveExpenseRows([row], this.customerInvoiceId).then((result) => {
                if (result.success) {
                    completion.completed();
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.messagingService.publish(Constants.EVENT_RELOAD_INVOICE, { guid: this.guid });
                this.loadGridData();
            }, error => {
            });
    }

    public deleteRow(row: any) {
        this.progress.startDeleteProgress((completion) => {
            this.coreService.deleteExpenseRow(row.expenseRowId).then((result) => {
                if (result.success) {
                    completion.completed(row);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.messagingService.publish(Constants.EVENT_RELOAD_INVOICE, { guid: this.guid });
            this.loadGridData();
        });
    }

    private saveAttest(option: any) {
        const attestStateTo: AttestStateDTO = this.userValidPayrollAttestStates.find(x => x.attestStateId === (option.id));
        if (!attestStateTo)
            return;

        const transactionItems: AttestPayrollTransactionDTO[] = [];

        const selectedRows = this.gridAg.options.getSelectedRows();

        this.progress.startSaveProgress((completion) => {

            _.forEach(selectedRows, (row: ExpenseRowGridDTO) => {
                _.forEach(row.timePayrollTransactionIds, (id: number) => {
                    const transactionItem: AttestPayrollTransactionDTO = new AttestPayrollTransactionDTO();
                    transactionItem.employeeId = row.employeeId;
                    transactionItem.timePayrollTransactionId = id;
                    transactionItem.attestStateId = row.payrollAttestStateId;
                    transactionItem.date = row.payrollTransactionDate; //row.from;
                    transactionItem.isScheduleTransaction = false;
                    transactionItem.isExported = false;
                    transactionItem.isPreliminary = false;
                    transactionItems.push(transactionItem);
                });
            });

            this.validateSaveAttestTransactions(selectedRows, transactionItems, attestStateTo).then((validItems: any[]) => {
                if (validItems) {
                    return this.projectService.saveAttestForTransactions(validItems, attestStateTo.attestStateId, false).then((result: IActionResult) => {
                        if (result.success) {
                            completion.completed("");
                            this.loadFilteredGridData();
                        }
                        else {
                            completion.failed(result.errorMessage);
                        }
                    });
                }
                else {
                    completion.failed("");
                }
            })
        }, this.guid);
    }

    private validateSaveAttestTransactions(selectedRows: any[], transactionItems: AttestPayrollTransactionDTO[], attestStateTo: AttestStateDTO): ng.IPromise<any[]> {

        const deferral = this.$q.defer<any[]>();

        this.projectService.saveAttestForTransactionsValidation(transactionItems, attestStateTo.attestStateId, false).then((validationResult) => {
            if (validationResult.success && this.userSettingTimeAttestDisableSaveAttestWarning) {
                deferral.resolve(validationResult.validItems);
            }
            else {
                this.translationService.translateMany(["billing.project.timesheet.timerowsstatuschange", "core.donotshowagain"]).then((terms) => {
                    let message = validationResult.success ? terms["billing.project.timesheet.timerowsstatuschange"].format(selectedRows.length.toString(), attestStateTo.name) : validationResult.message;
                    const modal = this.notificationService.showDialog(validationResult.title, message, TimePayrollUtility.getSaveAttestValidationMessageIcon(validationResult), TimePayrollUtility.getSaveAttestValidationMessageButton(validationResult), SOEMessageBoxSize.Medium, false, validationResult.success, terms["core.donotshowagain"]);
                    modal.result.then(result => {
                        if (validationResult.success) {
                            if (result) {
                                if (result.isChecked)
                                    this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeDisableApplySaveAttestWarning, this.userSettingTimeAttestDisableSaveAttestWarning);
                                deferral.resolve(validationResult.validItems);
                            }
                            else {
                                deferral.resolve(null);
                            }
                        }
                        else {
                            deferral.resolve(null);
                        }
                    });
                });
            }
        });

        return deferral.promise;
    }

    // Filters
    private decreaseDate() {
        const diffDays = this.fromDate.diffDays(this.toDate) - 1;
        this.fromDate = this.fromDate.addDays(diffDays);
        this.toDate = this.toDate.addDays(diffDays);
    }

    private increaseDate() {
        const diffDays = this.toDate.diffDays(this.fromDate) + 1;
        this.fromDate = this.fromDate.addDays(diffDays);
        this.toDate = this.toDate.addDays(diffDays);
    }

    private populateEmployeeCategories(): ng.IPromise<any> {
        if (this.filteredEmployeeCategoriesDict.length == 0) {
            return this.coreService.getCategories(SoeCategoryType.Employee, false, false, false, true).then((categories: any[]) => {
                categories.forEach((c) => {
                    this.filteredEmployeeCategoriesDict.push({ id: c.categoryId, label: c.name });
                })
            });
        }
    }

    private employeeCategoriesSelectionComplete() {
        this.selectedEmployeesDict = [];
        this.loadEmployees();
    }

    private employeeSelectionComplete() {
        this.populateProjectAndInvoice(this.getSelectedEmployees());
        this.sortSelectedRows();
    }

    private getSelectedEmployees(): number[] {
        const employeeIds = [];
        if (this.selectedEmployeesDict.length > 0) {
            this.selectedEmployeesDict.forEach((e) => {
                employeeIds.push(e.id);
            });
        }
        return employeeIds.length === 0 ? this.employeesDict.map(a => a.id) : employeeIds;
    }

    private populateProjectAndInvoice(employeeIds: number[] = []): ng.IPromise<any> {
        if (employeeIds.length === 0) {
            employeeIds = this.getSelectedEmployees();
            if (employeeIds.filter(x => x == this.employeeId).length == 0) {
                employeeIds.push(this.employeeId);
            }
        }

        const currentEmployees = this.projectInvoices.map(a => a.employeeId);
        const difference = _.difference(employeeIds, currentEmployees);
        employeeIds = difference;

        if (employeeIds.length === 0) {
            const deferral = this.$q.defer();
            deferral.resolve();
            return deferral.promise;
        }

        return this.loadProjectInvoices(employeeIds);
    }

    private sortSelectedRows() {
        var selected: any[] = [];
        var notSelected: any[] = [];
        if (this.selectedProjectsDict.length > 0) {
            for (let o of this.filteredProjectsDict) {
                if (_.filter(this.selectedProjectsDict, x => x.id === o.id).length > 0)
                    selected.push(o);
                else
                    notSelected.push(o);
            }
            this.filteredProjectsDict = selected.concat(notSelected);
        }
        else {
            this.filteredProjectsDict = this.allProjects;
        }

        selected = [];
        notSelected = [];
        if (this.selectedOrdersDict.length > 0) {
            for (let o of this.filteredOrdersDict) {
                if (_.filter(this.selectedOrdersDict, x => x.id === o.id).length > 0)
                    selected.push(o);
                else
                    notSelected.push(o);
            }
            this.filteredOrdersDict = selected.concat(notSelected);
        }
        else {
            this.filteredOrdersDict = this.allOrders;
        }
    }

    private executeSearchButtonFunction(option) {
        switch (option.id) {
            case TimeProjectSearchFunctions.SearchIntervall:
                this.loadFilteredGridData();
                break;
            /*case TimeProjectSearchFunctions.GetAll:
                this.loadData(false);
                break;
            case TimeProjectSearchFunctions.SearchWithGroupOnDate:
                this.groupByDate = true;
                this.searchIncPlannedAbsence = true;
                this.loadData(true, this.searchIncPlannedAbsence);
                break;*/
        }
    }

    // Functions
    private reloadData() {
        this.loadGridData();
    }
}

export class ExpenseRowsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Common/Directives/ExpenseRows/ExpenseRows.html"),
            scope: {
                guid: "=",
                readOnly: "=",
                projectId: "=?",
                employeeId: "=?",
                customerInvoiceId: "=?",
                currencyCode: "=?",
                isBaseCurrency: "=?",
                priceListTypeInclusiveVat: '=?',
                includeExpenseInReport: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: ExpenseRowController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}