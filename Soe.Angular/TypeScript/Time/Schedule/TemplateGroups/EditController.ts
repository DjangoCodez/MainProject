import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IFocusService } from "../../../Core/Services/FocusService";
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { IScheduleService } from "../ScheduleService";
import { ScheduleService as SharedScheduleService } from "../../../Shared/Time/Schedule/Scheduleservice";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { EditController as TemplateEditController } from "../Templates/EditController";
import { TimeScheduleTemplateGroupDTO, TimeScheduleTemplateGroupEmployeeDTO, TimeScheduleTemplateGroupRowDTO, TimeScheduleTemplateHeadRangeDTO, TimeScheduleTemplateHeadSmallDTO } from "../../../Common/Models/TimeScheduleTemplateDTOs";
import { Constants } from "../../../Util/Constants";
import { DailyRecurrenceParamsDTO, DailyRecurrenceRangeDTO } from "../../../Common/Models/DailyRecurrencePatternDTOs";
import { DailyRecurrencePatternController } from "../../../Common/Dialogs/DailyRecurrencePattern/DailyRecurrencePatternController";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SelectEmployeesController } from "../../../Common/Dialogs/SelectEmployees/SelectEmployeesController";
import { EmployeeGridDTO } from "../../../Common/Models/EmployeeUserDTO";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { EmployeeSmallDTO } from "../../../Common/Models/EmployeeListDTO";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { INotificationService } from "../../../Core/Services/NotificationService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {
    // Terms
    private terms: { [index: string]: string; };

    // Data
    private timeScheduleTemplateGroupId: number;
    private templateGroup: TimeScheduleTemplateGroupDTO;
    private templateHeads: TimeScheduleTemplateHeadSmallDTO[];
    private employees: EmployeeSmallDTO[] = [];
    private simulatedHeads: TimeScheduleTemplateHeadRangeDTO[] = [];

    private selectedEmployeeIds: number[] = [];

    // Grid
    private gridHandler: EmbeddedGridController;

    // Properties
    private employeeDefaultFromDate: Date;
    private employeeDefaultToDate: Date;

    private modalInstance: any;
    private edit: ng.IFormController;

    //@ngInject
    constructor(
        private $uibModal,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: SharedScheduleService,
        private sharedEmployeeService: SharedEmployeeService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "TemplateGroupEmployees");

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP

    public onInit(parameters: any) {
        this.timeScheduleTemplateGroupId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Schedule_TemplateGroups, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.timeScheduleTemplateGroupId);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Schedule_TemplateGroups].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_TemplateGroups].modifyPermission;
    }

    private setupGrid() {
        this.gridHandler.gridAg.options.enableGridMenu = false;
        this.gridHandler.gridAg.options.enableFiltering = true;
        this.gridHandler.gridAg.options.enableRowSelection = true;
        this.gridHandler.gridAg.options.disableHorizontalScrollbar = true;
        this.gridHandler.gridAg.options.setMinRowsToShow(8);

        this.gridHandler.gridAg.addColumnTypeAhead("employeeNr", this.terms["common.name"], 200, {
            minWidth: 200,
            maxWidth: 200,
            editable: (row) => this.isEmployeeEditable(row),
            cellStyle: (row: TimeScheduleTemplateGroupEmployeeDTO) => this.getCellStyle(row),
            secondRow: "employeeName",
            hideSecondRowSeparator: true,
            onChanged: (data) => this.onEmployeeChanged(data),
            typeAheadOptions: {
                source: (filter) => this.filterEmployees(filter),
                displayField: "numberAndName",
                dataField: "employeeNr",
                updater: null,
                minLength: 0,
                delay: 0,
                useScroll: true,
                allowNavigationFromTypeAhead: (value) => this.allowNavigationFromEmployee(value)
            }
        });

        this.gridHandler.gridAg.addColumnDate("fromDate", this.terms["common.datefrom"], 150, false, null, { editable: this.isEmployeeEditable.bind(this), suppressSizeToFit: true, cellStyle: (row: TimeScheduleTemplateGroupEmployeeDTO) => this.getCellStyle(row) });
        this.gridHandler.gridAg.addColumnDate("toDate", this.terms["common.dateto"], 150, false, null, { editable: this.isEmployeeEditable.bind(this), suppressSizeToFit: true, cellStyle: (row: TimeScheduleTemplateGroupEmployeeDTO) => this.getCellStyle(row) });
        this.gridHandler.gridAg.addColumnText("errorMessage", this.terms["core.error"], null, false, { toolTipField: 'errorMessage', cellStyle: (row: TimeScheduleTemplateGroupEmployeeDTO) => this.getCellStyle(row) });
        this.gridHandler.gridAg.addColumnText("warningMessage", this.terms["core.warning"], null, false, { toolTipField: 'warningMessage', cellStyle: (row: TimeScheduleTemplateGroupEmployeeDTO) => this.getCellStyle(row) });
        this.gridHandler.gridAg.addColumnText("infoMessage", this.terms["core.info"], null, false, { toolTipField: 'infoMessage', cellStyle: (row: TimeScheduleTemplateGroupEmployeeDTO) => this.getCellStyle(row) });
        this.gridHandler.gridAg.addColumnIcon(null, null, 50, { icon: 'fal fa-calendar-check', showIcon: (row: TimeScheduleTemplateGroupEmployeeDTO) => row && row.employeeId && !!row.fromDate && !row.readOnly, toolTip: this.terms["time.schedule.templategroupemployee.checkoverlappingtemplates"], onClick: this.validateOverlappingTemplates.bind(this), cellStyle: (row: TimeScheduleTemplateGroupEmployeeDTO) => this.getCellStyle(row), pinned: 'right' });
        this.gridHandler.gridAg.addColumnDelete(this.terms["core.deleterow"], this.deleteEmployee.bind(this), false, this.isEmployeeEditable.bind(this));

        let events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.IsRowSelectable, (row) => {
            return row.data && !row.data.readOnly;
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterEmployeeCellEdit(entity, colDef, newValue, oldValue); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row) => { this.employeeSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row) => { this.employeeSelectionChanged(); }));
        this.gridHandler.gridAg.options.subscribe(events);

        this.gridHandler.gridAg.finalizeInitGrid("common.employees", true);
    }

    private isEmployeeEditable(row: TimeScheduleTemplateGroupEmployeeDTO) {
        return this.modifyPermission && !row.readOnly;
    }

    private getCellStyle(row: TimeScheduleTemplateGroupEmployeeDTO): any {
        return row.readOnly ? { 'background-color': "#f5f5f5", 'font-style': 'italic' } : undefined;
    }

    // LOOKUPS

    private doLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadTerms(),
            () => this.loadTemplateHeads(),
            () => this.loadEmployees()
        ]).then(() => {
            this.setupGrid();

            if (this.timeScheduleTemplateGroupId)
                this.onLoadData();
            else
                this.new();
        });
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.deleterow",
            "core.error",
            "core.warning",
            "core.info",
            "common.name",
            "common.datefrom",
            "common.dateto",
            "time.schedule.templategroupemployee.checkoverlappingtemplates",
            "time.schedule.templategroupemployee.overlappingfromdate.message",
            "time.schedule.templategroupemployee.overlappingdaterange.message",
            "time.employee.employee.overlappingtemplates.title",
            "time.schedule.templategroup.getongoingtemplates.warning"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadTemplateHeads(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTemplateHeadsForActivate().then(x => {
            this.templateHeads = x;
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        return this.sharedEmployeeService.getEmployeesForGridSmall(true).then(x => {
            this.employees = x;
        });
    }

    private getOverlappingTemplates(employeeId: number, date: Date): ng.IPromise<string[]> {
        return this.sharedScheduleService.getOverlappingTemplates(employeeId, date);
    }

    private onLoadData(): ng.IPromise<any> {
        this.simulatedHeads = [];
        return this.progress.startLoadingProgress([
            () => this.scheduleService.getTimeScheduleTemplateGroup(this.timeScheduleTemplateGroupId, true, true, true, true).then(x => {
                this.isNew = false;
                this.templateGroup = x;
                if (this.templateGroup.rows) {
                    _.forEach(this.templateGroup.rows, row => {
                        this.setRecurrenceInfo(row);
                    });
                }
                if (this.templateGroup.employees) {
                    let employeeIds: number[] = this.employees.map(e => e.employeeId);
                    _.forEach(this.templateGroup.employees, empRow => {
                        if (!_.includes(employeeIds, empRow.employeeId))
                            empRow.readOnly = true;
                    });
                }
                this.refreshEmployeeGrid(true);
            })
        ]);
    }

    // ACTIONS

    private new() {
        this.isNew = true;
        this.timeScheduleTemplateGroupId = 0;
        this.templateGroup = new TimeScheduleTemplateGroupDTO();
        this.templateGroup.isActive = true;

        this.focusService.focusByName("ctrl_templateGroup_name");
    }

    protected copy() {
        super.copy();

        // Clear template head
        this.timeScheduleTemplateGroupId = this.templateGroup.timeScheduleTemplateGroupId = 0;
        this.templateGroup.name = this.templateGroup.description = undefined;
        this.templateGroup.isActive = true;

        this.templateGroup.created = null;
        this.templateGroup.createdBy = null;
        this.templateGroup.modified = null;
        this.templateGroup.modifiedBy = null;


        // Clear template period data
        _.forEach(this.templateGroup.rows, row => {
            row.timeScheduleTemplateGroupRowId = 0;
        });

        this.focusService.focusByName("ctrl_templateGroup_name");
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveTimeScheduleTemplateGroup(this.templateGroup).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        this.timeScheduleTemplateGroupId = result.integerValue;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.templateGroup);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.onLoadData();
            });
    }

    private delete() {
        if (!this.templateGroup.timeScheduleTemplateGroupId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteTimeScheduleTemplateGroup(this.templateGroup.timeScheduleTemplateGroupId).then(result => {
                if (result.success) {
                    completion.completed(this.templateGroup, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(true);
        });
    }

    // EVENTS

    private addRow() {
        if (!this.templateGroup.rows)
            this.templateGroup.rows = [];

        let row = new TimeScheduleTemplateGroupRowDTO();
        row.startDate = new Date();
        this.templateGroup.rows.push(row);

        this.setDirty();
    }

    private rowTemplateChanged(row: TimeScheduleTemplateGroupRowDTO) {
        this.$timeout(() => {
            let template = this.templateHeads.find(t => t.timeScheduleTemplateHeadId === row.timeScheduleTemplateHeadId);
            if (template) {
                row.startDate = template.startDate;
                if (template.stopDate)
                    row.stopDate = template.stopDate;
            }
        });
    }

    private rowDateChanged(row: TimeScheduleTemplateGroupRowDTO) {
        this.$timeout(() => {
            this.setRowNextStartDate(row);
        });
    }

    private openRecurrencePatternDialog(row: TimeScheduleTemplateGroupRowDTO) {
        let params = new DailyRecurrenceParamsDTO(row);

        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/DailyRecurrencePattern/Views/dailyRecurrencePattern.html"),
            controller: DailyRecurrencePatternController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                pattern: () => { return params.pattern },
                range: () => { return params.range },
                excludedDates: () => { return [] },
                date: () => { return params.date },
                hideRange: () => { return true }
            }
        }

        this.$uibModal.open(options).result.then(result => {
            if (result) {
                params.parseResult(row, result);
                this.setRecurrenceInfo(row);
                this.setRowNextStartDate(row);
                this.dirtyHandler.setDirty();
            }
        });
    }

    private openEditTemplateDialog(row: TimeScheduleTemplateGroupRowDTO) {
        let modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Templates/Views/edit.html"),
            controller: TemplateEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                source: 'templateGroup',
                modal: modal,
                id: row.timeScheduleTemplateHeadId,
                employeeId: undefined
            });
        });
    }

    private deleteRow(row: TimeScheduleTemplateGroupRowDTO) {
        _.pull(this.templateGroup.rows, row);
        this.setDirty();
    }

    private addEmployee(employeeId: number, refreshGrid: boolean) {
        if (!this.templateGroup.employees)
            this.templateGroup.employees = [];

        let emp = employeeId ? _.find(this.employees, e => e.employeeId === employeeId) : undefined;

        let empRow = new TimeScheduleTemplateGroupEmployeeDTO();
        empRow.employeeId = emp?.employeeId;
        empRow.employeeNr = emp?.employeeNr || '';
        empRow.employeeName = emp?.name || '';
        empRow.fromDate = this.employeeDefaultFromDate;
        empRow.toDate = this.employeeDefaultToDate;
        this.templateGroup.employees.push(empRow);

        this.validateOverlappingTemplates(empRow);

        if (refreshGrid) {
            this.gridHandler.gridAg.options.addRow(empRow, !employeeId, this.gridHandler.gridAg.options.getColumnByField('employeeNr'));
            this.setDirty();
        }
    }

    private addEmployees() {
        if (!this.employeeDefaultFromDate) {
            let keys: string[] = [
                "time.schedule.templategroupemployee.nodefaultfromdatewarning.title",
                "time.schedule.templategroupemployee.nodefaultfromdatewarning.message"
            ];
            this.translationService.translateMany(keys).then(terms => {
                let modal = this.notificationService.showDialogEx(terms["time.schedule.templategroupemployee.nodefaultfromdatewarning.title"], terms["time.schedule.templategroupemployee.nodefaultfromdatewarning.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.openAddEmployeesDialog();
                });
            });
        } else {
            this.openAddEmployeesDialog();
        }
    }

    private openAddEmployeesDialog() {
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectEmployees/SelectEmployees.html"),
            controller: SelectEmployeesController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
            }
        }

        this.$uibModal.open(options).result.then(result => {
            if (result && result.selectedEmployees) {
                _.forEach(result.selectedEmployees, (employee: EmployeeGridDTO) => {
                    this.addEmployee(employee.employeeId, false);
                });
                this.$timeout(() => {
                    // Trigger validation directive
                    this.$scope.$digest();
                    this.refreshEmployeeGrid(true);
                    this.setDirty();
                });
            }
        });
    }

    private afterEmployeeCellEdit(entity, colDef, newValue, oldValue) {
        if (newValue !== oldValue) {
            this.validateOverlappingTemplates(entity);
            this.refreshEmployeeGrid(true);
            this.setDirty();
        }
    }

    private onEmployeeChanged(data) {
        let row: TimeScheduleTemplateGroupEmployeeDTO = data.data;
        let employee = this.findEmployee(row);

        row.employeeId = employee ? employee.employeeId : 0;
        row.employeeNr = employee ? employee.employeeNr : '';
        row.employeeName = employee ? employee.name : '';
    }

    private deleteEmployee(employee: TimeScheduleTemplateGroupEmployeeDTO) {
        _.pull(this.templateGroup.employees, employee);

        // Trigger validation directive
        this.$scope.$digest();

        this.refreshEmployeeGrid(true);
        this.setDirty();
    }

    private employeeSelectionChanged() {
        this.$timeout(() => {
            this.selectedEmployeeIds = this.gridHandler.gridAg.options.getSelectedIds("employeeId");
        });
    }

    private getOngoingTemplates() {
        _.forEach(_.uniq(this.selectedEmployeeIds), employeeId => {
            let empRow = _.find(this.templateGroup.employees, e => e.employeeId === employeeId);
            if (empRow && empRow.infoMessage) {
                empRow.infoMessage = '';
                this.gridHandler.gridAg.options.refreshRows(empRow);
            }
        });

        this.scheduleService.getOngoingTimeScheduleTemplateHeads(this.getSelectedEmployeesAsDict()).then(x => {
            _.forEach(x, head => {
                let empRow = _.find(this.templateGroup.employees, e => e.employeeId === head.employeeId);
                if (empRow) {
                    let msg = "{0}: {1}".format(this.terms["time.schedule.templategroup.getongoingtemplates.warning"], head.name);
                    if (!head.name.endsWith(head.startDate.toFormattedDate()))
                        msg += ", {0}".format(head.startDate.toFormattedDate());
                    empRow.infoMessage = msg;
                    this.gridHandler.gridAg.options.refreshRows(empRow);
                }
            });
        });
    }

    private setStopDate() {
        this.scheduleService.setStopDateOnTimeScheduleTemplateHeads(this.getSelectedEmployeesAsDict()).then(x => {
            if (x.success) {
                this.$timeout(() => {
                    this.getOngoingTemplates();
                });
            } else {
                this.notificationService.showDialogEx('', x.errorMessage, SOEMessageBoxImage.Error);
            }
        });
    }

    private getSelectedEmployeesAsDict() {
        let dict: any = {};
        _.forEach(_.uniq(this.selectedEmployeeIds), employeeId => {
            let empRow = _.find(this.templateGroup.employees, e => e.employeeId === employeeId);
            if (empRow)
                dict[employeeId] = empRow.fromDate;
        });

        return dict;
    }

    private simulate() {
        this.simulatedHeads = [];
        this.scheduleService.getTimeScheduleTemplateHeadsRange(this.timeScheduleTemplateGroupId, CalendarUtility.getDateToday(), CalendarUtility.getDateToday().addYears(5)).then(x => {
            this.simulatedHeads = x.heads;
        });
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    private setRecurrenceInfo(row: TimeScheduleTemplateGroupRowDTO) {
        if (row) {
            DailyRecurrenceRangeDTO.setRecurrenceInfo(row, this.translationService);
            this.scheduleService.getRecurrenceDescription(row.recurrencePattern).then((x) => {
                row["patternDescription"] = x;
            });
        }
    }

    private setRowNextStartDate(row: TimeScheduleTemplateGroupRowDTO) {
        if (row.startDate && row.recurrencePattern) {
            this.scheduleService.getTimeScheduleTemplateGroupRowNextStartDate(row.startDate, row.stopDate, row.recurrencePattern).then(x => {
                row.nextStartDate = x;
            });
        } else {
            row.nextStartDate = null;
        }
    }

    private refreshEmployeeGrid(setValidationErrorMessages: boolean) {
        if (setValidationErrorMessages)
            this.setEmployeeValidationErrorMessages();

        this.gridHandler.gridAg.setData(this.templateGroup.employees);
    }

    private filterEmployees(filter) {
        return _.orderBy(this.employees.filter(e => {
            if (parseInt(filter))
                return e.employeeNr.startsWithCaseInsensitive(filter);

            return e.name.startsWithCaseInsensitive(filter) || e.name.contains(filter);
        }), 'name');
    }

    private findEmployee(row: TimeScheduleTemplateGroupEmployeeDTO): EmployeeSmallDTO {
        if (!row.employeeNr)
            return null;

        return this.employees.find(e => e.employeeNr === row.employeeNr);
    }

    private allowNavigationFromEmployee(value): boolean {
        return (!value || this.employees.filter(p => p.employeeNr === value).length > 0);
    }

    // VALIDATION    

    private validateOverlappingTemplates(empRow: TimeScheduleTemplateGroupEmployeeDTO) {
        if (empRow.employeeId && empRow.fromDate) {
            this.getOverlappingTemplates(empRow.employeeId, empRow.fromDate).then(x => {
                empRow.warningMessage = x.length > 0 ? "{0}: {1}".format(this.terms["time.employee.employee.overlappingtemplates.title"], x.join(", ")) : '';
                this.gridHandler.gridAg.options.refreshRows(empRow);
            });
        }
    }

    private setEmployeeValidationErrorMessages() {
        this.templateGroup.employees.forEach(emp => {
            if (emp['fromDateOverlapping'])
                emp.errorMessage = this.terms["time.schedule.templategroupemployee.overlappingfromdate.message"];
            else if (emp['dateRangeOverlapping'])
                emp.errorMessage = this.terms["time.schedule.templategroupemployee.overlappingdaterange.message"];
            else
                emp.errorMessage = '';
        });
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.templateGroup) {
                var errors = this['edit'].$error;

                // Mandatory fields
                if (!this.templateGroup.name)
                    mandatoryFieldKeys.push("common.name");

                // Rows
                if (errors['rowTemplateMandatory'])
                    validationErrorKeys.push("time.schedule.templategrouprow.missingtemplate");
                if (errors['rowStartDateMandatory'])
                    validationErrorKeys.push("time.schedule.templategrouprow.missingstartdate");
                if (errors['rowDatesValidOrder'])
                    validationErrorKeys.push("time.schedule.templategrouprow.datesinvalidorder");

                // Employees
                if (errors['employeeEmployeeMandatory'])
                    validationErrorKeys.push("time.schedule.templategroupemployee.missingemployee");
                if (errors['employeeFromDateMandatory'])
                    validationErrorKeys.push("time.schedule.templategroupemployee.missingfromdate");
                if (errors['employeeDatesValidOrder'])
                    validationErrorKeys.push("time.schedule.templategroupemployee.datesinvalidorder");
                if (errors['employeeFromDateOverlapping'])
                    validationErrorKeys.push("time.schedule.templategroupemployee.overlappingfromdate");
                if (errors['employeeDateRangeOverlapping'])
                    validationErrorKeys.push("time.schedule.templategroupemployee.overlappingdaterange");
            }
        });
    }
}
