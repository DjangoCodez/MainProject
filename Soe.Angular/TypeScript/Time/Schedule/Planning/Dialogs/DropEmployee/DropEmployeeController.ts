import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IScheduleService } from "../../../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../../../Shared/Time/Schedule/ScheduleService";
import { EditControllerBase } from "../../../../../Core/Controllers/EditControllerBase";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { ShiftDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ITimeService } from "../../../../Time/TimeService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmployeeListDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { Guid } from "../../../../../Util/StringUtility";
import { TimeSchedulePlanningMode, Feature, SoeScheduleWorkRules, TermGroup_ShiftHistoryType, TermGroup_TimeSchedulePlanningViews } from "../../../../../Util/CommonEnumerations";

export class DropEmployeeController extends EditControllerBase {

    // Terms
    private terms: any = [];
    private title: string;
    private headingRow1: string;
    private headingRow2: string;
    private targetShiftLabel: string;
    private targetShiftDate: string;
    private shiftDefined: string;
    private shiftUndefined: string;
    private shiftsDefined: string;
    private shiftsUndefined: string;

    private hasInvalidSkills: boolean = false;

    // Properties
    private get multipleTargetShifts(): boolean {
        return this.targetShifts.length > 1;
    }

    private get multipleTargetDates(): boolean {
        var first = this.firstTargetShift;
        var last = this.lastTargetShift;

        if (first && first.actualStartTime && last && last.actualStartTime)
            return !first.actualStartTime.date().isSameDayAs(last.actualStopTime.date());
        else
            return false;
    }

    private get targetShiftIsOrder(): boolean {
        return _.filter(this.targetShifts, s => s.isOrder).length > 0;
    }

    private get targetShiftIsBooking(): boolean {
        return _.filter(this.targetShifts, s => s.isBooking).length > 0;
    }

    private get firstTargetShift(): ShiftDTO {
        return _.head(_.orderBy(this.targetShifts, 'actualStartTime'));
    }

    private get lastTargetShift(): ShiftDTO {
        return _.head(_.orderBy(this.targetShifts, 'actualStopTime', 'desc'));
    }

    public get isTemplateView(): boolean {
        return (this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TemplateDay || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TemplateSchedule);
    }

