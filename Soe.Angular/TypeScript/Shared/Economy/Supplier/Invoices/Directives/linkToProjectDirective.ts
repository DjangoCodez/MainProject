import { SupplierInvoiceDTO } from "../../../../../Common/Models/InvoiceDTO";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ISupplierService } from "../../../../../Shared/Economy/Supplier/SupplierService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { TypeAheadOptions, GridEvent } from "../../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { ISupplierInvoiceProjectRowDTO } from "../../../../../Scripts/TypeLite.Net4";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { Feature, TermGroup_ProjectStatus, SoeEntityState, TermGroup_ProjectType, SoeTimeCodeType } from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { GridControllerBaseAg } from "../../../../../Core/Controllers/GridControllerBaseAg";
import { TypeAheadOptionsAg, IColumnAggregations } from "../../../../../Util/SoeGridOptionsAg";
import { Guid } from "../../../../../Util/StringUtility";

export class LinkToProjectController extends GridControllerBaseAg {
    private invoice: SupplierInvoiceDTO;
    public linkToProjectProjectSet: boolean;
    public linkToProjectTimeCodeSet: boolean;
    public defaultTimeCodeId: number;
    public chargeCostsToProject: boolean;
    private parentGuid: Guid;

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

    //Collections
    projects: any[] = [];
    filteredProjects: any[] = [];
    customerInvoices: any[] = [];
    filteredCustomerInvoices: any[] = [];
    timecodes: any[] = [];
    employees: any[] = [];
    project: any = {};

    // Flags
    rowWatchNotSet = true;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        coreService: ICoreService,
        private supplierService: ISupplierService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("economy.supplier.invoice.linktoproject", "economy.supplier.invoice.linktoproject", Feature.Economy_Supplier_Invoice_Invoices_Edit, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        this.initialData();

        this.$scope.$on('costsToProjectChanged', (e, a) => {
            this.setGridData();
        });

        if (this.invoice) {
            
            this.$scope.$watch(() => this.invoice.supplierInvoiceProjectRows, (newValue, oldValue) => {
                this.rowWatchNotSet = false;
                if (newValue != oldValue) {
                    this.soeGridOptions.setData(this.invoice.supplierInvoiceProjectRows);
                }
            });
        }
        else {
            this.$scope.$watch(() => this.invoice, (newValue, oldValue) => {
                if ((newValue && newValue != oldValue) || this.rowWatchNotSet) {
                    if (this.invoice.supplierInvoiceProjectRows)
                        this.soeGridOptions.setData(this.invoice.supplierInvoiceProjectRows);

                    this.rowWatchNotSet = false;
                    this.$scope.$watch(() => this.invoice.supplierInvoiceProjectRows, (newValue, oldValue) => {
                        if (newValue != oldValue) {
                            this.soeGridOptions.setData(this.invoice.supplierInvoiceProjectRows);
                        }
                    });
                }
            });
        }
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.remove",
            "common.employee",
            "economy.supplier.invoice.project",
            "economy.supplier.invoice.timecodes",
            "common.sum",
            "economy.supplier.invoice.customerinvoice",
            "economy.supplier.invoice.chargecosttoproject",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "economy.supplier.invoice.includeimage"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    public setupGrid(): void {
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = false;
        //this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.setMinRowsToShow(10);

        var projectOptions = new TypeAheadOptionsAg();
        projectOptions.source = (filter) => this.filterProjects(filter);
        projectOptions.displayField = "label"
        projectOptions.dataField = "label";
        projectOptions.minLength = 0;
        projectOptions.delay = 0;
        projectOptions.useScroll = true;
        projectOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef);
        this.soeGridOptions.addColumnTypeAhead("projectName", this.terms["economy.supplier.invoice.project"], null, { typeAheadOptions: projectOptions, editable: true, suppressSorting: true });

        var invoiceOptions = new TypeAheadOptionsAg();
        invoiceOptions.source = (filter) => this.filterCustomerInvoices(filter);
        invoiceOptions.displayField = "label"
        invoiceOptions.dataField = "label";
        invoiceOptions.minLength = 0;
        invoiceOptions.delay = 0;
        invoiceOptions.useScroll = true;
        invoiceOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef);
        this.soeGridOptions.addColumnTypeAhead("customerInvoiceNumberName", this.terms["economy.supplier.invoice.customerinvoice"], null, { typeAheadOptions: invoiceOptions, editable: true, suppressSorting: true });

