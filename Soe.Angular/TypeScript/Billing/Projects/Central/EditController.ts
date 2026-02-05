import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ProjectGridDTO, ProjectCentralStatusDTO } from "../../../Common/Models/ProjectDTO";
import { IProjectService } from "../../../Shared/Billing/Projects/ProjectService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../Util/Enumerations";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SelectProjectController } from "../../../Common/Dialogs/SelectProject/SelectProjectController";
import { SelectReportController } from "../../../Common/Dialogs/SelectReport/SelectReportController";
import { Feature, SoeOriginType, ProjectCentralHeaderGroupType, ProjectCentralBudgetRowType, SoeReportTemplateType, SettingMainType, UserSettingType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { TimeColumnOptions } from "../../../Util/SoeGridOptionsAg";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { IRequestReportService } from "../../../Shared/Reports/RequestReportService";
import { ProjectPrintDTO } from "../../../Common/Models/RequestReports/ProjectPrintDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {
    //Parameters
    projectId: number;

    //Terms
    terms: { [index: string]: string; };

    //Properties
    project: ProjectGridDTO;
    fromDate: Date;
    toDate: Date;
    includeChildProjects = false;
    infoAccordionOpen = true;
    private projectBreadCrumbs: any[];
    private showBreadCrumbs: boolean;
    private projectInfoLabel: string;

    //Sums
    incomeinvoiced: number;
    costs: number;
    result: number;
    resultbudget: number;
    days: string = "N/A";
    workedhours: string;
    notbilledhours: string;
    fixedprice: number;
    marginalincomeratio: number;

    //Datagrid
    private gridHandler: EmbeddedGridController;
    projectCentralRows: ProjectCentralStatusDTO[];

    //Permissions
    private hasEditProjectPermission: boolean;
    private hasEditCustomerPermission: boolean;
    private hasEditSupplierInvoicePermission: boolean;
    private hasEditCustomerInvoicePermission: boolean;
    private hasEditCustomerOrderPermission: boolean;
    private hasProjectListPermission: boolean;
    private hasBillingReportPermission: boolean;

    // Flags
    gridSetupDone = false;

    _loadDetails = false;
    get loadDetails(): boolean {
        return this._loadDetails;
    }
    set loadDetails(value: boolean) {
        this._loadDetails = value;

        if (this.gridSetupDone)
            this.handleDetails();
    }

    //@ngInject
    constructor(
        private $window,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private projectService: IProjectService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,

        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,

        private urlHelperService: IUrlHelperService,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService,
        private readonly requestReportService: IRequestReportService,
    ) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        //super("Billing.Projects.Central", Feature.Billing_Project_Central, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);

        this.flowHandler = this.controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            // .onLoadData(() => this.onLoadData())
            .onSetUpGUI(() => this.onSetUpGUI())
            .onDoLookUp(() => this.onDoLookups()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "projectCentralGrid");

        this.gridHandler.gridAg.options.enableFiltering = false;
        this.gridHandler.gridAg.options.enableRowSelection = false;


        this.messagingService.subscribe(Constants.EVENT_REFRESH_PROJECTCENTRALDATA, (x) => {
            if (x.customerId && x.customerId === this.project.customerId) {
                this.loadProject(false);
            }
            if (x.projectId && x.projectEdit === true && x.projectId === this.project.projectId) {
                this.loadProject(true);
            }
        });
    }

    public onInit(parameters: any) {
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.projectId = soeConfig.projectId ? soeConfig.projectId : (parameters.id || 0);

        this.flowHandler.start([{ feature: Feature.Billing_Project_Central, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Customer_Customers_Edit, loadReadPermissions: false, loadModifyPermissions: true },
            { feature: Feature.Billing_Project_Edit, loadReadPermissions: false, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit, loadReadPermissions: false, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_Orders_Edit, loadReadPermissions: false, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_Invoices_Edit, loadReadPermissions: false, loadModifyPermissions: true },
            { feature: Feature.Billing_Project_List, loadReadPermissions: false, loadModifyPermissions: true },
            { feature: Feature.Billing_Distribution_Reports, loadReadPermissions: false, loadModifyPermissions: true }
        ]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Project_Central].readPermission;
        this.modifyPermission = response[Feature.Billing_Project_Central].modifyPermission;

        this.hasEditCustomerPermission = response[Feature.Billing_Customer_Customers_Edit].modifyPermission;
        this.hasEditProjectPermission = response[Feature.Billing_Project_Edit].modifyPermission;
        this.hasEditCustomerInvoicePermission = response[Feature.Billing_Invoice_Invoices_Edit].modifyPermission;
        this.hasEditCustomerOrderPermission = response[Feature.Billing_Order_Orders_Edit].modifyPermission;
        this.hasEditSupplierInvoicePermission = response[Feature.Economy_Supplier_Invoice_Invoices_Edit].modifyPermission;
        this.hasProjectListPermission = response[Feature.Billing_Project_List].modifyPermission;
        this.hasBillingReportPermission = response[Feature.Billing_Distribution_Reports].modifyPermission;
    }

    private onSetUpGUI() {
        this.setupGridColumns();
        if (this.projectId && this.projectId > 0) {
            this.$q.all([
                this.loadProject()]).then(() => {
                    this.loadProjectData();
                });
        }
        else {
            this.showSelectProject();
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createEmpty();
        //if (this.setupDefaultToolBar()) {
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("billing.project.central.findproject", "billing.project.central.findproject", IconLibrary.FontAwesome, "fa-search",
            () => { this.showSelectProject(); },
            null,
            null,
            { buttonClass: "ngSoeMainButton pull-left" })));

        if (this.hasEditCustomerOrderPermission) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("billing.project.central.neworder", "billing.project.central.newordertooltip", IconLibrary.FontAwesome, "fa-plus", () => {
                if (this.project)
                    this.messagingService.publish(Constants.EVENT_NEW_ORDER, { projectId: this.project.projectId, customerId: this.project.customerId });
                else
                    this.messagingService.publish(Constants.EVENT_NEW_ORDER, null);
            }, () => {
            })));
        }

        if (this.hasProjectListPermission) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("billing.project.central.projectlist", "billing.project.central.projectlisttooltip", IconLibrary.FontAwesome, "fa-list", () => {
                HtmlUtility.openInSameTab(this.$window, "/soe/billing/project/list/");
            }, () => {
            })));
        }

        if (this.hasBillingReportPermission) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("billing.project.central.projectreports", "billing.project.central.projectreportstooltip", IconLibrary.FontAwesome, "fa-print", () => {
                this.showProjectReportsDialog();
            }, () => {
                if (!this.project)
                    return true;
                else if (!this.project.projectId)
                    return true;
                else if (this.project.projectId === 0)
                    return true;
                else
                    return false;
            })));
        }
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([this.loadTerms(), this.loadUserSettings()]);
    }

    private setupGridColumns() {
        this.$timeout(() => {
            const keys: string[] = [
                "billing.project.central.specification",
                "billing.project.central.budget",
                "common.type",
                "common.time",
                "common.amount",
                "billing.project.central.outcome",
                "billing.project.central.deviation",
                "billing.project.central.showinfo",
                "core.edit",
                "economy.supplier.invoice.timecodes",
                "common.employee"
            ];

            return this.translationService.translateMany(keys).then((terms) => {
                this.gridHandler.gridAg.options.addGroupTimeSpanSumAggFunction(false);
                
                //const timeColumnOptions: TimeColumnOptions =
                //{
                //    aggFuncOnGrouping: "sumTimeSpan"
                //}
                const timeColumnOptions: TimeColumnOptions = {
                    enableHiding: true, enableRowGrouping: true,
                    clearZero: true, alignLeft: false, minDigits: 5, aggFuncOnGrouping: "sumTimeSpan", cellClassRules: {
                        "excelTime": () => true,
                    },
                    maxWidth: 100,
                };

                const nameColumn = this.gridHandler.gridAg.addColumnText("groupRowTypeName", terms["common.type"], null, false, { enableRowGrouping: true, });
                nameColumn.name = "namecolumn";
                nameColumn.rowGroup = true;
                nameColumn.hide = true;

                this.gridHandler.gridAg.addColumnText("typeName", terms["billing.project.central.specification"], null, false, {
                    enableRowGrouping: true, buttonConfiguration: {
                        iconClass: "fal fa-pencil iconEdit",
                        callback: (params) => this.openOrderInvoice(params),
                        show: (params) => this.showIcon(params)
                    }
                });

                this.gridHandler.gridAg.addColumnText("costTypeName", terms["economy.supplier.invoice.timecodes"], null, false, { enableRowGrouping: true, hide: true });

                this.gridHandler.gridAg.addColumnText("employeeName", terms["common.employee"], null, false, { enableRowGrouping: true, hide: true });

                const colBudgetHeader = this.gridHandler.gridAg.options.addColumnHeader("budget", terms["billing.project.central.budget"], null);
                colBudgetHeader.marryChildren = true;
                this.gridHandler.gridAg.addColumnNumber("budget", terms["common.amount"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum', maxWidth: 150 }, colBudgetHeader);
                this.gridHandler.gridAg.addColumnTimeSpan("budgetTimeFormatted", terms["common.time"], null, timeColumnOptions, colBudgetHeader);

                const colOutcomeHeader = this.gridHandler.gridAg.options.addColumnHeader("value", terms["billing.project.central.outcome"], null);
                colOutcomeHeader.marryChildren = true;
                this.gridHandler.gridAg.addColumnNumber("value", terms["common.amount"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum', maxWidth: 150 }, colOutcomeHeader);
                this.gridHandler.gridAg.addColumnTimeSpan("valueTimeFormatted", terms["common.time"], null, timeColumnOptions, colOutcomeHeader);

                const colDiffHeader = this.gridHandler.gridAg.options.addColumnHeader("diff", terms["billing.project.central.deviation"], null);
                colDiffHeader.marryChildren = true;
                this.gridHandler.gridAg.addColumnNumber("diff", terms["common.amount"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum', maxWidth: 150 }, colDiffHeader);
                this.gridHandler.gridAg.addColumnTimeSpan("diffTimeFormatted", terms["common.time"], null, timeColumnOptions, colDiffHeader);


                const defs = this.gridHandler.gridAg.options.getColumnDefs();
                _.forEach(defs, (colDef: any) => {
                    if (!colDef.cellClass || !angular.isFunction(colDef.cellClass)) {
                        const cellClass: string = colDef.cellClass ? colDef.cellClass.toString() : "";
                        colDef.cellClass = (params) => {
                            const { value, data } = params;
                            if (!data) {
                                return value != undefined ? cellClass + " activeRow" : cellClass;
                            }
                            else {
                                //return colDef.field === "diffTimeFormatted" ? cellClass + " hiddenValue" : cellClass;
                                return cellClass;
                            }
                        };
                    }
                });

                this.gridHandler.gridAg.options.setAutoHeight(true);
                this.gridHandler.gridAg.options.useGrouping(true, false, {
                    keepColumnsAfterGroup: false,
                    selectChildren: true,
                    minAutoGroupColumnWidth: 200
                });
                this.gridHandler.gridAg.finalizeInitGrid("billing.projects.list.projects", false);

                this.gridSetupDone = true;
                this.handleDetails();
            });
       },100)
    }

    private handleDetails() {
        /*if (this.loadDetails) {
            this.gridHandler.gridAg.options.groupRowsByColumn("groupRowTypeName", false);
            this.gridHandler.gridAg.options.groupRowsByColumn("costTypeName", false);
        }
        else {
            this.gridHandler.gridAg.options.ungroupColumn("costTypeName");
            this.gridHandler.gridAg.options.groupRowsByColumn("groupRowTypeName", false);
        }*/

        this.$timeout(() => {
            if (this.loadDetails) {
                this.gridHandler.gridAg.options.showColumn("costTypeName");

                this.gridHandler.gridAg.options.groupRowsByColumn("groupRowTypeName", false);
                this.gridHandler.gridAg.options.groupRowsByColumn("costTypeName", false);
            }
            else {
                this.gridHandler.gridAg.options.ungroupColumn("costTypeName");
                this.gridHandler.gridAg.options.groupRowsByColumn("groupRowTypeName", false);

                this.gridHandler.gridAg.options.hideColumn("costTypeName");
            }
        }, 100).then(() => {
            this.gridHandler.gridAg.options.resetColumns();

            if (this.loadDetails) {
                this.gridHandler.gridAg.options.showColumn("employeeName");
            }
            else {
                this.gridHandler.gridAg.options.hideColumn("employeeName");
            }
        });
    }

    //Action
    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.project.central.changed",
            "billing.project.central.changedby",
            "billing.project.central.showinfo",
            "billing.project.central.gettingdata",
            "common.customer",
            "billing.project.project"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.projectInfoLabel = this.terms["billing.project.project"];
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(UserSettingType.ProjectUseDetailedViewInProjectOverview);
        settingTypes.push(UserSettingType.ProjectUseChildProjectsInProjectOverview);

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.loadDetails = SettingsUtility.getBoolUserSetting(x, UserSettingType.ProjectUseDetailedViewInProjectOverview, false);
            this.includeChildProjects = SettingsUtility.getBoolUserSetting(x, UserSettingType.ProjectUseChildProjectsInProjectOverview, false);
        });
    }

    private loadProject(reloadBreadCrumbs = true) {
        return this.projectService.getProjectGridDTO(this.projectId).then((x) => {
            this.changeActiveProject(x, reloadBreadCrumbs);
            this.project.startDate = CalendarUtility.convertToDate(this.project.startDate);
            this.project.stopDate = CalendarUtility.convertToDate(this.project.stopDate);
        });
    }

    private initiateLoadBreadCrumbs() {
        this.projectBreadCrumbs = [];
        this.projectBreadCrumbs.push(this.project);
        this.recursivelyLoadProjectBreadCrumbs(this.project.parentProjectId);
    }

    private recursivelyLoadProjectBreadCrumbs(parentProjectId: number) {
        if (parentProjectId) {
            this.projectService.getProjectGridDTO(parentProjectId).then((x) => {
                if (!this.projectBreadCrumbs.some(el => el.projectId === x.projectId))
                    this.projectBreadCrumbs.unshift(x);
                this.recursivelyLoadProjectBreadCrumbs(x.parentProjectId)
            })
        }
        this.showBreadCrumbs = this.projectBreadCrumbs.length > 1 ? true : false;
    }

    private changeActiveProject(project: ProjectGridDTO, reloadBreadCrumbs = true) {
        this.project = project;
        this.projectId = project.projectId;
        this.projectInfoLabel = this.project.number + " " + this.project.name + " | " + this.project.statusName;
        if (reloadBreadCrumbs)
            this.initiateLoadBreadCrumbs();
    }

    private loadProjectData() {
        this.progress.startLoadingProgress([() => {

            return this.projectService.getProjectCentralStatus(this.projectId, this.includeChildProjects, this.fromDate, this.toDate, this.loadDetails).then((x) => {

                this.projectCentralRows = x;
                const orders: number[] = [];
                const customerInvoices: number[] = [];
                const supplierInvoices: number[] = [];
                this.projectCentralRows.forEach( row => {
                    if (row.originType === SoeOriginType.Order && !_.includes(orders, row.associatedId)) {
                        orders.push(row.associatedId);
                    }
                    else if (row.originType === SoeOriginType.CustomerInvoice && !_.includes(customerInvoices, row.associatedId)) {
                        customerInvoices.push(row.associatedId);
                    }
                    else if (row.originType === SoeOriginType.SupplierInvoice && !_.includes(supplierInvoices, row.associatedId)) {
                        supplierInvoices.push(row.associatedId);
                    }

                    if (row.type === ProjectCentralBudgetRowType.CostPersonell) {
                        row['budgetTimeFormatted'] = (row.budgetTime > 0) ? CalendarUtility.minutesToTimeSpan(row.budgetTime) : '';
                        row['valueTimeFormatted'] = (row.value2 > 0) ? CalendarUtility.minutesToTimeSpan(row.value2 * 60) : '';
                        row['diffTimeFormatted'] = (row.budgetTime > 0 || row.value2 > 0) ? CalendarUtility.minutesToTimeSpan(row.diff2 * 60) : '';
                    }
                    else if (row.type === ProjectCentralBudgetRowType.CostExpense) {
                        row['budgetTimeFormatted'] = (row.budgetTime > 0) ? CalendarUtility.minutesToTimeSpan(row.budgetTime) : '';
                        row['valueTimeFormatted'] = (row.value2 > 0) ? CalendarUtility.minutesToTimeSpan(row.value2) : '';
                        row['diffTimeFormatted'] = (row.budgetTime > 0 || row.value2 > 0) ? CalendarUtility.minutesToTimeSpan((row.value2) - row.budgetTime) : '';
                    }

                });

                this.summarize();
                this.gridHandler.gridAg.setData(_.filter(this.projectCentralRows, r => r.groupRowType != ProjectCentralHeaderGroupType.Time && r.groupRowType != ProjectCentralHeaderGroupType.None));
                this.updateHeight();

                this.messagingService.publish(Constants.EVENT_DISABLE_TAB, { identifier: "projectcentral_order", disable: false });
                this.messagingService.publish(Constants.EVENT_DISABLE_TAB, { identifier: "projectcentral_customerinvoices", disable: false });
                this.messagingService.publish(Constants.EVENT_DISABLE_TAB, { identifier: "projectcentral_supplierinvoices", disable: false });
                this.messagingService.publish(Constants.EVENT_DISABLE_TAB, { identifier: "projectcentral_timesheet", disable: false });
                this.messagingService.publish(Constants.EVENT_DISABLE_TAB, { identifier: "projectcentral_productrows", disable: false });
                this.messagingService.publish(Constants.EVENT_DISABLE_TAB, { identifier: "projectcentral_analytics", disable: false });
                this.messagingService.publish(Constants.EVENT_LOAD_PROJECTCENTRALDATA, { projectId: this.project.projectId, customerId: this.project.customerId, includeChildProjects: this.includeChildProjects, orders: orders, customerInvoices: customerInvoices, supplierInvoices: supplierInvoices, projectCentralRows: this.projectCentralRows, fromDate: this.fromDate, toDate: this.toDate });

            });
        }]);
    }

    private clearSelection() {
        this.fromDate = undefined;
        this.toDate = undefined;
        this.includeChildProjects = false;
    }

    private openOrderInvoice(row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_PROJECTCENTRAL, { row: row });
    }

    public showDetailsChanged() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.ProjectUseDetailedViewInProjectOverview, this.loadDetails);
        });
    }

    public childProjectChanged() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.ProjectUseChildProjectsInProjectOverview, this.includeChildProjects);
        });
    }

    //Helper methods
    private summarize() {
        this.incomeinvoiced = _.reduce(_.filter(this.projectCentralRows, r => r.type === ProjectCentralBudgetRowType.IncomeInvoiced), (x, y) => { return x + y.value; }, 0);
        this.costs = _.reduce(_.filter(this.projectCentralRows, r => r.type === ProjectCentralBudgetRowType.CostExpense || r.type === ProjectCentralBudgetRowType.CostMaterial || r.type === ProjectCentralBudgetRowType.CostPersonell || r.type === ProjectCentralBudgetRowType.OverheadCost), (x, y) => { return x + y.value; }, 0);
        this.result = this.incomeinvoiced - this.costs;

        const budgetIncome = _.reduce(_.filter(this.projectCentralRows, r => r.type === ProjectCentralBudgetRowType.IncomeInvoiced || r.type === ProjectCentralBudgetRowType.IncomeNotInvoiced), (x, y) => { return x + y.budget; }, 0);
        const budgetCosts = _.reduce(_.filter(this.projectCentralRows, r => r.type === ProjectCentralBudgetRowType.CostExpense || r.type === ProjectCentralBudgetRowType.CostMaterial || r.type === ProjectCentralBudgetRowType.CostPersonell || r.type === ProjectCentralBudgetRowType.OverheadCost || r.type === ProjectCentralBudgetRowType.OverheadCostPerHour), (x, y) => { return x + y.budget; }, 0);
        this.resultbudget = budgetIncome - budgetCosts;

        this.fixedprice = _.reduce(_.filter(this.projectCentralRows, r => r.type === ProjectCentralBudgetRowType.FixedPriceTotal), (x, y) => { return x + y.value; }, 0);

        this.marginalincomeratio = ((this.incomeinvoiced - this.costs) / this.incomeinvoiced) * 100;

        const personellCost = _.reduce(_.filter(this.projectCentralRows, r => r.type === ProjectCentralBudgetRowType.CostPersonell), (x, y) => { return x + y.value2; }, 0);
        this.workedhours = CalendarUtility.minutesToTimeSpan(personellCost * 60);

        const billableNotInvoiced = _.reduce(_.filter(this.projectCentralRows, r => r.type === ProjectCentralBudgetRowType.BillableMinutesNotInvoiced), (x, y) => { return x + y.value; }, 0);
        this.notbilledhours = CalendarUtility.minutesToTimeSpan(billableNotInvoiced);

        if (this.project.startDate) {
            const today: Date = new Date();
            const startDate = CalendarUtility.convertToDate(this.project.startDate);
            const oneday = (1000 * 60 * 60 * 24);
            const timediff = today.getTime() - startDate.getTime();
            this.days = (Math.ceil(timediff / oneday)).toString();
        }
        else {
            this.days = "N/A";
        }
    }

    private showIcon(row: any) {
        if (!row || !row.originType)
            return false;
        let editPermission: boolean;
        switch (row.originType) {
            case SoeOriginType.SupplierInvoice:
                editPermission = this.hasEditSupplierInvoicePermission;
                break;
            case SoeOriginType.CustomerInvoice:
                editPermission = this.hasEditCustomerOrderPermission;
                break;
            case SoeOriginType.Order:
                editPermission = this.hasEditCustomerOrderPermission;
                break;
        }
        return editPermission && row.isEditable;
    }

    private searchIntervalValidated() {
        if (this.toDate && this.fromDate && this.fromDate.isAfterOnDay(this.toDate))
            return false;
        else
            return true;
    }

    //Dialogs
    private showSelectProject(): any {

        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectProject/Views/selectproject.html"),
            controller: SelectProjectController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                projects: () => { return null },
                customerId: () => { return null },
                projectsWithoutCustomer: () => { return null },
                showFindHidden: () => { return true },
                loadHidden: () => { return null },
                useDelete: () => { return false },
                currentProjectNr: () => { return null },
                currentProjectId: () => { return null },
                excludedProjectId: () => { return null },
                showAllProjects: () => { return false },
            }
        });

        modal.result.then((result) => {
            if (result) {
                this.changeActiveProject(result, true);
            }
        }, function () {
        });

        return modal;
    }

    public openCustomer() {
        if (!this.project.customerId || !this.hasEditCustomerPermission)
            return;

        this.messagingService.publish(Constants.EVENT_OPEN_EDITCUSTOMER, {
            id: this.project.customerId,
            name: this.terms["common.customer"] + " " + this.project.customerNr
        });
    }

    public openProject() {
        if (!this.project.projectId || !this.hasEditProjectPermission)
            return;

        this.messagingService.publish(Constants.EVENT_OPEN_EDITPROJECT, {
            id: this.project.projectId,
            name: this.terms["billing.project.project"] + " " + this.project.name
        });
    }

    private showProjectReportsDialog() {

        const reportTypes: number[] = [
            SoeReportTemplateType.ProjectTransactionsReport,
            SoeReportTemplateType.ProjectStatisticsReport,
            SoeReportTemplateType.OrderContractChange
        ];

        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectReport/SelectReport.html"),
            controller: SelectReportController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                module: () => { return null },
                reportTypes: () => { return reportTypes },
                showCopy: () => { return false },
                showEmail: () => { return false },
                copyValue: () => { return false },
                reports: () => { return null },
                defaultReportId: () => { return null },
                langId: () => { return null },
                showReminder: () => { return false },
                showLangSelection: () => { return false },
                showSavePrintout: () => { return false },
                savePrintout: () => { return false }
            }
        });

        modal.result.then((result: any) => {
            if (result?.reportId) {
                const reportItem = new ProjectPrintDTO([this.projectId]);
                reportItem.reportId = result.reportId;
                reportItem.sysReportTemplateTypeId = result.reportType;
                reportItem.dateFrom = this.fromDate;
                reportItem.dateTo = this.toDate;
                reportItem.includeChildProjects = this.includeChildProjects;

                this.requestReportService.printProjectReport(reportItem);
            }
        });
    }

    private updateHeight(timeout = 0) {
        this.$timeout(() => {
            this.messagingService.publish(Constants.EVENT_RESIZE_WINDOW, null);
        }, timeout)

    }
}