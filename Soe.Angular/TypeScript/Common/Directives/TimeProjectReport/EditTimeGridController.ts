import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { EmployeeInfoController } from "./EmployeeInfoController";
import { ProjectInvoiceSmallDTO, ProjectTimeBlockDTO, ProjectSmallDTO } from "../../Models/ProjectDTO";
import { IEmployeeTimeCodeDTO, ITimeDeviationCauseDTO, IEmployeeProjectInvoiceDTO, IEmployeeScheduleTransactionInfoDTO } from "../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ProjectTimeRegistrationType, SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { Constants } from "../../../Util/Constants";
import { SmallGenericType } from "../../Models/SmallGenericType";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { TimeColumnOptions } from "../../../Util/SoeGridOptionsAg";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { EditTimeHelper } from "./EditTimeHelper";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { IShortCutService } from "../../../Core/Services/ShortCutService";
import { EditNoteController } from "./EditNoteController";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { CompanySettingType, SettingMainType } from "../../../Util/CommonEnumerations";

export class EditTimeGridController {

    // Data
    private deletedRows: ProjectTimeBlockDTO[] = [];
    private editTimeHelper: EditTimeHelper;

    // Terms
    private terms: any = [];
    private title: string;

    private lastEmployeeId = 0; 
    
    //Collections
    private startTimes: any[] = [];
    private filteredTimeDeviationCauses: ITimeDeviationCauseDTO[];

    // GUI
    private gridHandler: EmbeddedGridController;
    private internalNoteExpanded = false;
    private isDirty = false;

    private showEmployee: boolean;
    private timestamp: any;

    private progress: IProgressHandler;

    private limitToProjectUser: boolean = true;
    
    //@ngInject
    constructor(
        shortCutService: IShortCutService,
        gridHandlerFactory: IGridHandlerFactory,
        private $scope: ng.IScope,
        private $uibModalInstance,
        private $uibModal,
        protected urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        private coreService: ICoreService,
        private rows: ProjectTimeBlockDTO[],
        private employee: IEmployeeTimeCodeDTO,
        private employees: any[],
        private timeCodes: any[],
        private defaultTimeCodeId: number,
        private invoiceTimePermission,
        private workTimePermission,
        private modifyOtherEmployeesPermission,
        private registrationType: ProjectTimeRegistrationType,
        private useExtendedTimeRegistration: boolean,
        private createTransactionsBasedOnTimeRules: boolean,
        private projectInvoices: IEmployeeProjectInvoiceDTO[],
        private employeeDaysWithSchedule: any[],
        private readOnly: boolean,
        private enableAddNew: boolean,
        private showAdditionalTime: boolean,
        private invoiceTimeAsWorkTime: boolean,
        private populateProjectAndInvoiceCallback: ((employeeId: number[]) => ng.IPromise<any>),
        private getEmployeeChildsCallback: ((employeeId: number) => ng.IPromise<any>),
        private getTimeDeviationCausesCallback: ((employeeGroupId: number) => ng.IPromise<ITimeDeviationCauseDTO[]>)) {

        //this.progressHandler = new ProgressHandler(this.$uibModal, this.translationService, this.$q, messagingService, this.urlHelperService, this.notificationService);
        this.progress = progressHandlerFactory.create()
        this.showEmployee = this.registrationType !== ProjectTimeRegistrationType.Attest;

        this.editTimeHelper = new EditTimeHelper(coreService, this.$q, (employeeId: number) => { return this.getEmployee(employeeId) });

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "timeRowGrid");
        this.gridHandler.gridAg.options.setMinRowsToShow(5);
        this.gridHandler.gridAg.options.enableRowSelection = false;
        this.gridHandler.gridAg.options.enableFiltering = false;
        this.gridHandler.gridAg.options.enableGridMenu = false;

        shortCutService.bindNew($scope, () => { this.addRow(); });
        shortCutService.bindSave($scope, () => { this.save(); });

