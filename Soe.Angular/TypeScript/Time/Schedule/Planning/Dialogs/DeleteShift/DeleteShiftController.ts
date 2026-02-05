import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IScheduleService as ISharedScheduleService } from "../../../../../Shared/Time/Schedule/ScheduleService";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { ShiftDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { TermGroup_TimeSchedulePlanningViews } from "../../../../../Util/CommonEnumerations";
import { EmployeeSchedulePlacementGridViewDTO } from "../../../../../Common/Models/EmployeeScheduleDTOs";

export class DeleteShiftController {

    // Terms
    private shiftDefined: string;
    private shiftUndefined: string;
    private shiftsDefined: string;
    private shiftsUndefined: string;
    private title: string;
    private selectInfo: string;
    private noShiftsInfo: string;
    private onDutyLabel: string;
    private shiftDate: string;
    private shiftEmployee: string;

    private isOrder = false;
    private isBooking = false;
    private hasAbsence = false;

    private placement: EmployeeSchedulePlacementGridViewDTO;

    // Properties
    private get isScheduleView(): boolean {
        return (this.viewDefinition === TermGroup_TimeSchedulePlanningViews.Day || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.Schedule);
    }

    private get isTemplateView(): boolean {
        return (this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TemplateDay || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TemplateSchedule);
    }

    private get isEmployeePostView(): boolean {
        return (this.viewDefinition === TermGroup_TimeSchedulePlanningViews.EmployeePostsDay || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule);
    }

    private get isScenarioView(): boolean {
        return (this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioDay || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioSchedule || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioComplete);
    }

    private get isStandbyView(): boolean {
        return (this.viewDefinition === TermGroup_TimeSchedulePlanningViews.StandbyDay || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.StandbySchedule);
    }

    private get multipleDates(): boolean {
        return this.shifts.length > 0 && !this.firstShift.date.isSameDayAs(this.lastShift.date);
    }

    private get firstShift(): ShiftDTO {
        return _.head(_.orderBy(this.shifts, 'actualStartTime'));
    }

    private get lastShift(): ShiftDTO {
        return _.head(_.orderBy(this.shifts, 'actualStopTime', 'desc'));
    }

    private get nbrOfSelectedShifts(): number {
        return _.filter(this.shifts, s => s.selected).length;
    }

    // Flags
    private executing = false;
    private includeOnDutyShifts = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        $uibModal,
        $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private sharedScheduleService: ISharedScheduleService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private shifts: ShiftDTO[],
        private viewDefinition: TermGroup_TimeSchedulePlanningViews,
        private onDutyShiftsModifyPermission: boolean,
        private onDutyShifts: ShiftDTO[]) {

        // Exclude repeating shifts (in template view)
        this.shifts = _.filter(this.shifts, s => !s.originalBlockId);

        // Approved absence can't be deleted
        this.hasAbsence = (_.filter(this.shifts, s => s.timeDeviationCauseId && !s.isBooking).length > 0);
        if (this.hasAbsence) {
            // If only absence shifts, show error
            // Otherwise just filter absence away and show other shifts
            if (_.filter(this.shifts, s => !s.timeDeviationCauseId).length > 0) {
                this.shifts = _.filter(this.shifts, s => !s.timeDeviationCauseId);
                this.hasAbsence = false;
            }
        }

        if (this.shifts.length > 0) {
            _.forEach(this.shifts, shift => {
                shift.selected = true;
            });
            this.shiftDate = this.multipleDates ? "{0} - {1}".format(this.firstShift.actualStartDate.toFormattedDate(), this.lastShift.actualStopTime.toFormattedDate()) : this.firstShift.actualStartDate.toFormattedDate();
            this.shiftEmployee = this.firstShift.employeeName;
            this.isOrder = this.firstShift.isOrder;
            this.isBooking = this.firstShift.isBooking;
        } else {
            if (!this.hasAbsence) {
                // No shifts, close dialog
                $timeout(() => {
                    this.cancel();
                }, 2000);
            }
        }

        this.loadTerms().then(() => {
            if (this.isTemplateView)
                this.loadPlacement();
        });

        if (this.onDutyShiftsModifyPermission && this.isScheduleView && this.onDutyShifts.length > 0)
            this.includeOnDutyShifts = true;
    }

    // LOOKUPS
    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [];

        keys.push("time.schedule.planning.deleteshift.title");
        keys.push("time.schedule.planning.deleteshift.selectshifts");
        keys.push("time.schedule.planning.deleteshift.noshifts");
        keys.push("time.schedule.planning.blocktype.onduty");

        if (this.isOrder) {
            keys.push("time.schedule.planning.assignmentdefined");
            keys.push("time.schedule.planning.assignmentundefined");
            keys.push("time.schedule.planning.assignmentsdefined");
            keys.push("time.schedule.planning.assignmentsundefined");
        } else if (this.isBooking) {
            keys.push("time.schedule.planning.bookingdefined");
            keys.push("time.schedule.planning.bookingundefined");
            keys.push("time.schedule.planning.bookingsdefined");
            keys.push("time.schedule.planning.bookingsundefined");
        } else {
            keys.push("time.schedule.planning.shiftdefined");
            keys.push("time.schedule.planning.shiftundefined");
            keys.push("time.schedule.planning.shiftsdefined");
            keys.push("time.schedule.planning.shiftsundefined");
        }

        return this.translationService.translateMany(keys).then((terms) => {
            if (this.isOrder) {
                this.shiftDefined = terms["time.schedule.planning.assignmentdefined"];
                this.shiftUndefined = terms["time.schedule.planning.assignmentundefined"];
                this.shiftsDefined = terms["time.schedule.planning.assignmentsdefined"];
                this.shiftsUndefined = terms["time.schedule.planning.assignmentsundefined"];
            } else if (this.isBooking) {
                this.shiftDefined = terms["time.schedule.planning.bookingdefined"];
                this.shiftUndefined = terms["time.schedule.planning.bookingundefined"];
                this.shiftsDefined = terms["time.schedule.planning.bookingsdefined"];
                this.shiftsUndefined = terms["time.schedule.planning.bookingsundefined"];
            } else {
                this.shiftDefined = terms["time.schedule.planning.shiftdefined"];
                this.shiftUndefined = terms["time.schedule.planning.shiftundefined"];
                this.shiftsDefined = terms["time.schedule.planning.shiftsdefined"];
                this.shiftsUndefined = terms["time.schedule.planning.shiftsundefined"];
            }

            this.title = terms["time.schedule.planning.deleteshift.title"].format(this.shifts.length > 1 ? this.shiftsUndefined : this.shiftUndefined);
            this.selectInfo = terms["time.schedule.planning.deleteshift.selectshifts"].format(this.shiftsUndefined);
            this.noShiftsInfo = terms["time.schedule.planning.deleteshift.noshifts"].format(this.shiftsUndefined);
            this.onDutyLabel = ` (${terms["time.schedule.planning.blocktype.onduty"].toLocaleLowerCase()})`;            
        });
    }

    private loadPlacement(): ng.IPromise<any> {
        return this.sharedScheduleService.getPlacementForEmployee(this.firstShift.actualStartDate, this.firstShift.employeeId).then(x => {
            this.placement = x;
        });
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.executing = true;

        this.validate().then(result => {
            if (result)
                this.$uibModalInstance.close({ selectedShifts: _.filter(this.shifts, s => s.selected), includeOnDutyShifts: this.includeOnDutyShifts, onDutyShiftIds: this.onDutyShifts.map(s => s.timeScheduleTemplateBlockId) });
            else
                this.executing = false;
        });
    }


    // VALIDATION

    private validate(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        if (this.nbrOfSelectedShifts < this.shifts.length) {
            // If not all shifts are selected, check if a break overlaps any shifts
            const breaks = this.shifts[0].createBreaksFromShift();
            if (breaks.length === 0)
                deferral.resolve(true);
            else {
                _.forEach(breaks, brk => {
                    // It's OK if all overlapping shifts are either selected or not selected.
                    // But it's not OK if one shift is selected and another is not.
                    const overlappingShifts = ShiftDTO.getOverlappingShifts(this.shifts, brk);
                    if (overlappingShifts.length > 0) {
                        const selectedOverlappingShifts = _.filter(overlappingShifts, s => s.selected);

                        if (overlappingShifts.length !== selectedOverlappingShifts.length && selectedOverlappingShifts.length !== 0) {
                            const keys = [
                                "time.schedule.planning.deleteshift.cantsplitshifts.title",
                                "time.schedule.planning.deleteshift.cantsplitshifts.message",
                            ];
                            this.translationService.translateMany(keys).then(terms => {
                                this.notificationService.showDialogEx(terms["time.schedule.planning.deleteshift.cantsplitshifts.title"].format(this.shiftsDefined), terms["time.schedule.planning.deleteshift.cantsplitshifts.message"].format(this.shiftsDefined.toUpperCaseFirstLetter(), this.shiftsDefined), SOEMessageBoxImage.Forbidden);
                            });
                            deferral.resolve(false);
                        }
                    }
                });
            }
        } else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }
}
