import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Guid } from "../../../Util/StringUtility";
import { ProjectTimeBlockDTO, ProjectTimeBlockSaveDTO } from "../../Models/ProjectDTO";
import { TimeProjectContainer, SOEMessageBoxImage, SOEMessageBoxButtons, ProjectTimeRegistrationType, SoeGridOptionsEvent, TimeProjectSearchFunctions, TimeProjectButtonFunctions } from "../../../Util/Enumerations";
import { IEmployeeTimeCodeDTO, ITimeCodeDTO, ITimeDeviationCauseDTO, IProjectSmallDTO, IActionResult, IProjectTimeBookPrintDTO } from "../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../Models/SmallGenericType";
import { IProjectService } from "../../../Shared/Billing/Projects/ProjectService";
import { TimeSheetRowDTO } from "../../Models/TimeSheetDTOs";
import { TermGroup_InvoiceVatType, TermGroup_ProjectType, SoeProjectRecordType, Feature, CompanySettingType, SoeTimeCodeType, TermGroup, SoeEntityState, SoeOriginType, SoeCategoryType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { GridControllerBaseAg } from "../../../Core/Controllers/GridControllerBaseAg";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { TimeColumnOptions } from "../../../Util/SoeGridOptionsAg";
import { EditNoteController } from "./EditNoteController";
import { SelectCustomerInvoiceController } from "../../Dialogs/SelectCustomerInvoice/SelectCustomerInvoiceController";
import { SelectDateController } from "../../Dialogs/SelectDate/SelectDateController";
import { EditTimeGridController } from "./EditTimeGridController";
import { IRequestReportService } from "../../../Shared/Reports/RequestReportService";

export class TimeProjectReportDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Common/Directives/TimeProjectReport/Views/TimeProjectReport.html'),
            scope: {
                readOnly: '=?',
                rows: '=?',
                vatType: "=",
                projectType: "=",
                recordType: "=",
                employeeId: "=",
                projectId: '=',
                printTimeReport: '=?',
                includeOnlyInvoicedTime: '=?',
                setLoading: '=',
                fromDate: '=',
                toDate: '=',
                groupByDate: '=',
                invoiceId: '=',
                projectContainer: '=',
                parentGuid: '=',
                parentIsDirty: '=?',
                gridOptions: '=?',
                loadAttestStatesCallback: '&',
                hasSelectedRows: '=?',
                selectProductRowCallback: '&',
                includeChildProjects: '=?',
                progressBusy: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: TimeProjectReportController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class TimeProjectReportController extends GridControllerBaseAg {

    // Init parameters
    private readOnly: boolean;
    private parentIsDirty: boolean;
    private rows: ProjectTimeBlockDTO[];
    private vatType: TermGroup_InvoiceVatType;
    private projectTyp: TermGroup_ProjectType;
    private recordType: SoeProjectRecordType;
    private employeeId: number;
    private projectId: number;
    private printTimeReport: boolean;
    private includeOnlyInvoicedTime: boolean;
    private setLoading: boolean;
    private fromDate: Date;
    private toDate: Date;
    private invoiceId: number;
    private projectContainer: TimeProjectContainer;
    private parentGuid: Guid;
    private includeChildProjects: boolean;
    
    // Permissions
    private editProjectPermission = false;
    private invoiceTimePermission = false;
    private workTimePermission = false;
    private showAllProjectParticipants = false;
    private modifyOtherEmployeesPermission = false;
    private isProjectParticipant = true;
    private editOrderPermission = false;
    private splitTimeProductRowsPermission = false;
    private editCustomerPermission = false;
    private onlyMyOrders = false;

    // Company settings
    private projectCreateInvoiceRowFromTransaction = false;
    private projectLimitOrderToProjectUsers = false;
    private defaultTimeCodeId = 0;
    private timeProjectReportId = 0;
    private attestStateTransferredOrderToInvoiceId = 0;
    private useExtendedTimeRegistration = false;
    private createTransactionsBasedOnTimeRules = false;
    private invoiceTimeAsWorkTime = false;

    // Lookups
    private terms: { [index: string]: string; };
    private employee: IEmployeeTimeCodeDTO;
    private employees: IEmployeeTimeCodeDTO[] = [];
    private employeesDict: any[] = [];
    private selectedEmployeesDict: any[] = [];
    private selectedProjectsDict: any[] = [];
    private selectedOrdersDict: any[] = [];
    private selectedEmployeeCategoryDict: any[] = [];
    private selectedTimeDeviationCauseDict: any[] = [];
    private timeCodes: ITimeCodeDTO[] = [];
    private timeCodesDict: any[] = [];
    private customers: SmallGenericType[] = [];
    private weekdays: SmallGenericType[] = [];
    private includeTimeInReportItems: any[] = [];
    private projects: IProjectSmallDTO[];
    private employeeDaysWithSchedule: any[];

    // GUI
    private toolbarInclude: any;
    private showAdditionalTime = false;
    private steppingRules: any;
    private showWeekendTimesWarning = false;
    private sumInvoicedTime: string;
    private sumWorkedTime: string;
    private sumOtherTime: string;
    private customersDict: any[] = [];
    private allProjects: any[] = [];
    private allProjectsAndInvoices: any[] = [];
    private allOrders: any[] = [];
    private filteredProjectsDict: any[] = [];
    private filteredOrdersDict: any[] = [];
    private filteredEmployeeCategoriesDict: any[] = [];
    private filteredTimeDeviationCauseDict: any[] = [];
    
    private projectInvoices: any[] = [];
    private executing = false;
    
    private _includeTimeInReport: number;
    private set includeTimeInReport(id: number) {
        this.printTimeReport = (id !== 0);
        this.includeOnlyInvoicedTime = (id === 2);
        this._includeTimeInReport = id;
    }
    private get includeTimeInReport() {
        return this._includeTimeInReport;
    }

    private _groupByDate = false;
    private get groupByDate() {
        return this._groupByDate;
    }
    private set groupByDate(group: boolean) {
        if (this._groupByDate !== group) {
            this._groupByDate = group;
            this.toggleScheduledQuantityFormatted();
        }
    }

    private modalInstance: any;

    // Flags
    private loadingRows = false;
    private hasSelectedRows = false;
    private hasSelectedMyOwnRows = false;
    private doReload = false;
    private searchIncPlannedAbsence = false;

    // Properties
    public get isOrder(): boolean {
        return this.projectContainer === TimeProjectContainer.Order;
    }

    public get isTimeSheet(): boolean {
        return this.projectContainer === TimeProjectContainer.TimeSheet;
    }

    public get isOrderRows(): boolean {
        return this.projectContainer === TimeProjectContainer.OrderRows;
    }

    public get isProjectCentral(): boolean {
        return this.projectContainer === TimeProjectContainer.ProjectCentral;
    }

    public get showFooter(): boolean {
        return this.isOrder;
    }

    public get showEmployeeSelect(): boolean {
        return (this.isTimeSheet || this.isProjectCentral) && this.modifyOtherEmployeesPermission === true;
    }

    public get showTimeDeviationCauseSelect(): boolean {
        return (this.isTimeSheet) && this.useExtendedTimeRegistration === true;
    }

    //Functions
    searchButtonFunctions: any = [];
    buttonFunctions: any = [];

    gridOptions: any;
    loadAttestStatesCallback: (employeeGroupId: any) => void;
    selectProductRowCallback: () => ng.IPromise<number>;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $window: ng.IWindowService,
        protected coreService: ICoreService,
        private readonly projectService: IProjectService,
        private readonly requestReportService: IRequestReportService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
        super("Common.Directives.TimeProjectReport", "billing.project.timesheet.timesheet", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants, null, null, "directiveCtrl");
        
        this.toolbarInclude = urlHelperService.getGlobalUrl("Common/Directives/TimeProjectReport/Views/gridHeader.html");

        this.modalInstance = $uibModal;

        this.$scope.$on(Constants.EVENT_RELOAD_GRID, (e, a) => {
            if (this.parentGuid == a.guid) {
                this.reloadGrid();
            }
        })
    }

    public $onInit() {
        if (this.isTimeSheet) {
            this.gridOptions = this.soeGridOptions;
            this.soeGridOptions.setName(this.soeGridOptions.getName() + ".TimeSheet");
        }
        else if (this.isProjectCentral) {
            this.gridOptions = this.soeGridOptions;
            this.soeGridOptions.setName(this.soeGridOptions.getName() + ".ProjectCentral");
            this.messagingService.subscribe(Constants.EVENT_TAB_ACTIVATED, (x) => {
                this.onControllActivated(x);
            });

            this.$scope.$on('onTabActivated', (e, a) => {
                this.onControllActivated(a);
            });
        }
        else {
            this.soeGridOptions.setMinRowsToShow(10);            
        }

        this.translationService.translate("core.loading").then((term) => {
            this.filteredOrdersDict.push({ id: 0, label: term });
            this.filteredProjectsDict.push({ id: 0, label: term });
            
            this.$scope.$on('editTimeRow', (e, a) => {
                if (a && a.guid === this.parentGuid) {
                    if (a.move)
                        this.moveRows();
                    else if (a.changeDate)
                        this.changeDate();
                    else if (a.open)
                        this.edit(a.timeRow);
                }
            });
        });
    }

    public onControllActivated(tabGuid: any) {
        if (tabGuid !== this.parentGuid)
            return;

        if (this.doReload) {
            this.projectInvoices = [];
            this.selectedProjectsDict = [];
            this.selectedOrdersDict = [];
            this.doReload = false;
        }

        this.populateProjectAndInvoice([]);
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
            if (this.isTimeSheet) {
                this.searchButtonFunctions.push({ id: TimeProjectSearchFunctions.SearchIntervall, name: terms["core.search"] });
                if (this.useExtendedTimeRegistration) {
                    this.searchButtonFunctions.push({ id: TimeProjectSearchFunctions.SearchIncPlannedAbsence, name: terms["billing.project.timesheet.searchincplannedabsence"] });
                }
                this.searchButtonFunctions.push({ id: TimeProjectSearchFunctions.SearchWithGroupOnDate, name: this.useExtendedTimeRegistration ? terms["billing.project.timesheet.groupbydateincplannedabsence"] : terms["billing.project.timesheet.groupbydate"] });
            }
            else if (this.isProjectCentral) {
                this.searchButtonFunctions.push({ id: TimeProjectSearchFunctions.SearchIntervall, name: terms["core.search"] });
                this.searchButtonFunctions.push({ id: TimeProjectSearchFunctions.GetAll, name: terms["billing.order.timeproject.getall"] });
            }
            else {
                this.searchButtonFunctions.push({ id: TimeProjectSearchFunctions.SearchIntervall, name: terms["billing.order.timeproject.searchintervall"] });
                this.searchButtonFunctions.push({ id: TimeProjectSearchFunctions.GetAll, name: terms["billing.order.timeproject.getall"] });
            }

            if (!this.readOnly && !this.isOrderRows) {
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
            }
        });
    }

    private getInvoiceColumnIndex() {
        return this.soeGridOptions.getColumnIndex('invoiceNr');
    }

    private toggleScheduledQuantityFormatted() {
        if (this.groupByDate) {
            this.soeGridOptions.showColumn("scheduledQuantityFormatted");
        }
        else {
            this.soeGridOptions.hideColumn("scheduledQuantityFormatted");
        }
    }

    private loadBaseData(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions(),
            this.loadReadOnlyPermissions(),
            this.loadCompanySettings()
        ])
    }

    public setupGrid() {
        this.startLoad();
        if (this.isOrder) {
            this.loadBaseData().then(() => {
                this.loadEmployee().then(() => {
                    this.$q.all([
                        this.loadTimeCodes(),
                        this.loadEmployees(),
                        this.loadWeekdays(),
                        this.populateTimeDeviationCause()
                    ]).then(() => {
                        this.setButtonFunctions();
                        this.gridAndDataIsReady();
                    })
                })
            })
        }
        else if (this.isOrderRows) {
            this.loadBaseData().then(() => {
                this.setButtonFunctions();
                this.gridAndDataIsReady();
            })
        }
        else {
            this.$q.all([this.loadTerms(),
            this.loadModifyPermissions(),
            this.loadReadOnlyPermissions(),
            this.loadCompanySettings(),
            this.loadCustomers(),
            this.loadEmployee().then(() => {
                this.$q.all([
                    this.loadTimeCodes(),
                    this.populateTimeDeviationCause(),
                    this.loadWeekdays()]).then(() => {
                        this.$q.all([this.loadEmployees()]).then(() => {
                            this.$q.all([ /*this.loadProjectInvoices(),*/  this.loadUserValidPayrollAttestStates(false)]).then(() => {
                                this.setButtonFunctions();
                                this.gridAndDataIsReady();
                            });
                        });
                    });
            })
            ])
        }
    }

    protected setupCustomToolBar() {
        this.setupDefaultToolBar(false,true);
    }

    private gridAndDataIsReady() {
        if (this.projectLimitOrderToProjectUsers && this.employeeId)
            this.isProjectParticipant = _.includes(_.map(this.employees, e => e.employeeId), this.employeeId);

        this.setIncludeTimeInReportItems();
        this.setupGridColumns();
        this.setupWatchers();
    }

    private setIncludeTimeInReportItems() {
        this.includeTimeInReportItems.push({ id: 0, name: this.terms["billing.project.timesheet.includetimeinreport.none"] });
        this.includeTimeInReportItems.push({ id: 1, name: this.terms["billing.project.timesheet.includetimeinreport.all"] });
        this.includeTimeInReportItems.push({ id: 2, name: this.terms["billing.project.timesheet.includetimeinreport.invoiced"] });
        if (this.printTimeReport && this.includeOnlyInvoicedTime)
            this.includeTimeInReport = 2;
        else if (this.printTimeReport)
            this.includeTimeInReport = 1;
        else
            this.includeTimeInReport = 0;
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.rows, () => {
            if (this.rows) {
                this.clearGridStates();
                this.rows.forEach(r => {
                    r["dateFormatted"] = CalendarUtility.toFormattedTime(r.startTime) + " - " + CalendarUtility.toFormattedTime(r.stopTime);
                    
                    if (r.customerInvoiceRowAttestStateId === 0 && !r.customerInvoiceRowAttestStateName) {
                        r.customerInvoiceRowAttestStateName = " ";
                    }

                    if (r.employeeIsInactive)
                        r["columnNameTooltip"] = this.terms['billing.project.timesheet.employeeinactivated'];
                    else
                        r["columnNameTooltip"] = undefined;
                });
            }
            this.gridDataLoaded(this.rows, this.isTimeSheet || this.isProjectCentral);
            
            this.loadingRows = false;
        });
        this.$scope.$watch(() => this.setLoading, () => {
            if (this.setLoading)
                this.startLoadModal();
        });
        this.$scope.$watch(() => this.projectId, () => {
            if (this.isOrder)
                this.loadProjectTotals();
        });
        this.$scope.$watch(() => this.fromDate, (newValue: Date, oldValue: Date) => {
            if (newValue && oldValue && (newValue.getTime() > this.toDate.getTime())) {
                this.toDate = this.fromDate.addDays(6);
            }
            else if ( (newValue != oldValue) && this.modifyOtherEmployeesPermission) {
                this.loadEmployeesFromDateChange();
            }
        });
        this.$scope.$watch(() => this.toDate, (newValue: Date, oldValue: Date) => {
            if (newValue && oldValue && (newValue.getTime() < this.fromDate.getTime())) {
                this.fromDate = this.toDate;
            }
            else if ( (newValue != oldValue) && this.modifyOtherEmployeesPermission) {
                this.loadEmployeesFromDateChange();
            }
        });
        this.$scope.$watch(() => this.projectId, (newVal, oldVal) => {
            if (newVal && oldVal && newVal > 0) {
                this.soeGridOptions.setData(null);
                this.doReload = true;
            }
        });
    }

    private setupGridColumns() {

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.edit(row); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows: ProjectTimeBlockDTO[]) => {
            const localhasSelectedMyOwnRows = (rows.filter(x => x.employeeId === this.employeeId).length > 0);
            this.hasSelectedRows = (Array.isArray(rows) && rows.length > 0);
            
            if (localhasSelectedMyOwnRows !== this.hasSelectedMyOwnRows) {
                this.hasSelectedMyOwnRows = localhasSelectedMyOwnRows;
                this.loadUserValidPayrollAttestStates(this.hasSelectedMyOwnRows && this.employees.length > 0);
            }
            this.$scope.$applyAsync();
        }));

        this.soeGridOptions.subscribe(events);
        this.soeGridOptions.addGroupTimeSpanSumAggFunction(false);
        const timeColumnOptions: TimeColumnOptions = { enableHiding: true, enableRowGrouping: true,
            clearZero: false, alignLeft: false, minDigits: 5, aggFuncOnGrouping: "sumTimeSpan", cellClassRules: {
                "excelTime": () => true,
            }};

        const timePayrollColumnOptions: TimeColumnOptions = {
            enableHiding: true,
            clearZero: false, alignLeft: false, enableRowGrouping: true, minDigits: 5, aggFuncOnGrouping: "sumTimeSpan", cellClassRules: {
                "errorRow": (gridRow: any) => gridRow.data && (gridRow.data.timePayrollQuantity < gridRow.data.scheduledQuantity),
                "excelTime": () => true,
            }
        };
        
        this.addColumnText("employeeNr", this.terms["billing.project.timesheet.employeenr"], null, { enableHiding: true, hide: true, enableRowGrouping: true });
        super.addColumnText("employeeName", this.terms["common.employee"], null, { toolTipField: "columnNameTooltip", enableRowGrouping: true, cellClassRules: { "errorRow": (row: any) => row && row.data && row.data.employeeIsInactive } });
        this.soeGridOptions.addColumnDate("date", this.terms["common.date"], null, true, null, null, {
            toolTipField: "dateFormatted", enableRowGrouping: true, cellClassRules: {
                "excelDate": () => true,
            }
        });
        this.addColumnText("yearWeek", this.terms["common.yearweek"], null, { enableHiding: true, hide: true, enableRowGrouping: true });
        this.addColumnText("weekDay", this.terms["common.weekday"], null, { enableHiding: true, enableRowGrouping: true });
        
        if (this.useExtendedTimeRegistration) {
            super.addColumnText("timeDeviationCauseName", this.terms["common.time.timedeviationcause"], null, { enableHiding: true, enableRowGrouping: true });
        }

        super.addColumnText("timeCodeName", this.terms["billing.project.timesheet.chargingtype"], null, { enableRowGrouping: true});
        const iconWhenEmpty = "fas fa-exclamation warningColor";
        if (this.workTimePermission) {
            const workIconWhenEmpty = this.useExtendedTimeRegistration ? iconWhenEmpty : "";

            if (this.showAdditionalTime) {
                this.addColumnTimeSpan("timeAdditionalQuantityFormatted", this.terms["billing.project.timesheet.othertime"], null, timePayrollColumnOptions);
            }

            this.addColumnTimeSpan("timePayrollQuantityFormatted", this.terms["billing.project.timesheet.workedtime"], null, timePayrollColumnOptions);
            this.addColumnShape("timePayrollAttestStateName", null, 40, { cellStyle: (row) => { return !row?.timePayrollAttestStateId ? { 'margin-left': '3px' } : undefined }, maxWidth: 40, shape: Constants.SHAPE_CIRCLE, toolTipField: "timePayrollAttestStateName", colorField: "timePayrollAttestStateColor", showIconField: "timePayrollAttestStateColor", showEmptyIcon: (data: any) => !data.timePayrollAttestStateId && data.timePayrollQuantity ? workIconWhenEmpty : "" });
        }

        if (this.invoiceTimePermission) {
            super.addColumnTimeSpan("invoiceQuantityFormatted", this.terms["billing.project.timesheet.invoicedtime"], null, timeColumnOptions);
            this.addColumnShape("customerInvoiceRowAttestStateName", null, 40, { cellStyle: (row) => { return !row?.customerInvoiceRowAttestStateId ? { 'margin-left': '3px' } : undefined }, maxWidth: 40, shape: Constants.SHAPE_CIRCLE, colorField: "customerInvoiceRowAttestStateColor", toolTipField: "customerInvoiceRowAttestStateName", showIconField: "customerInvoiceRowAttestStateColor", showEmptyIcon: (data: any) => !data.customerInvoiceRowAttestStateId && data.invoiceQuantity ? iconWhenEmpty : "" });
        }

        if (this.isTimeSheet || this.isProjectCentral) {
            super.addColumnTimeSpan("scheduledQuantityFormatted", this.terms["billing.project.timesheet.scheduletime"], null, timeColumnOptions);
        }

        this.addColumnIcon("noteIcon", "", null, { onClick: this.showNote.bind(this), suppressExport:true });

        if (this.isTimeSheet || this.isProjectCentral) {
            super.addColumnText("invoiceNr", this.terms["billing.project.timesheet.invoice"], null, { enableHiding: true, enableRowGrouping: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => row.showOrderButton && this.editOrderPermission, callback: this.openOrder.bind(this) } } );
            //var invoiceCol = super.addColumnText("invoiceNr", this.terms["billing.project.timesheet.invoice"], null, true, null, null, null, null, null, null, this.isTimeSheet ? "iconEdit fal fa-pencil" : null, this.isTimeSheet ? "openOrder" : null, "directiveCtrl", "showOrderButton");
            super.addColumnText("customerName", this.terms["common.customer.customer.customer"], null, {
                enableHiding: true, enableRowGrouping: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => row.showCustomerButton && this.editCustomerPermission, callback: this.openCustomer.bind(this) } });
            //var customerCol = super.addColumnText("customerName", this.terms["common.customer.customer.customer"], null, true, null, null, null, null, null, null, this.isTimeSheet ? "iconEdit fal fa-pencil" : null, this.isTimeSheet ? "openCustomer" : null, "directiveCtrl", "showCustomerButton");

            this.addColumnText("projectNr", this.terms["billing.project.projectnr"], null, { enableHiding: true, hide: true, enableRowGrouping: true });
            super.addColumnText("projectName", this.terms["billing.project.project"], null, {
                enableHiding: true, enableRowGrouping: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => row.showProjectButton && this.editProjectPermission, callback: this.openProject.bind(this) } } );
            //var projectCol = super.addColumnText("projectName", this.terms["billing.project.project"], null, true, null, null, null, null, null, null, this.isTimeSheet ? "iconEdit fal fa-pencil" : null, this.isTimeSheet ? "openProject" : null, "directiveCtrl", "showProjectButton");

            this.addColumnText("referenceOur", this.terms["billing.project.timesheet.ourreference"], null, { enableHiding: true, hide: true, enableRowGrouping: true });
            this.addColumnText("internOrderText", this.terms["billing.project.timesheet.internaltext"], null, { enableHiding: true, hide: true, enableRowGrouping: true });
        }

        this.addColumnText("externalNote", this.terms["billing.project.timesheet.edittime.externalnote"], null, { enableHiding: true, hide: true, enableRowGrouping: true, toolTipField: "externalNote" });
        this.addColumnText("internalNote", this.terms["billing.project.timesheet.edittime.internalnote"], null, { enableHiding: true, hide: true, enableRowGrouping: true, toolTipField: "internalNote" });

        if (this.isProjectParticipant && !this.readOnly) {
            this.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));
        }

        this.soeGridOptions.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"],
            selected: this.terms["core.aggrid.totals.selected"]
        });
        /*
        const timeSpanColumnAggregate = {
            getSeed: () => 0,
            accumulator: (acc, next) => CalendarUtility.sumTimeSpan(acc, next),
            cellRenderer: this.timeSpanAggregateRenderer.bind(this)
        } as IColumnAggregate;

        
        this.soeGridOptions.addFooterRow("#time-row-grid-sum-footer", {
            "timePayrollQuantityFormatted": timeSpanColumnAggregate,
            "invoiceQuantityFormatted": timeSpanColumnAggregate,
        } as IColumnAggregations)
        */
        
        this.soeGridOptions.useGrouping(true, true, {
            keepColumnsAfterGroup: false, selectChildren: true, keepGroupState: true, groupSelectsFiltered: true, totalTerm: this.terms["common.sum"]
        });
        this.soeGridOptions.finalizeInitGrid();
        
        this.restoreState(this.isTimeSheet || this.isProjectCentral);
        this.toggleScheduledQuantityFormatted();
    }
    /*
    private timeSpanAggregateRenderer({ data, colDef, formatValue }) {
        var value = data[colDef.field];
        if (!value || value === "0" || value === "00:00")
            return "<div></div>";
        return "<b>" + data[colDef.field] + "<b>";
    }
    */
    // Lookups
    private loadTerms(): ng.IPromise<any> {
        // Columns
        const keys: string[] = [
            "core.edit",
            "core.newrow",
            "core.deleterow",
            "core.donotshowagain",
            "core.warning",
            "common.date",
            "common.year",
            "common.month",
            "common.missing",
            "common.yearmonth",
            "common.yearweek",
            "common.weekday",
            "common.employee",
            "common.sum",
            "common.time.timedeviationcause",
            "billing.project.timesheet.chargingtype",
            "common.order",
            "common.customer.customer.customer",
            "billing.project.timesheet.invoice",
            "billing.project.project",
            "billing.project.timesheet.quantity.short",
            "billing.project.timesheet.invoicequantity.short",
            "billing.project.timesheet.totalquantity",
            "billing.project.timesheet.note",
            "billing.project.timesheet.note.edit",
            "billing.project.timesheet.note.editfor",
            "billing.project.timesheet.note.internal",
            "billing.project.timesheet.note.external",
            "billing.project.timesheet.wholeweek",
            "billing.project.timesheet.invoicedtime",
            "billing.project.timesheet.workedtime",
            "billing.project.timesheet.includetimeinreport.none",
            "billing.project.timesheet.includetimeinreport.all",
            "billing.project.timesheet.includetimeinreport.invoiced",
            "billing.project.timesheet.timesheet",
            "billing.project.timesheet.savebeforeedittimerow",
            "billing.project.timesheet.timerowsstatuschange",
            "billing.project.timesheet.employeenr",
            "billing.project.timesheet.scheduletime",
            "billing.project.timesheet.othertime",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "core.deleterowwarning",
            "billing.project.timesheet.employeeinactivated",
            "billing.project.projectnr",
            "billing.project.timesheet.asksaveorder",
            "billing.project.timesheet.ourreference",
            "billing.project.timesheet.internaltext",
            "billing.project.timesheet.edittime.externalnote",
            "billing.project.timesheet.edittime.internalnote"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [];
        featureIds.push(Feature.Billing_Project_Edit); // Edit project
        featureIds.push(Feature.Billing_Project_TimeSheetUser_OtherEmployees);
        featureIds.push(Feature.Time_Time_TimeSheetUser_OtherEmployees);
        featureIds.push(Feature.Billing_Order_Orders_Edit);
        featureIds.push(Feature.Billing_Order_Orders_Edit_Splitt_TimeRows);
        featureIds.push(Feature.Billing_Customer_Customers_Edit);
        featureIds.push(Feature.Billing_Project_Central_TimeSheetUser);
        featureIds.push(Feature.Billing_Order_Orders);
        featureIds.push(Feature.Billing_Order_OrdersAll);
        featureIds.push(Feature.Billing_Order_OrdersUser);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.editProjectPermission = x[Feature.Billing_Project_Edit];
            this.editOrderPermission = x[Feature.Billing_Order_Orders_Edit];
            this.splitTimeProductRowsPermission = x[Feature.Billing_Order_Orders_Edit_Splitt_TimeRows];
            this.editCustomerPermission = x[Feature.Billing_Customer_Customers_Edit];
            this.modifyOtherEmployeesPermission = (this.isTimeSheet || this.isProjectCentral) ? (x[Feature.Billing_Project_TimeSheetUser_OtherEmployees] || x[Feature.Time_Time_TimeSheetUser_OtherEmployees]) : true;
            if (this.isProjectCentral) {
                this.readOnly = (!this.modifyOtherEmployeesPermission && x[Feature.Billing_Project_Central_TimeSheetUser])
            }
            this.onlyMyOrders = !x[Feature.Billing_Order_Orders] && !x[Feature.Billing_Order_OrdersAll] && x[Feature.Billing_Order_OrdersUser];
        });
    }

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [];
        featureIds.push(Feature.Time_Project_Invoice_WorkedTime);       // Show worked time
        featureIds.push(Feature.Time_Project_Invoice_InvoicedTime);     // Show invoiced time
        featureIds.push(Feature.Time_Project_Invoice_ShowAllPersons);   // Show all project participants

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            this.workTimePermission = x[Feature.Time_Project_Invoice_WorkedTime];
            this.invoiceTimePermission = x[Feature.Time_Project_Invoice_InvoicedTime];
            this.showAllProjectParticipants = x[Feature.Time_Project_Invoice_ShowAllPersons];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.ProjectCreateInvoiceRowFromTransaction);
        settingTypes.push(CompanySettingType.ProjectLimitOrderToProjectUsers);
        settingTypes.push(CompanySettingType.TimeDefaultTimeCode);
        settingTypes.push(CompanySettingType.BillingDefaultTimeProjectReportTemplate);
        settingTypes.push(CompanySettingType.BillingStatusTransferredOrderToInvoice);
        settingTypes.push(CompanySettingType.ProjectUseExtendedTimeRegistration);
        settingTypes.push(CompanySettingType.ProjectCreateTransactionsBaseOnTimeRules);
        settingTypes.push(CompanySettingType.ProjectInvoiceTimeAsWorkTime);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.projectCreateInvoiceRowFromTransaction = x[CompanySettingType.ProjectCreateInvoiceRowFromTransaction];
            this.projectLimitOrderToProjectUsers = x[CompanySettingType.ProjectLimitOrderToProjectUsers];
            this.defaultTimeCodeId = x[CompanySettingType.TimeDefaultTimeCode];
            this.timeProjectReportId = x[CompanySettingType.BillingDefaultTimeProjectReportTemplate];
            this.attestStateTransferredOrderToInvoiceId = x[CompanySettingType.BillingStatusTransferredOrderToInvoice];
            this.useExtendedTimeRegistration = x[CompanySettingType.ProjectUseExtendedTimeRegistration];
            this.createTransactionsBasedOnTimeRules = x[CompanySettingType.ProjectCreateTransactionsBaseOnTimeRules];
            this.invoiceTimeAsWorkTime = x[CompanySettingType.ProjectInvoiceTimeAsWorkTime];
        });
    }

    private loadEmployee(): ng.IPromise<any> {
        return this.projectService.getEmployeeForUserWithTimeCode(this.fromDate).then(x => {
            this.employee = x;
        });
    }

    public loadEmployeesFromDateChange = _.debounce(() => {
        if (!this.isOrderRows) {
            if (!this.employee) {
                this.loadEmployee().then(() => {
                    this.loadEmployees();
                })
            }
            else {
                this.loadEmployees();
            }
        }

    }, 100, { leading: false, trailing: true });

    private loadEmployees(): ng.IPromise<any> {
        const deferral = this.$q.defer();
        this.employees = [];
        this.employeesDict = [];

        if (this.projectId) {
            if (this.modifyOtherEmployeesPermission) {
                return this.projectService.getEmployeesForTimeProjectRegistrationSmall(this.projectId, this.fromDate, this.toDate).then(x => {
                    this.employees = x;
                    _.forEach(x, (e) => {
                        this.employeesDict.push({ id: e.employeeId, label: e.name });
                    });
                });
            } else {
                if (this.employee) {
                    this.employees = [];
                    this.employees.push({ employeeId: this.employee.employeeId, name: this.employee.name, employeeNr: this.employee.employeeNr, defaultTimeCodeId: this.employee.defaultTimeCodeId, timeDeviationCauseId: this.employee.timeDeviationCauseId, employeeGroupId: this.employee.employeeGroupId, autoGenTimeAndBreakForProject: this.employee.autoGenTimeAndBreakForProject });
                    this.employeesDict.push({ id: this.employee.employeeId, label: this.employee.name });
                }
                deferral.resolve();
            }
        } else {
            if (this.modifyOtherEmployeesPermission) {
                const categories = this.selectedEmployeeCategoryDict.map(a => a.id);
                return this.projectService.getEmployeesForProjectTimeCode(false, false, false, this.employeeId, this.fromDate, this.toDate, categories).then( (x: IEmployeeTimeCodeDTO[]) => {
                    this.employees = x;
                    x.forEach( (e) => {
                        this.employeesDict.push({ id: e.employeeId, label: e.name + " (" + e.employeeNr +")" });
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

    private getSelectedEmployees(): number[] {
        const employeeIds = [];
        if (this.selectedEmployeesDict.length > 0) {
            this.selectedEmployeesDict.forEach( (e) => {
                employeeIds.push(e.id);
            });
        }
        return employeeIds;
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

    private populateTimeDeviationCause(): ng.IPromise<any> {
        if (this.useExtendedTimeRegistration && this.filteredTimeDeviationCauseDict.length == 0) {
            return this.projectService.getTimeDeviationCauses(0, false, true).then((deviationCauses: ITimeDeviationCauseDTO[]) => {
                _.sortBy(deviationCauses, 'name').forEach((c) => {
                    if (c.calculateAsOtherTimeInSales) {
                        this.showAdditionalTime = true;
                    }
                    this.filteredTimeDeviationCauseDict.push({ id: c.timeDeviationCauseId, label: c.name });
                });
            });
        }
        else {
            const deferral = this.$q.defer();
            deferral.resolve();
            return deferral.promise;
        }
    }

    private populateProjectAndInvoice(employeeIds: number[] = []): ng.IPromise<any> {
        //var currentEmployees = this.projectInvoices.filter(x => x.employeeId)
        if (!this.isOrder) {
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
        else {
            const deferral = this.$q.defer();
            deferral.resolve();
            return deferral.promise;
        }
    }

    private loadProjectInvoices(employeeIds: number[]): ng.IPromise<any[]> {
        //var employeeIds = _.map(this.employees, e => e.employeeId);

        const deferral = this.$q.defer<any[]>();
        
        this.projectService.getProjectsForTimeSheetEmployees(employeeIds, this.projectId).then((result: any[]) => {
            if (this.isProjectCentral)
                this.projectInvoices = result;
            else
                this.projectInvoices = this.projectInvoices.concat(result);
            
            this.allProjectsAndInvoices = [];
            this.allProjects = [];
            this.filteredProjectsDict = [];
            this.allOrders = [];
            this.filteredOrdersDict = [];

            for (let e of this.projectInvoices) {
                //Filter projects
                for (let p of e.projects) {
                    p.numberName = p.numberName?.replace(/\r?\n|\r/g, " ") ?? "";
                    p.name = p.name?.replace(/\r?\n|\r/g, " ") ?? "";
                    p.customerName = p.customerName?.replace(/\r?\n|\r/g, " ") ?? "";
                    if (_.filter(this.allProjects, x => x.id === p.projectId).length === 0) {
                        this.allProjectsAndInvoices.push(p);
                        this.allProjects.push({ id: p.projectId, label: p.numberName });
                        this.filteredProjectsDict.push({ id: p.projectId, label: p.numberName })
                    }
                }
                //Filter invoices
                for (let i of e.invoices) {
                    i.customerName = i.customerName?.replace(/\r?\n|\r/g, " ") ?? "";
                    i.numberName = i.numberName?.replace(/\r?\n|\r/g, " ") ?? "";

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

    private loadUserValidPayrollAttestStates(forceUseEmployeeGroupId: boolean) {
        if (!this.employee && (this.employees.length < 2) )
            return;

        const employeeGroupId = this.employees.length > 1 && !forceUseEmployeeGroupId ? undefined : this.employee.employeeGroupId;

        this.loadAttestStatesCallback({ employeeGroupId: employeeGroupId });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        const type = this.useExtendedTimeRegistration ? SoeTimeCodeType.Work : SoeTimeCodeType.WorkAndAbsense;
        const onlyWithProducts = this.useExtendedTimeRegistration ? true : false;
        return this.projectService.getTimeCodesByType(type, true, false, onlyWithProducts).then((x) => {
            this.timeCodes = x;
            _.forEach(x, (t) => {
                this.timeCodesDict.push({ value: t.timeCodeId, label: t.name });
            });
        });
    }

    private loadTimeDeviationCausesForEmployee(employeeGroupId: number): ng.IPromise<ITimeDeviationCauseDTO[]> {
        return this.projectService.getTimeDeviationCauses(employeeGroupId, false, true);
    }

    private getEmployeeChilds(employeeId: number): ng.IPromise<SmallGenericType[]> {
        return this.projectService.getEmployeeChilds(employeeId);
    }

    private loadCustomers(): ng.IPromise<any> {
        return this.projectService.getCustomersDict(true, true).then((x) => {
            this.customers = x;
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

    private loadProjectTotals(): ng.IPromise<any> {
        return this.projectService.getProjectTotals(this.projectId, this.invoiceId, this.recordType).then((totals: any) => {
            this.sumWorkedTime = CalendarUtility.minutesToTimeSpan(totals.workTime);
            this.sumInvoicedTime = CalendarUtility.minutesToTimeSpan(totals.invoiceTime);
            this.sumOtherTime = CalendarUtility.minutesToTimeSpan(totals.otherTime);
        });
    }

    // Events
    private timeDeviationCauseSelectionComplete() {
        console.log("timeDeviationCauseSelectionComplete");
    }
    private employeeSelectionComplete() {
        this.populateProjectAndInvoice(this.getSelectedEmployees());
        this.sortSelectedRows();
    }

    private projectSelectionComplete() {
        this.sortSelectedRows();
    }

    private orderSelectionComplete() {
        loop1: for (let o of this.selectedOrdersDict) {
            for (let p of this.allProjectsAndInvoices) {
                if (_.filter(p.invoices, x => x['invoiceId'] === o.id).length > 0 && _.filter(this.selectedProjectsDict, p => p.key === p['projectId']).length === 0) {
                    this.selectedProjectsDict.push({ id: p['projectId'] });
                    break loop1;
                }
            }
        }

        this.sortSelectedRows();
    }

    private employeeCategoriesSelectionComplete() {
        this.selectedEmployeesDict = [];
        this.loadEmployees();
    }

    private loadData(getIntervall = true, incPlannedAbsence = false) {
        this.loadingRows = true;
        if (this.isTimeSheet) {
            const projects: number[] = [];
            this.selectedProjectsDict.forEach( p => {
                projects.push(p.id);
            });
            const orders: number[] = [];
            this.selectedOrdersDict.forEach( o => {
                orders.push(o.id);
            });

            const categories: number[] = [];
            this.selectedEmployeeCategoryDict.forEach(o => {
                categories.push(o.id);
            });

            const timeDeviationCauses: number[] = [];
            this.selectedTimeDeviationCauseDict.forEach(o => {
                timeDeviationCauses.push(o.id);
            });

            this.SendSearchTimeSheetRows(projects, orders, categories, timeDeviationCauses, incPlannedAbsence);
        }
        else if (this.isProjectCentral) {
            let projects: number[] = [];
            let orders: number[] = [];
            if (!getIntervall) {
                if (this.projectId > 0) {
                    if (this.includeChildProjects)
                        projects = _.map(this.allProjects, 'id');
                    else
                        projects.push(this.projectId);
                }
            }
            else {
                this.selectedProjectsDict.forEach(p => {
                    projects.push(p.id);
                });
                this.selectedOrdersDict.forEach(o => {
                    orders.push(o.id);
                });

                if (projects.length === 0 && this.projectId > 0) {
                    if (this.includeChildProjects)
                        projects = _.map(this.allProjects, 'id');
                    else
                        projects.push(this.projectId);
                }
            }

            if (this.isTimeSheet || (projects.length > 0 || orders.length > 0))
                this.SendSearchTimeSheetRows(projects, orders, [],[], incPlannedAbsence, !getIntervall);
        }
        else {
            this.messagingService.publish(Constants.EVENT_SEARCH_TIME_PROJECT_ROWS, {guid: this.parentGuid, GetIntervall: getIntervall });
        }
    }

    private SendSearchTimeSheetRows(projects: number[], orders: number[], categories: number[], timeDeviationCauses: number[], incPlannedAbsence: boolean, getAll = false) {
        const emps: number[] = this.getSelectedEmployees();
        const incInternOrderText = (this.isTimeSheet && this.soeGridOptions.isColumnVisible("internOrderText") );
        this.messagingService.publish(Constants.EVENT_SEARCH_TIME_PROJECT_ROWS_TIMESHEET, { guid: this.parentGuid, emps: emps, projs: projects, orders: orders, employeeCategories: categories, incPlannedAbsence: incPlannedAbsence, incInternOrderText: incInternOrderText, getAll: getAll, timeDeviationCauses: timeDeviationCauses });
     
    }

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

    public edit(row: ProjectTimeBlockDTO) {
        if (this.executing || !this.isProjectParticipant || (row && row.employeeIsInactive) || this.isOrderRows || this.readOnly)
            return;
        
        if (this.parentIsDirty && this.invoiceId > 0) {
            //var modal = this.notificationService.showDialog(this.terms["billing.project.timesheet.timesheet"], this.terms["billing.project.timesheet.savebeforeedittimerow"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["billing.project.timesheet.asksaveorder"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val)
                    this.messagingService.publish(Constants.EVENT_SAVE_ORDER, { guid: this.parentGuid, open: true, delete: false, timeRow: row });
                return;
            });
        }
        else {
            this.executing = true;
            this.startProgress();
            this.populateProjectAndInvoice([]).then(() => {
                this.executing = false;
                this.stopProgress();
                this.showEditDialog(row);
            })
        }
    }

    public showEditDialog(row: ProjectTimeBlockDTO) {

        // Get all rows for current employee and date
        //var rows: ProjectTimeBlockDTO[] = row ? _.filter(this.rows, r => r.employeeId === row.employeeId && r.date.isSameDayAs(row.date)) : [];
        
        // Get all rows for current employee and date and set previous times
        let rows: ProjectTimeBlockDTO[] = [];
        if (row) {
            rows = this.rows.filter(r => r.employeeId === row.employeeId && r.date.isSameDayAs(row.date));
        }

        // Show edit time dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/TimeProjectReport/Views/editTimeGrid.html"),
            controller: EditTimeGridController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: this.isTimeSheet || this.isProjectCentral || this.useExtendedTimeRegistration ? 'xl' : 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                rows: () => { return rows },
                employee: () => { return this.employee },
                employees: () => { return this.employees },
                timeCodes: () => { return this.timeCodesDict },
                defaultTimeCodeId: () => { return this.defaultTimeCodeId },
                invoiceTimePermission: () => { return this.invoiceTimePermission },
                workTimePermission: () => { return this.workTimePermission },
                modifyOtherEmployeesPermission: () => { return this.modifyOtherEmployeesPermission },
                registrationType: () => { return (this.isTimeSheet || this.isProjectCentral ? ProjectTimeRegistrationType.TimeSheet : ProjectTimeRegistrationType.Order) },
                useExtendedTimeRegistration: () => { return this.useExtendedTimeRegistration }, 
                createTransactionsBasedOnTimeRules: () => { return this.createTransactionsBasedOnTimeRules },
                projectInvoices: () => { return this.projectInvoices },
                employeeDaysWithSchedule: () => { return this.employeeDaysWithSchedule },
                readOnly: () => { return this.readOnly },
                enableAddNew: () => { return true },
                showAdditionalTime: () => { return this.showAdditionalTime },
                invoiceTimeAsWorkTime: () => { return this.invoiceTimeAsWorkTime },
                getEmployeeChildsCallback: () => { return (employeeId: number) => this.getEmployeeChilds(employeeId) },
                populateProjectAndInvoiceCallback: () => { return (employeeIds: number[] = []) => this.populateProjectAndInvoice(employeeIds) },
                getTimeDeviationCausesCallback: () => { return (employeeGroupId: number) => this.loadTimeDeviationCausesForEmployee(employeeGroupId) }
                //() => { this.save(false); }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.rows && result.rows.length > 0) {
                // Get modified rows back from dialog
                const rowsToSave: ProjectTimeBlockDTO[] = result.rows;
                // Add invoice id to new rows
                _.forEach(_.filter(rowsToSave, r => !r.customerInvoiceId), row => {
                    row.customerInvoiceId = this.invoiceId;
                });

                this.saveRows(result.rows);
            }
        }, (result:any) => {
            //Cancel
                if (result && result.isModified) {
                    this.loadData(true, this.searchIncPlannedAbsence);
                }
        });
    }

    private createSaveDTOFromProjectTimeBlock(row: ProjectTimeBlockDTO): ProjectTimeBlockSaveDTO {
        const dto = new ProjectTimeBlockSaveDTO();
        dto.projectTimeBlockId = row.projectTimeBlockId;
        dto.actorCompanyId = CoreUtility.actorCompanyId;
        dto.customerInvoiceId = row.customerInvoiceId;
        dto.date = row.date;
        dto.employeeId = row.employeeId;
        dto.from = row.startTime ? row.startTime : Constants.DATETIME_DEFAULT.beginningOfDay();
        dto.to = row.stopTime ? row.stopTime : Constants.DATETIME_DEFAULT.beginningOfDay();
        dto.timeDeviationCauseId = row.timeDeviationCauseId;
        dto.externalNote = row.externalNote;
        dto.internalNote = row.internalNote;
        dto.invoiceQuantity = row.invoiceQuantity;
        dto.isFromTimeSheet = this.isTimeSheet || this.isProjectCentral;
        dto.projectId = this.isTimeSheet ? row.projectId : this.projectId;
        dto.projectInvoiceDayId = 0;
        dto.projectInvoiceWeekId = row.projectInvoiceWeekId;
        dto.state = row.isDeleted ? SoeEntityState.Deleted : SoeEntityState.Active;
        dto.timeBlockDateId = row.timeBlockDateId;
        dto.timeCodeId = row.timeCodeId;
        dto.timePayrollQuantity = row.timePayrollQuantity;
        dto.timeSheetWeekId = row.timeSheetWeekId;
        dto.autoGenTimeAndBreakForProject = row.autoGenTimeAndBreakForProject;
        dto.employeeChildId = row.employeeChildId;
        return dto;
    }

    private saveRows(rows: ProjectTimeBlockDTO[]) {
        this.startSave();
        
        this.messagingService.publish(Constants.EVENT_SAVE_ORDER, { guid: this.parentGuid });

        const dtos: ProjectTimeBlockSaveDTO[] = [];
        rows.forEach( (row: ProjectTimeBlockDTO) => {
            const dto = this.createSaveDTOFromProjectTimeBlock(row);
            dtos.push(dto);
        });

        this.$timeout(() => {
            console.time("SaveTimeRows");
            this.projectService.saveProjectTimeBlocks(dtos).then(saveResult => {
                if (saveResult.success) {
                    this.saveComplete();
                    this.completedSave(null, true);
                } else {
                    return this.translationService.translate("error.unabletosave_title").then((term) => {
                        this.notificationService.showErrorDialog(term, saveResult.errorMessage, "");
                        this.failedSave("");
                    });
                }
                console.timeEnd("SaveTimeRows");
            });
        });
    }

    private saveComplete() {
        this.messagingService.publish(Constants.EVENT_RELOAD_INVOICE, { guid: this.parentGuid });
        this.reloadGrid();
    }

    public showComment(row: ProjectTimeBlockDTO) {
        let message: string = "";
        if (row.externalNote) {
            message += "<b>" + this.terms["billing.project.timesheet.note.external"] + "</b><br/>" + row.externalNote;
        }
        if (row.internalNote) {
            if (row.externalNote)
                message += "<br/>";
            message += "<b>" + this.terms["billing.project.timesheet.note.internal"] + "</b><br/>" + row.internalNote;
        }
        const modal = this.notificationService.showDialog(this.terms["billing.project.timesheet.note"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        modal.result.then(val => {
            if (val) {
                this.messagingService.publish(Constants.EVENT_REGENERATE_ACCOUNTING_ROWS, null);
            }
        });
    }

    public showNote(row: ProjectTimeBlockDTO) {
        // Show edit note dialog
        
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/TimeProjectReport/Views/editNote.html"),
            controller: EditNoteController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: this.isTimeSheet || this.isProjectCentral || this.useExtendedTimeRegistration ? 'xl' : 'lg',
            windowClass: '',
            resolve: {
                rows: () => { return this.rows },
                row: () => { return row },
                isReadonly: () => { return this.readOnly },
                workTimePermission: () => { return this.workTimePermission },
                invoiceTimePermission: () => { return this.invoiceTimePermission },
                saveDirect: () => { return true }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.rowsAreModified) {
                this.loadData();
            }
        });
    }

    private addRow() {
        // Create a new row and set some properties
        const row = new TimeSheetRowDTO();
        this.calculateRow(row);

        // Add the row to the collection
        this.soeGridOptions.addRow(row, true, this.getInvoiceColumnIndex());

        return row;
    }

    private deleteRows() {

        if (!this.isProjectParticipant)
            return;

        if (this.parentIsDirty && this.invoiceId > 0) {
            const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["billing.project.timesheet.asksaveorder"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val)
                    this.messagingService.publish(Constants.EVENT_SAVE_ORDER, { guid: this.parentGuid, open: true, delete: false, timeRow: null });
                return;
            });
        }

        const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["core.deleterowwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val) {
                // Set row as deleted
                const selectedRows = this.soeGridOptions.getSelectedRows();
                if (selectedRows && selectedRows.length > 0) {
                    this.startDelete();
                    const dtos: ProjectTimeBlockSaveDTO[] = [];
                    _.forEach(selectedRows, (row: ProjectTimeBlockDTO) => {
                        if (!row.isPayrollEditable || !row.isEditable) {
                            this.translationService.translate("billing.project.timesheet.isattestedorinvoiced").then((term) => { 
                                this.showErrorDialog(term);
                            })

                            return false;
                        }
                        row.isDeleted = true;
                        dtos.push( this.createSaveDTOFromProjectTimeBlock(row) );
                    });

                    if (dtos.length > 0) {
                        this.projectService.saveProjectTimeBlocks(dtos).then((result: IActionResult) => {
                            if (result.success) {
                                this.saveComplete();
                                this.completedDelete(null);
                            } else {
                                this.failedDelete(result.errorMessage);
                                this.showErrorDialog(result.errorMessage);
                            }
                        });
                    }
                };
            }
        });
    }

    private okToModifyRows(changeDate = false, move = false): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();
        if (this.parentIsDirty && this.invoiceId > 0) {
            const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["billing.project.timesheet.asksaveorder"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val)
                    this.messagingService.publish(Constants.EVENT_SAVE_ORDER, { guid: this.parentGuid, open: true, delete: false, timeRow: null, changeDate: changeDate, move: move });

                deferral.resolve(false);
            });
        }
        else {
            deferral.resolve(true);
        }
        return deferral.promise;
    }

    private changeDate() {
        const selectedRows: ProjectTimeBlockDTO[] = this.soeGridOptions.getSelectedRows();
        if ( selectedRows.length < 1 ) {
            return;
        }

        this.okToModifyRows(true, false).then((ok: boolean) => {
            if (ok) {
                this.showSelectDateDialog(selectedRows[0].date).then((selectedDate: Date) => {
                    if (selectedDate) {
                        if (selectedRows && selectedRows.length > 0) {
                            this.startSave();
                            const ids: number[] = [];
                            _.forEach(selectedRows, (row: ProjectTimeBlockDTO) => {
                                if (!row.isPayrollEditable || !row.isEditable) {
                                    this.translationService.translate("billing.project.timesheet.isattestedorinvoiced").then((term) => {
                                        this.showErrorDialog(term);
                                    })

                                    return false;
                                }
                                ids.push(row.projectTimeBlockId);
                            });
                            
                            if (ids.length > 0) {
                                this.projectService.moveTimeRowsToDate(selectedDate, ids).then((result: IActionResult) => {
                                    if (result.success) {
                                        this.saveComplete();
                                        this.completedSave(null);
                                    }
                                    else {
                                        this.failedSave(result.errorMessage);
                                        this.showErrorDialog(result.errorMessage);
                                    }
                                })
                            }
                        }
                    }
                });
            }
        });
    }

    private moveRows() {
        this.okToModifyRows(false, true).then((ok: boolean) => {
            if (ok) {
                this.showOrderDialog().then((customerInvoiceId: number) => {
                    if (customerInvoiceId) {
                        const selectedRows = this.soeGridOptions.getSelectedRows();
                        if (selectedRows && selectedRows.length > 0) {
                            this.startSave();
                            const ids: number[] = [];
                            _.forEach(selectedRows, (row: ProjectTimeBlockDTO) => {
                                if (!row.isEditable) {
                                    this.translationService.translate("billing.project.timesheet.wrongtimerowstatus").then((term) => {
                                        this.showErrorDialog(term);
                                    })
                                    return false;
                                };
                                ids.push(row.projectTimeBlockId);
                            });

                            if (ids.length > 0) {
                                this.projectService.moveTimeRowsToOrder(customerInvoiceId, ids).then((result: IActionResult) => {
                                    if (result.success) {
                                        this.saveComplete();
                                        this.completedSave(null);
                                    }
                                    else {
                                        this.failedSave(result.errorMessage);
                                        this.showErrorDialog(result.errorMessage);
                                    }
                                })
                            }
                        }
                    }
                });
            }
        });
    }

    private moveRowsToExistingInvoiceRow() {
        this.okToModifyRows().then((ok: boolean) => {
            if (ok) {
                this.selectProductRowCallback().then((customerInvoiceRowId: number) => {
                    if (customerInvoiceRowId) {
                        this.moveRowsToInvoiceRow(customerInvoiceRowId);
                    }
                });
            }
        });
    }

    private moveRowsToInvoiceRow(customerInvoiceRowId = 0) {
        this.okToModifyRows().then((ok: boolean) => {
            if (ok) {
                const selectedRows = this.soeGridOptions.getSelectedRows();
                if (selectedRows && selectedRows.length > 0) {
                    this.startSave();
                    const ids: number[] = [];
                    _.forEach(selectedRows, (row: ProjectTimeBlockDTO) => {
                        if (!row.isEditable) {
                            this.translationService.translate("billing.project.timesheet.wrongtimerowstatus").then((term) => {
                                this.showErrorDialog(term);
                            })
                            return false;
                        };
                        ids.push(row.projectTimeBlockId);
                    });

                    if (ids.length > 0) {
                        this.projectService.moveTimeRowsToOrderRow(this.invoiceId, customerInvoiceRowId,ids).then((result: IActionResult) => {
                            if (result.success) {
                                this.saveComplete();
                                this.completedSave(null);
                            }
                            else {
                                this.failedSave(result.errorMessage);
                                this.showErrorDialog(result.errorMessage);
                            }
                        })
                    }
                }
            }
        });
    }

    private showOrderDialog(): ng.IPromise<number> {
        const deferral = this.$q.defer<number>();

        this.translationService.translate("common.customer.invoices.selectorder").then((term) => {
            //var invoice = this.invoice.orderNr ? _.find(this.customerInvoices, { 'invoiceNr': this.invoice.orderNr }) : undefined;
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectCustomerInvoice", "selectcustomerinvoice.html"),
                controller: SelectCustomerInvoiceController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    title: () => { return term },
                    isNew: () => { return false },
                    ignoreChildren: () => { return false },
                    originType: () => { return SoeOriginType.Order },
                    customerId: () => { return null },
                    projectId: () => { return null },
                    invoiceId: () => { return null },
                    currentMainInvoiceId: () => { return null },
                    selectedProjectName: () => { return null },
                    userId: () => { return this.onlyMyOrders ? soeConfig.userId : null },
                    includePreliminary: () => { return null },
                    includeVoucher: () => { return null },
                    fullyPaid: () => { return null },
                    useExternalInvoiceNr: () => { return null },
                    importRow: () => { return null },
                }
            });

            modal.result.then(result => {
                if (result && result.invoice) {
                    deferral.resolve(result.invoice.customerInvoiceId);
                }
                else {
                    deferral.resolve(0);
                }
            });
        });

        return deferral.promise;
    }

    private showSelectDateDialog(defaultDate:Date): ng.IPromise<Date> {
        const deferral = this.$q.defer<Date>();

        this.translationService.translate("common.customer.invoices.selectorder").then((term) => {
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectDate", "SelectDate.html"),
                controller: SelectDateController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    title: () => { return term },
                    defaultDate: () => { return defaultDate}
                }
            });

            modal.result.then(result => {
                if (result && result.selectedDate) {
                    deferral.resolve(result.selectedDate);
                }
                else {
                    deferral.resolve(undefined);
                }
            });
        });

        return deferral.promise;
    }

    private clearGridStates() {
        this.hasSelectedRows = false;
        this.hasSelectedMyOwnRows = false;
    }

    private reloadGrid() {
        this.clearGridStates();
        
        if (this.isOrder)
            this.loadProjectTotals();
        else
            this.loadData(true,this.searchIncPlannedAbsence);
    }

    private openOrder(row: any) {
        if (this.isProjectCentral) {
            this.messagingService.publish(Constants.EVENT_OPEN_ORDER, {
                row: row,
                name: this.terms["common.order"] + " " + row.invoiceNr
            });
        }
        else {
            this.messagingService.publish(Constants.EVENT_OPEN_ORDER, {
                id: row.customerInvoiceId,
                name: this.terms["common.order"] + " " + row.invoiceNr
            });
        }
    }

    private openCustomer(row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_EDITCUSTOMER, {
            id: row.customerId,
            name: this.terms["common.customer.customer.customer"] + " " + row.customerName
        });
    }

    private openProject(row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_EDITPROJECT, {
            id: row.projectId,
            name: this.terms["billing.project.project"] + " " + row.projectNr
        });
    }

    protected onDeleteEvent(row: any) {
        // TODO: Publish event to container
        //if (row.isTimeProjectRow)
        //    invoice.hasManuallyDeletedTimeProjectRows = true;

        for (var i = 0; i < this.rows.length; i++) {
            if (this.rows[i] === row) {
                this.rows.splice(i, 1);
                break;
            }
        }
    }

    protected editNote(data) {
        const row: TimeSheetRowDTO = data.row;
        const field: string = data.field;
        let dayNr: number = 0;
        if (field === 'monday')
            dayNr = 1;
        else if (field === 'tuesday')
            dayNr = 2;
        else if (field === 'wednesday')
            dayNr = 3;
        else if (field === 'thursday')
            dayNr = 4;
        else if (field === 'friday')
            dayNr = 5;
        else if (field === 'saturday')
            dayNr = 6;
        else if (field === 'sunday')
            dayNr = 7;

        const title: string = this.terms["billing.project.timesheet.note.editfor"].format(dayNr !== 0 ? CalendarUtility.getDayName(dayNr) : this.terms["billing.project.timesheet.wholeweek"]);

        // Show edit note dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getViewUrl("editNoteDialog.html"),
            controller: EditNoteDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                title: () => { return title },
                noteInternal: () => { return field === "week" ? row.weekNoteInternal : row[field + 'NoteInternal'] },
                noteExternal: () => { return field === "week" ? row.weekNoteExternal : row[field + 'NoteExternal'] }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                field === "week" ? row.weekNoteInternal = result.noteInternal : row[field + 'NoteInternal'] = result.noteInternal;
                field === "week" ? row.weekNoteExternal = result.noteExternal : row[field + 'NoteExternal'] = result.noteExternal;
            }
        });
    }

    private print() {
        const employeeIds: any[] = [];

        _.forEach(this.employees, (row: IEmployeeTimeCodeDTO) => {
            employeeIds.push(row.employeeId);
        });

        const reportItem = {
            ids: employeeIds,
            invoiceId: this.invoiceId,
            projectId: this.projectId,
            dateFrom: this.fromDate,
            dateTo: this.toDate,
        } as IProjectTimeBookPrintDTO;

        this.requestReportService.printProjectTimebookReport(reportItem);
    }

    // Help-methods
    private calculateRow(row: TimeSheetRowDTO) {
        row.weekSumQuantity = (row.mondayQuantity || 0) + (row.tuesdayQuantity || 0) + (row.wednesdayQuantity || 0) + (row.thursdayQuantity || 0) + (row.fridayQuantity || 0) + (row.saturdayQuantity || 0) + (row.sundayQuantity || 0);
        row.weekSumQuantityFormatted = CalendarUtility.minutesToTimeSpan(row.weekSumQuantity);
        row.weekSumInvoiceQuantity = (row.mondayInvoiceQuantity || 0) + (row.tuesdayInvoiceQuantity || 0) + (row.wednesdayInvoiceQuantity || 0) + (row.thursdayInvoiceQuantity || 0) + (row.fridayInvoiceQuantity || 0) + (row.saturdayInvoiceQuantity || 0) + (row.sundayInvoiceQuantity || 0);
        row.weekSumInvoiceQuantityFormatted = CalendarUtility.minutesToTimeSpan(row.weekSumInvoiceQuantity);
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
        if (this.groupByDate !== undefined) {
            this.groupByDate = false;
        }
        this.searchIncPlannedAbsence = false;

        switch (option.id) {
            case TimeProjectSearchFunctions.SearchIntervall:
                this.loadData();
                break;
            case TimeProjectSearchFunctions.GetAll:
                this.loadData(false);
                break;
            case TimeProjectSearchFunctions.SearchWithGroupOnDate:
                this.groupByDate = true;
                this.searchIncPlannedAbsence = true;
                this.loadData(true, this.searchIncPlannedAbsence);
                break;
            case TimeProjectSearchFunctions.SearchIncPlannedAbsence:
                this.searchIncPlannedAbsence = true;
                this.loadData(true, this.searchIncPlannedAbsence);
                break;
        }
    }

    private executeButtonFunction(option) {
        switch (option.id) {
            case TimeProjectButtonFunctions.AddRow:
                this.edit(null);
                break;
            case TimeProjectButtonFunctions.DeleteRow:
                this.deleteRows();
                break;
            case TimeProjectButtonFunctions.MoveRow:
                this.moveRows();
                break;
            case TimeProjectButtonFunctions.MoveRowToNewInvoiceRow:
                this.moveRowsToInvoiceRow();
                break;
            case TimeProjectButtonFunctions.MoveRowToExistingInvoiceRow:
                this.moveRowsToExistingInvoiceRow();
                break;
            case TimeProjectButtonFunctions.ChangeDate:
                this.changeDate();
                break;
        }
    }
}

export class EditNoteDialogController {

    public result: any = {};
    private title: string;
    private noteInternal: string;
    private noteExternal: string;

    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, title: string, noteInternal: string, noteExternal: string) {
        this.title = title;
        this.noteInternal = noteInternal;
        this.noteExternal = noteExternal;
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.result.noteInternal = this.noteInternal;
        this.result.noteExternal = this.noteExternal;
        this.$uibModalInstance.close(this.result);
    }
}