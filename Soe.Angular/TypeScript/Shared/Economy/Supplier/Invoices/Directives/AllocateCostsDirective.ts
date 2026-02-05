import { SupplierInvoiceDTO, SupplierInvoiceCostAllocationDTO } from "../../../../../Common/Models/InvoiceDTO";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ISupplierService } from "../../../../../Shared/Economy/Supplier/SupplierService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { IPriceListTypeMarkupDTO, IProductSmallDTO } from "../../../../../Scripts/TypeLite.Net4";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { Feature, TermGroup_ProjectStatus, SoeEntityState, TermGroup_ProjectType, SoeOriginType, SoeTimeCodeType, UserSettingType, CompanySettingType} from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { TypeAheadOptionsAg, IColumnAggregations } from "../../../../../Util/SoeGridOptionsAg";
import { IControllerFlowHandlerFactory } from "../../../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../../Core/Handlers/messaginghandlerfactory";
import { GridControllerBase2Ag } from "../../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../../Core/ICompositionGridController";
import { Guid, StringUtility } from "../../../../../Util/StringUtility";
import { TabMessage } from "../../../../../Core/Controllers/TabsControllerBase1";
import { EditController as BillingProjectsEditController } from "../../../../../Shared/Billing/Projects/EditController";
import { EditController as BillingOrdersEditController } from "../../../../../Shared/Billing/Orders/EditController";
import { IPermissionRetrievalResponse } from "../../../../../Core/Handlers/ControllerFlowHandler";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { IProductService } from "../../../../Billing/Products/ProductService";
import { uiGridEditDropdownWithFocusDelay } from "../../../../../Core/UiGridPatches/uiGridEditDropdownWithFocusDelay";
import { IInvoiceService } from "../../../../Billing/Invoices/InvoiceService";

