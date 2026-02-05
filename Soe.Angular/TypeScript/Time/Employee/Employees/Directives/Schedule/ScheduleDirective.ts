import { IUrlHelperService, UrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { CoreService } from "../../../../../Core/Services/CoreService";
import { TranslationService } from "../../../../../Core/Services/TranslationService";
import { IScheduleService as ISharedScheduleService } from "../../../../../Shared/Time/Schedule/ScheduleService";
import { EditController as TemplateEditController } from "../../../../Schedule/Templates/EditController";
import { TimeScheduleTemplateGroupDTO, TimeScheduleTemplateGroupEmployeeDTO, TimeScheduleTemplateHeadRangeDTO, TimeScheduleTemplateHeadSmallDTO } from "../../../../../Common/Models/TimeScheduleTemplateDTOs";
import { ShiftDTO, TemplateScheduleShiftDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { Guid } from "../../../../../Util/StringUtility";
import { Constants } from "../../../../../Util/Constants";
import { Feature } from "../../../../../Util/CommonEnumerations";
import { IScheduleService } from "../../../../Schedule/ScheduleService";
import { ActivateScheduleControlDTO, ActivateScheduleGridDTO } from "../../../../../Common/Models/EmployeeScheduleDTOs";
import { EditPlacementDialogController } from "./EditPlacementDialogController";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IProgressHandler, WorkProgressCompletion } from "../../../../../Core/Handlers/ProgressHandler";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { EditTemplateGroupDialogController } from "./EditTemplateGroupDialogController";
import { ActiveScheduleControlDialogController } from "../../../../Dialogs/ActiveScheduleControl/ActiveScheduleControlDialogController";

export class ScheduleDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/Schedule/Views/Schedule.html'),
            scope: {
                employeeId: '=',
                employeeTemplateGroups: '=',
                parentGuid: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: ScheduleController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class ScheduleController {

    // Init parameters
    private employeeId: number;
    private employeeTemplateGroups: TimeScheduleTemplateGroupEmployeeDTO[];
    private parentGuid: Guid;
    private readOnly: boolean;
    private onChange: Function;

    // Terms
    private terms: { [index: string]: string; };

    // Permissions
    private editPlacementPermission: boolean = false;
    private editTemplatePermission: boolean = false;

    // Data
    private placements: ActivateScheduleGridDTO[] = [];
    private templateSchedules: TimeScheduleTemplateHeadSmallDTO[] = [];
    private templateShifts: TemplateScheduleShiftDTO[] = [];
    private templateGroups: ISmallGenericType[] = [];
    private simulatedHeads: TimeScheduleTemplateHeadRangeDTO[] = [];

    // Properties
    private _selectedTemplateHead: TimeScheduleTemplateHeadSmallDTO;
    private set selectedTemplateHead(head: TimeScheduleTemplateHeadSmallDTO) {
        this._selectedTemplateHead = head;
        this.loadTemplateHead();
    }
    private get selectedTemplateHead(): TimeScheduleTemplateHeadSmallDTO {
        return this._selectedTemplateHead;
    }

    private selectedTemplateGroupId: number = 0;

    // Flags
    private loadingPlacements: boolean = false;
    private loadingTemplates: boolean = false;
    private loadingTemplate: boolean = false;
    private templateGroupsModified: boolean = false;

    private progress: IProgressHandler;
    private modalInstance: any;

    //@ngInject
    constructor(
        $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private coreService: CoreService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private urlHelperService: UrlHelperService,
        private translationService: TranslationService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.modalInstance = $uibModal;
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions()
        ]).then(() => {
        });

        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.employeeId, (newVal, oldVal) => {
            if (oldVal !== newVal) {
                this.templateGroupsModified = false;
                this.simulatedHeads = [];
                this.loadPlacements();
                this.loadTemplateHeads();
            }
        });

        this.$scope.$on('employeeScheduleInitialLoad', (e, a) => {
            if (a && a.guid && a.guid === this.parentGuid) {
                this.loadPlacements();
                this.loadTemplateHeads();
                this.loadAllTemplateGroups();
            }
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.weekshort",
            "time.employee.employee.placement.templates.ispublic",
            "time.employee.employee.placement.templates.ispersonal",
            "time.schedule.planning.templateschedule"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        features.push(Feature.Time_Schedule_Placement);
        features.push(Feature.Time_Schedule_Templates_Edit);

        return this.coreService.hasModifyPermissions(features).then(x => {
            this.editPlacementPermission = x[Feature.Time_Schedule_Placement]
            this.editTemplatePermission = x[Feature.Time_Schedule_Templates_Edit]
        });
    }

    private loadPlacements() {
        this.loadingPlacements = true;

        const fromDate = new Date();
        fromDate.setFullYear(fromDate.getFullYear() - 1);
        const toDate = new Date();
        toDate.setFullYear(toDate.getFullYear() + 1);

        return this.scheduleService.getPlacementsForGrid(false, false, [this.employeeId], fromDate, toDate)
            .then(x => {
                this.placements = x;
                this.loadingPlacements = false;
            });
    }

    private loadTemplateHeads(selectCurrent: boolean = true): ng.IPromise<any> {
        this.loadingTemplates = true;
        this.templateSchedules = [];
        this.templateShifts = [];

        return this.sharedScheduleService.getTimeScheduleTemplateHeadsForEmployee(this.employeeId, null, null, false, false, true).then(x => {
            this.templateSchedules = x;            
           
            _.forEach(this.templateSchedules, t => {
                t['description'] = '';
             
                if (t.startDate)
                    t['description'] = "{0}, ".format(t.startDate.toFormattedDate(CoreUtility.languageDateFormat));

                t['description'] += "{0} {1}, {2}".format((t.noOfDays / 7).toString(), this.terms["common.weekshort"], t.name);

                if (t.accountName)
                    t['description'] += " ({0})".format(t.accountName);

                if (t.simpleSchedule)
                    t['description'] += " ({0})".format(t.employeeId ? this.terms["time.employee.employee.placement.templates.ispersonal"] : this.terms["time.employee.employee.placement.templates.ispublic"]);
                else
                    t['description'] += " ({0})".format(this.terms["time.schedule.planning.templateschedule"]);
            });

            // Select current template
            if (selectCurrent) {
                let headId = 0;
                let today: Date = CalendarUtility.getDateToday();
                _.forEach(_.orderBy(_.filter(this.templateSchedules, t => t.startDate), t => t.startDate), tmpl => {
                    if (headId && tmpl.startDate.isAfterOnDay(today))
                        return false;

                    headId = tmpl.timeScheduleTemplateHeadId;
                });

                if (headId)
                    this.selectedTemplateHead = _.find(this.templateSchedules, t => t.timeScheduleTemplateHeadId == headId);
            }
            this.loadingTemplates = false;
        });
    }

    private loadTemplateHead() {
        if (!this.selectedTemplateHead)
            return;

        this.loadingTemplate = true;
        this.templateShifts = [];
        let today: Date = CalendarUtility.getDateToday();

        if (this.selectedTemplateHead.noOfDays === 0)
            this.selectedTemplateHead.noOfDays = 7;

        this.sharedScheduleService.getTimeScheduleTemplateHeadForEmployee(today.beginningOfWeek(), today.endOfWeek(), this.selectedTemplateHead.timeScheduleTemplateHeadId).then(x => {
            let shifts = x.map(s => {
                let obj = new ShiftDTO();
                angular.extend(obj, s);
                obj.fixDates();
                obj.startTime = obj.actualStartTime;
                obj.stopTime = obj.actualStopTime;

                // Correct dates so DayNumber 1 is the same as start date of template
                var offset: number = today.beginningOfWeek().addDays(obj.dayNumber - 1).diffDays(obj.startTime.date());
                obj.startTime = obj.startTime.addDays(offset);
                obj.actualStartTime = obj.actualStartTime.addDays(offset);
                obj.stopTime = obj.stopTime.addDays(offset);
                obj.actualStopTime = obj.actualStopTime.addDays(offset);
                if (obj.break1TimeCodeId)
                    obj.break1StartTime = obj.break1StartTime.addDays(offset);
                if (obj.break2TimeCodeId)
                    obj.break2StartTime = obj.break2StartTime.addDays(offset);
                if (obj.break3TimeCodeId)
                    obj.break3StartTime = obj.break3StartTime.addDays(offset);
                if (obj.break4TimeCodeId)
                    obj.break4StartTime = obj.break4StartTime.addDays(offset);

                return obj;
            });

            // Get start date from template
            let startDayNumber: number = this.getTemplateScheduleDayNumber();
            let startDate = today.beginningOfWeek().addDays(-(startDayNumber - 1)).beginningOfWeek();

            // Group shifts on day number (one row per day)
            // New and modified shifts
            _.forEach(_.sortBy(_.map(_.uniqBy(shifts, s => s.dayNumber), s => s.dayNumber)), dayNumber => {
                this.templateShifts.push(TemplateScheduleShiftDTO.convertShiftsToDTO(_.filter(shifts, s => s.dayNumber === dayNumber), startDate, this.selectedTemplateHead.noOfDays));
            });

            // Fill up with empty shifts
            for (var i = 0; i < this.selectedTemplateHead.noOfDays; i++) {
                if (_.filter(shifts, s => s.dayNumber === i + 1).length === 0) {
                    var date = startDate.addDays(i);
                    var dto = new TemplateScheduleShiftDTO();
                    dto.startTime = date;
                    dto.stopTime = date;
                    dto.dayOfWeek = date.dayOfWeek();
                    dto.weekNbr = Math.floor(i / 7) + 1;
                    this.templateShifts.push(dto);
                }
            }

            _.forEach(this.templateShifts, shift => {
                shift['sortDayOfWeek'] = shift.startTime.sortOnMonday();
            });

            this.templateShifts = _.orderBy(this.templateShifts, ['weekNbr', 'sortDayOfWeek', 'startTime']);

            this.loadingTemplate = false;
        });
    }

    private loadAllTemplateGroups(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTemplateGroupsDict(true, true).then(x => {
            this.templateGroups = x;
        });
    }

    private getOverlappingTemplates(date: Date): ng.IPromise<string[]> {
        return this.sharedScheduleService.getOverlappingTemplates(this.employeeId, date);
    }

    private simulate() {
        this.simulatedHeads = [];
        this.scheduleService.getTimeScheduleTemplateHeadsRangeForEmployee(this.employeeId, CalendarUtility.getDateToday(), CalendarUtility.getDateToday().addYears(5)).then(x => {
            this.simulatedHeads = x.heads;
        });
    }

    // EVENTS

    private newPlacement() {
        this.openEditPlacement(null, EditPlacementMode.New);
    }

    private shortenPlacement(placement: ActivateScheduleGridDTO) {
        this.openEditPlacement(placement, EditPlacementMode.Shorten);
    }

    private extendPlacement(placement: ActivateScheduleGridDTO) {
        this.openEditPlacement(placement, EditPlacementMode.Extend);
    }

    private initDeletePlacement(item: ActivateScheduleGridDTO) {
        var keys: string[] = [
            "time.schedule.activate.delete.message",
            "time.schedule.activate.delete.message.nocheck",
            "time.schedule.activate.delete.message.nocheck.info",
            "time.recalculatetimestatus.activateschedulecontrol",
        ];

        this.translationService.translateMany(keys).then(deleteTerms => {
            var msg: string = deleteTerms["time.schedule.activate.delete.message"]
            if (CoreUtility.isSupportAdmin)
                msg += "\n\n" + deleteTerms["time.schedule.activate.delete.message.nocheck.info"]

            this.progress.startWorkProgress((completion) => {
                var modal = this.notificationService.showDialogEx(this.terms["time.schedule.activate.delete"], msg, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel, { showCheckBox: CoreUtility.isSupportAdmin, checkBoxLabel: deleteTerms["time.schedule.activate.delete.message.nocheck"] });
                modal.result.then(result => {
                    if (result) {
                        var items: ActivateScheduleGridDTO[] = [];
                        items.push(item);

                        this.scheduleService.controlActivations(items, null, null, true).then(control => {
                            control.discardCheckesAll = !!result.isChecked;
                            if (!control.hasWarnings) {
                                this.deletePlacement(control, item, completion);
                            }
                            else {
                                var modalControl = this.modalInstance.open({
                                    templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/ActiveScheduleControl/Views/ActiveScheduleControlDialog.html"),
                                    controller: ActiveScheduleControlDialogController,
                                    controllerAs: 'ctrl',
                                    bindToController: true,
                                    backdrop: 'static',
                                    keyboard: true,
                                    size: 'xl',
                                    windowClass: 'fullsize-modal',
                                    scope: this.$scope,
                                    resolve: {
                                        control: () => { return control; },
                                        activateDate: () => { return item.employeeScheduleStartDate.addDays(-1); }
                                    }
                                });

                                modalControl.result.then(val => {
                                    control.createResult();
                                    this.deletePlacement(control, item, completion);
                                }, (reason) => {
                                    // Cancelled
                                    completion.completed(null, true);
                                });
                            }
                        });

                    }
                }, (reason) => {
                    // Cancelled
                    completion.completed(null, true);
                });
            }, null, deleteTerms["time.recalculatetimestatus.activateschedulecontrol"]);
        });
    }

    private deletePlacement(control: ActivateScheduleControlDTO, item: ActivateScheduleGridDTO, completion: WorkProgressCompletion) {
        return this.scheduleService.deleteEmployeeSchedule(control, item).then(result => {
            if (result.success) {
                completion.completed(result, true);
                this.loadPlacements();
            } else {
                completion.failed(result.errorMessage);
            }
        });
    }

    private openEditPlacement(placement: ActivateScheduleGridDTO, mode: EditPlacementMode) {

        let startDate: Date;
        if (!placement && this.placements.length > 0) {
            startDate = this.placements[0].employeeScheduleStopDate.addDays(1);
        }

        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/Schedule/Views/EditPlacementDialog.html"),
            controller: EditPlacementDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                personalTemplateSchedules: () => { return _.filter(this.templateSchedules, t => t.employeeId === this.employeeId) },
                selectedTemplateHead: () => { return this.selectedTemplateHead },
                employeeId: () => { return this.employeeId },
                startDate: () => { return startDate },
                placement: () => { return placement },
                mode: () => { return mode }
            }
        }

        let modal = this.modalInstance.open(options);

        modal.result.then(result => {
            if (result)
                this.loadPlacements();
        });
    }

    private openEditTemplateSchedule(timeScheduleTemplateHeadId: number) {
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
                source: 'employeeTemplate',
                modal: modal,
                id: timeScheduleTemplateHeadId,
                employeeId: this.employeeId
            });
        });

        modal.result.then(result => {
            if (result.modified) {
                if (timeScheduleTemplateHeadId && !result.deleted)
                    this.loadTemplateHead();
                else {
                    if (result.deleted)
                        result.id = 0;
                    this.loadTemplateHeads(!result.id).then(() => {
                        if (result.id)
                            this.selectedTemplateHead = this.templateSchedules.find(t => t.timeScheduleTemplateHeadId == result.id);
                    });
                }
            }
        });
    }

    private addTemplateGroup() {
        this.$timeout(() => {
            if (!this.selectedTemplateGroupId)
                return;

            this.openEditTemplateGroup(null);
        });
    }

    private openEditTemplateGroup(empGroup: TimeScheduleTemplateGroupEmployeeDTO) {
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/Schedule/Views/EditTemplateGroupDialog.html"),
            controller: EditTemplateGroupDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                templateGroup: () => { return empGroup },
                initialFromDate: () => { return CalendarUtility.getDateToday() }    // TODO: Set date from employment?
            }
        }

        this.modalInstance.open(options).result.then(result => {
            if (result && result.templateGroup) {
                if (!empGroup) {
                    let group = this.templateGroups.find(t => t.id === this.selectedTemplateGroupId);
                    if (group) {
                        // Create a complete group object to be able to set some display properties
                        let tmpGroup = new TimeScheduleTemplateGroupDTO();
                        tmpGroup.name = group.name;

                        empGroup = new TimeScheduleTemplateGroupEmployeeDTO();
                        empGroup.timeScheduleTemplateGroupId = this.selectedTemplateGroupId;
                        empGroup.group = tmpGroup;

                        if (!this.employeeTemplateGroups)
                            this.employeeTemplateGroups = [];
                        this.employeeTemplateGroups.splice(0, 0, empGroup);
                    }
                }

                empGroup.fromDate = result.templateGroup.fromDate;
                empGroup.toDate = result.templateGroup.toDate;

                this.validateTemplateGroup(empGroup);
                this.templateGroupsModified = true;
                this.simulatedHeads = [];
                if (this.onChange)
                    this.onChange();
            }

            this.selectedTemplateGroupId = 0;
        });
    }

    private deleteTemplateGroup(empGroup: TimeScheduleTemplateGroupEmployeeDTO) {
        _.pull(this.employeeTemplateGroups, empGroup);

        this.templateGroupsModified = true;
        this.simulatedHeads = [];
        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private getTemplateScheduleDayNumber(): number {
        // Get number of days that has passed from schedule start to specified date
        var daysPassed: number = this.selectedTemplateHead.startDate ? this.selectedTemplateHead.startDate.date().diffDays(CalendarUtility.getDateToday().beginningOfWeek().date()) : 0;
        // Add schedule's start day
        daysPassed += 1;
        if (this.selectedTemplateHead.noOfDays > 0) {
            // If daysPassed is larger than total number of days in template, decrease it with one complete period length until it gets below.
            while (daysPassed > this.selectedTemplateHead.noOfDays) {
                daysPassed -= this.selectedTemplateHead.noOfDays;
            }
        }

        return daysPassed;
    }

    // VALIDATION

    private validateTemplateGroup(empGroup: TimeScheduleTemplateGroupEmployeeDTO) {
        if (!empGroup.fromDate)
            return;

        this.getOverlappingTemplates(empGroup.fromDate).then(x => {
            let msg: string = x.join("\n");
            if (msg) {
                let keys: string[] = [
                    "time.employee.employee.overlappingtemplates.title",
                    "time.employee.employee.overlappingtemplates.row1",
                    "time.employee.employee.overlappingtemplates.row2",
                    "time.employee.employee.overlappingtemplates.row3"
                ];
                this.translationService.translateMany(keys).then(terms => {
                    msg = "{0}\n\n{1}\n{2}\n\n{3}".format(terms["time.employee.employee.overlappingtemplates.row1"], terms["time.employee.employee.overlappingtemplates.row2"], terms["time.employee.employee.overlappingtemplates.row3"], msg);
                    this.notificationService.showDialogEx(terms["time.employee.employee.overlappingtemplates.title"], msg, SOEMessageBoxImage.Warning);
                });
            }
        });
    }
}

export enum EditPlacementMode {
    New = 0,
    Shorten = 1,
    Extend = 2,
    Delete = 3
}