import { SupplierInvoiceDTO } from "../../../../../Common/Models/InvoiceDTO";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ISupplierService } from "../../../../../Shared/Economy/Supplier/SupplierService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { ISupplierInvoiceOrderRowDTO } from "../../../../../Scripts/TypeLite.Net4";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { Feature, TermGroup_ProjectStatus, SoeEntityState, TermGroup_ProjectType, SoeOriginType} from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { TypeAheadOptionsAg, IColumnAggregations } from "../../../../../Util/SoeGridOptionsAg";
import { GridControllerBaseAg } from "../../../../../Core/Controllers/GridControllerBaseAg";
import { Guid } from "../../../../../Util/StringUtility";
import { TabMessage } from "../../../../../Core/Controllers/TabsControllerBase1";
import { EditController as BillingProjectsEditController } from "../../../../../Shared/Billing/Projects/EditController";
import { EditController as BillingOrdersEditController } from "../../../../../Shared/Billing/Orders/EditController";

export class LinkToOrderController extends GridControllerBaseAg {
    private invoice: SupplierInvoiceDTO;
    public linkToOrderOrderSet: boolean;
    public linkToOrderValidityText: string;
    private parentGuid: Guid;

    // Initial values
    private initialFiltering = true;

    //Terms
    terms: { [index: string]: string; };

    // Properties
    private _showHiddenProjects: any;
    get showHiddenProjects() {
        return this._showHiddenProjects;
    }
    set showHiddenProjects(item: any) {
        this._showHiddenProjects = item;
        //this.filterProjects();
        this.filterProjectsByStatus();
        this.filterCustomerInvoicesByProject(this.project.projectId);
    }

    // Permissions
    private editProjectPermission = false;
    private editOrderPermission = false;

    //Collections
    projects: any[] = [];
    filteredProjects: any[] = [];
    customerInvoices: any[] = [];
    filteredCustomerInvoices: any[] = [];
    project: any = {};

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
        super("Economy.Supplier.Invoice.LinkToOrder", "economy.supplier.invoice.linktoorder", Feature.Economy_Supplier_Invoice_Invoices_Edit, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
        //this.progressBusy = true;
        this.showHiddenProjects = false;

        this.initialData();

        if (this.invoice) {
            this.$scope.$watch(() => this.invoice.supplierInvoiceOrderRows, (newValue, oldValue) => {
                if (newValue != oldValue) {
                    this.soeGridOptions.setData(this.invoice.supplierInvoiceOrderRows);
                }
            });
        }
        else {
            this.$scope.$watch(() => this.invoice, (newValue, oldValue) => {
                if (newValue && newValue != oldValue) {
                    this.$scope.$watch(() => this.invoice.supplierInvoiceOrderRows, (newValue, oldValue) => {
                        if (newValue != oldValue) {
                            this.soeGridOptions.setData(this.invoice.supplierInvoiceOrderRows);
                        }
                    });
                }
            });
        }
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.remove",
            "common.amount",
            "economy.supplier.invoice.project",
            "economy.supplier.invoice.surcharge",
            "common.sum",
            "economy.supplier.invoice.customerinvoice",
            "economy.supplier.invoice.chargecosttoproject",
            "economy.supplier.invoice.showimage",
            "economy.supplier.invoice.includeimage",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "common.order",
            "common.customer.customer.orderproject"
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
        this.soeGridOptions.addColumnIcon(null, null, null, { icon: "iconEdit fal fa-pencil", onClick: this.openProject.bind(this), toolTip: this.terms["common.customer.customer.orderproject"], showIcon: (r) => r.projectId && this.editProjectPermission });

        var invoiceOptions = new TypeAheadOptionsAg();
        invoiceOptions.source = (filter) => this.filterCustomerInvoices(filter);
        invoiceOptions.displayField = "label"
        invoiceOptions.dataField = "label";
        invoiceOptions.minLength = 0;
        invoiceOptions.delay = 0;
        invoiceOptions.useScroll = true;
        invoiceOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef);
        this.soeGridOptions.addColumnTypeAhead("customerInvoiceNumberName", this.terms["economy.supplier.invoice.customerinvoice"], null, { typeAheadOptions: invoiceOptions, editable: true, suppressSorting: true });
        this.soeGridOptions.addColumnIcon(null, null, null, { icon: "iconEdit fal fa-pencil", onClick: this.openOrder.bind(this), toolTip: this.terms["common.order"], showIcon: (r) => r.customerInvoiceId && this.editOrderPermission });
        
