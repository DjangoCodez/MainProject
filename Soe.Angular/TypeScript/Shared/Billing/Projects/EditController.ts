import { TimeProjectDTO, ProjectCentralStatusDTO, ProjectUserDTO } from "../../../Common/Models/ProjectDTO";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";
import { IAccountingSettingsRowDTO, IActionResult, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ProjectBudgetHelper } from "./Helpers/ProjectBudgetHelper";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IInvoiceService } from "../Invoices/InvoiceService";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { IProjectService } from "./ProjectService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Guid } from "../../../Util/StringUtility";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../Util/Enumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { NumberUtility } from "../../../Util/NumberUtility";
import { SelectProjectController } from "../../../Common/Dialogs/SelectProject/SelectProjectController";
import { CoreUtility } from "../../../Util/CoreUtility";
import { BudgetHeadDTO } from "../../../Common/Models/BudgetDTOs";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { SoeEntityState, Feature, CompanySettingType, TermGroup, SoeCategoryType, SoeCategoryRecordEntity, ProjectAccountType, TermGroup_TimeProjectPayrollProductAccountingPrio, TermGroup_TimeProjectInvoiceProductAccountingPrio, TermGroup_ProjectStatus } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IOrderService } from "../Orders/OrderService";
import { EditController as EditOrderController } from "../Orders/EditController";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { EditController as PriceListEditController } from "../../../Shared/Billing/Invoices/PriceLists/EditController";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Modal
    modal: any;
    isModal: boolean = false;

    private modalInstance: any;
    projectId: number;
    underProjectId: number;
    // Data
    project: TimeProjectDTO;
    categoryIds: number[];
    customers: SmallGenericType[] = [];
    allocationTypes: SmallGenericType[] = [];
    budgetRows: ProjectCentralStatusDTO[] = [];
    projects: any[] = [];
    projectsDict: SmallGenericType[] = [];
    timeCodes: any[] = [];
    private categoryRecords: any = [];
    projectStatuses: any[] = [];
    attestGroups: any[] = [];

    projectAccountSettingTypes: SmallGenericType[];
    accountingSettings: IAccountingSettingsRowDTO[];
    projectBaseAccounts: SmallGenericType[];
    useExistingDict: SmallGenericType[];
    orderTemplates: ISmallGenericType[];

    //Gui
    private traceRowsRendered = false;
    private projectUserRendered = false;
    private projectUserExpanderIsOpen = false;
    private pricelistExpanderIsOpen = false;
    private reloadAccountDims = false;

    //Prios
    private invoiceProductAccountingPrios: any[];
    private payrollProductAccountingPrios: any[];
    payrollProductAccountPriorityRows: any[];
    invoiceProductAccountPriorityRows: any[];
    accountingInvoicePrios: any[] = [];
    accountingPayrollPrios: any[] = [];
    invoiceprio1 = 0;
    invoiceprio2 = 0;
    invoiceprio3 = 0;
    invoiceprio4 = 0;
    invoiceprio5 = 0;
    payrollprio1 = 0;
    payrollprio2 = 0;
    payrollprio3 = 0;
    payrollprio4 = 0;
    payrollprio5 = 0;

    private _selectedCustomer;
    get selectedCustomer(): ISmallGenericType {
        return this._selectedCustomer;
    }
    set selectedCustomer(item: ISmallGenericType) {
        if (item && item.id > 0) {
            this._selectedCustomer = item;
            if (this.project)
                this.project.customerId = this._selectedCustomer.id;
        }
        else {
            this._selectedCustomer = undefined;
            if (this.project)
                this.project.customerId = undefined;
        }
    }

    //Pricelist
    private priceListGridOptions: ISoeGridOptionsAg;
    comparisonPriceLists: ISmallGenericType[];
    projectPriceLists: ISmallGenericType[];
    priceListGridData: any[] = [];
    pricelistName: string;

    //Project users
    projectUsers: ProjectUserDTO[] = [];

    //Permissions
    budgetPermission = false;
    attestFlowPermission = false;
    showProjectsWithoutCustomer = false;
    defaultPriceListType = false;
    modifyPermission = true;
    readonly = false;
    lockCustomer = false;

    budgethelper: ProjectBudgetHelper;

    //Flags
    loading = false;
    showExisting = false;
    showName = false;

    private _priceDate: any;
    get priceDate() {
        return this._priceDate;
    }
    set priceDate(item: any) {
        this._priceDate = item;
        this.loadPriceListGridData();
    }

    private _comparisonPricelistId: any;
    get comparisonPricelistId() {
        return this._comparisonPricelistId;
    }
    set comparisonPricelistId(item: any) {
        this._comparisonPricelistId = item;
        this.loadPriceListGridData();
    }

    private _loadAllProducts = false;
    get loadAllProducts(): boolean {
        return this._loadAllProducts;
    }
    set loadAllProducts(item: boolean) {
        this._loadAllProducts = item;
        this.loadPriceListGridData();
    }

    private _useExisting: any;
    get useExisting() {
        return this._useExisting;
    }
    set useExisting(item: any) {
        this._useExisting = item;
        switch (this.useExisting) {
            case 0:
                this.showExisting = false;
                this.showName = false;
                if (this.project)
                    this.project.priceListTypeId = undefined;
                this.pricelistName = '';
                this.loadPriceListGridData();
                break;
            case 1:
                this.showExisting = false;
                if (this.project)
                    this.project.priceListTypeId = undefined;
                this.showName = true;
                break;
            case 2:
                this.showExisting = true;
                this.showName = false;
                this.pricelistName = '';
                break;
        }
    }

    private _selectedOrderTemplate: ISmallGenericType;
    get selectedOrderTemplate() {
        return this._selectedOrderTemplate;
    }
    set selectedOrderTemplate(item: ISmallGenericType) {
        if (this._selectedOrderTemplate === item)
            return;

        this._selectedOrderTemplate = item;
        if (item && item.id > 0) {
            this.project.orderTemplateId = item.id;
        }
        else {
            this.project.orderTemplateId = undefined;
        }
        this.dirtyHandler.setDirty();
    }
    //@ngInject
    constructor(
        $uibModal,
        private $window,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private invoiceService: IInvoiceService,
        private commonCustomerService: ICommonCustomerService,
        private projectService: IProjectService,
        private orderService: IOrderService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private focusService: IFocusService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory    ) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.load())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.modalInstance = $uibModal;

        this._priceDate = CalendarUtility.getDateToday();
        this.budgethelper = new ProjectBudgetHelper();
        
        this.priceListGridOptions = new SoeGridOptionsAg("billing.products.product.stocktransactions", this.$timeout);

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {

            if (parameters && parameters.sourceGuid === this.guid) {
                return;
            }

            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.onInit(parameters);
            this.modal = parameters.modal;
            this.focusService.focusByName(parameters.id ? "ctrl_project_name" : "ctrl_project_number");
        });
    }

    onInit(parameters: any) {
        this.loading = true;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.underProjectId = parameters.underProjectId || 0;
        this.projectId = parameters.id || 0;

        this.flowHandler.start([
            { feature: Feature.Billing_Project_Edit, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_AttestFlow, loadModifyPermissions: true },
            { feature: Feature.Billing_Project_Edit_Budget, loadModifyPermissions: true },
            { feature: Feature.Time_Project_Invoice_ShowProjectsWithoutCustomer, loadModifyPermissions: true },
        ]);
    }

    private onDoLookups(): ng.IPromise<any> {
        if (this.projectId === 0) {
            this.new();
            return this.$q.all([
                 this.loadModifyPermissions(),
                 this.loadCompanySettings()]).then(() => {
                     this.$q.all([
                         this.loadProjects(),
                         this.loadCategories(),
                         this.loadCustomers(),
                         this.loadSettingTypes(),
                         this.loadAttestGroups(),
                         this.loadPayrollProductAccountingPriority(),
                         this.loadInvoiceProductAccountingPriority(),
                         this.loadAllocationTypes(),
                         this.loadOrderTemplates(),
                         this.loadProjectStatuses()]).then(() => {
                             this.setupPricelistGridColumns();
                             this.loadInvoiceProductAccountingPriorityRows();
                             this.loadPayrollProductAccountingPriorityRows();
                             this.budgethelper.setupBudgetValues(this.project, this.loading);
                             this.setSuggestedProjectNumber();
                             this.loading = false;
                         });
                 })
        }
        else {
            this.lockCustomer = true;
            
            return this.$q.all([
                this.loadModifyPermissions(),
                this.loadCompanySettings(),
            ]).then(() => {
                    this.$q.all([
                        this.loadProjects(),
                        this.loadCategories(),
                        this.loadCustomers(),
                        this.loadSettingTypes(),
                        this.loadAttestGroups(),
                        this.loadPayrollProductAccountingPriority(),
                        this.loadInvoiceProductAccountingPriority(),
                        this.loadAllocationTypes(),
                        this.loadOrderTemplates(),
                        this.loadProjectStatuses()]).then(() => {
                            this.$q.all([
                            ]).then(() => {
                                this.setupPricelistGridColumns();
                                this.loadInvoiceProductAccountingPriorityRows();
                                this.loadPayrollProductAccountingPriorityRows();
                                this.budgethelper.setupBudgetValues(this.project, this.loading);
                                this.setupWatchers();
                                this.loading = false;
                            })
                        });
                })
        }
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        // Expanders
        this.modifyPermission = response[Feature.Billing_Project_Edit].modifyPermission;
        this.budgetPermission = response[Feature.Billing_Project_Edit_Budget].modifyPermission;
        this.attestFlowPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow].modifyPermission;
        if (this.budgetPermission == true)
            this.showProjectsWithoutCustomer = response[Feature.Time_Project_Invoice_ShowProjectsWithoutCustomer].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null);
        const statusGroup = ToolBarUtility.createGroup();

        statusGroup.buttons.push(new ToolBarButton("", "billing.projects.list.unlockcustomer", IconLibrary.FontAwesome, "fa-unlock-alt", () => {
            this.unlockCustomer();
        }, null, () => {
            return !(this.project.projectId > 0);
        }));

        statusGroup.buttons.push(new ToolBarButton("", "billing.projects.list.openprojectcentral", IconLibrary.FontAwesome, "fa-calculator-alt", () => {
            this.openProjectCentral();
        }, null, () => {
            return !(this.project.projectId > 0);
        }, { newTabLink: this.getProjectCentralUrl()}));

        statusGroup.buttons.push(new ToolBarButton("", "billing.projects.list.createunderproject", IconLibrary.FontAwesome, "fa-plus", () => {
            this.createUnderProject();
        }, null, () => {
            return !(this.project.projectId > 0);
        }));

        this.toolbar.addButtonGroup(statusGroup);
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.project.priceListTypeId, () => {
            this.loadPriceListGridData();
        });
    }

    // LOOKUPS

    private loadModifyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [
            Feature.Billing_Project_Edit,
            Feature.Billing_Project_Edit_Budget,
            Feature.Time_Project_Invoice_ShowProjectsWithoutCustomer,
            Feature.Economy_Supplier_Invoice_AttestFlow
        ];

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.modifyPermission = x[Feature.Billing_Project_Edit];
            this.budgetPermission = x[Feature.Billing_Project_Edit_Budget];
            this.attestFlowPermission = x[Feature.Economy_Supplier_Invoice_AttestFlow];
            if (this.budgetPermission === true)
                this.showProjectsWithoutCustomer = x[Feature.Time_Project_Invoice_ShowProjectsWithoutCustomer];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            CompanySettingType.ProjectOverheadCostAsFixedAmount,
            CompanySettingType.ProjectOverheadCostAsAmountPerHour,
            CompanySettingType.BillingDefaultPriceListType
        ];
        
        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.budgethelper.overheadCostAsFixedAmount = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectOverheadCostAsFixedAmount);
            this.budgethelper.overheadCostAsAmountPerHour = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectOverheadCostAsAmountPerHour);
            this.defaultPriceListType = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingDefaultPriceListType);
        });
    }

    private loadAllocationTypes(): ng.IPromise<any> {
        this.allocationTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.ProjectAllocationType, false, true).then((x) => {
            this.allocationTypes = x;
        });
    }

    private loadCustomers(): ng.IPromise<any> {
         return this.commonCustomerService.getCustomersDict(true, true, true).then((x) => {
            this.customers = x;
        })
    }

    private loadOrderTemplates(useCache = true): ng.IPromise<any> {
        return this.orderService.getOrderTemplates(useCache).then(x => {
            this.orderTemplates = x;
        });
    }

    private loadProjects(): ng.IPromise<any> {
        this.projectsDict = [];
        return this.invoiceService.getProjectsSmall(true, true, true).then((x) => {
            this.projects = x;
            this.projectsDict.push({ id: null, name: "" });
            for (let i = 0; i < this.projects.length; i++) {
                const row = this.projects[i];
                if ((!this.projectId || row.projectId !== this.projectId) && row.status < 4)
                    this.projectsDict.push({ id: row.projectId, name: row.number + ' ' + row.name });

            }

            if (this.isNew)
                if (this.underProjectId > 0)
                    this.project.parentProjectId = this.underProjectId;
        })
    }

    private loadCategories(): ng.IPromise<any> {
        this.categoryIds = [];
        const tempcategoryIds = [];
        return this.coreService.getCompanyCategoryRecords(SoeCategoryType.Project, SoeCategoryRecordEntity.Project, this.projectId, false).then((x) => {
            
            _.forEach(x, (row: any) => {
                tempcategoryIds.push(row.categoryId);
                
            });
            
            this.categoryIds = tempcategoryIds;
        });
    }

    private loadAttestGroups(): ng.IPromise<any> {
        return this.projectService.getAttestWorkFlowGroupsDict(true).then((x) => {
            this.attestGroups = x;
        });
    }

    private openProjectUserExpander() {
        if (!this.projectUserRendered) {
            if (this.projectId) {
                this.loadProjectPersons().then(() => {
                    this.projectUserRendered = true;
                })
            }
            else {
                this.projectUserRendered = true;
            }
        }
    }

    private openPricelistExpander() {
        if (!this.pricelistExpanderIsOpen) {
            this.pricelistExpanderIsOpen = true;
            this.loadPriceLists().then(x => {
                this.loadPriceListGridData();
            });
        }
    }

    private loadSettingTypes(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.projects.list.accountincome",
            "billing.projects.list.accountcost",
            "billing.products.products.accountingsettingtype.salesnovat",
            "billing.products.products.accountingsettingtype.salescontractor"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.projectAccountSettingTypes = [];
            this.projectAccountSettingTypes.push(new SmallGenericType(ProjectAccountType.Debit, terms["billing.projects.list.accountcost"]));
            this.projectAccountSettingTypes.push(new SmallGenericType(ProjectAccountType.Credit, terms["billing.projects.list.accountincome"]));
            this.projectAccountSettingTypes.push(new SmallGenericType(ProjectAccountType.SalesNoVat, terms["billing.products.products.accountingsettingtype.salesnovat"]));
            this.projectAccountSettingTypes.push(new SmallGenericType(ProjectAccountType.SalesContractor, terms["billing.products.products.accountingsettingtype.salescontractor"]));
        });
    }

    private loadPayrollProductAccountingPriority() {
        this.payrollProductAccountingPrios = [];
        return this.coreService.getTermGroupContent(TermGroup.TimeProjectPayrollProductAccountingPrio, false, false).then(x => {

            var notUsed = _.find(x, { id: TermGroup_TimeProjectPayrollProductAccountingPrio.NotUsed });
            if (notUsed)
                this.payrollProductAccountingPrios.push(notUsed);

            var employee = _.find(x, { id: TermGroup_TimeProjectPayrollProductAccountingPrio.EmploymentAccount });
            if (employee)
                this.payrollProductAccountingPrios.push(employee);

            var project = _.find(x, { id: TermGroup_TimeProjectPayrollProductAccountingPrio.Project });
            if (project)
                this.payrollProductAccountingPrios.push(project);

            var customer = _.find(x, { id: TermGroup_TimeProjectPayrollProductAccountingPrio.Customer });
            if (customer)
                this.payrollProductAccountingPrios.push(customer);

            var payrollProduct = _.find(x, { id: TermGroup_TimeProjectPayrollProductAccountingPrio.PayrollProduct });
            if (payrollProduct)
                this.payrollProductAccountingPrios.push(payrollProduct);

            var employeeGroup = _.find(x, { id: TermGroup_TimeProjectPayrollProductAccountingPrio.EmployeeGroup });
            if (employeeGroup)
                this.payrollProductAccountingPrios.push(employeeGroup);
        });
    }

    private loadInvoiceProductAccountingPriority() {
        this.invoiceProductAccountingPrios = [];
        return this.coreService.getTermGroupContent(TermGroup.TimeProjectInvoiceProductAccountingPrio, false, false).then(x => {

            const notUsed = _.find(x, { id: TermGroup_TimeProjectInvoiceProductAccountingPrio.NotUsed });
            if (notUsed)
                this.invoiceProductAccountingPrios.push(notUsed);

            const employee = _.find(x, { id: TermGroup_TimeProjectInvoiceProductAccountingPrio.EmploymentAccount });
            if (employee)
                this.invoiceProductAccountingPrios.push(employee);

            const project = _.find(x, { id: TermGroup_TimeProjectInvoiceProductAccountingPrio.Project });
            if (project)
                this.invoiceProductAccountingPrios.push(project);

            const customer = _.find(x, { id: TermGroup_TimeProjectInvoiceProductAccountingPrio.Customer });
            if (customer)
                this.invoiceProductAccountingPrios.push(customer);

            const invoiceProduct = _.find(x, { id: TermGroup_TimeProjectInvoiceProductAccountingPrio.InvoiceProduct });
            if (invoiceProduct)
                this.invoiceProductAccountingPrios.push(invoiceProduct);
        });
    }

    private loadPayrollProductAccountingPriorityRows() {
        if (!this.project.payrollProductAccountingPrio) {
            this.project.payrollProductAccountingPrio = "0, 0, 0, 0, 0";
        }

        const array = this.project.payrollProductAccountingPrio.split(',');
        let counter = 1;
        _.forEach(array, (row) => {
            if (counter === 1)
                this.payrollprio1 = NumberUtility.tryParseInt(row, 0);
            if (counter === 2)
                this.payrollprio2 = NumberUtility.tryParseInt(row, 0);
            if (counter === 3)
                this.payrollprio3 = NumberUtility.tryParseInt(row, 0);
            if (counter === 4)
                this.payrollprio4 = NumberUtility.tryParseInt(row, 0);
            if (counter === 5)
                this.payrollprio5 = NumberUtility.tryParseInt(row, 0);
            counter = counter + 1;
        });

    }

    private loadUseExistingTypes(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.projects.list.nopricelist",
            "billing.projects.list.newpricelist",
            "billing.projects.list.existingpricelist"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.useExistingDict = [];
            this.useExistingDict.push(new SmallGenericType(0, terms["billing.projects.list.nopricelist"]));
            this.useExistingDict.push(new SmallGenericType(1, terms["billing.projects.list.newpricelist"]));
            this.useExistingDict.push(new SmallGenericType(2, terms["billing.projects.list.existingpricelist"]));

            //Set default
            this.loadAllProducts = false;
            this.useExisting = 0;
        });
    }

    private loadPriceLists(useCache = true, setPriceListId = undefined): ng.IPromise<any> {
        this.comparisonPriceLists = [];
        this.projectPriceLists = [];

        return this.translationService.translate("billing.projects.list.nopricelist").then((term) => {
            return this.commonCustomerService.getPriceListsDict(false, useCache).then(x => {
                this.projectPriceLists.push(new SmallGenericType(0, term));
                _.forEach(x, (y) => {
                    this.comparisonPriceLists.push(y);
                    this.projectPriceLists.push(y);
                });
                
                if (setPriceListId && setPriceListId > 0)
                    this.project.priceListTypeId = setPriceListId;

                //Set default
                this.comparisonPricelistId = this.defaultPriceListType;
            });
        });
    }

    private loadInvoiceAccountingPriority(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceProductAccountingPrio, true, false).then(x => {
            this.accountingInvoicePrios = x;
        });
    }

    private loadPayrollAccountingPriority(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollProductAccountingPrio, true, false).then(x => {
            this.accountingPayrollPrios = x;
        });
    }

    private loadInvoiceProductAccountingPriorityRows() {
        if (!this.project.invoiceProductAccountingPrio) {
            this.project.invoiceProductAccountingPrio = "0,0,0,0,0";
        }

        const array = this.project.invoiceProductAccountingPrio.split(',');
        let counter = 1;
        _.forEach(array, (row) => {
            if (counter === 1)
                this.invoiceprio1 = NumberUtility.tryParseInt(row, 0);
            if (counter === 2)
                this.invoiceprio2 = NumberUtility.tryParseInt(row, 0);
            if (counter === 3)
                this.invoiceprio3 = NumberUtility.tryParseInt(row, 0);
            if (counter === 4)
                this.invoiceprio4 = NumberUtility.tryParseInt(row, 0);
            if (counter === 5)
                this.invoiceprio5 = NumberUtility.tryParseInt(row, 0);
            counter = counter + 1;
        });

    }

    private loadProjectStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ProjectStatus, true, false).then(x => {
            this.projectStatuses = x;
        });
    }

    private load(resetProject = false): ng.IPromise<any> {
        if (this.projectId) {
            return this.projectService.getProject(this.projectId).then((x) => {

                this.dirtyHandler.clean();
                this.isNew = false;
                this.project = x;
                this.project.startDate = CalendarUtility.convertToDate(this.project.startDate);
                this.project.stopDate = CalendarUtility.convertToDate(this.project.stopDate);

                this.selectedCustomer = _.find(this.customers, c => c.id === this.project.customerId);
                this.setSelectedOrderTemplate(this.project.orderTemplateId);

                if (this.project.priceListTypeId && this.project.priceListTypeId > 0) {
                    this.showExisting = true;
                }

                if (resetProject)
                    this.budgethelper.setupBudgetValues(this.project, false);

                if (this.project.parentProjectId && this.project.parentProjectId > 0) {
                    if (!_.find(this.projectsDict, (p) => p.id === this.project.parentProjectId))
                        this.projectsDict.push({ id: this.project.parentProjectId, name: this.project.parentProjectNr + ' ' + this.project.parentProjectName });
                }
            });
        }
        else {
            const deferral = this.$q.defer();
            deferral.resolve();
            return deferral.promise;
        }
    }

    private setSelectedOrderTemplate(orderTemplateId: number) {
        this._selectedOrderTemplate = _.find(this.orderTemplates, c => c.id === orderTemplateId);
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.dirtyHandler.clean();
            //categories
            this.categoryRecords = [];
            _.forEach(this.categoryIds, (id) => {
                this.categoryRecords.push({
                    categoryId: id,
                    default: false,
                });
            });

            //accounting priorities
            const prioString = this.payrollprio1 + "," + this.payrollprio2 + "," + this.payrollprio3 + "," + this.payrollprio4 + "," + this.payrollprio5;
            this.project.payrollProductAccountingPrio = prioString;

            const prioInvString = this.invoiceprio1 + "," + this.invoiceprio2 + "," + this.invoiceprio3 + "," + this.invoiceprio4 + "," + this.invoiceprio5;
            this.project.invoiceProductAccountingPrio = prioInvString;

            //Null not accepted
            if (!this.project.payrollProductAccountingPrio) {
                this.project.payrollProductAccountingPrio = "0,0,0,0,0";
            }
            if (!this.project.invoiceProductAccountingPrio) {
                this.project.invoiceProductAccountingPrio = "0,0,0,0,0";
            }
            if (!this.projectUsers) {
                this.projectUsers = [];
            }

            const list: any[] = [];
            _.forEach(_.filter(this.priceListGridData, p => p.priceChanged === true), (p) => {
                list.push({ key: p.productId, value: p.price });
            });

            let newPricelist = false;
            if (this.showName && this.pricelistName)
                newPricelist = true;
            
            
            this.projectService.saveProject(this.project, list, this.categoryRecords, null, this.projectUsers.filter(u => u.isModified), newPricelist, this.pricelistName).then((result: IActionResult) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        this.projectId = result.integerValue;
                        this.project.projectId = this.projectId;
                    }
                    
                    if (newPricelist) {
                        this.loadPriceLists(false);
                        //clear cache for order
                        this.commonCustomerService.getPriceLists(false);
                        this.showName = false;
                    }
                    this.notifyProjectCentral();
                    this.setTabLabel();
                    this.lockCustomer = true;
                    if (this.isModal)
                        this.closeModal();
                    else {
                        this.reloadAccountDims = result.booleanValue; 
                        
                        //notify account dims in header
                        this.$scope.$broadcast('accountDims_reload', null);
                        //notify account settings expander
                        this.$scope.$broadcast('reloadAccounts', {guid:this.guid});
                                                
                        if (result.booleanValue || result.booleanValue2) {
                            this.$timeout(() => {
                                this.load()
                            }, 2000);
                        }
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.project);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });


        }, this.guid)
            .then(data => {
        }, error => { });
    }

    private setTabLabel() {
        let tabLabel: string;
        const keys: string[] = [
            "billing.projects.list.project"
        ]

        this.translationService.translateMany(keys).then((terms) => {
            tabLabel = `${terms["billing.projects.list.project"]} ${this.project.number || ""}`

            this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
                guid: this.guid,
                label: tabLabel
            });
        });
    }

    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.projectService.deleteProject(this.projectId).then((result) => {
                if (result.success) {
                    completion.completed(this.project);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    // EVENTS

    // ACTIONS
    private notifyProjectCentral() {
        this.messagingService.publish(Constants.EVENT_REFRESH_PROJECTCENTRALDATA, { projectEdit: true, projectId: this.projectId });
    }

    public closeModal() {
        if (this.isModal) {
            if (this.projectId) {
                this.modal.close({ id: this.projectId, number: this.project.number });
            } else {
                this.modal.dismiss();
            }
        }
    }

    private showSelectProject(): any {

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectProject", "selectproject.html"),
            controller: SelectProjectController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                projects: () => { return undefined },
                customerId: () => { return undefined },
                projectsWithoutCustomer: () => { return this.showProjectsWithoutCustomer },
                showFindHidden: () => { return null },
                loadHidden: () => { return null },
                useDelete: () => { return false },
                currentProjectNr: () => { return null },
                currentProjectId: () => { return null },
                excludedProjectId: () => { return this.projectId },
                showAllProjects: () => { return false },
            }
        });

        modal.result.then(selectedProject => {
            if (selectedProject) {
                this.project.parentProjectId = selectedProject.projectId; 
                this.dirtyHandler.setDirty();
            }
        }, function () {
        });

        return modal;
    }

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.projectId = 0;
        this.project = new TimeProjectDTO();
        this.projectUsers = [];
        this.project.state = SoeEntityState.Active;
        this.project.status = TermGroup_ProjectStatus.Active;
        this.project.payrollProductAccountingPrio = "0,0,0,0,0";
        this.project.invoiceProductAccountingPrio = "0,0,0,0,0";
        if (this.underProjectId > 0)
            this.project.parentProjectId = this.underProjectId;
        this.categoryRecords = [];
        this.project.budgetHead = new BudgetHeadDTO();
        this.project.budgetHead.actorCompanyId = CoreUtility.actorCompanyId;
        this.project.budgetHead.name = "Proj";
        this.project.number = '';
    }

    private unlockCustomer() {
        //unlocks customeredit
        this.lockCustomer = false;
    }

    private openProjectCentral() {
        if (this.projectId)
            HtmlUtility.openInSameTab(this.$window, this.getProjectCentralUrl());
    }

    private getProjectCentralUrl() {
        return "/soe/billing/project/central/?project=" + this.projectId;
    }

    private openOrderTemplate() {

        const keys: string[] = [
            "common.customer.customer.ordertemplate"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            const label: string = this.selectedOrderTemplate ? this.selectedOrderTemplate.name : terms["common.customer.customer.ordertemplate"];

            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Orders/Views/edit.html"),
                controller: EditOrderController,
                controllerAs: 'ctrl',
                bindToController: true,
                backdrop: 'static',
                size: 'xl',
                windowClass: 'fullsize-modal',
                scope: this.$scope
            });

            modal.rendered.then(() => {
                this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, sourceGuid: this.guid, label: label, id: this.project.orderTemplateId, isTemplate: true });
            });

            modal.result.then(result => {
                if (result.invoiceId) {
                    this.loadOrderTemplates(false).then(() => {
                        this.project.orderTemplateId = result.invoiceId; 
                        this.dirtyHandler.setDirty();
                        this.setSelectedOrderTemplate(this.project.orderTemplateId);
                    })
                }
            });

        });
    }

    private createUnderProject() {
        const keys: string[] = [
            "billing.projects.list.new_project"
        ];
        this.translationService.translateMany(keys).then(terms => {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(terms["billing.projects.list.new_project"], "", EditController, { Id: 0, underProjectId: this.projectId }, this.urlHelperService.getGlobalUrl('Billing/Projects/Views/edit.html')));
        });
    }

    //Pricelist
    private setupPricelistGridColumns(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.projects.list.productnr",
            "common.name",
            "billing.projects.list.purchaseprice",
            "billing.projects.list.comparisonprice",
            "billing.projects.list.price",
            "billing.products.pricelists.startdate",
            "billing.products.pricelists.stopdate",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.priceListGridOptions.enableGridMenu = false;
            this.priceListGridOptions.enableFiltering = true;
            this.priceListGridOptions.enableRowSelection = false;
            //this.priceListGridOptions.enableSingleSelection();
            this.priceListGridOptions.setMinRowsToShow(15);

            this.priceListGridOptions.addColumnText("number", terms["billing.projects.list.productnr"], null);
            this.priceListGridOptions.addColumnText("name", terms["common.name"], null);
            this.priceListGridOptions.addColumnNumber("purchasePrice", terms["billing.projects.list.purchaseprice"], null, { decimals: 2 });
            this.priceListGridOptions.addColumnNumber("comparisonPrice", terms["billing.projects.list.comparisonprice"], null, { decimals: 2 });
            this.priceListGridOptions.addColumnNumber("price", terms["billing.projects.list.price"], null, { decimals: 2, editable: true, onChanged: this.productPriceChanged.bind(this) });
            this.priceListGridOptions.addColumnDate("startDate", terms["billing.products.pricelists.startdate"], null);
            this.priceListGridOptions.addColumnDate("stopDate", terms["billing.products.pricelists.stopdate"], null);

            this.priceListGridOptions.addTotalRow("#totals-grid", {
                filtered: terms["core.aggrid.totals.filtered"],
                total: terms["core.aggrid.totals.total"]
            });

            this.priceListGridOptions.finalizeInitGrid();
        });
    }

    protected productPriceChanged(row) {
        if (row.data) {
            row.data['priceChanged'] = true; 
            this.dirtyHandler.setDirty();
        }
    }

    protected priceDateChanged() {
        this.loadPriceListGridData();
    }

    private loadPriceListGridData(): ng.IPromise<any> {
        if (!this.pricelistExpanderIsOpen)
            return;

        return this.progress.startLoadingProgress([() => {
            const ptid = this.comparisonPricelistId ? this.comparisonPricelistId : 0;
            const prid = this.project && this.project.priceListTypeId ? this.project.priceListTypeId : 0;
            return this.projectService.getPricelists(ptid, prid, this.loadAllProducts, this.priceDate).then(x => {
                this.priceListGridData = _.sortBy(x, 'numberName');
                const startDate = new Date("1901-01-02");
                const stopDate = new Date("9998-12-31");
                _.forEach(this.priceListGridData, (row: any) => {
                    var rowStart = CalendarUtility.convertToDate(row.startDate);
                    var rowStop = CalendarUtility.convertToDate(row.stopDate);
                    if (rowStart < startDate)
                        row.startDate = null;

                    if (rowStop > stopDate)
                        row.stopDate = null;


                });
                this.priceListGridOptions.setData(this.priceListGridData);
            });
        }]);
    }

    private loadProjectPersons(): ng.IPromise<any> {
        return this.projectService.getProjectUsers(this.projectId, true).then(x => {
            this.projectUsers = x;
        });
    }

    setSuggestedProjectNumber() {
        if (this.projects) {
            const numerics: number[] = _.sortBy(_.map(_.filter(this.projects, p => Number(p.number)), p => +p.number));
            this.project.number = (numerics[numerics.length - 1] + 1).toString();
        }
        else {
            this.project.number = '0';
        }
    };

    private addPriceList(edit = false) {
        let term = edit ? "billing.projects.list.pricelist" : "billing.projects.list.newpricelist";
        this.translationService.translate(term).then(term => {
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Invoices/PriceLists/Views/edit.html"),
                controller: PriceListEditController,
                controllerAs: 'ctrl',
                bindToController: true,
                backdrop: 'static',
                size: 'md',
                windowClass: 'fullsize-modal',
                scope: this.$scope,
                resolve: {
                    translationService: () => { return this.translationService },
                    coreService: () => { return this.coreService },
                    invoiceService: () => { return this.invoiceService }
                }
            });

            modal.rendered.then(() => {
                this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, sourceGuid: this.guid, title: term, id: edit ? this.project.priceListTypeId : 0 });
            });

            modal.result.then(result => {
                if (result && result.id)
                    this.loadPriceLists(false, result.id);
            });
        });
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            const errors = this['edit'].$error;
            // Mandatory fields
            if (this.project) {
                if (!this.project.number) {
                    if (errors['maxlength']) {
                        if (_.some(errors['maxlength'], (e) => e.$name === 'ctrl_project_number'))
                            validationErrorKeys.push("billing.projects.list.numbermaxvalidation");
                        else
                            mandatoryFieldKeys.push("common.name");
                    }
                    else {
                        mandatoryFieldKeys.push("common.number");
                    }
                }
                if (!this.project.name) {
                    if (errors['maxlength']) {
                        if (_.some(errors['maxlength'], (e) => e.$name === 'ctrl_project_name'))
                            validationErrorKeys.push("billing.projects.list.namemaxvalidation");
                        else
                            mandatoryFieldKeys.push("common.name");
                    }
                    else {
                        mandatoryFieldKeys.push("common.name");
                    }
                }
            }
            if (errors['numberInUse'])
                validationErrorKeys.push("billing.projects.list.numberinuse");

            if (errors['invalidDates'])
                validationErrorKeys.push("billing.projects.list.invaliddates");

            if (errors['nameIsEmpty'])
                validationErrorKeys.push("billing.projects.list.missingpricelistname");

            if (errors['nameInUse'])
                validationErrorKeys.push("billing.projects.list.invalidpricelistname");

            if (errors['invalidPricelist'])
                validationErrorKeys.push("billing.projects.list.missingprojectpricelist");
        });
    }
}