        this.init();
    }

    // SETUP
    private init() {

        this.loadLookups();
        let tempRowCounter = 1;

        this.employees.forEach( emp => {
            emp["nameNumber"] = emp.name + " (" + emp.employeeNr + ")";
        });

        this.rows.forEach(row => {
            if (!this.timestamp) {
                this.timestamp = { created: row.created, createdBy: row.createdBy, modified: row.modified, modifiedBy: row.modifiedBy };
            }
            else {
                if ((row.created && !this.timestamp.created) || row.created < this.timestamp.created) {
                    this.timestamp.created = row.created;
                    this.timestamp.createdBy = row.createdBy;
                }
                if ((row.modified && !this.timestamp.modified) || row.modified > this.timestamp.modified) {
                    this.timestamp.modified = row.modified;
                    this.timestamp.modifiedBy = row.modifiedBy;
                }
            }
            row.tempRowId = tempRowCounter;
            row.selectedEmployee = _.find(this.employees, e => e.employeeId === row.employeeId);
            if (this.registrationType === ProjectTimeRegistrationType.TimeSheet || this.registrationType === ProjectTimeRegistrationType.Attest) {

                this.setProjectsAndInvoices(row);

                if (row.customerInvoiceId && row.filteredInvoices) {
                    let invoice = _.find(row.filteredInvoices, e => e.invoiceId === row.customerInvoiceId);
                    if (!invoice) {
                        invoice = this.createProjectInvoiceSmallDTO(row);
                        row.filteredInvoices.push(invoice);
                    }
                    
                    row.selectedCustomerInvoice = invoice;
                }
                if (row.projectId && row.filteredProjects) {
                    let findProject = _.find(row.filteredProjects, e => e.projectId === row.projectId);
                    if (!findProject) {
                        findProject = this.createProjectSmallDTO(row);
                        row.filteredProjects.push(findProject);
                    }
                    row.selectedProject = findProject;
                }
            }

            tempRowCounter = tempRowCounter + 1;

            if ((row.timeCodeId) && (!this.timeCodeIdExists(row.timeCodeId))) {
                row.filteredTimeCodes = this.timeCodes.slice(1, this.timeCodes.length);
                row.filteredTimeCodes.push({ value: row.timeCodeId, label: row.timeCodeName })
            }
            else {
                row.filteredTimeCodes = this.timeCodes;
            }

            if (row.employeeChildId) {
                this.loadEmployeeChilds(row, false);
            }

            this.setTimeCodeReadOnly(row);

            if (!row['originalStartTime']) {
                row['originalStartTime'] = row.startTime;
                row['originalStopTime'] = row.stopTime;
                row['originalTimeDeviationCauseId'] = row.timeDeviationCauseId;
            }
            row.isModified = false;
            this.lastEmployeeId = row.employeeId;
        });


        if (this.rows && this.rows.length > 0) {
            const firstRow = this.rows[0];
            if (this.useExtendedTimeRegistration) {
                this.loadEmployeeTimesAndSchedule(firstRow);
            }
        }
        else {
            this.addRow();
        }

        this.$timeout(() => {
            this.setupGridColumns();
        });
    }

    private loadLookups() {
        this.$q.all([
            this.loadTerms(),
            this.loadSettings(),
        ]).then(() => {
            });
    }

    // LOOKUPS
    private loadSettings(): ng.IPromise<any> {
        return this.coreService.getBoolSetting(SettingMainType.Company, CompanySettingType.ProjectLimitOrderToProjectUsers).then(data => {
            this.limitToProjectUser = data;
        })
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "error.default_error",
            "core.continue",
            "core.error",
            "core.warning",
            "billing.project.timesheet.edittime.title",
            "billing.project.timesheet.edittime.noproject",
            "billing.project.timesheet.edittime.noorder",
            "billing.project.timesheet.edittime.noemployee",
            "billing.project.timesheet.edittime.dialogschema",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.title = terms["billing.project.timesheet.edittime.title"];
            if (!this.modifyOtherEmployeesPermission && this.rows[0] && this.rows[0].selectedEmployee) {
                this.title += " {0}".format(this.rows[0].selectedEmployee.name);
            }
        });
    }

    private isTimeSheetRegistration(): boolean {
        return (this.registrationType === ProjectTimeRegistrationType.TimeSheet || this.registrationType === ProjectTimeRegistrationType.Attest);
    }

    private setupGridColumns() {

        const keys: string[] = [
            "common.employee",
            "billing.project.timesheet.invoice",
            "billing.project.project",
            "common.date",
            "common.week",
            "common.time.timedeviationcause",
            "billing.project.timesheet.chargingtype",
            "billing.project.timesheet.edittime.workedtimefromto",
            "billing.project.timesheet.edittime.workedtime",
            "billing.project.timesheet.edittime.invoicedtime",
            "billing.project.timesheet.child",
            "billing.project.timesheet.edittime.externalnote",
            "billing.project.timesheet.edittime.internalnote",
            "common.from",
            "common.to",
            "billing.project.timesheet.child",
            "billing.project.timesheet.othertime"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            const employeeEditableFunc = (data: ProjectTimeBlockDTO) => data.isEditable && data.isPayrollEditable && data["isNew"] && this.modifyOtherEmployeesPermission;
            const newAndEditableFunc = (data: ProjectTimeBlockDTO) => data.isEditable && data["isNew"];
            const invoiceEditableFunc = (data: ProjectTimeBlockDTO) => data.isEditable;
            const invoiceTimeEditableFunc = (data: ProjectTimeBlockDTO) => data.isEditable && data.timeCodeId && !data["hasError"];
            const payRollTimeEditableFunc = (data: ProjectTimeBlockDTO) => data.isPayrollEditable && !data["hasError"] && !data.additionalTime;
            const payRollEditableFunc = (data: ProjectTimeBlockDTO) => data.isPayrollEditable && !data["hasError"];
            const addtionalTimeEditableFunc = (data: ProjectTimeBlockDTO) => data.isPayrollEditable && !data["hasError"] && data.additionalTime;
            const timeCodeEditableFunc = (data: ProjectTimeBlockDTO) => data.isEditable && !data.timeCodeReadOnly;
            const startStopEditableFunc = (data: ProjectTimeBlockDTO) => data.isPayrollEditable && (!data.autoGenTimeAndBreakForProject || data.mandatoryTime);

            const invoiceTimeColumnOptions: TimeColumnOptions = {
                editable: invoiceTimeEditableFunc,
                maxWidth: 100,
                clearZero: false, alignLeft: false, minDigits: 5,
                addNotEditableCSS: true,
                cellClassRules: {
                    "excelTime": () => true,
                }
            };

            const payrollTimeColumnOptions: TimeColumnOptions = {
                editable: payRollTimeEditableFunc,
                maxWidth: 100,
                clearZero: false, alignLeft: false, minDigits: 5,
                addNotEditableCSS: true,
                cellClassRules: {
                    "excelTime": () => true,
                }
            };

            const additionalTimeColumnOptions: TimeColumnOptions = {
                editable: addtionalTimeEditableFunc,
                maxWidth: 100,
                clearZero: false, alignLeft: false, minDigits: 5,
                addNotEditableCSS: true,
                cellClassRules: {
                    "excelTime": () => true,
                }
            };

            const startStopColumnOptions: TimeColumnOptions = {
                editable: startStopEditableFunc,
                hide: true,
                maxWidth: 60,
                minWidth: 60,
                handleAsTimeSpan: true,
                addNotEditableCSS: true,
            };

            this.gridHandler.gridAg.addColumnIsModified("isModified", "",25);

            const employeeOptions = this.editTimeHelper.createTypeAheadOptions("nameNumber");
            employeeOptions.source = (filter) => this.typeAheadFilterEmployees(filter);
            this.gridHandler.gridAg.addColumnTypeAhead("employeeName", terms["common.employee"], null, { typeAheadOptions: employeeOptions, editable: employeeEditableFunc,addNotEditableCSS:true, suppressSorting: false, suppressMovable: true }, null);

            if (this.isTimeSheetRegistration()) {
                const invoiceOptions = this.editTimeHelper.createTypeAheadOptions("numberName");
                invoiceOptions.source = (filter) => this.typeAheadFilterOrders(filter);
                this.gridHandler.gridAg.addColumnTypeAhead("invoiceNr", terms["billing.project.timesheet.invoice"], null, { typeAheadOptions: invoiceOptions, editable: newAndEditableFunc, addNotEditableCSS: true, suppressSorting: false, suppressMovable: true }, null);

                const projectOptions = this.editTimeHelper.createTypeAheadOptions("numberName");
                projectOptions.source = (filter) => this.typeAheadFilterProjects(filter);
                this.gridHandler.gridAg.addColumnTypeAhead("projectNr", terms["billing.project.project"], null, { typeAheadOptions: projectOptions, editable: newAndEditableFunc, addNotEditableCSS: true, suppressSorting: false, suppressMovable: true }, null);
            }

            this.gridHandler.gridAg.addColumnDate("date", terms["common.date"], 80, false, null, { maxWidth: 80, minWidth: 80, addNotEditableCSS: true, editable: newAndEditableFunc });
            this.gridHandler.gridAg.addColumnText("weekNo", terms["common.week"], 38, false, { maxWidth: 38, minWidth: 38, addNotEditableCSS: true, editable:false });

            if (this.useExtendedTimeRegistration) {
                const timeDevationCauseOptions = this.editTimeHelper.createTypeAheadOptions("name");
                timeDevationCauseOptions.source = (filter) => this.typeAheadFilterTimeDeviationCauses(filter);
                this.gridHandler.gridAg.addColumnTypeAhead("timeDeviationCauseName", terms["common.time.timedeviationcause"], null, { maxWidth: 140, typeAheadOptions: timeDevationCauseOptions, addNotEditableCSS: true, editable: payRollEditableFunc, suppressSorting: false, suppressMovable: true }, null);
            }

            const timeCodeOptions = this.editTimeHelper.createTypeAheadOptions("label");
            timeCodeOptions.source = (filter) => this.typeAheadFilterTimeCodes(filter);
            this.gridHandler.gridAg.addColumnTypeAhead("timeCodeName", terms["billing.project.timesheet.chargingtype"], null, { maxWidth: 140, typeAheadOptions: timeCodeOptions, addNotEditableCSS: true, editable: timeCodeEditableFunc, suppressSorting: false, suppressMovable: true }, null);

            this.gridHandler.gridAg.addColumnTime("startTime", terms["common.from"], 60, startStopColumnOptions);
            this.gridHandler.gridAg.addColumnTime("stopTime", terms["common.to"], 60, startStopColumnOptions);

            if (this.workTimePermission) {
                this.gridHandler.gridAg.addColumnTimeSpan("timePayrollQuantityFormattedEdit", terms["billing.project.timesheet.edittime.workedtime"], null, payrollTimeColumnOptions);
                if (this.showAdditionalTime) {
                    this.gridHandler.gridAg.addColumnTimeSpan("timeAdditionalQuantityFormatted", terms["billing.project.timesheet.othertime"], null, additionalTimeColumnOptions);
                }
            }
            
            if (this.invoiceTimePermission) {
                this.gridHandler.gridAg.addColumnTimeSpan("invoiceQuantityFormatted", terms["billing.project.timesheet.edittime.invoicedtime"], null, invoiceTimeColumnOptions);
            }

            const childsOptions = this.editTimeHelper.createTypeAheadOptions("name");
            childsOptions.source = (filter) => this.typeAheadFilterChilds(filter);
            this.gridHandler.gridAg.addColumnTypeAhead("employeeChildName", terms["billing.project.timesheet.child"], null, { typeAheadOptions: childsOptions, editable: payRollEditableFunc, suppressSorting: false, suppressMovable: true, hide:true }, null);

            this.gridHandler.gridAg.addColumnText("externalNote", terms["billing.project.timesheet.edittime.externalnote"], null, false, { editable: invoiceEditableFunc, addNotEditableCSS: true });
            this.gridHandler.gridAg.addColumnText("internalNote", terms["billing.project.timesheet.edittime.internalnote"], null, false, { editable: invoiceEditableFunc, addNotEditableCSS: true });

            this.gridHandler.gridAg.addColumnIcon("noteIcon", "", null, { onClick: this.showNote.bind(this), suppressExport: true });

            if (this.useExtendedTimeRegistration) {
                this.gridHandler.gridAg.addColumnIcon(null, "", 20, { maxWidth: 20, showIcon: (row) => row.hasError, icon: "fal fa-exclamation-triangle warningColor", toolTipField: "errorText" });
                this.gridHandler.gridAg.addColumnIcon(null, "", 20, { maxWidth: 20, icon: "fal fa-info-circle", onClick:this.showDayInfo.bind(this) } );
            }

            this.gridHandler.gridAg.addColumnDelete(null, this.deleteRow.bind(this), false, (row: ProjectTimeBlockDTO) => row.isEditable && row.isPayrollEditable && !row["hasError"]);

            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));

            this.gridHandler.gridAg.options.subscribe(events);
            this.gridHandler.gridAg.options.customTabToCellHandler = (params) => this.handleNavigateToNextCell(params);

            this.gridHandler.gridAg.finalizeInitGrid("", false);
            
            this.loadGridData();
        });
    }

    private typeAheadFilterEmployees(filter) {
        return _.orderBy(this.employees.filter(p => {
            return p.nameNumber.contains(filter);
        }), 'nameNumber');
    }

    private typeAheadFilterOrders(filter) {
        const currentRow: ProjectTimeBlockDTO = this.gridHandler.gridAg.options.getCurrentRow();
        return _.orderBy(currentRow.filteredInvoices.filter(p => {
            return p.numberName.contains(filter) || p.invoiceId === 0;
        }), 'numberName');
    }

    private typeAheadFilterProjects(filter) {
        const currentRow: ProjectTimeBlockDTO = this.gridHandler.gridAg.options.getCurrentRow();
        return _.orderBy(currentRow.filteredProjects.filter(p => {
            return p.numberName.contains(filter) || p.projectId === 0;
        }), 'numberName');
    }

    private typeAheadFilterTimeDeviationCauses(filter) {
        return _.orderBy(this.filteredTimeDeviationCauses.filter(p => {
            return p.name.contains(filter);
        }), 'name');
    }

    private typeAheadFilterTimeCodes(filter) {
        const currentRow: ProjectTimeBlockDTO = this.gridHandler.gridAg.options.getCurrentRow();

        return _.orderBy(currentRow.filteredTimeCodes.filter(p => {
            return p.label.contains(filter);
        }), 'label');
    }

    private typeAheadFilterChilds(filter) {
        const currentRow: ProjectTimeBlockDTO = this.gridHandler.gridAg.options.getCurrentRow();
        return _.orderBy(currentRow.childs.filter(p => {
            return p.name.contains(filter);
        }), 'name');
    }

    private loadGridData() {
        this.gridHandler.gridAg.setData(this.rows);
    }

    private handleNavigateToNextCell(params: any, skipChecks?: boolean): { rowIndex: number, column: any } {
        let { nextCellPosition, previousCellPosition } = params;

        if (!nextCellPosition) {
            nextCellPosition = previousCellPosition;
            this.$timeout(() => { this.addRow(); })
        }

        return { rowIndex: nextCellPosition.rowIndex, column: nextCellPosition.column };
    }

    private afterCellEdit(row: ProjectTimeBlockDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            case "employeeName":
                {
                    this.selectedEmployeeChanged(row, newValue);
                    break;
                }
            case "invoiceNr":
                {
                    const invoice = row.filteredInvoices.find(x => x.numberName === newValue);
                    this.selectedOrderInvoiceChanged(row, invoice ? invoice.invoiceId : 0 );
                    break;
                }
            case "projectNr":
                {
                    const project = row.filteredProjects.find(x => x.numberName === newValue);
                    this.selectedProjectChanged(row, project ? project.projectId : 0);
                    break;
                }
            case "timeDeviationCauseName":
                {
                    const timeDeviationCause = this.filteredTimeDeviationCauses.find(x => x.name === newValue);
                    this.timeDeviationCauseChanged(row, timeDeviationCause ? timeDeviationCause.timeDeviationCauseId : 0);
                    break;
                }
            case "timeCodeName":
                {
                    const timeCode = row.filteredTimeCodes.find(x => x.label === newValue);
                    this.timeCodeChanged(row, timeCode ? timeCode.value : 0);
                    break;
                }
            case "timePayrollQuantityFormattedEdit":
                {
                    this.payrollQuantityChanged(row);
                    break;
                }
            case "timeAdditionalQuantityFormatted":
                {
                    this.additionalQuantityChanged(row);
                    break;
                }
            case "startTime":
            case "stopTime":
                {
                    this.timeChanged(row);
                    break;
                }
            case "date":
                {
                    this.selectedDateChanged(row);
                    break;
                }
            case "employeeChildName":
                {
                    const child = row.childs.find(x => x.name === newValue);
                    row.employeeChildId = child ? child.id : 0;
                    row.employeeChildName = child ? child.name : "";
                    break;
                }
        }

        
        if (!this.isDirty) {
            this.isDirty = true;
            this.$scope.$applyAsync();
        }
    }

    // EVENTS

    private addRow() {
        const newRow = this.createNewRow();
        this.rows.push(newRow);
        this.setTimeCodeReadOnly(newRow);
        this.loadGridData();
        this.gridHandler.gridAg.options.startEditingCell(newRow, this.showEmployee ? "employeeName" : "invoiceNr");
    }

    private toggleChildColumn(show = false) {
        if (show) {
            this.gridHandler.gridAg.options.showColumn("employeeChildName");
        }
        else {
            this.gridHandler.gridAg.options.hideColumn("employeeChildName");
        }
    }
    private toggleStartStopColumns(show = false) {
        if (this.useExtendedTimeRegistration && this.workTimePermission) {
            show = this.rows.filter(x => !x.autoGenTimeAndBreakForProject || x.mandatoryTime).length > 0;
        }

        if (show) {
            this.gridHandler.gridAg.options.showColumn("startTime");
            this.gridHandler.gridAg.options.showColumn("stopTime");
        }
        else {
            this.gridHandler.gridAg.options.hideColumn("startTime");
            this.gridHandler.gridAg.options.hideColumn("stopTime");
        }
    }

    private deleteRow(row: ProjectTimeBlockDTO) {
        // If row to delete has been saved, store it in deleted collection
        // Must be passed when saving
        if (row.projectTimeBlockId || row.timeSheetWeekId) {
            row.isDeleted = true;
            this.deletedRows.push(row);
        }

        _.pull(this.rows, row);
        this.isDirty = true;
        this.loadGridData();
    }

    private showDayInfo(row: ProjectTimeBlockDTO) {
        const info = _.find(this.employeeDaysWithSchedule, e => e.employeeId === row.employeeId && e.date.toDateString() === row.date.toDateString());
        if (info) {
            this.showInfoDialog(info);
        }
        else {
            this.getEmployeeScheduleAndTransactionInfo(row, true);
        }
    }

    private cancel() {
        const modifiedRows = this.rows.filter(r => r.isModified && r.projectTimeBlockId > 0);
        this.$uibModalInstance.dismiss({ isModified: (modifiedRows && modifiedRows.length > 0) });
    }

    private save() {
        this.gridHandler.gridAg.options.stopEditing(false);
        this.$timeout(() => {
            this.save2();
        });
    }

    private save2() {
        let rowsToSave = this.rows.filter(r => r.isModified);

        //remove any new empty rows....
        rowsToSave = rowsToSave.filter(r => !( this.isNewRow(r) && !r.invoiceQuantity && !r.timePayrollQuantity && !r.externalNote && !r.internalNote));

        if (rowsToSave.filter(x => x.employeeId === 0).length > 0) {
            this.notificationService.showDialog(this.terms["core.error"], this.terms["billing.project.timesheet.edittime.noemployee"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            return;
        }

        // Add deleted rows to collection
        if (this.deletedRows.length > 0)
            rowsToSave = rowsToSave.concat(this.deletedRows);

        if (this.useExtendedTimeRegistration) {
            let counter = 1;
            const itemsToValidate = [];
            _.forEach(rowsToSave, (row) => {
                //Make sure dates are correct
                if (row.date)
                    row.date = new Date(<any>row.date);
                /*
                if (row.autoGenTimeAndBreakForProject) {
                    row.startTime = Constants.DATETIME_DEFAULT;
                    row.stopTime = (row.stopTime !== row['originalStopTime'] ) ? row.stopTime : Constants.DATETIME_DEFAULT;
                }
                else {
                    */
                if (this.isNewRow(row) && row["payrollQuantityChanged"] === false && row.autoGenTimeAndBreakForProject && !row.mandatoryTime) {
                    row.startTime = Constants.DATETIME_DEFAULT;
                    row.stopTime = Constants.DATETIME_DEFAULT;
                }
                else {
                    row.startTime = (row.startTime) ? row.startTime : Constants.DATETIME_DEFAULT;
                    row.stopTime = (row.stopTime) ? row.stopTime : Constants.DATETIME_DEFAULT;
                }

                let employeeItem = _.find(itemsToValidate, i => i.employeeId === row.employeeId);
                if (!employeeItem) {
                    employeeItem = { employeeId: row.employeeId, autoGenTimeAndBreakForProject: row.autoGenTimeAndBreakForProject, rows: [] };
                    itemsToValidate.push(employeeItem);
                }

                if (row.projectTimeBlockId && row.projectTimeBlockId > 0) {
                    employeeItem.rows.push({
                        id: row.projectTimeBlockId, workDate: row.date, startTime: row.startTime, stopTime: row.stopTime, originalStartTime: row['originalStartTime'], originalStopTime: row['originalStopTime'],
                        timeDeviationCauseId: row.timeDeviationCauseId, originalTimeDeviationCauseId: row['originalTimeDeviationCauseId'], employeeChildId: row.employeeChildId
                    });
                }
                else {
                    employeeItem.rows.push({ id: counter, workDate: row.date, startTime: row.startTime, stopTime: row.stopTime, timeDeviationCauseId: row.timeDeviationCauseId, employeeChildId: row.employeeChildId });
                    counter = counter + 1;
                }
            });

            this.coreService.validateSaveProjectTimeBlocks(itemsToValidate).then(result => {

                if (!result.success) {
                    this.notificationService.showDialog(this.terms["core.error"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                }
                else if (result.infoMessage) {
                    const modal = this.notificationService.showDialog(this.terms["core.warning"], result.infoMessage + "\n" + this.terms["core.continue"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then((val) => {
                        if (val != null && val === true) {
                            this.$uibModalInstance.close({ rows: rowsToSave });
                        }
                    });
                }
                else {
                    this.$uibModalInstance.close({ rows: rowsToSave });
                }
            });
        }
        else {
            this.$uibModalInstance.close({ rows: rowsToSave });
        }
    }

    private loadEmployeeChilds(row: ProjectTimeBlockDTO, setFirstChildAsDefault: boolean) {
        this.getEmployeeChildsCallback(row.employeeId).then((childs: SmallGenericType[]) => {
            row.childs = childs;
            this.toggleChildColumn(true);
            
            if (setFirstChildAsDefault && childs.length === 1 && !row.employeeChildId) {
                row.employeeChildId = childs[0].id;
                row.employeeChildName = childs[0].name;
            }
        })
    }

    private saveDisabled() {
        const projectNotSet = ((this.registrationType === ProjectTimeRegistrationType.TimeSheet || this.registrationType === ProjectTimeRegistrationType.Attest) ? _.filter(this.rows, r => !r.projectId || r["hasError"]).length > 0 : false);
        return !this.isDirty || this.readOnly || projectNotSet;
    }

    private timeCodeIdExists(timeCodeId: number): boolean {
        return this.timeCodes.filter(x => x.value === timeCodeId).length > 0;
    }

    private getTimeDeviationCause(timeDeviationCauseId: number): ITimeDeviationCauseDTO {
        return this.filteredTimeDeviationCauses ? this.filteredTimeDeviationCauses.find(x => x.timeDeviationCauseId === timeDeviationCauseId) : undefined;
    }

    private getProjectInvoiceRow(employeeId: number) {
        if (!this.limitToProjectUser && this.projectInvoices && this.projectInvoices.length > 0) {
            return this.projectInvoices[0];
        } else {
            return _.find(this.projectInvoices, p => p.employeeId === employeeId);
        }
    }

    private getDefaultTimeCodeId(row: ProjectTimeBlockDTO): number {
        if ((this.isNewRow(row)) && (row.timeDeviationCauseId > 0)) {
            const currentTimeDeviationCause = this.getTimeDeviationCause(row.timeDeviationCauseId);
            if (currentTimeDeviationCause && currentTimeDeviationCause.timeCodeId > 0) {
                if (this.timeCodeIdExists(currentTimeDeviationCause.timeCodeId)) {
                    return currentTimeDeviationCause.timeCodeId;
                }
                else {
                    return 0;
                }
            }
        }

        return row.selectedEmployee && row.selectedEmployee.defaultTimeCodeId && row.selectedEmployee.defaultTimeCodeId > 0 ? row.selectedEmployee.defaultTimeCodeId : this.defaultTimeCodeId;
    }

    private createTimeDeviationCode(row: ProjectTimeBlockDTO): ITimeDeviationCauseDTO {
        return {
            actorCompanyId: 0,
            adjustTimeInsideOfPlannedAbsence: 0,
            adjustTimeOutsideOfPlannedAbsence: 0,
            allowGapToPlannedAbsence: undefined,
            attachZeroDaysNbrOfDaysAfter: 0,
            attachZeroDaysNbrOfDaysBefore: 0,
            calculateAsOtherTimeInSales: false,
            changeCauseInsideOfPlannedAbsence: 0,
            changeCauseOutsideOfPlannedAbsence: 0,
            changeDeviationCauseAccordingToPlannedAbsence: undefined,
            created: undefined,
            createdBy: undefined,
            description: undefined,
            employeeGroupIds: [],
            employeeRequestPolicyNbrOfDaysBefore: undefined,
            employeeRequestPolicyNbrOfDaysBeforeCanOverride: undefined,
            excludeFromPresenceWorkRules: undefined,
            excludeFromScheduleWorkRules: undefined,
            extCode: undefined,
            externalCodes: [],
            imageSource: undefined,
            isAbsence: undefined,
            isPresence: undefined,
            isVacation: undefined,
            mandatoryNote: undefined,
            mandatoryTime: undefined,
            modified: undefined,
            modifiedBy: undefined,
            name: row.timeDeviationCauseName,
            notChargeable: undefined,
            onlyWholeDay: undefined,
            payed: undefined,
            showZeroDaysInAbsencePlanning: undefined,
            specifyChild: undefined,
            state: undefined,
            timeCode: undefined,
            timeCodeId: undefined,
            timeCodeName: undefined,
            timeDeviationCauseId: row.timeDeviationCauseId,
            type: undefined,
            typeName: undefined,
            validForHibernating: false,
            validForStandby: undefined,
            candidateForOvertime: false,
        }
    }

    private createProjectSmallDTO(row: ProjectTimeBlockDTO): ProjectSmallDTO {
        const project = new ProjectSmallDTO();
        project.projectId = row ? row.projectId : 0;
        project.name = row ? row.projectName : "";
        project.number = row ? row.projectNr : "";
        project.numberName = row ? row.projectNr + " " + row.projectName : " ";
        return project
    }

    private createProjectInvoiceSmallDTO(row: ProjectTimeBlockDTO): ProjectInvoiceSmallDTO {
        const invoice = new ProjectInvoiceSmallDTO();
        invoice.invoiceNr = row ? row.invoiceNr : "";
        invoice.invoiceId = row ? row.customerInvoiceId: 0;
        invoice.customerName = row? row.customerName : "";
        invoice.numberName = row ? row.invoiceNr + " " + row.customerName: " ";
        return invoice;
    }

    private setProjectsAndInvoices(row: ProjectTimeBlockDTO) {
        const projectInvoice = this.getProjectInvoiceRow(row.employeeId);
        if (projectInvoice) {
            row.filteredProjects = projectInvoice.projects;
            if (row.filteredProjects.length > 0 && row.filteredProjects[0].projectId) {
                row.filteredProjects.unshift(this.createProjectSmallDTO(undefined));
            }

            row.filteredInvoices = projectInvoice.invoices;
            if (row.filteredInvoices.length > 0 && row.filteredInvoices[0].invoiceId) {
                row.filteredInvoices.unshift(this.createProjectInvoiceSmallDTO(undefined));
            }
        }
        else if (row.projectId || row.customerInvoiceId) {
            row.filteredProjects = [];
            row.filteredProjects.push(this.createProjectSmallDTO(row));

            row.filteredInvoices = [];
            row.filteredInvoices.push(this.createProjectInvoiceSmallDTO(row));
        }
        else {
            this.loadProjectAndInvoices(row);
        }
    }

    // HELP-METHODS
    private createNewRow(): ProjectTimeBlockDTO {
        const newRow = new ProjectTimeBlockDTO();
        newRow['isNew'] = true;
        newRow['hasError'] = false;
        newRow['errorText'] = "";
        newRow["payrollQuantityChanged"] = false;
        newRow.filteredTimeCodes = this.timeCodes;
        newRow.tempRowId = this.rows.length + 1;

        //Copy some information from previous row (if exists)

        newRow.employeeId = this.lastEmployeeId;
        if (!newRow.employeeId && this.employee) {
            newRow.employeeId = this.employee.employeeId;
        }
        
        //dto.selectedEmployee = this.employee && this.employee.employeeId === dto.employeeId ? { employeeId: this.employee.employeeId, name: this.employee.name, employeeNr: this.employee.employeeNr, defaultTimeCodeId: this.employee.defaultTimeCodeId, timeDeviationCauseId: this.employee.timeDeviationCauseId } : _.find(this.employees, e => e.employeeId === dto.employeeId);
        newRow.selectedEmployee = this.getEmployee(newRow.employeeId);

        this.timeCodeChanged(newRow, this.getDefaultTimeCodeId(newRow));

        this.setProjectsAndInvoices(newRow);

        newRow.date = CalendarUtility.getDateToday();
        newRow.startTime = Constants.DATETIME_DEFAULT.beginningOfDay();
        newRow.stopTime = Constants.DATETIME_DEFAULT.beginningOfDay();

        //Set first time
        const employeeRows = _.orderBy(_.filter(this.rows, r => r.employeeId === newRow.employeeId && r.date.toDateString() === newRow.date.toDateString()), r => r.startTime);
        if (employeeRows && employeeRows.length > 0) {
            const lastRow = _.last(employeeRows);
            newRow.startTime = lastRow.stopTime;
        }
        else {
            const startTimeItem = _.find(this.startTimes, s => s.employeeId === newRow.employeeId && s.date.toDateString() === newRow.date.toDateString());
            if (startTimeItem) {
                newRow.startTime = startTimeItem.startTime;
            }
            else {
                this.getFirstEligableTimeForEmployee(newRow, newRow.date);
            }
        }

        //Load previous times and schedule
        if (this.useExtendedTimeRegistration && newRow.employeeId) {
            this.loadEmployeeTimesAndSchedule(newRow).then(() => {
                this.setDefaultTimeDeviationCause(newRow);
            });
        }

        // TODO: This is used to make employee and date read only on existing rows.
        // Replace functionality with a check on ID when that is implemented.
        newRow.isEditable = true;
        newRow.isPayrollEditable = true;

        return newRow;
    }

    private loadEmployeeTimesAndSchedule(row: ProjectTimeBlockDTO): ng.IPromise<any> {
        const deferral = this.$q.defer();
        
        //Load previous times and schedule
        const dayObject = this.employeeDaysWithSchedule ? this.employeeDaysWithSchedule.filter(e => e.employeeId === row.employeeId && e.date.toDateString() === row.date.toDateString()) : null;
        this.setRowErrorMessage("", row);
        if (dayObject && dayObject.length > 0) {
            this.filterTimeDeviationCauses(dayObject[0].employeeGroupId).then(() => {
                this.employeeScheduleInfoChanged(row, dayObject[0]);
                deferral.resolve();
            })
        }
        else {
            this.getEmployeeScheduleAndTransactionInfo(row).then(() => {
                deferral.resolve();
            });
        }

        return deferral.promise;
    }

    private getEmployee(employeeId: number): any {
        return _.find(this.employees, e => e.employeeId === employeeId);
    }

    private selectedEmployeeChanged(row: ProjectTimeBlockDTO, newValue: string) {

        row.selectedEmployee = newValue ? this.employees.find(e => e.nameNumber === newValue) : null;
        this.lastEmployeeId = row.employeeId = row.selectedEmployee ? row.selectedEmployee.employeeId : 0;
        row.filteredTimeCodes = this.timeCodes;
        
        //Set first time
        const employeeRows = _.orderBy(_.filter(this.rows, r => r.employeeId === row.employeeId && r.date.toDateString() === row.date.toDateString() && r.tempRowId !== row.tempRowId), r => r.startTime);
        if (employeeRows && employeeRows.length > 0) {
            const lastRow = _.last(employeeRows);
            row.startTime = lastRow.stopTime;
        }
        else {
            const startTimeItem = _.find(this.startTimes, s => s.employeeId === row.employeeId && s.date.toDateString() === row.date.toDateString());
            if (startTimeItem)
                row.startTime = startTimeItem.startTime;
            else
                this.getFirstEligableTimeForEmployee(row, row.date);
        }

        if (this.registrationType !== ProjectTimeRegistrationType.Order) {
            if (row.employeeId) {
                if (!this.filterProjectsAndInvoices(row)) {
                    row.filteredProjects = [];
                    row.filteredInvoices = [];
                    this.loadProjectAndInvoices(row);
                }
            }
        }

        if (this.useExtendedTimeRegistration) {
            row.employeeChildId = 0;
            row.childs = [];

            this.loadEmployeeTimesAndSchedule(row).then(() => {
                if (this.isNewRow(row)) {
                    this.setDefaultTimeDeviationCause(row);
                    this.timeCodeChanged(row, this.getDefaultTimeCodeId(row));
                    this.setTimeCodeReadOnly(row);
                }
            })
        }
        else {
            this.timeCodeChanged(row, this.getDefaultTimeCodeId(row));
        }
    }

    private setDefaultTimeDeviationCause(row: ProjectTimeBlockDTO) {
        let timeDeviationCauseId = 0;
        if (row.selectedEmployee && row.selectedEmployee.timeDeviationCauseId && row.selectedEmployee.timeDeviationCauseId > 0)
            timeDeviationCauseId = row.selectedEmployee.timeDeviationCauseId;
        else if (this.filteredTimeDeviationCauses && this.filteredTimeDeviationCauses.length > 0) {
            timeDeviationCauseId = this.filteredTimeDeviationCauses[0].timeDeviationCauseId;
        }

        this.timeDeviationCauseChanged(row, timeDeviationCauseId);
    }

    private loadProjectAndInvoices(row: ProjectTimeBlockDTO) {
        if (!this.limitToProjectUser && this.projectInvoices && this.projectInvoices.length > 0) {
            this.filterProjectsAndInvoices(row);
        }
        if (this.populateProjectAndInvoiceCallback) {
            this.progress.startLoadingProgress([
                () => this.populateProjectAndInvoiceCallback([row.employeeId]).then((result) => {
                    this.projectInvoices = result;
                    this.filterProjectsAndInvoices(row);
                })
            ])
        }
    }

    private filterProjectsAndInvoices(row: ProjectTimeBlockDTO): boolean {
        const projectInvoice = this.getProjectInvoiceRow(row.employeeId);
        if (projectInvoice) {
            row.filteredProjects = projectInvoice.projects;
            row.filteredInvoices = projectInvoice.invoices;
            return true;
        }
        return false;
    }

    private selectedProjectChanged(row: ProjectTimeBlockDTO, projectId: number) {

        row.selectedProject = projectId ? row.filteredProjects.find(x => x.projectId === projectId) : null;
        row.projectId = row.selectedProject ? row.selectedProject.projectId : 0;
        row.projectName = row.selectedProject ? row.selectedProject.name : "";
        row.projectNr = row.selectedProject ? row.selectedProject.number : "";

        const projectInvoice = this.getProjectInvoiceRow(row.employeeId);

        if (row.projectId) {
            if (projectInvoice) {
                row.filteredInvoices = projectInvoice.invoices.filter(i => i.invoiceId === 0 || i.projectId === row.projectId);
                if (row.customerInvoiceId && !row.filteredInvoices.find(i => i.invoiceId === row.customerInvoiceId)) {
                    this.selectedOrderInvoiceChanged(row, 0);
                    this.gridHandler.gridAg.options.refreshRows(row);
                }
                else if (!row.customerInvoiceId && row.filteredInvoices.length === 2) {
                    this.selectedOrderInvoiceChanged(row, row.filteredInvoices[1].invoiceId);
                }
            }
        }
        else {
            if (projectInvoice) {
                row.filteredProjects = projectInvoice.projects;
                row.filteredInvoices = projectInvoice.invoices;
            }
        }
    }

    private selectedOrderInvoiceChanged(row: ProjectTimeBlockDTO, invoiceId: number) {

        row.selectedCustomerInvoice = invoiceId ? row.filteredInvoices.find(x => x.invoiceId === invoiceId) : undefined;
        row.customerInvoiceId = row.selectedCustomerInvoice ? row.selectedCustomerInvoice.invoiceId : 0;
        row.invoiceNr = row.selectedCustomerInvoice ? row.selectedCustomerInvoice.invoiceNr : "";

        const projectInvoice = this.getProjectInvoiceRow(row.employeeId);

        if (row.customerInvoiceId) {
            if (projectInvoice) {
                row.projectId = row.selectedCustomerInvoice.projectId;
                this.selectedProjectChanged(row, row.projectId);
                this.gridHandler.gridAg.options.refreshRows(row);
            }
        } else if (projectInvoice) {
            row.filteredProjects = projectInvoice.projects;
            row.filteredInvoices = projectInvoice.invoices;
        }
    }

    private selectedDateChanged(row: ProjectTimeBlockDTO) {

        //Set first time
        const employeeRows = _.orderBy(_.filter(this.rows, r => r.employeeId === row.employeeId && r.date.toDateString() === row.date.toDateString() && r.tempRowId !== row.tempRowId), r => r.startTime);
        if (employeeRows && employeeRows.length > 0) {
            const lastRow = _.last(employeeRows);
            if (lastRow.stopTime) {
                row.startTime = lastRow.stopTime;
            }
        }
        else {
            const startTimeItem = _.find(this.startTimes, s => s.employeeId === row.employeeId && s.date.toDateString() === row.date.toDateString());
            if (startTimeItem)
                row.startTime = startTimeItem.startTime;
            else
                this.getFirstEligableTimeForEmployee(row, row.date);
        }

        //Load previous times and schedule
        if (this.useExtendedTimeRegistration) {
            this.loadEmployeeTimesAndSchedule(row).then(() => {
                if (this.isNewRow(row) && !row.timeDeviationCauseId) {
                    this.setDefaultTimeDeviationCause(row);
                }
                this.gridHandler.gridAg.options.refreshRows(row);
            });
        }
    }

    private isNewRow(row: ProjectTimeBlockDTO) {
        return row['isNew'] === true;
    }

    private setTimeCodeReadOnly(row: ProjectTimeBlockDTO) {
        const currentCause = this.getTimeDeviationCause(row.timeDeviationCauseId);

        if (currentCause && this.useExtendedTimeRegistration && row.isEditable) {
            row.timeCodeReadOnly = (currentCause.isPresence) && (!currentCause.notChargeable) ? false : true;

            if (row.timeCodeReadOnly) {
                this.timeCodeChanged(row, 0);
                row.invoiceQuantity = 0;
            }
        }
    }

    private timeDeviationCauseChanged(row: ProjectTimeBlockDTO, timeDeviationCauseId: number) {
        const timeDeviationCause = timeDeviationCauseId ? this.getTimeDeviationCause(timeDeviationCauseId) : undefined;

        row.timeDeviationCauseName = timeDeviationCause?.name ?? ""; 
        row.timeDeviationCauseId = timeDeviationCauseId;

        this.setTimeCodeReadOnly(row);
        if (row.isEditable && !row.timeCodeReadOnly) {
            this.timeCodeChanged(row, this.getDefaultTimeCodeId(row));
        }
        
        if (timeDeviationCause && timeDeviationCause.specifyChild && row.childs.length === 0) {
            this.loadEmployeeChilds(row, true);
        }
        else if (timeDeviationCause && !timeDeviationCause.specifyChild) {
            row.childs = [];
            row.employeeChildId = 0;
            row.employeeChildName = "";
        }

        if (!row.mandatoryTime && timeDeviationCause?.mandatoryTime) {
            row.startTime = Constants.DATETIME_DEFAULT.beginningOfDay();
            row.stopTime = Constants.DATETIME_DEFAULT.beginningOfDay();
        }

        row.mandatoryTime = timeDeviationCause?.mandatoryTime ?? false;
        row.additionalTime = timeDeviationCause?.calculateAsOtherTimeInSales ?? false;

        if (row.mandatoryTime) {
            this.toggleStartStopColumns();
        }
        this.gridHandler.gridAg.options.refreshRows(row);
    }

    private timeCodeChanged(row: ProjectTimeBlockDTO, timeCodeId: number) {
        const timeCode = row.filteredTimeCodes.find(x => x.value === timeCodeId);
        row.timeCodeId = timeCodeId;
        row.timeCodeName = timeCode ? timeCode.label : "";
    }

    private timeChanged(row: ProjectTimeBlockDTO) {
        if (row.stopTime.getHours() < row.startTime.getHours()) {
            if (row.startTime.getDay() === row.stopTime.getDay()) {
                row.stopTime = row.stopTime.addDays(1);
            }
        }
        else if (row.stopTime.getHours() >= row.startTime.getHours() && row.stopTime.getDay() > row.startTime.getDay()) {
            row.stopTime = row.stopTime.addDays(-1);
        }

        if (row.startTime && row.stopTime && (row.stopTime.hour() > 0 || row.stopTime.minutes() > 0)) {
            row.timePayrollQuantity = row.stopTime.diffMinutes(row.startTime);
            this.updateInvoiceQuantityFromPayroll(row);
        }
        this.gridHandler.gridAg.options.refreshRows(row);
    }

    private payrollQuantityChanged(row: ProjectTimeBlockDTO) {
        if (row?.startTime) {
            row.stopTime = row.startTime.addMinutes(row.timePayrollQuantity);
            row["payrollQuantityChanged"] = true;
        }
        this.updateInvoiceQuantityFromPayroll(row);
        this.gridHandler.gridAg.options.refreshRows(row);
    }
    private additionalQuantityChanged(row: ProjectTimeBlockDTO) {
        this.updateInvoiceQuantityFromPayroll(row);
        this.gridHandler.gridAg.options.refreshRows(row);
    }

    private updateInvoiceQuantityFromPayroll(row: ProjectTimeBlockDTO) {
        if (row['isNew'] === true && this.invoiceTimeAsWorkTime && this.invoiceTimePermission && !row.timeCodeReadOnly)
            row.invoiceQuantity = row.timePayrollQuantity;
    }

    private filterTimeDeviationCauses(employeeGroupId: number): ng.IPromise<any> {

        return this.getTimeDeviationCausesCallback(employeeGroupId).then((result) => {
            this.filteredTimeDeviationCauses = result;

            //check for current 
            this.rows.forEach((row: ProjectTimeBlockDTO) => {
                if (row.timeDeviationCauseId) {
                    if (!_.find(this.filteredTimeDeviationCauses, ["timeDeviationCauseId", row.timeDeviationCauseId])) {
                        this.filteredTimeDeviationCauses.push(this.createTimeDeviationCode(row));
                    }
                    if (row.projectTimeBlockId) {
                        this.setTimeCodeReadOnly(row);
                    }
                }
            });
        });

        /*
        _.forEach(this.timeDeviationCauses, (cause) => {
            if (_.includes(cause.employeeGroupIds, employeeGroupId))
                this.filteredTimeDeviationCauses.push(cause);
        });
        */
    }

    private filterTimeCodes(row: ProjectTimeBlockDTO, setTimeCode = true) {
        var removeTimeCode = false;
        var empRows = _.filter(this.rows, (r) => r.employeeId === row.employeeId && r.date.toDateString() === row.date.toDateString())
        if (empRows && empRows.length > 0) {
            var usedTimeCodes = _.map(empRows, (t) => t.timeCodeId);
            _.forEach(empRows, (r) => {
                if (r.timeCodeId === row.timeCodeId && r.tempRowId != row.tempRowId)
                    removeTimeCode = true;

                r.filteredTimeCodes = _.filter(this.timeCodes, t => t.value === r.timeCodeId || !_.includes(usedTimeCodes, t.value));
            });
        }
        else {
            row.filteredTimeCodes = this.timeCodes;
        }

        if (removeTimeCode) {
            row.timeCodeId = undefined;
            //this.filterTimeCodes(false);
        }

        if (setTimeCode && row.filteredTimeCodes && row.filteredTimeCodes.length > 0) { //&& !this.selectedRow.timeCodeId 
            if (row.employeeId) {
                const employee = this.getEmployee(row.employeeId);
                if (employee) {
                    if (employee.defaultTimeCodeId && employee.defaultTimeCodeId > 0) {
                        const timeCode = _.find(row.filteredTimeCodes, t => t.value === employee.defaultTimeCodeId);
                        if (timeCode)
                            row.timeCodeId = timeCode.value;
                        else
                            row.timeCodeId = row.filteredTimeCodes[0].value;
                    }
                    else
                        row.timeCodeId = row.filteredTimeCodes[0].value;
                }
                else {
                    row.timeCodeId = row.filteredTimeCodes[0].value;
                }
            }
            else {
                row.timeCodeId = row.filteredTimeCodes[0].value;
            }
        }
    }

    private getFirstEligableTimeForEmployee(row: ProjectTimeBlockDTO, date: Date) {
        this.coreService.getFirstEligableTimeForEmployee(row.employeeId, date).then((result) => {
            if (result) {
                result = CalendarUtility.convertToDate(result);
                row.startTime = new Date(row.startTime.getFullYear(), row.startTime.getMonth(), row.startTime.getDate(), result.getHours(), result.getMinutes(), result.getSeconds(), 0);
                this.startTimes.push({ employeeId: row.employeeId, date: date, startTime: row.startTime });
            }
        });
    }

    private getEmployeeScheduleAndTransactionInfo(row: ProjectTimeBlockDTO, openDialog = false): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (!this.employeeDaysWithSchedule)
            this.employeeDaysWithSchedule = [];

        this.coreService.getEmployeeScheduleAndTransactionInfo(row.employeeId, row.date).then(result => {
            if (result) {
                result.date = CalendarUtility.convertToDate(result.date);
                this.employeeScheduleInfoChanged(row, result);
                this.filterTimeDeviationCauses(result.employeeGroupId).then(() => {
                    deferral.resolve();
                });
                if (_.filter(this.employeeDaysWithSchedule, e => e.employeeId === row.employeeId && e.date === row.date).length === 0) {
                    this.employeeDaysWithSchedule.push(result);

                    if (openDialog)
                        this.showInfoDialog(result);
                }
            }
            else {
                deferral.resolve();
            }
        });

        return deferral.promise;
    }

    private employeeScheduleInfoChanged(row: ProjectTimeBlockDTO, data: IEmployeeScheduleTransactionInfoDTO) {
        row.autoGenTimeAndBreakForProject = data.autoGenTimeAndBreakForProject;
        const matchingRows = this.rows.filter(x => x.date.toDateString() === data.date.toDateString());

        if (matchingRows.length > 0) {
            matchingRows.forEach(r => r.autoGenTimeAndBreakForProject = data.autoGenTimeAndBreakForProject);
        }

        if (data.employeeGroupId) {
            const employee = this.getEmployee(data.employeeId);
            if (employee && !employee.timeDeviationCauseId && data.timeDeviationCauseId) {
                employee.timeDeviationCauseId = data.timeDeviationCauseId;
            }

            this.toggleStartStopColumns();
        }
        else {
            this.setRowErrorMessage("billing.project.timesheets.employeegroupmissing", row);
            row.timeDeviationCauseId = 0;
        }
    }

    private setRowErrorMessage(msgKey: string, row: ProjectTimeBlockDTO) {
        if (msgKey) {
            row['hasError'] = true;
            row['errorText'] = this.translationService.translateInstant(msgKey);
        }
        else {
            row['hasError'] = false;
            row['errorText'] = "";
        }
    }

    private showInfoDialog(infoItem: any) {
        const employee = _.find(this.employees, e => e.employeeId === infoItem.employeeId);
        // Show edit time dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/TimeProjectReport/Views/employeeInfo.html"),
            controller: EmployeeInfoController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            windowClass: 'fullsize-modal',
            resolve: {
                title: () => { return this.terms['billing.project.timesheet.edittime.dialogschema'] + (employee ? employee.name : "") + " (" + infoItem.date.toFormattedDate() + ")" },
                infoItem: () => { return infoItem }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {

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
            size: 'lg',
            windowClass: '',
            resolve: {
                rows: () => { return this.rows },
                row: () => { return row },
                isReadonly: () => { return this.readOnly },
                workTimePermission: () => { return this.workTimePermission },
                invoiceTimePermission: () => { return this.invoiceTimePermission },
                saveDirect: () => { return false }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.rowIsModified) {
                this.gridHandler.gridAg.options.refreshRows(row);
                this.isDirty = true;
            }
        });
    }
}