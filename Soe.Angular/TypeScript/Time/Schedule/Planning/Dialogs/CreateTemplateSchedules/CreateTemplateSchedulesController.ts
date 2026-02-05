import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { IScheduleService } from "../../../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../../../Shared/Time/Schedule/ScheduleService";
import { GridEvent, ISoeGridOptions, SoeGridOptions } from "../../../../../Util/SoeGridOptions";
import { TemplateScheduleEmployeeDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { TimeScheduleTemplateHeadSmallDTO } from "../../../../../Common/Models/TimeScheduleTemplateDTOs";
import { Constants } from "../../../../../Util/Constants";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { EmployeeAccountDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { IFocusService } from "../../../../../Core/Services/focusservice";

export class CreateTemplateSchedulesController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private soeGridOptions: ISoeGridOptions;
    private generatedEmployeeIds: number[] = [];
    private employeeTemplates: TimeScheduleTemplateHeadSmallDTO[];
    private employeeAccounts: EmployeeAccountDTO[] = [];

    // Flags
    private loadingTemplates: boolean = false;
    private executing: boolean = false;

    private progress: IProgressHandler;

    private useAccountingFromSourceSchedule: boolean = true;

    private _copyFromEmployeeId: any;
    private get copyFromEmployeeId(): any {
        return this._copyFromEmployeeId;
    }
    private set copyFromEmployeeId(id: any) {
        this.setSourceEmployeeId(id);
    }

    private _copyFromTemplateHeadId: number;
    private get copyFromTemplateHeadId(): number {
        return this._copyFromTemplateHeadId;
    }
    private set copyFromTemplateHeadId(id: number) {
        this.setSourceTemplateId(id);
    }

    //@ngInject
    constructor(private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        progressHandlerFactory: IProgressHandlerFactory,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private messagingService: IMessagingService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private useAccountHierarchy: boolean,
        private focusService: IFocusService,
        private useStopDate: boolean,
        private allEmployees: TemplateScheduleEmployeeDTO[],
        private employees: TemplateScheduleEmployeeDTO[],
        private dateFrom: Date,
        private dateTo: Date,
        private nbrOfWeeks: number) {

        this.progress = progressHandlerFactory.create();

        this.$q.all([
            this.loadTerms()
        ]).then(() => {
            this.setupGrid();
        });
    }

    private $onInit() {
        this.soeGridOptions = new SoeGridOptions("", this.$timeout, this.uiGridConstants);
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.showGridFooter = true;
        this.soeGridOptions.enableFullRowSelection = false;
        this.soeGridOptions.enableRowHeaderSelection = true;
        this.soeGridOptions.setMinRowsToShow(12);

        this.soeGridOptions.subscribe([new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: any) => {
            this.$timeout(() => {
                this.setSelectedSourceEmployeeIdOnRow(row);
                this.setSelectedSourceTemplateIdOnRow(row);
            });
        })]);

        if (this.employees.length > 0)
            this.loadingTemplates = true;

        this.getEmployeeAccounts();

        _.forEach(this.employees, e => {
            e.loadingTemplates = true;
            e.copyFromEmployeeId = e.employeeId;
            e.copyFromEmployeeName = e.numberAndName;
            this.loadTemplateHeadsForEmployee(e);
        });
    }

    private setupGrid() {
        this.soeGridOptions.addColumnText("numberAndName", this.terms["common.employee"], null);
        this.soeGridOptions.addColumnText("currentTemplate", this.terms["time.schedule.planning.createtemplateschedules.currenttemplate"], null);
        this.soeGridOptions.addColumnNumber("currentTemplateNbrOfWeeks", this.terms["core.time.weeks"], "75");
        this.soeGridOptions.addColumnSelect("copyFromEmployeeId", this.terms["time.schedule.planning.templateschedule.copyfrom.employee"], null, this.allEmployees, false, true, "copyFromEmployeeName", "employeeId", "numberAndName", "rowEmployeeChanged");
        this.soeGridOptions.addColumnSelect("copyFromTemplateHeadId", this.terms["time.schedule.planning.templateschedule.copyfrom.schedule"], null, null, false, true, "copyFromTemplateHeadName", "timeScheduleTemplateHeadId", "description", "rowTemplateChanged", null, "templates");
        this.soeGridOptions.addColumnText("status", this.terms["common.status"], null, false, "status");
        this.soeGridOptions.addColumnIcon("statusIcon", null, null, "showStatusDialog");

        this.soeGridOptions.setData(this.employees);
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.time.weeks",
            "common.employee",
            "common.status",
            "common.weekshort",
            "time.schedule.planning.createtemplateschedules.currenttemplate",
            "time.schedule.planning.createtemplateschedules.pending",
            "time.schedule.planning.createtemplateschedules.templatecreated",
            "time.schedule.planning.createtemplateschedules.templatenotcreated",
            "time.schedule.planning.templateschedule.copyfrom.employee",
            "time.schedule.planning.templateschedule.copyfrom.schedule"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadTemplateHeadsForEmployee(row: TemplateScheduleEmployeeDTO): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (row) {
            row.templates = [];
            if (!row.copyFromEmployeeId) {
                row.loadingTemplates = false;
                this.setLoadingTemplates();
                deferral.resolve();
            } else {
                this.loadTemplateHeadsForOneEmployee(row.copyFromEmployeeId).then(templates => {
                    row.templates = templates;
                    row.copyFromTemplateHeadId = 0;
                    row.loadingTemplates = false;
                    this.setLoadingTemplates();
                    deferral.resolve();
                });
            }
        } else {
            this.employeeTemplates = [];
            if (!this.copyFromEmployeeId)
                deferral.resolve();
            else {
                this.loadTemplateHeadsForOneEmployee(this.copyFromEmployeeId).then(templates => {
                    this.employeeTemplates = templates;
                    this.copyFromTemplateHeadId = 0;
                    deferral.resolve();
                });
            }
        }

        return deferral.promise;
    }

    private loadTemplateHeadsForOneEmployee(employeeId: number): ng.IPromise<TimeScheduleTemplateHeadSmallDTO[]> {
        return this.sharedScheduleService.getTimeScheduleTemplateHeadsForEmployee(employeeId, null, null, false, this.useAccountHierarchy && this.useAccountingFromSourceSchedule).then(x => {
            // Convert to typed DTOs
            let templates: TimeScheduleTemplateHeadSmallDTO[] = x;

            _.forEach(templates, t => {
                t['description'] = '';

                if (t.startDate)
                    t['description'] = "{0}, ".format(t.startDate.toFormattedDate());

                t['description'] += "{0} {1}, {2}".format((t.noOfDays / 7).toString(), this.terms["common.weekshort"], t.name);

                if (t.accountName)
                    t['description'] += " ({0})".format(t.accountName);
            });

            // Add empty
            let emptyTemplate = new TimeScheduleTemplateHeadSmallDTO();
            emptyTemplate.timeScheduleTemplateHeadId = 0;
            emptyTemplate['description'] = '';
            emptyTemplate.noOfDays = this.nbrOfWeeks * 7;
            templates.splice(0, 0, emptyTemplate);

            return templates;
        });
    }

    private getEmployeeAccounts() {
        if (!this.useAccountHierarchy)
            return;

        return this.scheduleService.getEmployeeAccounts(this.employees.map(e => e.employeeId), this.dateFrom, this.dateFrom.addDays(this.nbrOfWeeks * 7)).then(x => {
            this.employeeAccounts = x;
            this.filterEmployees();
        });
    }

    // ACTIONS

    private validateSetCopyOnAll(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        if (_.filter(this.employees, e => e.copyFromTemplateHeadId).length > 0) {
            var keys: string[] = [
                "time.schedule.planning.createtemplateschedules.askcopyall.title",
                "time.schedule.planning.createtemplateschedules.askcopyall.message"
            ];

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialogEx(terms["time.schedule.planning.createtemplateschedules.askcopyall.title"], terms["time.schedule.planning.createtemplateschedules.askcopyall.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    deferral.resolve(true);
                }, (reason) => {
                    deferral.resolve(false);
                });
            });

        } else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private initSave() {
        this.executing = true;

        // Prepare selected employees
        var selectedEmployees = this.getSelectedEmployees();
        _.forEach(selectedEmployees, employee => {
            employee.status = this.terms["time.schedule.planning.createtemplateschedules.pending"];
            employee.isProcessed = false;
        });

        this.saveOneEmployee();
    }

    private saveOneEmployee() {
        var employee = this.getNextEmployee();
        if (employee) {
            this.validateSave(employee).then(passed => {
                employee['statusIcon'] = "far fa-spinner fa-pulse fa-fw";
                if (passed) {
                    this.save(employee).then(success => {
                        if (success) {
                            this.notifyParent(employee.employeeId);
                            employee['statusIcon'] = "fal fa-check okColor";
                        } else {
                            employee['statusIcon'] = "fal fa-exclamation-triangle errorColor";
                        }
                        employee.resultSuccess = success;
                        employee.isProcessed = true;
                        this.saveOneEmployee();
                    });
                } else {
                    employee['statusIcon'] = "fal fa-exclamation-circle warningColor";
                    employee.resultSuccess = false;
                    employee.isProcessed = true;
                    this.saveOneEmployee();
                }
            });
        } else {
            this.executing = false;
            this.notifyParent(0);
            this.soeGridOptions.clearSelectedRows();
        }
    }

    private validateSave(employee: TemplateScheduleEmployeeDTO): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        // Check if any placement exists on current start date
        this.scheduleService.hasEmployeeSchedule(employee.employeeId, employee.templateStartDate).then(result => {
            if (result) {
                var keys: string[] = [
                    "core.warning",
                    "time.schedule.planning.templateschedule.placementexistsendafter",
                    "time.schedule.planning.createtemplateschedules.cancelled.overlapping"
                ];

                this.translationService.translateMany(keys).then(terms => {
                    var modal = this.notificationService.showDialogEx(terms["core.warning"], "{0}\n\n{1}".format(employee.numberAndName, terms["time.schedule.planning.templateschedule.placementexistsendafter"]), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        if (val)
                            deferral.resolve(true);
                    }, (reason) => {
                        employee.resultError = employee.status = terms["time.schedule.planning.createtemplateschedules.cancelled.overlapping"];
                        deferral.resolve(false);
                    });
                });
            } else {
                deferral.resolve(true);
            }
        });

        return deferral.promise;
    }

    private save(employee: TemplateScheduleEmployeeDTO): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        this.scheduleService.saveTimeScheduleTemplateAndPlacement(true, false, null, [], 0, employee.nbrOfWeeks * 7, employee.templateStartDate, employee.templateStopDate, employee.templateFirstMondayOfCycle, null, null, this.dateFrom, false, false, false, false, employee.employeeId, employee.copyFromTemplateHeadId, this.useAccountingFromSourceSchedule).then(result => {
            if (result.success) {
                employee.status = this.terms["time.schedule.planning.createtemplateschedules.templatecreated"];
                deferral.resolve(true);
            } else {
                employee.status = "{0}. {1}".format(this.terms["time.schedule.planning.createtemplateschedules.templatenotcreated"], result.errorMessage);
                deferral.resolve(false);
            }
        }).catch(reason => {
            employee.status = "{0}. {1}".format(this.terms["time.schedule.planning.createtemplateschedules.templatenotcreated"], reason);
            deferral.resolve(false);
        });

        return deferral.promise;
    }

    // EVENTS

    private dateFromChanged() {
        this.$timeout(() => {
            _.forEach(this.employees, emp => {
                emp.templateStartDate = this.dateFrom;
                emp.templateFirstMondayOfCycle = emp.templateStartDate.beginningOfWeek();
            });
            this.getEmployeeAccounts();
        });
    }

    private dateToChanged() {
        this.$timeout(() => {
            _.forEach(this.employees, emp => {
                emp.templateStopDate = this.dateTo;
            });
        });
    }

    private nbrOfWeeksChanged() {
        this.$timeout(() => {
            _.forEach(this.employees, emp => {
                emp.nbrOfWeeks = this.nbrOfWeeks;
            });
            this.getEmployeeAccounts();
        });
    }

    private useAccountingFromSourceScheduleChanged() {
        this.$timeout(() => {
            if (this.copyFromEmployeeId)
                this.setSourceEmployeeId(this.copyFromEmployeeId);
        });
    }

    private rowEmployeeChanged(row: TemplateScheduleEmployeeDTO) {
        if (!row)
            return;

        this.$timeout(() => {
            row.templates = [];
            row.copyFromTemplateHeadId = undefined;
            row.copyFromTemplateHeadName = '';

            let employee = _.find(this.allEmployees, e => e.employeeId == row.copyFromEmployeeId);
            if (employee) {
                row.copyFromEmployeeName = employee.numberAndName;
                this.loadTemplateHeadsForEmployee(row);
            } else {
                row.copyFromEmployeeName = '';
            }
        });
    }

    private rowTemplateChanged(row: TemplateScheduleEmployeeDTO) {
        this.$timeout(() => {
            let template = _.find(row.templates, t => t.timeScheduleTemplateHeadId === row.copyFromTemplateHeadId);
            row.copyFromTemplateHeadName = template ? template['description'] : '';
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    // HELP-METHODS

    private setSourceEmployeeId(id: number) {
        this.validateSetCopyOnAll().then(passed => {
            if (passed) {
                var emp = _.find(this.allEmployees, e => e.employeeId === id);
                if (emp) {
                    let selectedEmployeeIds = this.soeGridOptions.getSelectedRows().map(r => r.employeeId);
                    this._copyFromEmployeeId = id;
                    this.loadTemplateHeadsForEmployee(null).then(() => {
                        _.forEach(this.employees, e => {
                            _.forEach(selectedEmployeeIds, selectedEmployeeId => {
                                if (selectedEmployeeId == e.employeeId) {
                                    e.copyFromEmployeeId = id;
                                    e.copyFromEmployeeName = emp.numberAndName;
                                    e.templates = this.employeeTemplates;
                                }
                            });
                        });
                        this.filterEmployees();
                    });
                } else {
                    this._copyFromEmployeeId = undefined;
                    _.forEach(this.employees, e => {
                        e.copyFromEmployeeId = undefined;
                        e.copyFromEmployeeName = '';
                        e.copyFromTemplateHeadId = 0;
                        e.templates = [];
                    });
                    this.filterEmployees();
                    this.employeeTemplates = [];
                    this.focusService.focusByName("ctrl_copyFromEmployeeId");
                }
                if (emp) this._copyFromEmployeeId = { id: emp.employeeId, numberAndName: emp.numberAndName };
            }
        });
    }

    private setSelectedSourceEmployeeIdOnRow(row: any) {
        if (row.isSelected) {
            if (this.copyFromEmployeeId && this.copyFromEmployeeId.id > 0) {
                var emp = _.find(this.allEmployees, e => e.employeeId === this.copyFromEmployeeId.id);
                row.entity.copyFromEmployeeName = emp.numberAndName;
                row.entity.copyFromEmployeeId = this.copyFromEmployeeId.id;
                row.entity.templates = this.employeeTemplates;
            }
        } else {
            if (this.copyFromEmployeeId && this.copyFromEmployeeId.id > 0) {
                row.entity.copyFromEmployeeId = undefined;
                row.entity.copyFromEmployeeName = '';
                row.entity.copyFromTemplateHeadId = 0;
                row.entity.templates = [];
            }
        }
    }

    private setSourceTemplateId(id: number) {
        var head = _.find(this.employeeTemplates, t => t.timeScheduleTemplateHeadId === id);
        if (head) {
            let selectedEmployeeIds = this.soeGridOptions.getSelectedRows().map(r => r.employeeId);
            this._copyFromTemplateHeadId = id;
            this.nbrOfWeeks = head.noOfDays / 7;

            _.forEach(this.employees, e => {
                _.forEach(selectedEmployeeIds, selectedEmployeeId => {
                    if (selectedEmployeeId == e.employeeId) {
                        e.copyFromTemplateHeadId = id;
                        e.copyFromTemplateHeadName = head['description'];
                        e.nbrOfWeeks = this.nbrOfWeeks;
                    }
                });
            });

            this.getEmployeeAccounts();
        } else {
            this._copyFromTemplateHeadId = undefined;
            _.forEach(this.employees, e => {
                e.copyFromTemplateHeadId = undefined;
                e.copyFromTemplateHeadName = '';
            });
        }
    }

    private setSelectedSourceTemplateIdOnRow(row: any) {
        if (row.isSelected) {
            if (this.copyFromTemplateHeadId && this.copyFromTemplateHeadId > 0) {
                var head = _.find(this.employeeTemplates, t => t.timeScheduleTemplateHeadId === this.copyFromTemplateHeadId);
                row.entity.copyFromTemplateHeadId = this.copyFromTemplateHeadId;
                row.entity.copyFromTemplateHeadName = head['description'];
                row.entity.nbrOfWeeks = this.nbrOfWeeks;
            }
        } else {
            if (this.copyFromTemplateHeadId && this.copyFromTemplateHeadId > 0) {
                row.entity.copyFromTemplateHeadId = undefined;
                row.entity.copyFromTemplateHeadName = '';
            }
        }
    }

    private filterEmployees() {
        if (this.useAccountHierarchy) {
            let accountId = 0;
            if (this.copyFromTemplateHeadId) {
                let head = _.find(this.employeeTemplates, t => t.timeScheduleTemplateHeadId === this.copyFromTemplateHeadId);
                if (head && head.accountId)
                    accountId = head.accountId;
            }

            let employeeIds: number[];
            if (accountId)
                employeeIds = _.uniq(_.filter(this.employeeAccounts, e => e.accountId === accountId).map(e => e.employeeId));
            else
                employeeIds = _.uniq(this.employeeAccounts.map(e => e.employeeId));

            let emps = _.filter(this.employees, e => _.includes(employeeIds, e.employeeId));

            this.soeGridOptions.setData(emps);
        } else {
            this.soeGridOptions.refreshRows();
        }
    }

    private setLoadingTemplates() {
        this.loadingTemplates = _.filter(this.employees, e => e.loadingTemplates).length > 0;
    }

    private getSelectedEmployees() {
        var ids: number[] = this.soeGridOptions.getSelectedIds('employeeId');
        return _.filter(this.employees, e => _.includes(ids, e.employeeId));
    }

    private getNextEmployee() {
        var selectedEmployees = this.getSelectedEmployees();
        return _.find(selectedEmployees, e => !e.isProcessed);
    }

    private notifyParent(employeeId: number) {
        // When loop is finished, employeeId 0 is passed

        if (employeeId)
            this.generatedEmployeeIds.push(employeeId);

        if (this.generatedEmployeeIds.length > 4 || employeeId === 0) {
            // Raise event to reload employee in schedule planning
            var employeeIds: number[] = _.pullAt(this.generatedEmployeeIds, [0, 1, 2, 3, 4]).filter(Number);
            if (employeeIds.length > 0)
                this.messagingService.publish(Constants.EVENT_RELOAD_TEMPLATE_SCHEDULES, employeeIds);
        }
    }

    private showStatusDialog(employee: TemplateScheduleEmployeeDTO) {
        this.notificationService.showDialogEx(this.terms["common.status"], employee.status, SOEMessageBoxImage.Information);
    }
}
