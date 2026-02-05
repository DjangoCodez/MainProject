import { IScheduleService as ISharedScheduleService } from "../../../ScheduleService";
import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../../Core/Services/NotificationService";
import { ShiftDTO } from "../../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { EmployeeListDTO } from "../../../../../../Common/Models/EmployeeListDTO";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize } from "../../../../../../Util/Enumerations";
import { TermGroup_ShiftHistoryType, SoeScheduleWorkRules, TermGroup_TimeScheduleTemplateBlockType } from "../../../../../../Util/CommonEnumerations";

export class EditBookingController {

    // Data
    private scheduleShifts: ShiftDTO[] = [];
    private existingShifts: ShiftDTO[] = [];

    // Terms
    private terms: any = [];
    private shiftDefined: string;
    private shiftUndefined: string;
    private shiftsDefined: string;
    private shiftsUndefined: string;

    // Flags
    private executing: boolean = false;

    // Properties
    private _selectedEmployee: EmployeeListDTO;
    private get selectedEmployee() {
        return this._selectedEmployee;
    }
    private set selectedEmployee(item: EmployeeListDTO) {
        this._selectedEmployee = item;

        this.shift.employeeId = item ? item.employeeId : 0;
        this.shift.employeeName = item ? item.name : '';
    }

    private _start: Date;
    private get start(): Date {
        return this._start;
    }
    private set start(date: Date) {
        var changingDate: boolean = !!((this._start && !date) || (!this._start && date) || !this._start.isSameDayAs(date));

        this._start = date;
        this.shift.startTime = this.shift.actualStartTime = date;

        this.calculateDurations();

        if (changingDate)
            this.loadSchedule();
    }

    private _stop: Date;
    private get stop(): Date {
        return this._stop;
    }
    private set stop(date: Date) {
        this._stop = date;
        this.shift.stopTime = this.shift.actualStopTime = date;

        this.calculateDurations();
    }

    private plannedTime: number = 0;
    private outsideSchedule: boolean = false;

    //@ngInject
    constructor(private $timeout: ng.ITimeoutService,
        private $uibModalInstance,
        private sharedScheduleService: ISharedScheduleService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private $q: ng.IQService,
        private isReadonly: boolean,
        private modifyPermission: boolean,
        private shift: ShiftDTO,
        private date: Date,
        private employeeId: number,
        private shiftTypes: { id: number, label: string, timeScheduleTypeId: number, defaultLength: number }[],
        private employees: EmployeeListDTO[],
        private hiddenEmployeeId: number,
        private dayStartTime: number,
        private dayEndTime: number,
        private shiftTypeMandatory: boolean,
        private skipWorkRules: boolean) {

        this.setup();

        if (this.shift)
            this.populate();
    }

    // SETUP