        var timeCodeOptions = new TypeAheadOptionsAg();
        timeCodeOptions.source = (filter) => this.filterTimeCodes(filter);
        timeCodeOptions.displayField = "label"
        timeCodeOptions.dataField = "label";
        timeCodeOptions.minLength = 0;
        timeCodeOptions.delay = 0;
        timeCodeOptions.useScroll = true;
        timeCodeOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef);
        this.soeGridOptions.addColumnTypeAhead("timeCodeName", this.terms["economy.supplier.invoice.timecodes"], null, { typeAheadOptions: timeCodeOptions, editable: true, suppressSorting: true });

        var employeeOptions = new TypeAheadOptionsAg();
        employeeOptions.source = (filter) => this.filterEmployees(filter);
        employeeOptions.displayField = "label"
        employeeOptions.dataField = "label";
        employeeOptions.minLength = 0;
        employeeOptions.delay = 0;
        employeeOptions.useScroll = true;
        employeeOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef);
        this.soeGridOptions.addColumnTypeAhead("employeeName", this.terms["common.employee"], null, { typeAheadOptions: employeeOptions, editable: true, suppressSorting: true });

        //this.soeGridOptions.addColumnSelect("timeCodeName", this.terms["economy.supplier.invoice.timecode"], null, { selectOptions: this.timecodes, dropdownIdLabel: "value", dropdownValueLabel: "label", displayField: "timeCodeName", onChanged: this.timeCodeChanged.bind(this), editable: true });
        //this.soeGridOptions.addColumnSelect("employeeId", this.terms["common.employee"], null, { selectOptions: this.employees, dropdownIdLabel: "value", dropdownValueLabel: "label", displayField: "employeeName", onChanged: this.employeeChanged.bind(this), editable: true });
        var aggregations = { "amount": "sum" };
        super.addColumnNumber("amount", this.terms["common.sum"], null, { enableHiding: false, decimals: 2, editable: true });
        this.soeGridOptions.addColumnBool("chargeCostToProject", this.terms["economy.supplier.invoice.chargecosttoproject"], 100, { enableEdit: true });
        this.soeGridOptions.addColumnBool("includeSupplierInvoiceImage", this.terms["economy.supplier.invoice.includeimage"], 100, { enableEdit: true, onChanged: this.validateIncludeImage.bind(this), toolTip: this.terms["economy.supplier.invoice.includeimage"] });
        this.soeGridOptions.addColumnIcon(null, null, null, { icon: "fal fa-times iconDelete", onClick: this.deleteProjectRow.bind(this), toolTip: this.terms["common.remove"] });
        _.forEach(this.soeGridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
            colDef.enableFiltering = false;
            colDef.enableSorting = false;
            colDef.enableColumnMenu = false;
        });

        this.soeGridOptions.addAggregatedFooterRow("#sum-footer-grid", aggregations as IColumnAggregations);

        //Set up totals row
        this.soeGridOptions.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"],
            selected: this.terms["core.aggrid.totals.selected"]
        });

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.soeGridOptions.subscribe(events);

        this.soeGridOptions.finalizeInitGrid();
    }

    private validateIncludeImage(row): boolean {
        if (!row.data)
            return;

        if (!row.data.customerInvoiceId || row.data.customerInvoiceId === 0) {
            row.data.includeSupplierInvoiceImage = false;
            this.$timeout(() => {
                // Hard reset
                this.soeGridOptions.setData(this.invoice.supplierInvoiceProjectRows);
            });
        }
        else {
            this.setAsDirty(true);
        }
    }

    protected edit(row: any) {

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
                    var ids = _.map(this.invoice.supplierInvoiceOrderRows, r => r.projectId);
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
                this.filteredCustomerInvoices.push({ value: invoice.invoiceId, label: (invoice.customerInvoiceNumberName.length > 70 ? invoice.customerInvoiceNumberName.substring(0, 70) + "..." : invoice.customerInvoiceNumberName), customerInvoiceId: invoice.invoiceId, projectId: invoice.projectId });
            });
        }
        else {
            var invoices = (_.filter(this.customerInvoices, { projectId: projectId }));
            _.forEach(invoices, (invoice: any) => {
                this.filteredCustomerInvoices.push({ value: invoice.invoiceId, label: (invoice.customerInvoiceNumberName.length > 70 ? invoice.customerInvoiceNumberName.substring(0, 70) + "..." : invoice.customerInvoiceNumberName), customerInvoiceId: invoice.invoiceId, projectId: invoice.projectId });
            });
        }
    }

    protected employeeChanged(row): void {
        var obj = (_.filter(this.employees, { value: row.employeeId }))[0];
        if (obj) {
            row.employeeId = obj["value"];
            row.employeeName = obj["label"];
        }
        this.validate();
    }

    protected timeCodeChanged(row): void {
        var obj = (_.filter(this.timecodes, { value: row.timeCodeId }))[0];
        if (obj) {
            row.timeCodeId = obj["value"];
            row.timeCodeName = obj["label"];
        }
        this.validate();
    }

    private afterCellEdit(row: ISupplierInvoiceProjectRowDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        if (colDef.field === 'projectName') {
            this.projectChanged(row, newValue);

            this.soeGridOptions.refreshRows(row);
            this.setAsDirty(true);
            this.validate();
            return;
        }

        if (colDef.field === 'customerInvoiceNumberName') {
            this.customerInvoiceChanged(row, newValue);

            this.soeGridOptions.refreshRows(row);
            this.setAsDirty(true);
            this.validate();
            return;
        }

        if (colDef.field === 'timeCodeName') {
            var timecode = (_.find(this.timecodes, { 'label': newValue }));
            if (timecode) {
                row.timeCodeId = timecode.value;
                row.timeCodeName = timecode.label;
            }
            else {
                row.timeCodeId = undefined;
                row.timeCodeName = '';
            }

            this.soeGridOptions.refreshRows(row);
            this.setAsDirty(true);
            this.validate();
            return;
        }

        if (colDef.field === 'employeeName') {
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

            this.soeGridOptions.refreshRows(row);
            this.setAsDirty(true);
            this.validate();
            return;
        }

        if (colDef.field === 'amount') {
            var amount: number = NumberUtility.parseDecimal(newValue);
            row.amount = amount;
            row.amountCurrency = amount;

            this.soeGridOptions.refreshRows(row);
            this.setAsDirty(true);
            this.validate();
            return;
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
        this.validate();
    }

    protected customerInvoiceChanged(row, newValue): void {
        var obj = _.find(this.filteredCustomerInvoices, { 'label': newValue });
        if (obj) {
            row.customerInvoiceId = obj["value"];
            row.customerInvoiceNumberName = obj["label"];
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
            
        }
        this.$timeout(() => {
            row['imageDisabled'] = !row.customerInvoiceId || row.customerInvoiceId === 0;
        });
        this.validate();
    }

    protected deleteProjectRow(projectRow) {
        for (var i = 0; i < this.invoice.supplierInvoiceProjectRows.length; i++) {
            if (this.invoice.supplierInvoiceProjectRows[i] === projectRow) {
                this.invoice.supplierInvoiceProjectRows.splice(i, 1);
                break;
            }
        }

        this.setGridData();
        this.validate();
        this.setAsDirty(true);
    }

    protected validate() {
        this.linkToProjectProjectSet = true;
        this.linkToProjectTimeCodeSet = true;

        _.forEach(this.invoice.supplierInvoiceProjectRows, (row: any) => {
            if (!row.projectId || row.projectId === 0) {
                this.linkToProjectProjectSet = false;
                return;
            }
            else if (!row.timeCodeId || row.timeCodeId === 0) {
                this.linkToProjectTimeCodeSet = false;
                return;
            }
        });
    }

    public addRow() {
        var row: any = {};
        row.state = SoeEntityState.Active;
        //TimeCodeTransaction
        row.timeCodeTransactionId = 0;
        //Amount
        row.amount = +(this.invoice.totalAmountCurrency - this.invoice.vatAmountCurrency) - _.sumBy(this.invoice.supplierInvoiceProjectRows, function (o) { return +o.amount; });
        //row.aountCurrency = cuAm;
        row.amountLedgerCurrency = 0;
        row.amountEntCurrency = 0;
        //TimeInvoiceTransaction
        row.timeInvoiceTransactionId = 0;
        //SupplierInvoice
        row.supplierInvoiceId = 0;
        //Project
        row.projectId = 0;
        var proj = _.find(this.projects, { projectId: this.invoice.projectId });
        if (proj) {
            row.projectId = proj.projectId;
            row.projectNr = proj.number;
            row.projectName = proj.number + " " + proj.name;

            // Check if filtered contains project
            var p = _.find(this.filteredProjects, { 'value': proj.projectId })
            if (!p)
                this.filteredProjects.push({ value: proj.projectId, label: proj.number + " " + proj.name, projectId: proj.projectId });
        }
        else {
            row.projectId = undefined;
            row.projectName = '';
        }

        //Customer Invoice                        
        row.customerInvoiceId = 0;
        var invoices = (this.invoice.orderNr) ? this.customerInvoices.filter(x => x.invoiceNr === this.invoice.orderNr.toString()) : [];

        if (invoices.length > 0) {
            row.customerInvoiceId = invoices[0].invoiceId;
            row.customerInvoiceNr = invoices[0].invoiceNr;
            row.customerInvoiceNumberName = invoices[0].customerInvoiceNumberName;
        }
        else {
            row.customerInvoiceId = undefined;
            row.customerInvoiceCustomerName = '';
        }

        //TimeCode            
        var timecode = this.defaultTimeCodeId && this.defaultTimeCodeId > 0 ? (_.find(this.timecodes, { value: this.defaultTimeCodeId })) : null;
        
        if (timecode) {
            row.timeCodeId = timecode.value;
            row.timeCodeName = timecode.label;
        }
        else {
            row.timeCodeId = undefined;
            row.timeCodeName = "";
        }

        //Employee
        row.employeeId = undefined;
        row.employeeName = "";
        row.employeeNr = "";
        row.employeeDescription = "";

        //TimeBlockDate
        row.timeBlockDateId = null;
        row.date = new Date().toJSON().slice(0, 10);
        row.chargeCostToProject = this.chargeCostsToProject;
        row.includeSupplierInvoiceImage = false;
        this.invoice.supplierInvoiceProjectRows.push(row);
        this.filterProjectsByStatus();
        this.filterCustomerInvoicesByProject(row.projectId);

        this.setGridData();
        this.validate();

        this.setAsDirty(true);
    }

    initialData() {
        this.$q.all([
            this.loadTerms(),
            this.loadCustomerProjects(),
            this.loadCustomerInvoices(),
            this.loadCustomerTimeCodes(),
            this.loadCustomerEmployees()]).then(() => {
                this.filterProjectsByStatus();
                this.filterCustomerInvoicesByProject(0);
                if(this.invoice)
                    this.setGridData();
            });
    }

    private loadCustomerProjects(): ng.IPromise<any> {
        return this.supplierService.getProjectList(TermGroup_ProjectType.TimeProject, true, true, true, false).then((x) => {
            this.projects = x;
        });
    }

    private loadCustomerInvoices(): ng.IPromise<any> {
        //return this.supplierService.getCustomerInvoices(SoeOriginType.Order, SoeOriginStatusClassification.OrdersOpen, TermGroup.ProjectType, OrderInvoiceRegistrationType.Order, true, true, false).then((x) => {
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

    public setGridData() {
        this.gridDataLoaded(this.invoice.supplierInvoiceProjectRows);
        this.validate();
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
}

//@ngInject
export function linkToProjectDirective(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        restrict: "E",
        templateUrl: urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/linkToProject.html'),
        replace: true,
        scope: {
            invoice: "=",
            invoiceIsLoaded: "=",
            linkToProjectProjectSet: "=",
            linkToProjectTimeCodeSet: "=",
            defaultTimeCodeId: "=",
            chargeCostsToProject: "=",
            parentGuid: "="
        },
        link(scope: ng.IScope, element: JQuery, attributes: ng.IAttributes, ngModelController: any) {
            scope.$watch(() => (ngModelController.invoiceIsLoaded), (newVAlue, oldvalue, scope) => {
                if (newVAlue && !oldvalue) {
                    ngModelController.setGridData();
                }

            }, true);
        },
        bindToController: true,
        controllerAs: "ctrl",
        controller: LinkToProjectController
    }
}