        var aggregations = { "amountCurrency": "sum" };
        this.soeGridOptions.addColumnNumber("amountCurrency", this.terms["common.amount"], null, { enableHiding: false, decimals: 2, editable: true });
        this.soeGridOptions.addColumnNumber("supplementCharge", this.terms["economy.supplier.invoice.surcharge"], null, { enableHiding: false, decimals: 2, editable: true });
        this.soeGridOptions.addColumnNumber("sumAmountCurrency", this.terms["common.sum"], null, { enableHiding: false, decimals: 2 });
        this.soeGridOptions.addColumnShape("attestStateColor", null, 55, { shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", showIconField: "attestStateColor" });
        this.soeGridOptions.addColumnBool("includeSupplierInvoiceImage", this.terms["economy.supplier.invoice.includeimage"], 100, { enableEdit: true, toolTip: this.terms["economy.supplier.invoice.includeimage"] });
        this.soeGridOptions.addColumnIcon(null, null, null, { icon: "fal fa-times iconDelete", onClick: this.deleteOrderRow.bind(this), toolTip: this.terms["common.remove"] });

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
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (row, colDef, newValue, oldValue) => { this.afterCellEdit(row, colDef, newValue, oldValue); }));
        this.soeGridOptions.subscribe(events);

        this.soeGridOptions.finalizeInitGrid();
    }

    private afterCellEdit(row: ISupplierInvoiceOrderRowDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {

        if (newValue === oldValue) {
            return;
        }

        if (colDef.field === 'projectName') {
            this.projectChanged(row, newValue);

            this.soeGridOptions.refreshRows(row);
            //this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            this.setAsDirty();
            this.validate();
            return;
        }

        if (colDef.field === 'customerInvoiceNumberName') {
            this.customerInvoiceChanged(row, newValue);

            this.soeGridOptions.refreshRows(row);
            this.setAsDirty();
            this.validate();
            return;
        }

        if (colDef.field === 'amountCurrency') {
            var amountCurrency: number = NumberUtility.parseDecimal(newValue);
            row.amountCurrency = amountCurrency;
            row.sumAmountCurrency = this.calculateAmountSum(amountCurrency, row.supplementCharge);

            this.soeGridOptions.refreshRows(row);
            //this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            this.setAsDirty();
            this.validate();
            return;
        }

        if (colDef.field === 'supplementCharge') {

            var supplementCharge: number = NumberUtility.parseDecimal(newValue);
            row.supplementCharge = supplementCharge;
            row.sumAmountCurrency = this.calculateAmountSum(row.amountCurrency, supplementCharge);

            this.soeGridOptions.refreshRows(row);
            //this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            this.setAsDirty();
            this.validate();
            return;
        }

        if (colDef.field === 'includeSupplierInvoiceImage') {
            this.soeGridOptions.refreshRows(row);
            //this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            this.setAsDirty();
            this.validate();
            return;
        }

    }

    private openOrder(row: any) {
        if(row.customerInvoiceId)
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["common.order"] + " " + row.customerInvoiceNr, row.orderId, BillingOrdersEditController, { id: row.customerInvoiceId, originType: SoeOriginType.Order, feature: Feature.Billing_Order_Status }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html')));
    }
    
    private openProject(row: any) {
        if(row.projectId)
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["common.customer.customer.orderproject"] + " " + row.projectNr, row.projectId, BillingProjectsEditController, { id: row.projectId }, this.urlHelperService.getGlobalUrl('Billing/Projects/Views/edit.html')));
    }

    private calculateAmountSum(amountCurrency: number, supplementCharge: number): number {
        if (amountCurrency && supplementCharge) {
            return amountCurrency * (1 + (supplementCharge / 100));
        }

        return amountCurrency;
    }

    protected allowNavigationFromTypeAhead(value, colDef) {
        if (!value)
            return true;

        if (colDef.field === 'customerInvoiceNumberName') {
            var matched = _.some(this.filteredCustomerInvoices, { 'label': value });
            if (matched)
                return true;
        }
        else {
            var matched = _.some(this.filteredProjects, { 'label': value });
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
            this.filterCustomerInvoicesByProject(0);
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
        //this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
        this.validate();
    }

    protected deleteOrderRow(orderRow) {

        var rows = [];
        for (var i = 0; i < this.invoice.supplierInvoiceOrderRows.length; i++) {
            if (this.invoice.supplierInvoiceOrderRows[i] === orderRow) {
                var row = this.invoice.supplierInvoiceOrderRows[i];
                if (row.customerInvoiceRowId && row.customerInvoiceRowId > 0) {
                    row.state = 2;  //SoeEntityState.Deleted
                    rows.push(row);
                }
                else {
                    this.invoice.supplierInvoiceOrderRows.splice(i, 1);
                }
            } else {
                rows.push(this.invoice.supplierInvoiceOrderRows[i]);
            }
        }
        this.invoice.supplierInvoiceOrderRows = rows;
        this.setGridData(false);

        this.setAsDirty();
    }

    protected validate() {
        this.linkToOrderOrderSet = true;

        _.forEach(_.filter(this.invoice.supplierInvoiceOrderRows, { state: 0 }), (row: any) => {
            if (!row.customerInvoiceId || row.customerInvoiceId === 0) {
                this.linkToOrderOrderSet = false;
                return;
            }
        });
    }

    public addRow() {
        
        var row: any = {};
        row.isReadOnly = false;
        row.isModified = true;
        row.state = SoeEntityState.Active;
        row.supplementCharge = 0;

        row.amount = +(this.invoice.totalAmount - this.invoice.vatAmount) - _.sumBy(this.invoice.supplierInvoiceOrderRows, function (o) { return +o.amount; });
        row.amountCurrency = +(this.invoice.totalAmountCurrency - this.invoice.vatAmountCurrency) - _.sumBy(this.invoice.supplierInvoiceOrderRows, function (o) { return +o.amountCurrency; });
        row.amountLedgerCurrency = 0;
        row.amountEntCurrency = 0;

        row.sumAmount = +(this.invoice.totalAmount - this.invoice.vatAmount) - _.sumBy(this.invoice.supplierInvoiceOrderRows, function (o) { return +o.amount; });
        row.sumAmountCurrency = +(this.invoice.totalAmountCurrency - this.invoice.vatAmountCurrency) - _.sumBy(this.invoice.supplierInvoiceOrderRows, function (o) { return +o.amountCurrency; });
        row.sumAmountEntCurrency = 0;
        row.sumAmountLedgerCurrency = 0;

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
        } else {
            row.projectId = 0;
            row.projectNr = "";
            row.projectName = "";
            row.projectDescription = ""
        }

        //Customer Invoice                        
        var invoices = (this.invoice.orderNr) ? this.customerInvoices.filter(x => x.invoiceNr === this.invoice.orderNr.toString()) : [];

        if (invoices.length > 0) {
            row.customerInvoiceId = invoices[0].invoiceId;
            row.customerInvoiceNr = invoices[0].invoiceNr;
            row.customerInvoiceRowId = 0;
            row.customerInvoiceNumberName = invoices[0].customerInvoiceNumberName;
            row.customerInvoiceDescription = "";
        } else {
            row.customerInvoiceId = 0;
            row.customerInvoiceNr = "";
            row.customerInvoiceRowId = 0;
            row.customerInvoiceNumberName = "";
            row.customerInvoiceDescription = "";
        }
        
        this.invoice.supplierInvoiceOrderRows.push(row);

        this.filterProjectsByStatus();
        this.filterCustomerInvoicesByProject(row.projectId);

        this.setGridData(false);
        //this.validate(); already 
        this.setAsDirty(true);
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


    // DATA
    initialData() {
        this.startProgress();
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions(),
            this.loadCustomerProjects(),
            this.loadCustomerInvoices()]).then(() => {
                this.filterProjectsByStatus();
                this.filterCustomerInvoicesByProject(0);
                if (this.invoice) {
                    this.setGridData(false);
                }
            });
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

    private loadCustomerProjects(): ng.IPromise<any> {
        return this.supplierService.getProjectList(TermGroup_ProjectType.TimeProject, undefined, true, true, false).then((x) => {
            this.projects = x;
        });
    }

    private loadCustomerInvoices(): ng.IPromise<any> {
        //return this.supplierService.getCustomerInvoices(SoeOriginType.Order, SoeOriginStatusClassification.OrdersOpen, TermGroup.ProjectType, OrderInvoiceRegistrationType.Order, true, true, false)
        return this.supplierService.getOrdersForSupplierInvoiceEdit(true).then(x => {
            this.customerInvoices = x;
        });
    }

    public setGridData(checkFiltering: boolean) {
        if (checkFiltering === true) {
            _.forEach(_.filter(this.invoice.supplierInvoiceOrderRows, { state: 0 }), (row: any) => {
                var proj = _.find(this.filteredProjects, { projectId: row.projectId });
                if (!proj) {
                    this.filteredProjects.push({ value: row.projectId, label: row.projectName, projectId: row.projectId });
                }
            });
        }
        this.gridDataLoaded(_.filter(this.invoice.supplierInvoiceOrderRows, { state: 0 }));
        this.$timeout(() => this.stopProgress());
        this.validate();
    }
}

//@ngInject
export function linkToOrderDirective(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        restrict: "E",
        templateUrl: urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/linkToOrder.html'),
        replace: true,
        scope: {
            invoice: "=",
            invoiceIsLoaded: "=",
            linkToOrderOrderSet: "=",
            parentGuid: "="
        },
        link(scope: ng.IScope, element: JQuery, attributes: ng.IAttributes, ngModelController: any) {
            scope.$watch(() => (ngModelController.invoiceIsLoaded), (newVAlue, oldvalue, scope) => {
                if (newVAlue && !oldvalue) {
                    ngModelController.setGridData(false);
                    //ngModelController.stopProgress();
                }

            }, true);
        },
        bindToController: true,
        controllerAs: "ctrl",
        controller: LinkToOrderController
    }
}