    private setup() {
        if (!this.modifyPermission)
            this.isReadonly = true;

        this.$q.all([this.loadTerms()]).then(() => { });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.unabletosave",
            "time.schedule.planning.bookingdefined",
            "time.schedule.planning.bookingundefined",
            "time.schedule.planning.bookingsdefined",
            "time.schedule.planning.bookingsundefined",
            "time.schedule.planning.breakprefix",
            "time.schedule.planning.editbooking.endbeforestart",
            "time.schedule.planning.editbooking.shifttypemandatory",
            "time.schedule.planning.editassignment.missingdates"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.shiftDefined = this.terms["time.schedule.planning.bookingdefined"];
            this.shiftUndefined = this.terms["time.schedule.planning.bookingundefined"];
            this.shiftsDefined = this.terms["time.schedule.planning.bookingsdefined"];
            this.shiftsUndefined = this.terms["time.schedule.planning.bookingsundefined"];
        });
    }

    private loadExistingShifts(): ng.IPromise<ShiftDTO[]> {
        return this.sharedScheduleService.getShiftsForDay(this.employeeId, this.date, [TermGroup_TimeScheduleTemplateBlockType.Order, TermGroup_TimeScheduleTemplateBlockType.Booking], false, false, null, false, false, false, true);
    }

    private loadSchedule = _.debounce(() => {
        this.scheduleShifts = [];
        this.existingShifts = [];

        if (!this.selectedEmployee || !this.start)
            return;

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

            // In day view, keep the times selected in the calendar
            // otherwise set times based on schedule
            //if (Shift.TimeScheduleTemplateBlockId == 0 && viewDefinition != TimeSchedulePlanning.ViewDefinitions.Day)
            if (!this.shift.timeScheduleTemplateBlockId)
                this.initSetTimesBasedOnSchedule();
            else
                this.calculateDurations();
        });
    }, 500, { leading: false, trailing: true });

    // ACTIONS

    private populate() {
        this.selectedEmployee = _.find(this.employees, e => e.employeeId === this.employeeId);
        this.start = this.shift.actualStartTime;
        this.stop = this.shift.actualStopTime;

        this.loadSchedule();
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.close({ reload: true, reloadEmployeeIds: [this.selectedEmployee.employeeId] });
    }

    private save() {
        this.executing = true;

        this.initSave().then(val => {
            if (val) {
                this.$uibModalInstance.close({ save: true, shifts: [this.shift] });
            } else {
                this.executing = false;
            }
        });
    }

    private initSave(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        // Common validation for shifts and breaks
        this.validateCommon().then(passedCommon => {
            if (!passedCommon)
                deferral.resolve(false);
            else {
                // No evaluation for hidden or vacant employees
                if (this.shift.employeeId === this.hiddenEmployeeId) {
                    deferral.resolve(true);
                } else {
                    // Validate work rules
                    this.validateWorkRules().then(passedWorkRules => {
                        deferral.resolve(passedWorkRules);
                    });
                }
            }
        });

        return deferral.promise;
    }

    private validateCommon(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        // Validation for all shifts
        let validationErrors: string = '';
        let isValid: boolean = true;

        // Validate shift start/end
        if (!this.start || !this.stop) {
            isValid = false;
            validationErrors += this.terms["time.schedule.planning.editassignment.missingdates"] + "\n";
        }

        if (this.shift.actualStartTime && this.shift.actualStopTime && this.shift.actualStartTime.isSameOrAfterOnMinute(this.shift.actualStopTime)) {
            isValid = false;
            validationErrors += this.terms["time.schedule.planning.editbooking.endbeforestart"].format(this.shiftDefined.toUpperCaseFirstLetter()) + "\n";
        }

        // ShiftType mandatory (company setting)
        if (this.shiftTypeMandatory && !this.shift.shiftTypeId) {
            isValid = false;
            validationErrors += this.terms["time.schedule.planning.editbooking.shifttypemandatory"] + "\n";
        }

        if (!isValid)
            this.notificationService.showDialog(this.terms["core.unabletosave"], validationErrors, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);

        deferral.resolve(isValid);

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

    private validateWorkRules(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        this.shift.setTimesForSave();

        let rules: SoeScheduleWorkRules[] = null;
        if (this.skipWorkRules) {
            // The following rules should always be evaluated
            rules = [];
            rules.push(SoeScheduleWorkRules.OverlappingShifts);
        }

        const employeeId: number = this.selectedEmployee.employeeId;
        this.sharedScheduleService.evaluatePlannedShiftsAgainstWorkRules([this.shift], rules, employeeId, false, null).then(result => {
            this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.TaskSaveBooking, result, employeeId).then(passed => {
                deferral.resolve(passed);
            });
        });

        return deferral.promise;
    }

    private shiftTypeChanged = () => {
        this.$timeout(() => {
            const length = this.shiftTypes.find(t => t.id === this.shift.shiftTypeId)?.defaultLength || 0;
            this.setTimesBasedOnSchedule(length);
        })
    }

    // HELP-METHODS

    private calculateDurations() {
        let length: number = 0;
        if (this.shift.actualStartTime && this.shift.actualStopTime)
            length = this.shift.actualStopTime.diffMinutes(this.shift.actualStartTime);

        // Check if any scheduled breaks exists
        let breakLength: number = 0;
        if (this.shift.order && this.shift.order.plannedStartDate && this.shift.order.plannedStopDate) {
            breakLength = this.shift.getBreakTimeWithinShift();
        }

        this.plannedTime = length - breakLength;

        this.validateSchedule();
    }

    private initSetTimesBasedOnSchedule() {
        // Check existing assignments and set start time to last existing assignments stop time
        this.loadExistingShifts().then(existingShifts => {
            if (existingShifts.length > 0) {
                let lastShift: ShiftDTO = _.head(_.orderBy(existingShifts, 'actualStopTime', 'desc'));
                this.shift.actualStartTime = lastShift.actualStopTime;
            }
            this.setTimesBasedOnSchedule();
        });
    }

    private setTimesBasedOnSchedule(length: number = 0) {
        // Get schedule start
        let startTime: Date = this.getScheduleStart();

        // Start time is set from planning (based on existing shifts in slot)
        if (this.shift.actualStartTime && this.shift.actualStartTime.hour() !== 0) {
            startTime = this.shift.actualStartTime;
        }

        // Make sure start time is not inside a break
        const breaks = this.getScheduleBreaks();
        _.forEach(breaks, brk => {
            if (startTime.isWithinRange(brk.actualStartTime, brk.actualStopTime))
                startTime = brk.actualStopTime;
        });

        // Get schedule stop
        let stopTime: Date = this.getScheduleStop();
        if (stopTime.isBeforeOnMinute(startTime))
            stopTime = startTime;

        // Get shift length (total remaining time on order)
        let shiftLength: number = length;//this.shift.getOrderRemainingTime();

        // If shift ends after schedule ends, set shift end to schedule end
        let shiftEnd: Date = startTime.addMinutes(shiftLength);
        // Calculate break length withing shift length
        let breakLength: number = this.shift.getBreakTimeWithinShift(startTime, shiftEnd);
        if (breakLength > 0) {
            shiftLength += breakLength;
            // Reset shift end (add break length)
            shiftEnd = startTime.addMinutes(breakLength);
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

        const date: Date = this.start ? this.start : new Date();

        return date.beginningOfDay().addMinutes(this.dayStartTime);
    }

    private getScheduleStop(): Date {
        const schedule: ShiftDTO = this.scheduleShifts.length > 0 ? _.orderBy(this.scheduleShifts, s => s.actualStopTime, 'desc')[0] : null;
        if (schedule)
            return schedule.actualStopTime;

        const date: Date = this.start ? this.start : new Date();

        return date.beginningOfDay().addMinutes(this.dayEndTime);
    }

    private getScheduleBreaks(): ShiftDTO[] {
        return _.orderBy(_.filter(this.scheduleShifts, s => s.isBreak), s => s.actualStartTime);
    }
}