export class AllocateCostsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/allocateCosts.html"),
            scope: {
                invoice: "=",
                invoiceIsLoaded: "=",
                linkToOrderOrderSet: "=",
                linkToProjectProjectSet: "=",
                linkToProjectTimeCodeSet: "=",
                defaultTimeCodeId: "=",
                chargeCostsToProject: "=",
                parentGuid: "=",
                transactionCurrencyRate: "=",
            },
            restrict: 'E',
            replace: true,
            controller: AllocateCostsDirective,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class AllocateCostsDirective extends GridControllerBase2Ag implements ICompositionGridController {
    private invoice: SupplierInvoiceDTO;
    private parentGuid: Guid;

    // Initial values
    private initialFiltering = true;

    private transactionCurrencyRate: number;

    //Terms
    terms: { [index: string]: string; };

    // Properties
    private _showHiddenProjects: any;
    get showHiddenProjects() {
        return this._showHiddenProjects;
    }
    set showHiddenProjects(item: any) {
        this._showHiddenProjects = item;
        this.filterProjectsByStatus();
        this.filterCustomerInvoicesByProject(this.project.projectId);
    }

    // Permissions
    private editProjectPermission = false;
    private editOrderPermission = false;

    //Collections
    priceListTypeMarkups: IPriceListTypeMarkupDTO[] = [];
    projects: any[] = [];
    filteredProjects: any[] = [];
    customerInvoices: any[] = [];
    filteredCustomerInvoices: any[] = [];
    timecodes: any[] = [];
    employees: any[] = [];
    project: any = {};
    products: IProductSmallDTO[];

    // Variables
    public defaultTimeCodeId: number;
    public linkToOrderValidityText: string;
    public delayAddRowParams: any;

    // Flags
    public lookupsLoaded = false;
    public linkToOrderOrderSet: boolean;
    public linkToProjectProjectSet: boolean;
    public linkToProjectTimeCodeSet: boolean;
    public chargeCostsToProject: boolean;
    public delayAddRow: boolean;

    // Settings
    private miscProductId = 0;
    private productSearchMinPrefixLength = 0;
    private productSearchMinPopulateDelay = 0;

    // Functions
    buttonFunctions: any = [];

    //@ngInject
    constructor($http,
        $templateCache,
        private $timeout: ng.ITimeoutService,
        $uibModal,
        private coreService: ICoreService,
        private invoiceService: IInvoiceService,
        private productService: IProductService,
        private supplierService: ISupplierService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
        super(gridHandlerFactory, "common.directives.supplierinvoiceallocatedcosts", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onDoLookUp(() => this.onDoLookups())
            .onSetUpGrid(() => this.setupGrid())

        this.onInit({});
    }

    //SETUP
    onInit(parameters: any) {
        this.parameters = parameters;

        this.showHiddenProjects = false;

        this.flowHandler.start([
            { feature: Feature.Billing_Project_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_Orders_Edit, loadReadPermissions: true, loadModifyPermissions: true },
        ]);

        this.$scope.$on('costsToProjectChanged', (e, a) => {
            if(a.guid === this.parentGuid)
                this.setGridData(false);
        });

        this.$scope.$on('addProjectRow', (e, a) => {
            if (a.guid === this.parentGuid) {
                if (this.lookupsLoaded)
                    this.addRow(true, a.project, a.timeCode, a.amount, a.setFocus)
                else {
                    this.delayAddRowParams = a;
                    this.delayAddRow = true;
                }
            }
        });

        this.$scope.$on('stopEditingCost', (e, a) => {
            this.gridAg.options.stopEditing(false);
            this.$timeout(() => {
                a.functionComplete("costAllocation");
            }, 0)
        });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.modifyPermission = response[Feature.Billing_Project_Edit].modifyPermission || response[Feature.Billing_Order_Orders_Edit].modifyPermission;
        this.readPermission = response[Feature.Billing_Project_Edit].readPermission || response[Feature.Billing_Order_Orders_Edit].readPermission;
        this.editProjectPermission = response[Feature.Billing_Project_Edit].modifyPermission;
        this.editOrderPermission = response[Feature.Billing_Order_Orders_Edit].modifyPermission;
    }

    private setupWatchers() {
        if (this.invoice) {
            this.$scope.$watch(() => this.invoice.supplierInvoiceCostAllocationRows, (newValue, oldValue) => {
                if (newValue != oldValue) {
                    this.setGridData(false);
                }
            });
        }
        else {
            /*this.$scope.$watch(() => this.invoice, (newValue, oldValue) => {
                if (newValue && newValue != oldValue) {
                    this.$scope.$watch(() => this.invoice.supplierInvoiceCostAllocationRows, (newValue, oldValue) => {
                        if (newValue != oldValue) {
                            console.log("supplierInvoiceCostAllocationRows 2", newValue, oldValue)
                            this.setGridData(false);
                        }
                    });
                }
            });*/
        }
    }

    private onDoLookups() {
        return this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions(),
            this.loadCompanySettings(),
            this.loadUserSettings(),
            this.loadPriceListTypeMarkups(),
            this.loadAllProducts(),
            this.loadCustomerProjects(),
            this.loadCustomerInvoices(),
            this.loadCustomerTimeCodes(),
            this.loadCustomerEmployees()]).then(() => {
                this.lookupsLoaded = true;
                this.setupWatchers();
                this.filterProjectsByStatus();
                this.filterCustomerInvoicesByProject(0);
                if (this.invoice) 
                    this.setGridData(false);
                if (this.delayAddRow)
                    this.addRow(true, this.delayAddRowParams.project, this.delayAddRowParams.timeCode, this.delayAddRowParams.amount, this.delayAddRowParams.setFocus);

                this.messagingService.publish(Constants.EVENT_COSTALLOCATIONDIRECTIVE_SETUP, this.parentGuid);
            });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.remove",
            "common.amount",
            "common.employee",
            "economy.supplier.invoice.project",
            "economy.supplier.invoice.surcharge",
            "economy.supplier.invoice.timecodes",
            "common.sum",
            "economy.supplier.invoice.customerinvoice",
            "economy.supplier.invoice.chargecosttoproject",
            "economy.supplier.invoice.showimage",
            "economy.supplier.invoice.includeimage",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "common.order",
            "common.customer.customer.orderproject",
            "economy.supplier.invoice.linktoproject",
            "economy.supplier.invoice.orders",
            "economy.supplier.invoice.productnr",
            "common.customer.customer.product.name",
            "economy.supplier.invoice.transferedtorder",
            "economy.supplier.invoice.connectedtoproject",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.buttonFunctions.push({ id: CostAllocationButtonFunctions.TransferToOrder, name: terms["economy.supplier.invoice.orders"] });
            this.buttonFunctions.push({ id: CostAllocationButtonFunctions.ConnectToProject, name: terms["economy.supplier.invoice.linktoproject"] });
        });
    }

    public setupGrid(): void {
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.options.setMinRowsToShow(10);

        const colGeneralHeader = this.gridAg.options.addColumnHeader("general", " ", null);
        colGeneralHeader.marryChildren = true;

        this.gridAg.addColumnIsModified(null, null, null, colGeneralHeader);
        this.gridAg.addColumnIcon("rowTypeIcon", null, 40, { toolTipField: "rowTypeTooltip", suppressSorting: false }, colGeneralHeader);

        var projectOptions = new TypeAheadOptionsAg();
        projectOptions.source = (filter) => this.filterProjects(filter);
        projectOptions.displayField = "label"
        projectOptions.dataField = "label";
        projectOptions.minLength = 0;
        projectOptions.delay = 0;
        projectOptions.useScroll = true;
        projectOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef);
        this.gridAg.addColumnTypeAhead("projectName", this.terms["economy.supplier.invoice.project"], null, { typeAheadOptions: projectOptions, editable: true, suppressSorting: true }, null, colGeneralHeader);
        this.gridAg.addColumnIcon(null, null, null, { icon: "iconEdit fal fa-pencil", onClick: this.openProject.bind(this), toolTip: this.terms["common.customer.customer.orderproject"], showIcon: (r) => r.projectId && this.editProjectPermission }, colGeneralHeader);

        var invoiceOptions = new TypeAheadOptionsAg();
        invoiceOptions.source = (filter) => this.filterCustomerInvoices(filter);
        invoiceOptions.displayField = "label"
        invoiceOptions.dataField = "label";
        invoiceOptions.minLength = 0;
        invoiceOptions.delay = 0;
        invoiceOptions.useScroll = true;
        invoiceOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef);
        this.gridAg.addColumnTypeAhead("customerInvoiceNumberName", this.terms["economy.supplier.invoice.customerinvoice"], null, { typeAheadOptions: invoiceOptions, editable: true, suppressSorting: true }, null, colGeneralHeader);
        this.gridAg.addColumnIcon(null, null, null, { icon: "iconEdit fal fa-pencil", onClick: this.openOrder.bind(this), toolTip: this.terms["common.order"], showIcon: (r) => r.orderId && this.editOrderPermission }, colGeneralHeader);

        if (this.editOrderPermission) {
            const colOrderHeader = this.gridAg.options.addColumnHeader("order", this.terms["economy.supplier.invoice.orders"], { enableHiding: true });
            //colOrderHeader.marryChildren = true;
            
            const productOptions = new TypeAheadOptionsAg();
            productOptions.source = (filter) => this.filterProducts(filter);
            productOptions.minLength = this.productSearchMinPrefixLength;
            productOptions.delay = this.productSearchMinPopulateDelay;
            productOptions.displayField = "numberName"
            productOptions.dataField = "number";
            productOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead.bind(value, colDef);
            productOptions.useScroll = true;
            this.gridAg.addColumnTypeAhead("productNr", this.terms["economy.supplier.invoice.productnr"], 100, { enableHiding: true, error: 'productError', typeAheadOptions: productOptions, editable: (r) => { return !r.projectLock && !r.isReadOnly } }, null, colOrderHeader);
            this.gridAg.addColumnText("productName", this.terms["common.customer.customer.product.name"], 100, null, { enableHiding: true, editable: (r) => { return !r.projectLock && !r.isReadOnly } }, colOrderHeader);

            this.gridAg.addColumnNumber("rowAmountCurrency", this.terms["common.amount"], null, { enableHiding: false, decimals: 2, editable: (r) => { return !r.projectLock && !r.isReadOnly } }, colOrderHeader);
            this.gridAg.addColumnNumber("supplementCharge", this.terms["economy.supplier.invoice.surcharge"], null, { enableHiding: false, decimals: 2, editable: (r) => { return !r.projectLock && !r.isReadOnly } }, colOrderHeader);
            this.gridAg.addColumnNumber("orderAmountCurrency", this.terms["common.sum"], null, { enableHiding: false, decimals: 2 }, colOrderHeader);
            this.gridAg.addColumnShape("attestStateColor", null, 55, { shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", showIconField: "attestStateColor" }, colOrderHeader);
        }

        if (this.editProjectPermission) {
            const colProjectHeader = this.gridAg.options.addColumnHeader("project", this.terms["economy.supplier.invoice.linktoproject"], null);
            colProjectHeader.marryChildren = true;

            var timeCodeOptions = new TypeAheadOptionsAg();
            timeCodeOptions.source = (filter) => this.filterTimeCodes(filter);
            timeCodeOptions.displayField = "label"
            timeCodeOptions.dataField = "label";
            timeCodeOptions.minLength = 0;
            timeCodeOptions.delay = 0;
            timeCodeOptions.useScroll = true;
            timeCodeOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef);
            this.gridAg.addColumnTypeAhead("timeCodeName", this.terms["economy.supplier.invoice.timecodes"], null, { typeAheadOptions: timeCodeOptions, editable: (r) => { return !r.orderLock && !r.isReadOnly }, suppressSorting: true }, null, colProjectHeader);

            var employeeOptions = new TypeAheadOptionsAg();
            employeeOptions.source = (filter) => this.filterEmployees(filter);
            employeeOptions.displayField = "label"
            employeeOptions.dataField = "label";
            employeeOptions.minLength = 0;
            employeeOptions.delay = 0;
            employeeOptions.useScroll = true;
            employeeOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef);
            this.gridAg.addColumnTypeAhead("employeeName", this.terms["common.employee"], null, { typeAheadOptions: employeeOptions, editable: (r) => { return !r.orderLock && !r.isReadOnly }, suppressSorting: true }, null, colProjectHeader);

            this.gridAg.addColumnNumber("projectAmountCurrency", this.terms["common.sum"], null, { enableHiding: false, decimals: 2, editable: (r) => { return !r.orderLock && !r.isReadOnly } }, colProjectHeader);
            this.gridAg.addColumnBoolEx("chargeCostToProject", this.terms["economy.supplier.invoice.chargecosttoproject"], 100, { enableEdit: true, disabledField: 'orderLock' }, colProjectHeader);
        }

        const colGeneralIIHeader = this.gridAg.options.addColumnHeader("general", " ", null);
        colGeneralIIHeader.marryChildren = true;

        this.gridAg.addColumnBoolEx("includeSupplierInvoiceImage", this.terms["economy.supplier.invoice.includeimage"], 100, { enableEdit: true, disabledField: 'isReadOnly' }, colGeneralIIHeader);
        this.gridAg.addColumnDelete(this.terms["common.remove"], (data) => this.deleteOrderRow(data), null, (row) => { return !row.isReadOnly }, null, colGeneralIIHeader);

        _.forEach(this.gridAg.options.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
            // Append closedRow to cellClass
            var cellCls: string = colDef.cellClass ? colDef.cellClass.toString() : "";
            colDef.cellClass = (grid: any) => {
                var newCls: string = cellCls;
                if (grid.data.isConnectToProjectRow && (colDef.field === 'productNr' || colDef.field === 'productName' || colDef.field === 'rowAmountCurrency' || colDef.field === 'supplementCharge' || colDef.field === 'orderAmountCurrency' || colDef.field === 'attestStateColor')) {
                    newCls += " closedRow";
                } else if (grid.data.isTransferToOrderRow && (colDef.field === 'timeCodeName' || colDef.field === 'employeeName' || colDef.field === 'projectAmountCurrency' || colDef.field === 'chargeCostToProject')) {
                    newCls += " closedRow";
                }

                return newCls;
            };
        });

        this.gridAg.options.addAggregatedFooterRow("#sum-footer-grid", {
            "rowAmountCurrency": "sum",
            "orderAmountCurrency": "sum",
            "projectAmountCurrency": "sum"
        } as IColumnAggregations);

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (row, colDef, newValue, oldValue) => { this.afterCellEdit(row, colDef, newValue, oldValue); }));
        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("common.directives.supplierinvoiceallocatedcosts", true);
    }

    private isProjectRow(row): boolean {
        return row && (!StringUtility.isEmpty(row.timeCodeName) || !StringUtility.isEmpty(row.employeeName) || (row.projectAmountCurrency && row.projectAmountCurrency !== 0));
    }

    private isOrderRow(row): boolean {
        return row && ((row.rowAmountCurrency && row.rowAmountCurrency !== 0) || (row.supplementCharge && row.supplementCharge !== 0));
    }

    private afterCellEdit(row: SupplierInvoiceCostAllocationDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        let resetRows = false;
        if (colDef.field === 'projectName') {
            this.projectChanged(row, newValue);
        }
        else if (colDef.field === 'customerInvoiceNumberName') {
            this.customerInvoiceChanged(row, newValue);
        }
        else if (colDef.field === 'productNr') {
            const product = _.find(this.products, { 'number': newValue });
            if (product) {
                row.productId = product.productId;
                row.productName = product.name;
            }

            resetRows = true;
        } 
        else if (colDef.field === 'rowAmountCurrency') {
            var amountCurrency: number = NumberUtility.parseDecimal(newValue);

            row.rowAmountCurrency = amountCurrency;
            if (this.transactionCurrencyRate) {
                row.rowAmount = amountCurrency * this.transactionCurrencyRate;          
            } else {
                row.rowAmount = row.rowAmountCurrency;
            }

            this.setOrderAmount(row);

            if (!row.productId || row.productId === 0)
                this.setDefaultMiscProduct(row);

            resetRows = true;
        }
        else if (colDef.field === 'supplementCharge') {
            var supplementCharge: number = NumberUtility.parseDecimal(newValue);
            row.supplementCharge = supplementCharge;

            this.setOrderAmount(row);

            if (!row.productId || row.productId === 0)
                this.setDefaultMiscProduct(row);

            resetRows = true;
        } 
        else if (colDef.field === 'timeCodeName') {
            var timecode = (_.find(this.timecodes, { 'label': newValue }));
            if (timecode) {
                row.timeCodeId = timecode.value;
                row.timeCodeName = timecode.label;

                if (row.chargeCostToProject === undefined) {
                    row.chargeCostToProject = this.chargeCostsToProject;
                    resetRows = true;
                }
            }
            else {
                row.timeCodeId = undefined;
                row.timeCodeName = '';
            }
        }
        else if (colDef.field === 'employeeName') {
            //Employee
            var employee = (_.find(this.employees, { 'label': newValue }));
            if (employee) {
                row.employeeId = employee.value;
                row.employeeName = employee.label;
            }
            else {
                row.employeeId = undefined;
                row.employeeName = '';
            }
        }
        else if (colDef.field === 'projectAmountCurrency') {
            var amount: number = NumberUtility.parseDecimal(newValue);
            row.projectAmount = amount;
            row.projectAmountCurrency = amount;

            if (row.chargeCostToProject === undefined) {
                row.chargeCostToProject = this.chargeCostsToProject;
                resetRows = true;
            }
        }

        if (resetRows)
            this.setGridData(true);
        else
            this.gridAg.options.refreshRows(row);

        this.setLocks(row);
        this.setAsDirty(true);
        this.validate();
    }

    private setOrderAmount(row: SupplierInvoiceCostAllocationDTO): void {
        row.orderAmount = this.calculateAmountSum(
            row.rowAmount,
            row.supplementCharge
        );

        row.orderAmountCurrency = this.calculateAmountSum(
            row.rowAmountCurrency,
            row.supplementCharge
        );
    }

    private openOrder(row: any) {
        if(row.orderId)
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["common.order"] + " " + row.orderNr, row.orderId, BillingOrdersEditController, { id: row.orderId, originType: SoeOriginType.Order, feature: Feature.Billing_Order_Status }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html')));
    }
    
    private openProject(row: any) {
        if(row.projectId)
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["common.customer.customer.orderproject"] + " " + row.projectNr, row.projectId, BillingProjectsEditController, { id: row.projectId }, this.urlHelperService.getGlobalUrl('Billing/Projects/Views/edit.html')));
    }

    private calculateAmountSum(amountCurrency: number, supplementCharge: number): number {
        if (amountCurrency && supplementCharge) {
            return amountCurrency * (1 + ((supplementCharge ?? 0)/ 100));
        }

        return amountCurrency;
    }

    protected allowNavigationFromTypeAhead(value, colDef) {
        if (!value)
            return true;

        if (colDef.field === 'projectName') {
            var matched = _.some(this.filteredProjects, { 'label': value });
            if (matched)
                return true;
        }
        else if (colDef.field === 'customerInvoiceNumberName') {
            var matched = _.some(this.filteredCustomerInvoices, { 'label': value });
            if (matched)
                return true;
        }
        else if (colDef.field === 'timeCodeName') {
            var matched = _.some(this.timecodes, { 'label': value });
            if (matched)
                return true;
        }
        else if (colDef.field === 'employeeName') {
            var matched = _.some(this.employees, { 'label': value });
            if (matched)
                return true;
        }
        else if (colDef.field === 'productNr') {
            var matched = _.some(this.products, { 'productNr': value });
            if (matched)
                return true;
        }

        return false;
    }

    protected filterProjectsByStatus() {
        this.filteredProjects.splice(0, this.filteredProjects.length);
        this.filteredProjects.push({ value: 0, label: " ", projectId: 0 })

        _.forEach(this.projects, (project: any) => {
            if (this.showHiddenProjects) {
                this.filteredProjects.push({ value: project.projectId, label: project.number + " " + project.name, projectId: project.projectId });
            }
            else {
                if (this.invoice) {
                    var ids = _.map(this.invoice.supplierInvoiceCostAllocationRows, r => r.projectId);
                    if (project.status != TermGroup_ProjectStatus.Hidden || _.includes(ids, project.projectId)) {
                        this.filteredProjects.push({ value: project.projectId, label: project.number + " " + project.name, projectId: project.projectId });
                    }
                }
                else {
                    if (project.status != TermGroup_ProjectStatus.Hidden) {
                        this.filteredProjects.push({ value: project.projectId, label: project.number + " " + project.name, projectId: project.projectId });
                    }
                }
            }
        });
    }

    protected filterProjects(filter) {
        return _.orderBy(this.filteredProjects.filter(p => {
            return p.label.contains(filter);
        }), 'label');
    }

    protected filterCustomerInvoices(filter) {
        return _.orderBy(this.filteredCustomerInvoices.filter(p => {
            return p.label.contains(filter);
        }), 'label');
    }

    public filterProducts(filter) {
        return this.products.filter(prod => {
            return prod.number.contains(filter) || prod.name.contains(filter);
        });
    }

    protected filterTimeCodes(filter) {
        return _.orderBy(this.timecodes.filter(p => {
            return p.label.contains(filter);
        }), 'label');
    }

    protected filterEmployees(filter) {
        return _.orderBy(this.employees.filter(p => {
            return p.label.contains(filter);
        }), 'label');
    }

    protected filterCustomerInvoicesByProject(projectId: number) {
        this.filteredCustomerInvoices.splice(0, this.filteredCustomerInvoices.length);
        this.filteredCustomerInvoices.push({ value: 0, label: " ", customerInvoiceId: 0 });
        if (!projectId || projectId === 0) {
            _.forEach(this.customerInvoices, (invoice: any) => {
                this.filteredCustomerInvoices.push({ value: invoice.invoiceId, label: (invoice.customerInvoiceNumberName.length > 70 ? invoice.customerInvoiceNumberName.substring(0, 70) + "..." : invoice.customerInvoiceNumberName), customerInvoiceId: invoice.invoiceId, projectId: invoice.projectId, priceListTypeId: invoice.priceListTypeId });
            });
        }
        else {
            var invoices = (_.filter(this.customerInvoices, { projectId: projectId }));
            _.forEach(invoices, (invoice: any) => {
                this.filteredCustomerInvoices.push({ value: invoice.invoiceId, label: (invoice.customerInvoiceNumberName.length > 70 ? invoice.customerInvoiceNumberName.substring(0, 70) + "..." : invoice.customerInvoiceNumberName), customerInvoiceId: invoice.invoiceId, projectId: invoice.projectId, priceListTypeId: invoice.priceListTypeId });
            });
        }
    }

    protected projectChanged(row, newValue): void {
        var obj = _.find(this.filteredProjects, { 'label': newValue });
        if (obj) {
            row.projectId = obj["value"];
            row.projectName = obj["label"];
            row.customerInvoiceId = 0;
            row.customerInvoiceNumberName = "";

            this.filterCustomerInvoicesByProject(row.projectId);
        }
        else {
            row.projectId = 0;
            row.projectName = "";
            row.customerInvoiceId = 0;
            row.customerInvoiceNumberName = "";
            this.filterCustomerInvoicesByProject(0);
        }
        this.validate();
    }

    protected setDefaultMiscProduct(row) {
        const product = _.find(this.products, { 'productId': this.miscProductId });
        if (product) {
            row.productId = product.productId;
            row.productId = product.productId;
            row.productNr = product.number
            row.productName = product.name;
        }
    }

    protected customerInvoiceChanged(row, newValue): void {
        var obj = _.find(this.filteredCustomerInvoices, { 'label': newValue });
        if (obj) {
            row.orderId = obj["value"];
            row.customerInvoiceNumberName = obj["label"];

            var pltMarkup = _.find(this.priceListTypeMarkups, { 'priceListTypeId': obj["priceListTypeId"] });
            row.supplementCharge = pltMarkup ? (pltMarkup.markup * 100) : 0;

            this.setOrderAmount(row);

            var proj = _.find(this.filteredProjects, { 'value': obj.projectId });
            if (proj) {
                row.projectId = proj.projectId;
                row.projectName = proj.label;
            }
            else {
                // Fail safe if project is hidden
                proj = _.find(this.projects, { 'projectId': obj.projectId });
                if (proj) {
                    row.projectId = proj.projectId;
                    row.projectName = proj.number + " " + proj.name;
                    this.filteredProjects.push({ value: proj.projectId, label: proj.number + " " + proj.name, projectId: proj.projectId });
                }
            }
        }
        else {
            row.orderId = 0;
            row.customerInvoiceNumberName = "";
        }
        //this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
        this.validate();
    }

    protected deleteOrderRow(orderRow) {
        var rows = [];
        for (var i = 0; i < this.invoice.supplierInvoiceCostAllocationRows.length; i++) {
            if (this.invoice.supplierInvoiceCostAllocationRows[i] === orderRow) {
                var row = this.invoice.supplierInvoiceCostAllocationRows[i];
                if ((row.customerInvoiceRowId && row.customerInvoiceRowId > 0) || (row.timeCodeTransactionId && row.timeCodeTransactionId > 0)) {
                    row.state = 2;  //SoeEntityState.Deleted
                    rows.push(row);
                }
            } else {
                rows.push(this.invoice.supplierInvoiceCostAllocationRows[i]);
            }
        }
        this.invoice.supplierInvoiceCostAllocationRows = rows;
        this.setGridData(false);

        this.setAsDirty();
    }

    protected setLocks(row: SupplierInvoiceCostAllocationDTO) {
        if (row.isConnectToProjectRow) {
            row.rowTypeIcon = "fal fa-link";
            row.rowTypeTooltip = this.terms["economy.supplier.invoice.connectedtoproject"];
            row.rowAmount = undefined;
            row.rowAmountCurrency = undefined;
            row.orderAmount = undefined;
            row.orderAmountCurrency = undefined;
            row.supplementCharge = undefined;
        }
        else {
            row.rowTypeIcon = "fal fa-file-import";
            row.rowTypeTooltip = this.terms["economy.supplier.invoice.transferedtorder"];
            row.projectAmount = undefined;
            row.projectAmountCurrency = undefined;
        }
        row.projectLock = row.isConnectToProjectRow;
        row.orderLock = !row.isConnectToProjectRow;
    }

    protected validate() {
        this.linkToOrderOrderSet = true;
        this.linkToProjectProjectSet = true;
        this.linkToProjectTimeCodeSet = true;

        _.forEach(_.filter(this.invoice.supplierInvoiceCostAllocationRows, { state: 0 }), (row: any) => {
            if ((row.rowAmountCurrency && row.rowAmountCurrency !== 0) && (!row.orderId || row.orderId === 0)) {
                this.linkToOrderOrderSet = false;
                return;
            }
            if ((row.projectAmountCurrency && row.projectAmountCurrency !== 0) && (!row.projectId || row.projectId === 0)) {
                this.linkToProjectProjectSet = false;
                return;
            }
            if (row.projectAmountCurrency && row.projectAmountCurrency !== 0 && (!row.timeCodeId || row.timeCodeId === 0)) {
                this.linkToProjectTimeCodeSet = false;
                return;
            }
        });
    }

    private executeButtonFunction(option) {
        switch (option.id) {
            case CostAllocationButtonFunctions.TransferToOrder:
                this.addRow(false);
                break;
            case CostAllocationButtonFunctions.ConnectToProject:
                this.addRow(true);
                break;
        }
    }

    public addRow(isConnectToProjectRow, project = undefined, timeCode = undefined, amount = undefined, setFocus = true) {
        let focusColProp = 'projectName';
        var row: any = {};
        row.isConnectToProjectRow = isConnectToProjectRow;
        row.isTransferToOrderRow = !isConnectToProjectRow;
        row.isReadOnly = false;
        row.isModified = true;
        row.state = SoeEntityState.Active;
        row.rowTypeIcon = row.isConnectToProjectRow ? "fal fa-link" : "fal fa-file-import";
        row.supplementCharge = null;

        row.productId = 0;
        row.productName = "";
        row.productNr = "";

        row.employeeId = 0;
        row.employeeName = "";
        row.employeeNr = "";

        if (row.isConnectToProjectRow) {
            row.rowTypeIcon = "fal fa-link";
            row.rowTypeTooltip = this.terms["economy.supplier.invoice.connectedtoproject"];

            row.projectAmount = (+(this.invoice.totalAmount - this.invoice.vatAmount) - _.sumBy(this.invoice.supplierInvoiceCostAllocationRows, function (o) { return o.projectAmount ? +o.projectAmount : 0; })
                ).round(2);
            row.projectAmountCurrency = (+(this.invoice.totalAmountCurrency - this.invoice.vatAmountCurrency) - _.sumBy(this.invoice.supplierInvoiceCostAllocationRows, function (o) { return o.projectAmountCurrency ? +o.projectAmountCurrency : 0; })
                ).round(2);
        }
        else {
            row.rowTypeIcon = "fal fa-file-import";
            row.rowTypeTooltip = this.terms["economy.supplier.invoice.transferedtorder"];

            row.rowAmount = (+(this.invoice.totalAmount - this.invoice.vatAmount) - _.sumBy(this.invoice.supplierInvoiceCostAllocationRows, function (o) { return o.rowAmount ? +o.rowAmount : 0; })
                ).round(2);
            row.rowAmountCurrency = (+(this.invoice.totalAmountCurrency - this.invoice.vatAmountCurrency) - _.sumBy(this.invoice.supplierInvoiceCostAllocationRows, function (o) { return o.rowAmountCurrency ? +o.rowAmountCurrency : 0; })
                ).round(2);

            row.orderAmount = (+(this.invoice.totalAmount - this.invoice.vatAmount) - _.sumBy(this.invoice.supplierInvoiceCostAllocationRows, function (o) { return o.rowAmount ? +o.rowAmount : 0; })
                ).round(2);
            row.orderAmountCurrency = (+(this.invoice.totalAmountCurrency - this.invoice.vatAmountCurrency) - _.sumBy(this.invoice.supplierInvoiceCostAllocationRows, function (o) { return o.rowAmountCurrency ? +o.rowAmountCurrency : 0; })
                ).round(2);

            this.setDefaultMiscProduct(row);
        }

        //SupplierInvoice
        row.supplierInvoiceId = this.invoice.invoiceId;

        //Project
        var proj = _.find(this.projects, { projectId: this.invoice.projectId });
        if (proj) {
            row.projectId = proj.projectId;
            row.projectNr = proj.number;
            row.projectName = proj.number + " " + proj.name;

            // Check if filtered contains project
            var p = _.find(this.filteredProjects, { 'value': proj.projectId })
            if (!p)
                this.filteredProjects.push({ value: proj.projectId, label: proj.number + " " + proj.name, projectId: proj.projectId });

            focusColProp = 'customerInvoiceNumberName';
        }
        else if (project) {
            row.projectId = project.id;
            row.projectNr = project.number;
            row.projectName = project.name;

            // Check if filtered contains project
            var p = _.find(this.filteredProjects, { 'value': project.id })
            if (!p)
                this.filteredProjects.push({ value: project.id, label: project.name, projectId: project.projectId });
        }
        else {
            row.projectId = 0;
            row.projectNr = "";
            row.projectName = "";
        }

        //Customer Invoice                        
        var invoices = (this.invoice.orderNr) ? this.customerInvoices.filter(x => x.invoiceNr === this.invoice.orderNr.toString()) : [];
        if (invoices.length > 0) {
            row.orderId = invoices[0].invoiceId;
            row.orderNr = invoices[0].invoiceNr;
            row.customerInvoiceNumberName = invoices[0].customerInvoiceNumberName;

            focusColProp = 'productNr';
        } else {
            row.orderId = 0;
            row.orderNr = "";
            row.customerInvoiceNumberName = "";
        }

        var timeCodeToSet = timeCode ? timeCode : this.defaultTimeCodeId && this.defaultTimeCodeId > 0 ? (_.find(this.timecodes, { value: this.defaultTimeCodeId })) : null;
        if (timeCodeToSet && row.isConnectToProjectRow) {
            row.timeCodeId = timeCodeToSet.value;
            row.timeCodeName = timeCodeToSet.label;
        }
        else {
            row.timeCodeId = 0;
            row.timeCodeName = 0;
        }

        if (row.isConnectToProjectRow)
            row.chargeCostToProject = this.chargeCostsToProject;

        if (amount) 
            row.projectAmount = row.projectAmountCurrency = amount;

        this.invoice.supplierInvoiceCostAllocationRows.push(row);

        this.filterProjectsByStatus();
        this.filterCustomerInvoicesByProject(row.projectId);

        this.setGridData(false);
        this.setAsDirty(true);

        if (setFocus) {
            this.$timeout(() => {
                try {
                    if (row.isConnectToProjectRow)
                        this.gridAg.options.startEditingCell(row, timeCode ? 'projectAmountCurrency' : 'timeCodeName');
                    else
                        this.gridAg.options.startEditingCell(row, focusColProp);
                } catch (e) {
                    console.error("Could not set focus to new row in Allocate Costs grid.", e, row);
                }
            });
        }
    }

    private setAsDirty(isDirty: boolean = true): void {
        this.$scope.$applyAsync();
        this.messagingService.publish(
            Constants.EVENT_SET_DIRTY,
            {
                guid: this.parentGuid,
                dirty: isDirty
            }
        )
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [];
        featureIds.push(Feature.Billing_Project_Edit); 
        featureIds.push(Feature.Billing_Order_Orders_Edit);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.editProjectPermission = x[Feature.Billing_Project_Edit];
            this.editOrderPermission = x[Feature.Billing_Order_Orders_Edit];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.ProductMisc);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.miscProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductMisc);
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            UserSettingType.BillingProductSearchMinPrefixLength,
            UserSettingType.BillingProductSearchMinPopulateDelay,
        ];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.productSearchMinPrefixLength = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingProductSearchMinPrefixLength, this.productSearchMinPrefixLength);
            this.productSearchMinPopulateDelay = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingProductSearchMinPopulateDelay, this.productSearchMinPopulateDelay);
        });
    }

    private loadAllProducts(): ng.IPromise<any> {
        this.products = [];
        return this.productService.getInvoiceProductsSmall().then(x => {
            this.products.push({ productId: 0, number: '', name: '', numberName: ''})
            this.products.push(...x);
        });
    }

    private loadPriceListTypeMarkups(): ng.IPromise<any> {
        return this.invoiceService.GetPriceListTypeMarkups().then((x) => {
            this.priceListTypeMarkups = x;
        });
    }

    private loadCustomerProjects(): ng.IPromise<any> {
        return this.supplierService.getProjectList(TermGroup_ProjectType.TimeProject, undefined, true, true, false).then((x) => {
            this.projects = x;
        });
    }

    private loadCustomerInvoices(): ng.IPromise<any> {
        return this.supplierService.getOrdersForSupplierInvoiceEdit(true).then(x => {
            this.customerInvoices = x;
        });
    }

    private loadCustomerTimeCodes(): ng.IPromise<any> {
        return this.supplierService.getTimeCodes(SoeTimeCodeType.WorkAndMaterial, true, false).then((x) => {
            this.timecodes.push({ value: 0, label: "" });
            _.forEach(x, (timeCode: any) => {
                this.timecodes.push({ value: timeCode.timeCodeId, label: timeCode.name, timeCodeId: timeCode.timeCodeId });
            });
        });
    }

    private loadCustomerEmployees(): ng.IPromise<any> {
        return this.supplierService.getEmployeesDict(true, false, false, true).then((result) => {
            _.forEach(result, (employee: any) => {
                this.employees.push({ value: employee.id, label: employee.name });
            });
        });
    }

    public setGridData(checkFiltering: boolean) {
        if (checkFiltering === true) {
            _.forEach(_.filter(this.invoice.supplierInvoiceCostAllocationRows, s => s.state === 0), (row: any) => {
                var proj = _.find(this.filteredProjects, { projectId: row.projectId });
                if (!proj) {
                    this.filteredProjects.push({ value: row.projectId, label: row.projectName, projectId: row.projectId });
                }
            });
        }

        var filteredItems = [];
        _.forEach(this.invoice.supplierInvoiceCostAllocationRows, (r) => {
            if (r.state === 0) {
                this.setLocks(r);
                filteredItems.push(r);
            }
        });
        
        this.gridAg.setData(filteredItems);

        this.validate();
    }
}

enum CostAllocationButtonFunctions {
    TransferToOrder = 1,
    ConnectToProject = 2,
}