    // Flags
    private executing: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        $uibModal,
        coreService: ICoreService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private timeService: ITimeService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private employee: EmployeeListDTO,
        private targetShifts: ShiftDTO[],
        private targetEmployeeName: string,
        private planningMode: TimeSchedulePlanningMode,
        private viewDefinition: TermGroup_TimeSchedulePlanningViews,
        private hiddenEmployeeId: number,
        private vacantEmployeeIds: number[],
        private keepShiftsTogether: boolean,
        private skillCantBeOverridden: boolean,
        private skipWorkRules: boolean,
        private skipXEMailOnChanges: boolean,
        private timeScheduleScenarioHeadId?: number,
        private planningPeriodStartDate?: Date,
        private planningPeriodStopDate?: Date) {

        super("Time.Schedule.Planning.DropEmployee",
            Feature.Time_Schedule_SchedulePlanning,
            $uibModal,
            translationService,
            messagingService,
            coreService,
            notificationService,
            urlHelperService);
    }

    // SETUP

    protected setupLookups() {
        super.setupLookups();

        this.$q.all([
            this.loadTerms()
        ]).then(() => {
            this.stopProgress();
            this.isDirty = true; // Enable Save button
        });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.edit",
            "common.obs",
            "time.schedule.planning.shiftdefined",
            "time.schedule.planning.shiftundefined",
            "time.schedule.planning.shiftsdefined",
            "time.schedule.planning.shiftsundefined",
            "time.schedule.planning.bookingdefined",
            "time.schedule.planning.bookingundefined",
            "time.schedule.planning.bookingsdefined",
            "time.schedule.planning.bookingsundefined",
            "time.schedule.planning.assignmentdefined",
            "time.schedule.planning.assignmentundefined",
            "time.schedule.planning.assignmentsdefined",
            "time.schedule.planning.assignmentsundefined",
            "time.schedule.planning.dropemployee.title",
            "time.schedule.planning.dropemployee.headingrow1",
            "time.schedule.planning.dropemployee.headingrow2",
            "time.schedule.planning.dropemployee.targetshiftlabel",
            "time.schedule.planning.editshift.missingskills",
            "time.schedule.planning.editshift.missingskillsoverride"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            if (this.targetShiftIsOrder) {
                this.shiftDefined = this.terms["time.schedule.planning.assignmentdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.assignmentundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.assignmentsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.assignmentsundefined"];
            } else if (this.targetShiftIsBooking) {
                this.shiftDefined = this.terms["time.schedule.planning.bookingdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.bookingundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.bookingsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.bookingsundefined"];
            } else {
                this.shiftDefined = this.terms["time.schedule.planning.shiftdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.shiftundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.shiftsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.shiftsundefined"];
            }

            this.title = this.terms["time.schedule.planning.dropemployee.title"].format(this.multipleTargetShifts ? this.shiftsUndefined : this.shiftUndefined);
            this.headingRow1 = this.terms["time.schedule.planning.dropemployee.headingrow1"].format(this.employee.name, this.shiftUndefined, this.shiftDefined);
            this.headingRow2 = this.terms["time.schedule.planning.dropemployee.headingrow2"].format(this.shiftDefined, this.shiftsUndefined, this.employee.name);
            this.targetShiftLabel = this.terms["time.schedule.planning.dropemployee.targetshiftlabel"].format(this.multipleTargetShifts ? this.shiftsUndefined : this.shiftUndefined);
            this.targetShiftDate = this.multipleTargetDates ? "{0} - {1}".format(this.firstTargetShift.actualStartTime.toFormattedDate(), this.lastTargetShift.actualStopTime.toFormattedDate()) : this.firstTargetShift.actualStartTime.toFormattedDate();
        });
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private close() {
        this.$uibModalInstance.close({ success: true });
    }

    private save() {
        this.executing = true;

        this.initSave().then(save => {
            if (save) {
                this.close();
            } else {
                this.executing = false;
            }
        });
    }

    private initSave(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        // Prepare target shifts(s) for save
        var link = Guid.newGuid();
        _.forEach(this.targetShifts, shift => {
            shift.employeeId = this.employee.employeeId;

            // Fix times
            shift.setTimesForSave();

            shift.link = link;
            if (shift.break1TimeCodeId)
                shift.break1Link = link;
            if (shift.break2TimeCodeId)
                shift.break2Link = link;
            if (shift.break3TimeCodeId)
                shift.break3Link = link;
            if (shift.break4TimeCodeId)
                shift.break4Link = link;
        });

        this.validateSkills().then(passedSkills => {
            if (passedSkills) {
                this.validateWorkRules().then(passedWorkRules => {
                    deferral.resolve(passedWorkRules);
                });
            } else {
                deferral.resolve(false);
            }
        });

        return deferral.promise;
    }

    private saveShiftsForDayView() {
        this.close();
    }

    // VALIDATION

    private validateSkills(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var nbrOfShiftsChecked: number = 0;
        var nbrOfShiftsToCheck: number = this.targetShifts.length;
        this.hasInvalidSkills = false;

        // Target shifts
        _.forEach(this.targetShifts, shift => {
            this.employeeHasSkill(this.employee.employeeId, shift).then(() => {
                nbrOfShiftsChecked++;
                if (nbrOfShiftsToCheck === nbrOfShiftsChecked) {
                    if (this.hasInvalidSkills) {
                        var message = this.terms["time.schedule.planning.editshift.missingskills"].format(this.shiftUndefined);
                        if (!this.skillCantBeOverridden)
                            message += "\n" + this.terms["time.schedule.planning.editshift.missingskillsoverride"];

                        var modal = this.notificationService.showDialog(this.terms["common.obs"], message, SOEMessageBoxImage.Forbidden, this.skillCantBeOverridden ? SOEMessageBoxButtons.OK : SOEMessageBoxButtons.OKCancel);
                        modal.result.then(val => {
                            deferral.resolve(val && !this.skillCantBeOverridden);
                        }, (reason) => {
                            deferral.resolve(false);
                        });
                    } else {
                        deferral.resolve(true);
                    }
                }
            });
        });

        return deferral.promise;
    }

    private employeeHasSkill(employeeId: number, shift: ShiftDTO): ng.IPromise<any> {
        return this.scheduleService.employeeHasSkill(employeeId, shift.shiftTypeId, shift.actualStartTime).then(hasSkill => {
            if (!hasSkill)
                this.hasInvalidSkills = true;
        });
    }

    private validateWorkRules(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var rules: SoeScheduleWorkRules[] = null;
        if (this.skipWorkRules) {
            // The following rules should always be evaluated
            rules = [];
            rules.push(SoeScheduleWorkRules.OverlappingShifts);
            if (!this.isTemplateView)
                rules.push(SoeScheduleWorkRules.AttestedDay);
        }

        this.sharedScheduleService.evaluatePlannedShiftsAgainstWorkRules(this.targetShifts, rules, this.employee.employeeId, this.isTemplateView, this.timeScheduleScenarioHeadId, this.planningPeriodStartDate, this.planningPeriodStopDate).then(result => {
            this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.DropEmployeeOnShift, result, this.isTemplateView ? 0 : this.employee.employeeId).then(passed => {
                deferral.resolve(passed);
            });
        });

        return deferral.promise;
    }
}
