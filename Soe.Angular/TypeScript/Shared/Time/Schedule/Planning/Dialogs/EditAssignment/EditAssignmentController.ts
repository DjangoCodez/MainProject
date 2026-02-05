import { ShiftDTO, AvailableTimeDTO, OrderListDTO } from "../../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { EmployeeListDTO } from "../../../../../../Common/Models/EmployeeListDTO";
import { ICoreService } from "../../../../../../Core/Services/CoreService";
import { IScheduleService as ISharedScheduleService } from "../../../ScheduleService";
import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../../../Core/Services/UrlHelperService";
import { EditAssignmentFunctions, SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize, AbsenceRequestViewMode, AbsenceRequestGuiMode, AbsenceRequestParentMode } from "../../../../../../Util/Enumerations";
import { SoeScheduleWorkRules, TermGroup_ShiftHistoryType, TermGroup_MessageType, SoeTimeAttestFunctionOption, TermGroup_TimeScheduleTemplateBlockType, DragShiftAction, TermGroup_AssignmentTimeAdjustmentType, TermGroup, XEMailType } from "../../../../../../Util/CommonEnumerations";
import { AttestEmployeeDaySmallDTO } from "../../../../../../Common/Models/TimeEmployeeTreeDTO";
import { SplitShiftController } from "../SplitShift/SplitShiftController";
import { EditController as MessageEditController } from "../../../../../../Core/RightMenu/MessageMenu/EditController";
import { EditController as AbsenceRequestsEditController } from "../../../Absencerequests/EditController";
import { Constants } from "../../../../../../Util/Constants";
import { ShiftAccountingController } from "../ShiftAccounting/ShiftAccountingController";
import { CalendarUtility } from "../../../../../../Util/CalendarUtility";
import { ITimeScheduleShiftQueueDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { ShiftHistoryController } from "../ShiftHistory/ShiftHistoryController";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";

export class EditAssignmentController {

    // Data
    private scheduleShifts: ShiftDTO[] = [];
    private existingShifts: ShiftDTO[] = [];
    private shiftsToSave: ShiftDTO[] = [];
    private selectedQueue: ITimeScheduleShiftQueueDTO;
    private timeAdjustmentTypes: SmallGenericType[];

    // Terms
    private terms: any = [];
    private shiftDefined: string;
    private shiftUndefined: string;
    private shiftsDefined: string;
    private shiftsUndefined: string;
    private queueTitle: string;

    // Flags
    private populating: boolean = false;
    private executing: boolean = false;
    private createMultipleShifts: boolean = false;
    private outsideSchedule: boolean = false;

    // Skills
    private shiftTypeIds: number[] = [];
    private _invalidSkills: boolean = false;
    get invalidSkills(): boolean {
        return this._invalidSkills;
    }
    set invalidSkills(value: boolean) {
        this._invalidSkills = value;
        if (value) {
            this.skillsOpen = true;
        }
    }
    private skillsOpen: boolean = false;
    private skillsOpened() {
        this.skillsOpen = true;
    }
    private ignoreSkillEmployeeIds: number[] = [];

    get employeeIdForSkills(): number {
        if (this.selectedQueue)
            return this.selectedQueue.employeeId;
        else if (this.selectedEmployee)
            return this.selectedEmployee.employeeId;
        else
            return 0;
    }

    // Functions
    private functions: any = [];

    // Properties
    private get customerInfo(): string {
        return this.shift.order ? "({0}) {1}".format(this.shift.order.customerNr, this.shift.order.customerName) : '';
    }

    private get projectInfo(): string {
        return this.shift.order ? "({0}) {1}".format(this.shift.order.projectNr, this.shift.order.projectName) : '';
    }

    private get plannedInterval(): string {
        return this.shift.order ? "{0} - {1}".format(this.shift.order.plannedStartDate ? this.shift.order.plannedStartDate.toFormattedDate() : '', this.shift.order.plannedStopDate ? this.shift.order.plannedStopDate.toFormattedDate() : '') : '';
    }

    private _selectedEmployee: EmployeeListDTO;
    private get selectedEmployee() {
        return this._selectedEmployee;
    }
    private set selectedEmployee(item: EmployeeListDTO) {
        this._selectedEmployee = item;

        this.shift.employeeId = item ? item.employeeId : 0;
        this.shift.employeeName = item ? item.name : '';

        if (this.timeAdjustmentType === TermGroup_AssignmentTimeAdjustmentType.FillToEndDate)
            this.loadAvailableTimeForEmployee();
        else
            this.loadSchedule();
    }

    private _start: Date;
    private get start(): Date {
        return this._start;
    }
    private set start(date: Date) {
        if (this._start && date && date.isSameMinuteAs(this._start))
            return;

        let calculate: boolean = true;

        let changingDate: boolean = !!((this._start && !date) || (!this._start && date) || (this._start && !this._start.isSameDayAs(date)));

        this._start = date;
        this.shift.startTime = this.shift.actualStartTime = date;

        if (changingDate && this.isMultipleDays) {
            this.loadScheduleStart(!this.populating);
            calculate = false;
        }

        if (this.timeAdjustmentType === TermGroup_AssignmentTimeAdjustmentType.FillToEndDate) {
            this.loadAvailableTimeForEmployee();
            calculate = false;
        } else {
            if (changingDate) {
                this.loadSchedule();
                calculate = false;
            }
        }

        if (calculate)
            this.calculateDurations();
    }

    private _stop: Date;
    private get stop(): Date {
        return this._stop;
    }
    private set stop(date: Date) {
        if (this._stop && date && date.isSameMinuteAs(this._stop))
            return;

        let calculate: boolean = true;

        let changingDate: boolean = !!((this._stop && !date) || (!this._stop && date) || (this._stop && !this._stop.isSameDayAs(date)));

        this._stop = date;
        this.shift.stopTime = this.shift.actualStopTime = date;

        if (changingDate && this.isMultipleDays) {
            this.loadScheduleStop(!this.populating);
            calculate = false;
        }

        if (!this.isMultipleDays && this.stop && this.start && this.stop.diffDays(this.start) >= 1) {
            this.timeAdjustmentType = TermGroup_AssignmentTimeAdjustmentType.FillToEndDate;
            if (changingDate) {
                this.loadScheduleStop(!this.populating);
                calculate = false;
            }
        } else if (this.timeAdjustmentType === TermGroup_AssignmentTimeAdjustmentType.FillToEndDate) {
            this.loadAvailableTimeForEmployee();
            calculate = false;
        }

        if (calculate)
            this.calculateDurations();
    }

    private _timeAdjustmentType: TermGroup_AssignmentTimeAdjustmentType;
    private get timeAdjustmentType(): TermGroup_AssignmentTimeAdjustmentType {
        return this._timeAdjustmentType;
    }
    private set timeAdjustmentType(type: TermGroup_AssignmentTimeAdjustmentType) {
        this._timeAdjustmentType = type;

        if (!this.isMultipleDays) {
            this.stop = this.start.mergeTime(this.stop);
            this.loadSchedule();
        } else if (this.timeAdjustmentType === TermGroup_AssignmentTimeAdjustmentType.FillToEndDate)
            this.loadAvailableTimeForEmployee();
        else
            this.calculateDurations();
    }

    private get isMultipleDays(): boolean {
        return (this.timeAdjustmentType !== TermGroup_AssignmentTimeAdjustmentType.OneDay);
    }

    private originalLength: number = 0;
    private plannedTime: number = 0;
    private plannedTimeDate: Date;
    private remainingTimeToBe: number = 0;
    private availableTimeItem: AvailableTimeDTO = new AvailableTimeDTO();

    //@ngInject
    constructor(private $uibModalInstance,
        private $uibModal,
        private coreService: ICoreService,
        private sharedScheduleService: ISharedScheduleService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private currentEmployeeId: number,
        private isReadonly: boolean,
        private modifyPermission: boolean,
        private shift: ShiftDTO,
        private date: Date,
        private employeeId: number,
        private shiftTypes: any[],
        private employees: EmployeeListDTO[],
        private hiddenEmployeeId: number,
        private vacantEmployeeIds: number[],
        private showSkills: boolean,
        private dayStartTime: number,
        private dayEndTime: number,
        private showAvailability: boolean,
        private clockRounding: number,
        private shiftTypeMandatory: boolean,
        private keepShiftsTogether: boolean,
        private skillCantBeOverridden: boolean,
        private skipWorkRules: boolean,
        private skipXEMailOnChanges: boolean,
        private ignoreScheduledBreaksOnAssignment: boolean) {

        this.setup();

        if (this.shift)
            this.populate();
    }

    // SETUP

    private setup() {
        if (!this.modifyPermission)
            this.isReadonly = true;

        // Setup employees to ignore skill matching on
        this.ignoreSkillEmployeeIds.push(this.hiddenEmployeeId);
        _.forEach(this.vacantEmployeeIds, vacantEmployeeId => {
            this.ignoreSkillEmployeeIds.push(vacantEmployeeId);
        });

        this.$q.all([
            this.loadTerms(),
            this.loadTimeAdjustmentTypes()
        ]).then(() => {
            this.setupFunctions();
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.shift.order.remainingTime, () => {
            this.calculateDurations();
        });

        this.messagingService.subscribe(Constants.EVENT_ASSIGN_EMPLOYEE_FROM_QUEUE, (params) => {
            if (this.shift && this.shift.timeScheduleTemplateBlockId === params.timeScheduleTemplateBlockId) {
                // Validate skills
                this.validateSkills().then(passed => {
                    if (passed)
                        this.performDragAction(params.employeeId);
                });
            }
        }, this.$scope);

        this.messagingService.subscribe('editOrderDone', (order: OrderListDTO) => {
            if (order.orderId == this.shift.order.orderId) {
                this.shift.order = order;
            }
        }, this.$scope);
    }

    private setupFunctions() {
        this.functions.push({ id: EditAssignmentFunctions.EditOrder, name: this.terms["time.schedule.planning.editassignment.functions.editorder"], icon: "fal fa-fw fa-file-alt", hidden: () => { return !this.modifyPermission || this.isReadonly || !this.shift; }, disabled: () => { return !this.shift.order; } });
        this.functions.push({ id: EditAssignmentFunctions.SplitShift, name: this.terms["time.schedule.planning.editshift.functions.splitshift"].format(this.shiftUndefined), icon: "fal fa-fw fa-cut", hidden: () => { return !this.modifyPermission || this.isReadonly || !this.shift; }, disabled: () => { return !this.shift.timeScheduleTemplateBlockId; } });
        this.functions.push({ id: EditAssignmentFunctions.ShiftRequest, name: this.terms["time.schedule.planning.contextmenu.sendassignmentrequest"], icon: "fal fa-fw fa-envelope-o", hidden: () => { return !this.modifyPermission || this.isReadonly || !this.shift; }, disabled: () => { return !this.shift.timeScheduleTemplateBlockId; } });
        this.functions.push({ id: EditAssignmentFunctions.Absence, name: this.terms["time.schedule.planning.editshift.functions.absence"], icon: "fal fa-fw fa-medkit errorColor", hidden: () => { return !this.modifyPermission || this.isReadonly || !this.shift; }, disabled: () => { return !this.shift.timeScheduleTemplateBlockId; } });
        this.functions.push({ id: EditAssignmentFunctions.AbsenceRequest, name: this.terms["time.schedule.planning.editshift.functions.absencerequest"], icon: "fal fa-fw fa-plane errorColor", hidden: () => { return !this.shift || !this.shift.isAbsenceRequest; }, disabled: () => { return !this.shift.timeScheduleTemplateBlockId; } });
        this.functions.push({ id: EditAssignmentFunctions.RestoreToSchedule, name: this.terms["time.schedule.planning.editshift.functions.restoretoschedule"], icon: "fal fa-fw fa-undo warningColor", hidden: () => { return !this.modifyPermission || this.isReadonly || !this.shift; }, disabled: () => { return !this.shift.timeScheduleTemplateBlockId; } });
        this.functions.push({ id: EditAssignmentFunctions.History, name: this.terms["time.schedule.planning.editshift.functions.history"], icon: "fal fa-fw fa-calendar", hidden: () => { return !this.shift; }, disabled: () => { return !this.shift.timeScheduleTemplateBlockId; } });
        this.functions.push({ id: EditAssignmentFunctions.Accounting, name: this.terms["common.accounting"], icon: "fal fa-fw fa-table", hidden: () => { return !this.shift; }, disabled: () => { return !this.shift.timeScheduleTemplateBlockId; } });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.accounting",
            "common.obs",
            "common.order",
            "core.info",
            "core.or",
            "core.unabletosave",
            "error.default_error",
            "time.schedule.planning.breakprefix",
            "time.schedule.planning.hidden",
            "time.schedule.planning.shiftqueue.title",
            "time.schedule.planning.assignmentdefined",
            "time.schedule.planning.assignmentundefined",
            "time.schedule.planning.assignmentsdefined",
            "time.schedule.planning.assignmentsundefined",
            "time.schedule.planning.editshift.endbeforestart",
            "time.schedule.planning.editshift.employeemandatory",
            "time.schedule.planning.editshift.missingskills",
            "time.schedule.planning.editshift.missingskillsoverride",
            "time.schedule.planning.editshift.functions.splitshift",
            "time.schedule.planning.editshift.functions.absence",
            "time.schedule.planning.editshift.functions.absencerequest",
            "time.schedule.planning.editshift.functions.restoretoschedule",
            "time.schedule.planning.editshift.functions.restoretoschedule.message",
            "time.schedule.planning.editshift.functions.history",
            "time.schedule.planning.contextmenu.sendassignmentrequest",
            "time.schedule.planning.editassignment.missingdates",
            "time.schedule.planning.editassignment.shifttypemandatory",
            "time.schedule.planning.editassignment.remainingtimeafterthisnegative",
            "time.schedule.planning.editassignment.functions.editorder"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.shiftDefined = this.terms["time.schedule.planning.assignmentdefined"];
            this.shiftUndefined = this.terms["time.schedule.planning.assignmentundefined"];
            this.shiftsDefined = this.terms["time.schedule.planning.assignmentsdefined"];
            this.shiftsUndefined = this.terms["time.schedule.planning.assignmentsundefined"];
            this.queueTitle = this.terms["time.schedule.planning.shiftqueue.title"].format(this.shiftUndefined);
        });
    }

    private loadTimeAdjustmentTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AssignmentTimeAdjustmentType, false, false).then(x => {
            this.timeAdjustmentTypes = x;
            this.timeAdjustmentType = TermGroup_AssignmentTimeAdjustmentType.OneDay;
        });
    }

    private loadExistingShifts(): ng.IPromise<ShiftDTO[]> {
        return this.sharedScheduleService.getShiftsForDay(this.employeeId, this.date, [TermGroup_TimeScheduleTemplateBlockType.Order, TermGroup_TimeScheduleTemplateBlockType.Booking], false, false, null, false, false, false, true);
    }

    private loadScheduleStart(setStart: boolean) {
        if (!this.start)
            return;

        this.sharedScheduleService.getShiftsForDay(this.selectedEmployee.employeeId, this.start, [TermGroup_TimeScheduleTemplateBlockType.Schedule], false, false, null, false, false, false, true).then(shifts => {
            this.scheduleShifts = shifts;
            if (setStart)
                this.start = this.getScheduleStart();
            this.calculateDurations();
        });
    }

    private loadScheduleStop(setStop: boolean) {
        if (!this.stop)
            return;

        this.sharedScheduleService.getShiftsForDay(this.selectedEmployee.employeeId, this.stop, [TermGroup_TimeScheduleTemplateBlockType.Schedule], false, false, null, false, false, false, true).then(shifts => {
            this.scheduleShifts = shifts;
            if (setStop)
                this.stop = this.getScheduleStop();
            this.calculateDurations();
        });
    }

    private loadSchedule = _.debounce(() => {
        this.scheduleShifts = [];
        this.existingShifts = [];

        if (!this.selectedEmployee || !this.start) {
            this.populating = false;
            return;
        }

        this.sharedScheduleService.getShiftsForDay(this.selectedEmployee.employeeId, this.start, null, true, false, null, false, false, false, true).then(x => {
            let shifts: ShiftDTO[] = x;
            _.forEach(shifts, shift => {
                if (shift.isSchedule || shift.isStandby) {
                    this.scheduleShifts.push(shift);
                } else if (shift.isOrder || shift.isBooking) {
                    this.existingShifts.push(shift);
                }
            });

            // Create break shifts from break information on shift DTO
            let firstSchedule = this.scheduleShifts.length > 0 ? _.orderBy(this.scheduleShifts, s => s.actualStartTime)[0] : null;
            if (firstSchedule) {
                let breaks = firstSchedule.createBreaksFromShift();
                if (breaks.length > 0) {
                    _.forEach(breaks, breakShift => {
                        breakShift.shiftTypeName = this.terms["time.schedule.planning.breakprefix"];
                        this.scheduleShifts.push(breakShift);
                    });
                }
            }

            // Re-sort
            this.scheduleShifts = _.orderBy(this.scheduleShifts, ['actualStartTime', 'actualStopTime']);
            // In day view, keep the times seleced in the calendar
            // otherwise set times based on schedule
            //if (Shift.TimeScheduleTemplateBlockId == 0 && viewDefinition != TimeSchedulePlanning.ViewDefinitions.Day)
            if (!this.shift.timeScheduleTemplateBlockId)
                this.initSetTimesBasedOnSchedule();
            else
                this.calculateDurations(this.populating);

            this.populating = false;
        });
    }, 500, { leading: false, trailing: true });

    private loadAvailableTimeForEmployee() {
        if (!this.employeeId || !this.start || !this.stop)
            return;

        this.sharedScheduleService.getAvailableTime(this.employeeId, this.start, this.stop).then(x => {
            this.availableTimeItem = x;
            this.calculateDurations();
        });
    }

    // FUNCTIONS

    private executeFunction(option) {
        switch (option.id) {
            case EditAssignmentFunctions.EditOrder:
                this.openEditOrder();
                break;
            case EditAssignmentFunctions.SplitShift:
                this.openSplitShift();
                break;
            case EditAssignmentFunctions.ShiftRequest:
                this.openShiftRequestDialog();
                break;
            case EditAssignmentFunctions.Absence:
                this.openAbsenceDialog();
                break;
            case EditAssignmentFunctions.AbsenceRequest:
                this.openAbsenceRequestDialog();
                break;
            case EditAssignmentFunctions.RestoreToSchedule:
                this.restoreToSchedule();
                break;
            case EditAssignmentFunctions.History:
                this.openShiftHistory();
                break;
            case EditAssignmentFunctions.Accounting:
                this.openAccountingDialog();
                break;
        }
    }

    // ACTIONS

    private populate() {
        this.populating = true;

        // Null check
        if (!this.shift.shiftTypeId)
            this.shift.shiftTypeId = 0;
        this.selectedEmployee = _.find(this.employees, e => e.employeeId === this.employeeId);
        this.start = this.shift.actualStartTime;
        this.stop = this.shift.actualStopTime;
        // Remember shifts original length, to be able to calculate remaining time
        this.originalLength = this.shift.timeScheduleTemplateBlockId !== 0 && this.shift.actualStopTime && this.shift.actualStartTime ? this.shift.actualStopTime.diffMinutes(this.shift.actualStartTime) : 0;
        
        if (this.shift.plannedTime && this.shift.plannedTime > 0) {
            if (this.ignoreScheduledBreaksOnAssignment && this.shift.timeScheduleTemplateBlockId !== 0)
                this.originalLength = (this.originalLength - this.breakMinutesInIntervall(this.shift));

            this.plannedTime = this.shift.plannedTime;
            this.plannedTimeDate = new Date(1900, 0, 1, 0, this.shift.plannedTime);
        }
        else if (!this.ignoreScheduledBreaksOnAssignment && this.shift.shiftTypeId > 0 && this.shift.actualStopTime && this.shift.actualStartTime) {
            this.plannedTime = this.shift.actualStopTime.diffMinutes(this.shift.actualStartTime);
            this.plannedTimeDate = new Date(1900, 0, 1, 0, this.plannedTime);
        }

        this.setShiftTypeIds();
        this.loadSchedule();
    }

    private breakMinutesInIntervall(shift:ShiftDTO):number
    {
        let breakMinutes = 0;
        if (this.shift.break1Minutes && this.shift.break1StartTime && CalendarUtility.isRangesOverlapping(this.start, this.stop, shift.break1StartTime, shift.break1StartTime.addMinutes(this.shift.break1Minutes))) {
            breakMinutes += this.shift.break1Minutes;
        }
        if (this.shift.break2Minutes && this.shift.break2StartTime && CalendarUtility.isRangesOverlapping(this.start, this.stop, shift.break2StartTime, shift.break2StartTime.addMinutes(this.shift.break2Minutes))) {
            breakMinutes += this.shift.break2Minutes;
        }
        if (this.shift.break3Minutes && this.shift.break3StartTime && CalendarUtility.isRangesOverlapping(this.start, this.stop, shift.break3StartTime, shift.break3StartTime.addMinutes(this.shift.break3Minutes))) {
            breakMinutes += this.shift.break3Minutes;
        }
        if (this.shift.break4Minutes && this.shift.break4StartTime && CalendarUtility.isRangesOverlapping(this.start, this.stop, shift.break4StartTime, shift.break4StartTime.addMinutes(this.shift.break4Minutes))) {
            breakMinutes += this.shift.break4Minutes;
        }
        
        return breakMinutes;
    }
    // EVENTS

    private keepAsPlannedChanged() {
        this.$timeout(() => {
            // Save option to order directly
            this.sharedScheduleService.setOrderKeepAsPlanned(this.shift.order.orderId, this.shift.order.keepAsPlanned).then(result => {
                // TODO: Show error?
            })
        });
    }

    private shiftTypeChanged(shift: ShiftDTO) {
        this.$timeout(() => {
            this.setShiftTypeIds();
        });
    }

    private timeChanged() {
        this.calculateDurations();
    }

    private plannedTimeChanged() {
        this.plannedTime = CalendarUtility.timeSpanToMinutes(this.plannedTimeDate.getHours().toString() + ':' + this.plannedTimeDate.getMinutes().toString());
        this.calculateDurations(true);
    }

    private cancel(employeeIds: number[] = []) {
        if (employeeIds.length === 0) {
            if (this.employeeId)
                employeeIds.push(this.employeeId);
            if (this.shift && this.shift.employeeId && this.shift.employeeId !== this.employeeId)
                employeeIds.push(this.shift.employeeId);
        }

        this.$uibModalInstance.close({ reload: employeeIds.length > 0, reloadEmployeeIds: employeeIds });
    }

    private save() {
        this.executing = true;

        this.initSave().then(val => {
            if (val) {
                var toDate: Date = undefined;
                if (this.shift.isOrder && this.shift.order && this.shift.order.hasPlannedStopDate) {
                    toDate = this.shift.order.plannedStopDate;
                    toDate.setHours(this.stop.getHours())
                }
                else {
                    toDate = this.stop;
                }
                this.askSave(toDate).then(passed => {
                    if (!passed)
                        this.executing = false;
                    else {
                        if (this.isMultipleDays) {
                            // Use specific save for multiple days
                            this.$uibModalInstance.close({
                                isMultipleDays: true,
                                save: true,
                                employeeId: this.selectedEmployee.employeeId,
                                orderId: this.shift.order.orderId,
                                shiftTypeId: !this.shift.shiftTypeId ? 0 : this.shift.shiftTypeId,
                                startTime: this.start,
                                stopTime: this.timeAdjustmentType === TermGroup_AssignmentTimeAdjustmentType.FillToZeroRemaining ? null : toDate,
                                timeAdjustmentType: this.timeAdjustmentType
                            });
                        } else {
                            // Use general save shifts in schedule planning
                            if (this.ignoreScheduledBreaksOnAssignment && this.shiftsToSave.length === 1)
                                this.shiftsToSave[0].plannedTime = this.plannedTime;

                            this.$uibModalInstance.close({
                                isMultipleDays: false,
                                save: true,
                                shifts: this.shiftsToSave,
                            });
                        }
                    }
                });
            } else {
                this.executing = false;
            }
        });
    }

    private initSave(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        if (!this.isMultipleDays)
            this.setShiftsToSave();

        // Common validation for shifts and breaks
        this.validateCommon().then(passedCommon => {
            if (!passedCommon)
                deferral.resolve(false);
            else {
                // Validate remaining time
                this.validateRemainingTime().then(passedRemainingTime => {
                    if (!passedRemainingTime)
                        deferral.resolve(false);
                    else {
                        // Validate skills
                        this.validateSkills().then(passedSkills => {
                            if (!passedSkills)
                                deferral.resolve(false);
                            else {
                                // No evaluation for hidden or vacant employee
                                var onlyHidden: boolean = _.filter(this.shiftsToSave, s => s.employeeId === this.hiddenEmployeeId).length == this.shiftsToSave.length;
                                if (this.isMultipleDays || (this.shiftsToSave.length > 0 && onlyHidden)) {
                                    deferral.resolve(true);
                                } else {
                                    // Validate work rules
                                    this.validateWorkRules().then(passedWorkRules => {
                                        deferral.resolve(passedWorkRules);
                                    });
                                }
                            }
                        });
                    }
                });
            }
        });

        return deferral.promise;
    }

    private askSave(toDate: Date): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        if (this.isMultipleDays) {
            const keys: string[] = [
                "time.schedule.planning.editassignment.timeadjustmenttype",
                "time.schedule.planning.editassignment.asksavemultiple.filltozeroremaining",
                "time.schedule.planning.editassignment.asksavemultiple.filltoenddate"
            ];

            this.translationService.translateMany(keys).then(terms => {
                let msg: string;
                if (this.timeAdjustmentType === TermGroup_AssignmentTimeAdjustmentType.FillToZeroRemaining)
                    msg = terms["time.schedule.planning.editassignment.asksavemultiple.filltozeroremaining"].format(CalendarUtility.minutesToTimeSpan(this.plannedTime), this.selectedEmployee.name, this.start.toFormattedDateTime());
                else
                    msg = terms["time.schedule.planning.editassignment.asksavemultiple.filltoenddate"].format(this.selectedEmployee.name, CalendarUtility.minutesToTimeSpan(this.plannedTime), this.start.toFormattedDateTime(), toDate.toFormattedDateTime());
                const modal = this.notificationService.showDialogEx(terms["time.schedule.planning.editassignment.timeadjustmenttype"], msg, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    deferral.resolve(true);
                }, (reason) => {
                    deferral.resolve(false);
                });
            });
        } else {
            // No question for single day
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private validateCommon(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        // Validation for all shifts
        var validationErrors: string = '';
        var isValid: boolean = true;

        // Validate shift start/end
        if (!this.isMultipleDays) {
            if (_.filter(this.shiftsToSave, s => !s.actualStartTime || !s.actualStopTime).length > 0) {
                isValid = false;
                validationErrors += this.terms["time.schedule.planning.editassignment.missingdates"] + "\n";
            }

            if (_.filter(this.shiftsToSave, s => s.actualStartTime.isSameOrAfterOnMinute(s.actualStopTime)).length > 0) {
                isValid = false;
                validationErrors += this.terms["time.schedule.planning.editshift.endbeforestart"].format(this.shiftDefined.toUpperCaseFirstLetter()) + "\n";
            }
        }

        // ShiftType mandatory (company setting)
        // Not mandatory on breaks or absence
        if (this.shiftTypeMandatory) {
            if ((this.isMultipleDays && !this.shift.shiftTypeId) || (_.filter(this.shiftsToSave, s => !s.isBreak && !s.timeDeviationCauseId && !s.shiftTypeId).length > 0)) {
                isValid = false;
                validationErrors += this.terms["time.schedule.planning.editassignment.shifttypemandatory"] + "\n";
            }
        }

        // Employee mandatory
        if ((this.isMultipleDays && !this.selectedEmployee) || (_.filter(this.shiftsToSave, s => !s.employeeId).length > 0)) {
            isValid = false;
            validationErrors += this.terms["time.schedule.planning.editshift.employeemandatory"];
            validationErrors += " {0} '{1}'".format(this.terms["core.or"].toLocaleLowerCase(), this.terms["time.schedule.planning.hidden"].format(this.shiftUndefined));
            validationErrors += "\n";
        }

        // Validate remaining time
        if (this.remainingTimeToBe < 0) {
            isValid = false;
            validationErrors += this.terms["time.schedule.planning.editassignment.remainingtimeafterthisnegative"];
            validationErrors += "\n";
        }

        if (!isValid)
            this.notificationService.showDialog(this.terms["core.unabletosave"], validationErrors, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);

        deferral.resolve(isValid);

        return deferral.promise;
    }

    private validateRemainingTime(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        // If fill to end date is selected, 'Keep as planned' must be checked on order to be able to plan more than remaining time
        if (this.timeAdjustmentType === TermGroup_AssignmentTimeAdjustmentType.FillToEndDate && this.plannedTime > this.shift.order.remainingTime) {
            const keys: string[] = [
                "time.schedule.planning.editassignment.plannedmorethanremaining.message.info",
                "time.schedule.planning.editassignment.plannedmorethanremaining.message.error",
                "time.schedule.planning.editassignment.plannedmorethanremaining.message.warning"
            ];
            this.translationService.translateMany(keys).then(terms => {
                let message = terms["time.schedule.planning.editassignment.plannedmorethanremaining.message.info"].format(CalendarUtility.minutesToTimeSpan(this.plannedTime), CalendarUtility.minutesToTimeSpan(this.shift.order.remainingTime));
                message += "\n\n";

                if (!this.shift.order.keepAsPlanned) {
                    // Show error
                    message += terms["time.schedule.planning.editassignment.plannedmorethanremaining.message.error"];
                    this.notificationService.showDialog(this.terms["common.obs"], message, SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                    deferral.resolve(false);
                } else {
                    // Show warning
                    message += terms["time.schedule.planning.editassignment.plannedmorethanremaining.message.warning"].format(CalendarUtility.minutesToTimeSpan(this.plannedTime));
                    var modal = this.notificationService.showDialog(this.terms["common.obs"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        deferral.resolve(true);
                    }, (reason) => {
                        deferral.resolve(false);
                    });
                }
            });
        } else
            deferral.resolve(true);

        return deferral.promise;
    }

    private validateSchedule() {
        // Check if start or end is outside schedule
        this.outsideSchedule = false;
        if (this.scheduleShifts.length > 0) {
            if (this.start && this.stop) {
                if (this.getScheduleStart().isAfterOnMinute(this.start) || this.getScheduleStop().isBeforeOnMinute(this.stop))
                    this.outsideSchedule = true;
            }
        }
    }

    private validateSkills(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        if (this.invalidSkills) {
            let message = this.terms["time.schedule.planning.editshift.missingskills"].format(this.shiftUndefined);
            if (!this.skillCantBeOverridden)
                message += "\n" + this.terms["time.schedule.planning.editshift.missingskillsoverride"];

            const modal = this.notificationService.showDialog(this.terms["common.obs"], message, SOEMessageBoxImage.Forbidden, this.skillCantBeOverridden ? SOEMessageBoxButtons.OK : SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                deferral.resolve(val && !this.skillCantBeOverridden);
            }, (reason) => {
                deferral.resolve(false);
            });
        } else
            deferral.resolve(true);

        return deferral.promise;
    }

    private validateWorkRules(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        _.forEach(this.shiftsToSave, shift => {
            shift.setTimesForSave();
        });

        let rules: SoeScheduleWorkRules[] = null;
        if (this.skipWorkRules) {
            // The following rules should always be evaluated
            rules = [];
            rules.push(SoeScheduleWorkRules.OverlappingShifts);
            rules.push(SoeScheduleWorkRules.AttestedDay);
        }

        if (this.shiftsToSave.length === 0)
            deferral.resolve(true);
        else {
            var employeeId: number = this.selectedEmployee.employeeId;
            this.sharedScheduleService.evaluatePlannedShiftsAgainstWorkRules(this.shiftsToSave, rules, employeeId, false, null).then(result => {
                this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.TaskSaveOrderShift, result, employeeId).then(passed => {
                    deferral.resolve(passed);
                });
            });
        }

        return deferral.promise;
    }

    private validateWorkRulesForAssign(employeeId: number): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        if (this.shift) {
            let rules: SoeScheduleWorkRules[] = null;
            if (this.skipWorkRules) {
                // The following rules should always be evaluated
                rules = [];
                rules.push(SoeScheduleWorkRules.OverlappingShifts);
                rules.push(SoeScheduleWorkRules.AttestedDay);
            }

            this.sharedScheduleService.evaluateDragShiftAgainstWorkRules(DragShiftAction.Move, this.shift.timeScheduleTemplateBlockId, 0, this.shift.actualStartTime, this.shift.actualStopTime, employeeId, false, false, rules, false).then(result => {
                this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.AssignEmployeeFromQueue, result, employeeId).then(passed => {
                    deferral.resolve(passed);
                });
            });
        } else {
            deferral.resolve(false);
        }

        return deferral.promise;
    }

    private performDragAction(employeeId: number, evaluateWorkRules: boolean = true) {
        if (evaluateWorkRules) {
            this.validateWorkRulesForAssign(employeeId).then(result => {
                if (result)
                    this.performDragAction(employeeId, false);
            });
        } else {
            this.executing = true;
            this.sharedScheduleService.dragShift(DragShiftAction.Move, this.shift.timeScheduleTemplateBlockId, 0, this.shift.actualStartTime, this.shift.actualStopTime, employeeId, null, false, 0, null, false, this.skipXEMailOnChanges, false, false, null).then(result => {
                var errorMessage: string;

                if (result.success) {
                    // Success
                    var employeeIds: number[] = [];
                    employeeIds.push(this.shift.employeeId);
                    employeeIds.push(employeeId);
                    this.$uibModalInstance.close({ reload: true, reloadEmployeeIds: employeeIds });
                } else {
                    // Failure
                    this.translationService.translate("time.schedule.planning.shiftqueue.assignfromqueue.error").then(term => {
                        this.notificationService.showDialog(term.format(this.shiftUndefined), result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    });
                    this.executing = false;
                }
            });
        }
    }

    // HELP-METHODS

    private openEditOrder() {
        if (!this.shift.order || !this.shift.order.orderId)
            return;

        this.messagingService.publish(Constants.EVENT_EDIT_ORDER, this.shift);
    }

    private openSplitShift() {
        if (!this.shift)
            return;

        // Show split shift dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/SplitShift/Views/splitShift.html"),
            controller: SplitShiftController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                currentEmployeeId: () => { return this.employeeId },
                templateHelper: () => { return null },
                isTemplate: () => { return false },
                isEmployeePost: () => { return false },
                showSkills: () => { return this.showSkills },
                showExtraShift: () => { return false },
                showSubstitute: () => { return false },
                clockRounding: () => { return this.clockRounding },
                keepShiftsTogether: () => { return this.keepShiftsTogether },
                skillCantBeOverridden: () => { return this.skillCantBeOverridden },
                hiddenEmployeeId: () => { return this.hiddenEmployeeId },
                vacantEmployeeIds: () => { return this.vacantEmployeeIds },
                allEmployees: () => { return this.employees },
                shift: () => { return this.shift },
                timeScheduleScenarioHeadId: () => { return null; },
                planningPeriodStartDate: () => { return null; },
                planningPeriodStopDate: () => { return null; }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result)
                this.performSplitShift(this.shift, result.splitTime, result.employeeId1, result.employeeId2);
        });
    }

    private performSplitShift(shift: ShiftDTO, splitTime: Date, employeeId1: number, employeeId2: number) {
        this.sharedScheduleService.splitShift(shift, splitTime, employeeId1, employeeId2, this.keepShiftsTogether, false, this.skipXEMailOnChanges, null).then(result => {
            if (result.success) {
                this.cancel([employeeId1, employeeId2]);
            } else {
                this.notificationService.showDialogEx(this.terms["error.default_error"], result.errorMessage, SOEMessageBoxImage.Error);
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
        });
    }

    private openShiftRequestDialog() {
        if (!this.shift || !this.shift.timeScheduleTemplateBlockId)
            return;

        this.sharedScheduleService.getShiftRequestStatus(this.shift.timeScheduleTemplateBlockId).then(x => {
            let excludeEmployeeIds: number[] = [];
            if (x && x.recipients)
                excludeEmployeeIds = _.map(x.recipients, r => r['employeeId']);

            const modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Core/RightMenu/MessageMenu/edit.html"),
                controller: MessageEditController,
                controllerAs: 'ctrl',
                bindToController: true,
                backdrop: 'static',
                size: 'lg',
                scope: this.$scope,
            });

            modal.rendered.then(() => {
                this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                    source: 'Planning',
                    modal: modal,
                    title: this.terms["time.schedule.planning.contextmenu.sendassignmentrequest"],
                    id: 0,
                    messageMinHeight: 250,
                    type: XEMailType.Outgoing,
                    messageType: TermGroup_MessageType.ShiftRequest,
                    shift: this.shift,
                    showAvailableEmployees: true,
                    showAvailability: this.showAvailability,
                    allEmployees: _.filter(this.employees, e => e.employeeId && !e.hidden && !e.vacant && !_.includes(excludeEmployeeIds, e.employeeId))
                });
            });

            modal.result.then(result => {
                if (result && result.success) {
                    this.messagingService.publish(Constants.EVENT_RELOAD_SHIFTS_FOR_EMPLOYEE, [this.shift.employeeId]);
                }
            });
        });
    }

    private openAbsenceDialog() {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Absencerequests/Views/edit.html"),
            controller: AbsenceRequestsEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: 0,
                employeeId: this.shift.employeeId,
                viewMode: AbsenceRequestViewMode.Attest,
                guiMode: AbsenceRequestGuiMode.AbsenceDialog,
                skipXEMailOnShiftChanges: false,
                shiftId: this.shift.timeScheduleTemplateBlockId,
                date: this.shift.date,
                hideOptionSelectedShift: false,
                parentMode: AbsenceRequestParentMode.SchedulePlanning,
                timeScheduleScenarioHeadId: null,
            });
        });

        modal.result.then(reloadEmployeeIds => {
            this.$uibModalInstance.close({ reload: true, reloadEmployeeIds: reloadEmployeeIds });
        });
    }

    private openAbsenceRequestDialog() {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Absencerequests/Views/edit.html"),
            controller: AbsenceRequestsEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: 0,
                employeeId: this.shift.employeeId,
                viewMode: AbsenceRequestViewMode.Attest,
                guiMode: AbsenceRequestGuiMode.EmployeeRequest,
                loadRequestFromInterval: true,
                date: this.shift.date,
                skipXEMailOnShiftChanges: false,
                shiftId: 0,
                hideOptionSelectedShift: false,
                parentMode: AbsenceRequestParentMode.SchedulePlanning,
                timeScheduleScenarioHeadId: null,
            });
        });

        modal.result.then(reloadEmployeeIds => {
            this.$uibModalInstance.close({ reload: true, reloadEmployeeIds: reloadEmployeeIds });
        });
    }

    private restoreToSchedule() {
        const modal = this.notificationService.showDialog(this.terms["core.info"], this.terms["time.schedule.planning.editshift.functions.restoretoschedule.message"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {

            var items: AttestEmployeeDaySmallDTO[] = [];
            var item = new AttestEmployeeDaySmallDTO();
            item.employeeId = this.shift.employeeId;
            item.date = this.shift.date;
            items.push(item);

            this.applyAttestCalculationFunction(items, SoeTimeAttestFunctionOption.RestoreToSchedule).then(passed => {
                if (passed) {
                    var reloadEmployeeIds: number[] = [];
                    reloadEmployeeIds.push(this.shift.employeeId);

                    this.$uibModalInstance.close({ reload: true, reloadEmployeeIds: reloadEmployeeIds });
                }
            });
        });
    }

    private applyAttestCalculationFunction(items: AttestEmployeeDaySmallDTO[], option: SoeTimeAttestFunctionOption): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();
        this.executing = true;

        this.sharedScheduleService.applyAttestCalculationFunctionEmployee(items, option).then(result => {
            this.executing = false;
            if (result.success) {
                deferral.resolve(true);
            } else {
                this.notificationService.showDialogEx(this.terms["error.default_error"], result.errorMessage, SOEMessageBoxImage.Error);
                deferral.resolve(false);
            }
        }, error => {
            this.executing = false;
            deferral.resolve(false);
        });

        return deferral.promise;
    }

    private openShiftHistory() {
        // Show shifthistory dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/ShiftHistory/Views/shiftHistory.html"),
            controller: ShiftHistoryController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                shiftType: () => { return this.shift.type },
                timeScheduleTemplateBlockId: () => { return this.shift.timeScheduleTemplateBlockId }
            }
        }
        this.$uibModal.open(options);
    }

    private openAccountingDialog() {
        if (!this.shift)
            return;

        // Show accounting dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/ShiftAccounting/Views/shiftAccounting.html"),
            controller: ShiftAccountingController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                shifts: () => { return [this.shift] },
                selectedShift: () => { return this.shift }
            }
        }

        this.$uibModal.open(options);
    }

    private setShiftTypeIds() {
        this.shiftTypeIds = [this.shift.shiftTypeId];
    }

    private calculateDurations = _.debounce((ignoreCalculatePlanned = false) => {
        switch (this.timeAdjustmentType) {
            case (TermGroup_AssignmentTimeAdjustmentType.OneDay):
                // Check if any scheduled breaks exists
                var breaks = this.getScheduleBreaks();
                var breakLength: number = 0;
                if (breaks.length > 0 && this.shift.order && this.start && this.stop) {
                    _.forEach(breaks, brk => {
                        if (!this.ignoreScheduledBreaksOnAssignment) {
                            if (this.start.isSameOrAfterOnMinute(brk.actualStartTime) && this.start.isBeforeOnMinute(brk.actualStopTime)) {
                                this.start = brk.actualStopTime;
                            }
                            if (this.stop.isSameOrAfterOnMinute(brk.actualStartTime) && this.stop.isBeforeOnMinute(brk.actualStopTime)) {
                                this.stop = brk.actualStartTime;
                            }
                        }
                    });
                    breakLength = breaks[0].getBreakTimeWithinShift(this.start, this.stop)
                }
                var length: number = 0;
                if (this.shift.actualStartTime && this.shift.actualStopTime)
                    length = this.shift.actualStopTime.diffMinutes(this.shift.actualStartTime);

                this.createMultipleShifts = breakLength > 0;

                if (!ignoreCalculatePlanned && !this.populating) {
                    const planned = length - breakLength;
                    if (this.plannedTime !== planned) {
                        this.plannedTime = planned;
                        this.plannedTimeDate = new Date(1900, 0, 1, 0, this.plannedTime);
                    }
                }
                this.remainingTimeToBe = this.shift.getOrderRemainingTime() + this.originalLength - this.plannedTime;

                this.validateSchedule();
                break;
            case (TermGroup_AssignmentTimeAdjustmentType.FillToZeroRemaining):
                this.plannedTime = this.shift.getOrderRemainingTime();
                this.plannedTimeDate = new Date(1900, 0, 1, 0, this.plannedTime);
                this.remainingTimeToBe = 0;
                break;
            case (TermGroup_AssignmentTimeAdjustmentType.FillToEndDate):
                this.plannedTime = this.availableTimeItem.availableMinutes;
                this.plannedTimeDate = new Date(1900, 0, 1, 0, this.plannedTime);
                this.remainingTimeToBe = this.shift.getOrderRemainingTime() - this.availableTimeItem.availableMinutes;
                if (this.remainingTimeToBe < 0)
                    this.remainingTimeToBe = 0;
                break;
        }
        this.$scope.$apply();
    }, 200, { leading: false, trailing: true });

    private initSetTimesBasedOnSchedule() {
        // Check existing assignments and set start time to last existing assignments stop time
        this.loadExistingShifts().then(existingShifts => {
            if (existingShifts.length > 0) {
                var lastShift: ShiftDTO = _.head(_.orderBy(existingShifts, 'actualStopTime', 'desc'));
                this.shift.actualStartTime = lastShift.actualStopTime;
            }
            this.setTimesBasedOnSchedule();
        });
    }

    private setTimesBasedOnSchedule() {
        // Get schedule start
        var startTime: Date = this.getScheduleStart();

        // Start time is set from planning (based on existing shifts in slot)
        if (this.shift.actualStartTime && this.shift.actualStartTime.hour() !== 0) {
            startTime = this.shift.actualStartTime;
        }

        // Make sure start time is not inside a break
        if (!this.ignoreScheduledBreaksOnAssignment) {
            var breaks = this.getScheduleBreaks();
            _.forEach(breaks, brk => {
                if (startTime.isWithinRange(brk.actualStartTime, brk.actualStopTime))
                    startTime = brk.actualStopTime;
            });
        }

        // Get schedule stop
        var stopTime: Date = this.getScheduleStop();
        if (stopTime.isBeforeOnMinute(startTime))
            stopTime = startTime;

        // Get shift length (total remaining time on order)
        var shiftLength: number = this.shift.getOrderRemainingTime();

        // If shift ends after schedule ends, set shift end to schedule end
        var shiftEnd: Date = startTime.addMinutes(shiftLength);

        // Calculate break length withing shift length
        var breakLength: number = 0;
        _.forEach(_.filter(this.scheduleShifts, (x) => !x.isBreak), (s) => {
            breakLength += s.getBreakTimeWithinShift(startTime, shiftEnd);
        });

        //var breakLength: number = this.shift.getBreakTimeWithinShift(startTime, shiftEnd);
        if (breakLength > 0) {
            shiftLength += breakLength;
            // Reset shift end (add break length)
            shiftEnd = startTime.addMinutes(shiftLength);
        }
        // Limit shift end to end of schedule
        if (shiftEnd.isAfterOnMinute(stopTime) && stopTime.isAfterOnMinute(startTime))
            shiftEnd = stopTime;
        // Limit shift end to shift length (including breaks)
        if (stopTime.isAfterOnMinute(shiftEnd))
            stopTime = shiftEnd;

        this.shift.actualStartTime = this.start = startTime;
        this.shift.actualStopTime = this.stop = stopTime;

        this.calculateDurations();
    }

    private getScheduleStart(): Date {
        const schedule: ShiftDTO = this.scheduleShifts.length > 0 ? _.orderBy(this.scheduleShifts, s => s.actualStartTime)[0] : null;
        if (schedule)
            return schedule.actualStartTime;

        var date: Date = this.start ? this.start : new Date();

        return date.beginningOfDay().addMinutes(this.dayStartTime);
    }

    private getScheduleStop(): Date {
        var schedule: ShiftDTO = this.scheduleShifts.length > 0 ? _.orderBy(this.scheduleShifts, s => s.actualStopTime, 'desc')[0] : null;
        if (schedule)
            return schedule.actualStopTime;

        var date: Date = this.stop ? this.stop : new Date();

        // This will prevent looping.
        // If dayMinutes will exceed 1440, stop will be set to next date,
        // that in turn will load schedule for next day, which in turn will start a loop.
        let dayMinutes = this.dayEndTime;
        if (dayMinutes >= (24 * 60))
            dayMinutes = (17 * 60);

        return date.beginningOfDay().addMinutes(dayMinutes);
    }

    private getScheduleBreaks(): ShiftDTO[] {
        return _.orderBy(_.filter(this.scheduleShifts, s => s.isBreak), s => s.actualStartTime);
    }

    private setShiftsToSave() {
        // If scheduled breaks exists that overlaps the shift, split the shift
        this.shiftsToSave = (this.createMultipleShifts && !this.ignoreScheduledBreaksOnAssignment ? this.splitShiftsBasedOnBreaks() : [this.shift]);
    }

    private splitShiftsBasedOnBreaks(): ShiftDTO[] {
        var shifts: ShiftDTO[] = [];

        if (this.start && this.stop) {
            var startTime: Date = this.start;
            var stopTime: Date = this.stop;

            var breaks: ShiftDTO[] = this.getScheduleBreaks();
            _.forEach(breaks, brk => {
                let breakLength: number = brk.getShiftLength();
                let intersectLength: number = CalendarUtility.getIntersectingDuration(this.shift.actualStartTime, this.shift.actualStopTime, brk.actualStartTime, brk.actualStopTime);

                // Calculate break time inside shift
                if (intersectLength === breakLength) {
                    // Break is completely overlapped by a presence shift
                    // Split shift
                    let clone = this.shift.copy(true, false);
                    clone.startTime = clone.actualStartTime = startTime;
                    clone.stopTime = clone.actualStopTime = brk.actualStartTime;
                    shifts.push(clone);

                    startTime = brk.actualStopTime;
                } else if (intersectLength > 0) {
                    // Break intersects with a presence shift
                    // This is OK
                    let clone = this.shift.copy(true, false);
                    clone.startTime = clone.actualStartTime = startTime;
                    clone.stopTime = clone.actualStopTime = stopTime;
                    shifts.push(clone);

                    startTime = stopTime;
                }
            });

            if (startTime.isBeforeOnMinute(stopTime)) {
                // Create shift after last break
                let clone = this.shift.copy(true, false);
                clone.startTime = clone.actualStartTime = startTime;
                clone.stopTime = clone.actualStopTime = stopTime;
                shifts.push(clone);
            }

            if (shifts.length > 0)
                shifts[0].timeScheduleTemplateBlockId = this.shift.timeScheduleTemplateBlockId;
        }
        return shifts;
    }
}