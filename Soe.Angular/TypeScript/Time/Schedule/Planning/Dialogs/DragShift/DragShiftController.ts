import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IScheduleService } from "../../../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../../../Shared/Time/Schedule/ScheduleService";
import { EditControllerBase } from "../../../../../Core/Controllers/EditControllerBase";
import { ShiftDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { ITimeDeviationCauseDTO, ISmallGenericType, IActionResult } from "../../../../../Scripts/TypeLite.Net4";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize } from "../../../../../Util/Enumerations";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ITimeService as ISharedTimeService } from "../../../../../Shared/Time/Time/TimeService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmployeeListDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { StringUtility, Guid } from "../../../../../Util/StringUtility";
import { TimeScheduleTemplateHeadSmallDTO } from "../../../../../Common/Models/TimeScheduleTemplateDTOs";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { TemplateHelper } from "../../TemplateHelper";
import { DragShiftAction, TimeSchedulePlanningMode, Feature, TermGroup_TimeScheduleTemplateBlockType, SoeScheduleWorkRules, TermGroup_ShiftHistoryType, TermGroup_TimeSchedulePlanningViews } from "../../../../../Util/CommonEnumerations";
import { EmployeeSchedulePlacementGridViewDTO } from "../../../../../Common/Models/EmployeeScheduleDTOs";
import { DateRangeDTO } from "../../../../../Common/Models/DateRangeDTO";

export class DragShiftController extends EditControllerBase {

    // Data
    private targetDates: Date[];
    private targetShifts: ShiftDTO[] = [];
    private targetShiftsOnDuty: ShiftDTO[] = [];
    private sourcePlacement: EmployeeSchedulePlacementGridViewDTO;
    private targetPlacement: DateRangeDTO;

    // Terms
    private terms: any = [];
    private title: string;
    private headingRow1: string;
    private sourceShiftLabel: string;
    private sourceShiftDate: string;
    private sourceShiftEmployee: string;
    private targetShiftLabel: string;
    private targetShiftDate: string;
    private informationMove: string;
    private informationCopy: string;
    private informationReplace: string;
    private informationReplaceAndFree: string;
    private informationSwapEmployee: string;
    private informationAbsence: string;
    private informationWholeDayAbsence: string;
    private shiftDefined: string;
    private shiftUndefined: string;
    private shiftsDefined: string;
    private shiftsUndefined: string;
    private shiftExtraInfo: string;
    private shiftSubstituteInfo: string;
    private shiftExtraAndSubstituteInfo: string;
    private noActionAvailable: string;
    private noActionAvailableInfo: string;
    private onDutyLabel: string;

    // Lookups
    private deviationCauses: ITimeDeviationCauseDTO[];
    private employeeChilds: ISmallGenericType[];

    // Flags
    private showMove = false;
    private showCopy = false;
    private showReplace = false;
    private showReplaceAndFree = false;
    private showSwapEmployee = false;
    private showAbsence = false;

    private sourceTaskExists = false;
    private targetTaskExists = false;
    private isOverlappingForCopy = false;
    private isOverlappingForMove = false;
    private isOverlappingLended = false;

    private copyTaskWithShift = false;
    private hasExtraShift = false;
    private hasSubstituteShift = false;

    private hasInvalidSkills = false;
    private executing = false;
    private showOnDutyShifts = false;
    private includeOnDutyShifts = false;

    // Properties
    private get multipleSourceShifts(): boolean {
        return this.sourceShifts.length > 1;
    }

    private get multipleSourceDates(): boolean {
        return !this.firstSourceShift.actualStartDate.isSameDayAs(this.lastSourceShift.actualStartDate);
    }

    private get multipleSourceGuids(): boolean {
        return _.map(_.uniqBy(this.sourceShifts, s => s.link), s => s.link).length > 1;
    }

    private get firstSourceShift(): ShiftDTO {
        return _.head(_.orderBy(this.sourceShifts, 'actualStartTime'));
    }

    private get lastSourceShift(): ShiftDTO {
        return _.head(_.orderBy(this.sourceShifts, 'actualStopTime', 'desc'));
    }

    private get sourceEmployeeIsHidden(): boolean {
        return this.sourceShifts[0].employeeId === this.hiddenEmployeeId;
    }

    private get sourceShiftsWithoutAccount(): boolean {
        return _.filter(this.sourceShifts, s => s.accountId).length === 0;
    }

    private get multipleTargetShifts(): boolean {
        return this.targetShifts.length > 1;
    }

    private get multipleTargetDates(): boolean {
        const first = this.firstTargetShift;
        const last = this.lastTargetShift;

        if (first?.actualStartTime && last && last.actualStopTime)
            return !first.actualStartTime.isSameDayAs(last.actualStopTime);
        else
            return false;
    }

    private get multipleTargetGuids(): boolean {
        return _.map(_.uniqBy(this.targetShifts, s => s.link), s => s.link).length > 1;
    }

    private get firstTargetShift(): ShiftDTO {
        return _.head(_.orderBy(this.targetShifts, 'actualStartTime'));
    }

    private get lastTargetShift(): ShiftDTO {
        return _.head(_.orderBy(this.targetShifts, 'actualStopTime', 'desc'));
    }

    private get targetSlotIsEmpty(): boolean {
        return this.targetShifts.length === 0;
    }

    private get targetShiftIsAbsence(): boolean {
        return _.filter(this.targetShifts, s => s.timeDeviationCauseId).length > 0;
    }

    private get targetShiftIsLended(): boolean {
        return _.filter(this.targetShifts, s => s.isLended).length > 0;
    }

    private get targetEmployeeIsHidden(): boolean {
        return this.targetEmployee && this.targetEmployee.employeeId === this.hiddenEmployeeId;
    }

    private get targetEmployeeHasShiftAccountId(): boolean {
        let valid: boolean = true;

        if (this.targetEmployeeIsHidden && this.targetEmployee.accounts) {
            _.forEach(this.sourceShifts, shift => {
                if (!_.includes(this.targetEmployee.accounts.map(a => a.accountId), shift.accountId)) {
                    valid = false;
                    return false;
                }
            });
        }

        return valid;
    }

    private get sameAccount(): boolean {
        return this.sourceShiftsWithoutAccount || this.targetEmployeeHasShiftAccountId;
    }

    private get sourceAndTargetIsSameEmployee(): boolean {
        return (this.firstTargetShift && this.firstSourceShift.employeeId === this.firstTargetShift.employeeId);
    }

    private get sourceShiftIsOrder(): boolean {
        return _.filter(this.sourceShifts, s => s.isOrder).length > 0;
    }

    private get sourceShiftIsBooking(): boolean {
        return _.filter(this.sourceShifts, s => s.isBooking).length > 0;
    }

    private get sourceShiftIsStandby(): boolean {
        return _.filter(this.sourceShifts, s => s.isStandby).length > 0;
    }

    private get sourceShiftIsOnDuty(): boolean {
        return _.filter(this.sourceShifts, s => s.isOnDuty).length > 0;
    }

    private get sourceShiftIsAbsence(): boolean {
        return _.filter(this.sourceShifts, s => s.isAbsence).length > 0;
    }

    private get isScheduleView(): boolean {
        return (this.viewDefinition === TermGroup_TimeSchedulePlanningViews.Day || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.Schedule);
    }

    public get isTemplateView(): boolean {
        return (this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TemplateDay || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TemplateSchedule);
    }

    public get isEmployeePostView(): boolean {
        return (this.viewDefinition === TermGroup_TimeSchedulePlanningViews.EmployeePostsDay || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule);
    }

    private get isScenarioView(): boolean {
        return (this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioDay || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioSchedule || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioComplete);
    }

    private get isStandbyView(): boolean {
        return (this.viewDefinition === TermGroup_TimeSchedulePlanningViews.StandbyDay || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.StandbySchedule);
    }

    private _selectedAction: DragShiftAction = DragShiftAction.Cancel;
    private get selectedAction(): DragShiftAction {
        return this._selectedAction;
    }
    private set selectedAction(value: DragShiftAction) {
        this._selectedAction = value;

        if (this.selectedAction == DragShiftAction.Copy && this.firstSourceShift.isOrder)
            this.showOrderRemainingTimeWarning();
        else if (this._selectedAction == DragShiftAction.Absence)
            this.loadDeviationCauses();

        this.setShowOnDutyShifts();
    }

    private get selectedActionValue(): string {
        return this.selectedAction.toString();
    }
    private set selectedActionValue(value: string) {
        this.selectedAction = parseInt(value);
    }

    private _selectedDeviationCause: ITimeDeviationCauseDTO;
    private get selectedDeviationCause(): ITimeDeviationCauseDTO {
        return this._selectedDeviationCause;
    }
    private set selectedDeviationCause(value: ITimeDeviationCauseDTO) {
        this._selectedDeviationCause = value;

        if (this._selectedDeviationCause?.specifyChild)
            this.loadEmployeeChilds();
    }

    private _sourceShiftIds: number[]
    private get sourceShiftIds(): number[] {
        return this._sourceShiftIds
    }
    private set sourceShiftIds(value: number[]) {
        this._sourceShiftIds = value;
    }

    private _targetShiftIds: number[]
    private get targetShiftIds(): number[] {
        return this._targetShiftIds;
    }
    private set targetShiftIds(value: number[]) {
        this._targetShiftIds = value;
    }

    private selectedEmployeeChildId: number;
    private isWholeDayAbsence = false;

    private isFullyUnavailable = false;
    private isPartlyUnavailable = false;
    private setUnavailable() {
        this.isFullyUnavailable = false;
        this.isPartlyUnavailable = false;

        if (!this.targetEmployee || !this.targetDates)
            return;

        _.forEach(this.targetDates, date => {
            if (this.targetEmployee.isFullyUnavailableInRange(date.beginningOfDay(), date.endOfDay()))
                this.isFullyUnavailable = true;
            if (this.targetEmployee.isPartlyUnavailableInRange(date.beginningOfDay(), date.endOfDay()))
                this.isPartlyUnavailable = true;
        });

        if (this.isFullyUnavailable && this.isPartlyUnavailable)
            this.isPartlyUnavailable = false;
    }

    private standbyCycleWeek = 1;
    private standbyCycleDateFrom: Date = null;
    private standbyCycleDateTo: Date = null;

    //@ngInject
    constructor(
        private $uibModalInstance,
        $uibModal,
        coreService: ICoreService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private sharedTimeService: ISharedTimeService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private templateHelper: TemplateHelper,
        private sourceShifts: ShiftDTO[],
        private targetEmployee: EmployeeListDTO,
        private targetDate: Date,
        private targetShift: ShiftDTO,
        private moveOffsetDays: number,
        private planningMode: TimeSchedulePlanningMode,
        private viewDefinition: TermGroup_TimeSchedulePlanningViews,
        private hiddenEmployeeId: number,
        private vacantEmployeeIds: number[],
        private useVacant: boolean,
        private showExtraShift: boolean,
        private showSubstitute: boolean,
        private keepShiftsTogether: boolean,
        private skillCantBeOverridden: boolean,
        private skipWorkRules: boolean,
        private skipXEMailOnChanges: boolean,
        private useAccountHierarchy: boolean,
        private validAccountIds: number[],
        private inactivateLending: boolean,
        private timeScheduleScenarioHeadId: number,
        private onDutyShiftsModifyPermission: boolean,
        private onDutyShifts: ShiftDTO[],
        private defaultAction?: DragShiftAction,
        private changeEmployeeMode?: boolean,
        private employees?: EmployeeListDTO[],
        private planningPeriodStartDate?: Date,
        private planningPeriodStopDate?: Date) {

        super("Time.Schedule.Planning.DragShift",
            Feature.Time_Schedule_SchedulePlanning,
            $uibModal,
            translationService,
            messagingService,
            coreService,
            notificationService,
            urlHelperService);
    }

    // SETUP

    public $onInit() {
        this.sourceShiftIds = _.map(this.sourceShifts, s => s.timeScheduleTemplateBlockId);

        this.hasExtraShift = _.filter(this.sourceShifts, s => s.extraShift).length > 0;
        this.hasSubstituteShift = _.filter(this.sourceShifts, s => s.substituteShift).length > 0;

        this.createTargetDates();
        this.setUnavailable();
    }

    protected setupLookups() {
        super.setupLookups();

        this.$q.all([
            this.loadTerms(),
            this.loadTargetShifts()
        ]).then(() => {
            if (this.isTemplateView) {
                this.loadSourcePlacement();
                this.targetPlacement = this.targetEmployee && this.targetDate ? this.targetEmployee.getEmployeeSchedule(this.targetDate) : null;
            }
            this.targetShiftsLoaded();
            this.showButtons();
            this.setShowOnDutyShifts();

            this.isDirty = true; // Enable Save button
        });
    }

    private setShowOnDutyShifts() {
        if (this.onDutyShiftsModifyPermission && this.isScheduleView && (this.onDutyShifts.length > 0 || this.targetShiftsOnDuty.length > 0)) {
            this.includeOnDutyShifts = true;
            this.showOnDutyShifts = true;

            if (this.onDutyShifts.length === 0 && (this.selectedAction === DragShiftAction.Copy || this.selectedAction === DragShiftAction.Move || this.selectedAction === DragShiftAction.Absence)) {
                this.includeOnDutyShifts = false;
                this.showOnDutyShifts = false;
            }

            this.setHeadingRow1();
        }
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
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
            "time.schedule.planning.dragshift.headingrow1",
            "time.schedule.planning.dragshift.sourceshiftlabel",
            "time.schedule.planning.dragshift.targetshiftlabelempty",
            "time.schedule.planning.dragshift.targetshiftlabelexisting",
            "time.schedule.planning.dragshift.information.move",
            "time.schedule.planning.dragshift.information.copy",
            "time.schedule.planning.dragshift.information.copymultiple",
            "time.schedule.planning.dragshift.information.replace",
            "time.schedule.planning.dragshift.information.replaceandfree",
            "time.schedule.planning.dragshift.information.swapemployee",
            "time.schedule.planning.dragshift.information.absence",
            "time.schedule.planning.dragshift.information.absencemultiple",
            "time.schedule.planning.dragshift.information.wholedayabsence",
            "time.schedule.planning.dragshift.shiftcollision",
            "time.schedule.planning.editshift.missingskills",
            "time.schedule.planning.editshift.missingskillsoverride",
            "time.schedule.planning.dragshift.extrashiftinfo",
            "time.schedule.planning.dragshift.substituteinfo",
            "time.schedule.planning.dragshift.extrashiftandsubstituteinfo",
            "time.schedule.planning.dragshift.noactionavailable",
            "time.schedule.planning.dragshift.noactionavailable.targetisabsence",
            "time.schedule.planning.dragshift.noactionavailable.notsameaccount",
            "time.schedule.planning.blocktype.onduty"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            if (this.sourceShiftIsOrder) {
                this.shiftDefined = this.terms["time.schedule.planning.assignmentdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.assignmentundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.assignmentsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.assignmentsundefined"];
            } else if (this.sourceShiftIsBooking) {
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

            this.title = "{0} {1}".format(this.terms["core.edit"], this.multipleSourceShifts ? this.shiftsUndefined : this.shiftUndefined);
            this.sourceShiftLabel = this.terms["time.schedule.planning.dragshift.sourceshiftlabel"].format(this.multipleSourceShifts ? this.shiftsDefined : this.shiftDefined);
            this.sourceShiftDate = this.multipleSourceDates ? "{0} - {1}".format(this.firstSourceShift.actualStartTime.toFormattedDate(), this.lastSourceShift.actualStopTime.toFormattedDate()) : this.firstSourceShift.actualStartDate.toFormattedDate();
            this.sourceShiftEmployee = this.sourceShifts[0].employeeName;

            this.informationMove = this.terms["time.schedule.planning.dragshift.information.move"].format((this.multipleSourceShifts ? this.shiftsDefined : this.shiftDefined).toUpperCaseFirstLetter());
            this.informationCopy = (this.multipleSourceShifts ? this.terms["time.schedule.planning.dragshift.information.copymultiple"] : this.terms["time.schedule.planning.dragshift.information.copy"]).format((this.multipleSourceShifts ? this.shiftsDefined : this.shiftDefined).toUpperCaseFirstLetter());
            this.informationReplace = this.terms["time.schedule.planning.dragshift.information.replace"].format((this.multipleSourceShifts ? this.shiftsDefined : this.shiftDefined).toUpperCaseFirstLetter());
            this.informationReplaceAndFree = this.terms["time.schedule.planning.dragshift.information.replaceandfree"].format((this.multipleSourceShifts ? this.shiftsDefined : this.shiftDefined).toUpperCaseFirstLetter());
            this.informationSwapEmployee = this.terms["time.schedule.planning.dragshift.information.swapemployee"].format(this.shiftsDefined.toUpperCaseFirstLetter());
            this.informationAbsence = (this.multipleSourceShifts ? this.terms["time.schedule.planning.dragshift.information.absencemultiple"] : this.terms["time.schedule.planning.dragshift.information.absence"]).format((this.multipleSourceShifts ? this.shiftsDefined : this.shiftDefined).toUpperCaseFirstLetter());
            this.informationWholeDayAbsence = this.terms["time.schedule.planning.dragshift.information.wholedayabsence"].format(this.shiftsUndefined);

            this.shiftExtraInfo = StringUtility.ToBr(this.terms["time.schedule.planning.dragshift.extrashiftinfo"]);
            this.shiftSubstituteInfo = StringUtility.ToBr(this.terms["time.schedule.planning.dragshift.substituteinfo"]);
            this.shiftExtraAndSubstituteInfo = StringUtility.ToBr(this.terms["time.schedule.planning.dragshift.extrashiftandsubstituteinfo"]);

            this.onDutyLabel = ` (${this.terms["time.schedule.planning.blocktype.onduty"].toLocaleLowerCase()})`;

            this.setHeadingRow1();
        });
    }

    private setHeadingRow1() {
        this.headingRow1 = this.terms["time.schedule.planning.dragshift.headingrow1"].format(this.multipleSourceShifts || this.includeOnDutyShifts ? this.shiftsDefined : this.shiftDefined);
    }

    private loadDeviationCauses() {
        if (this.deviationCauses && this.deviationCauses.length > 0)
            return;

        this.sharedTimeService.getAbsenceTimeDeviationCausesFromEmployeeId(this.firstSourceShift.employeeId, this.firstSourceShift.actualStartDate, false).then(x => {
            this.deviationCauses = x;
        });
    }

    private loadTargetShifts(): ng.IPromise<any> {
        let deferral = this.$q.defer();

        this.targetShifts = [];

        if (!this.targetDate || !this.targetEmployee) {
            deferral.resolve();
        } else {
            let counter = 0;
            _.forEach(this.targetDates, date => {
                this.loadTargetShiftsForDay(date).then(x => {
                    _.forEach(_.orderBy(x, 'actualStartTime'), y => {
                        if (this.useAccountHierarchy && y.accountId && !this.inactivateLending && this.validAccountIds.length > 0 && !_.includes(this.validAccountIds, y.accountId)) {
                            y.isLended = true;
                        }
                        this.targetShifts.push(y);
                    });
                    counter++;
                    if (counter === this.targetDates.length)
                        deferral.resolve();

                    this.targetShiftIds = _.map(this.targetShifts, s => s.timeScheduleTemplateBlockId);
                });
            });
        }

        return deferral.promise;
    }

    private loadTargetShiftsForDay(date: Date): ng.IPromise<ShiftDTO[]> {
        if (this.isTemplateView) {
            return this.sharedScheduleService.getTemplateShiftsForDay(this.targetEmployee.employeeId, date, null, false, false, false, false);
        } else if (this.isEmployeePostView) {
            return this.sharedScheduleService.getEmployeePostTemplateShiftsForDay(this.targetEmployee.employeePostId, date, false, false);
        } else {
            if (this.planningMode === TimeSchedulePlanningMode.OrderPlanning)
                return this.sharedScheduleService.getShiftsForDay(this.targetEmployee.employeeId, date, [TermGroup_TimeScheduleTemplateBlockType.Booking, TermGroup_TimeScheduleTemplateBlockType.Order], false, false, null, false, false, false, true, this.timeScheduleScenarioHeadId);
            else
                return this.sharedScheduleService.getShiftsForDay(this.targetEmployee.employeeId, date, [TermGroup_TimeScheduleTemplateBlockType.Booking, TermGroup_TimeScheduleTemplateBlockType.Schedule, TermGroup_TimeScheduleTemplateBlockType.Standby, TermGroup_TimeScheduleTemplateBlockType.OnDuty], false, false, null, false, false, false, true, this.timeScheduleScenarioHeadId);
        }
    }

    private targetShiftsLoaded() {
        if (this.targetSlotIsEmpty || this.targetShiftIsAbsence)
            this.targetShiftLabel = this.terms["time.schedule.planning.dragshift.targetshiftlabelempty"].format(this.multipleSourceShifts ? this.shiftsDefined : this.shiftDefined);
        else
            this.targetShiftLabel = this.terms["time.schedule.planning.dragshift.targetshiftlabelexisting"].format(this.multipleTargetShifts ? this.shiftsDefined : this.shiftDefined);

        if (this.targetSlotIsEmpty) {
            this.targetShiftDate = this.targetDate.toFormattedDate();
            if (this.isStandbyView && this.sourceShiftIsStandby)
                this.standbyCycleDateFrom = this.standbyCycleDateTo = this.targetDate;
        } else {
            this.targetShiftDate = this.multipleTargetDates ? "{0} - {1}".format(this.firstTargetShift.actualStartTime.toFormattedDate(), this.lastTargetShift.actualStopTime.toFormattedDate()) : this.firstTargetShift.actualStartTime.toFormattedDate();
            if (this.isStandbyView && this.sourceShiftIsStandby)
                this.standbyCycleDateFrom = this.standbyCycleDateTo = this.firstTargetShift.actualStartTime.date();
        }

        this.targetShiftsOnDuty = this.targetShifts.filter(s => s.isOnDuty);
        this.targetShifts = this.targetShifts.filter(s => !s.isOnDuty);

        this.noActionAvailable = StringUtility.ToBr(this.terms["time.schedule.planning.dragshift.noactionavailable"]);
        this.noActionAvailableInfo = '';
        if (this.targetShiftIsAbsence)
            this.noActionAvailableInfo = StringUtility.ToBr(this.terms["time.schedule.planning.dragshift.noactionavailable.targetisabsence"]);
        else if (!this.sameAccount)
            this.noActionAvailableInfo = StringUtility.ToBr(this.terms["time.schedule.planning.dragshift.noactionavailable.notsameaccount"]);
    }

    private loadEmployeeChilds() {
        if (this.employeeChilds && this.employeeChilds.length > 0)
            return;

        this.sharedTimeService.getEmployeeChildsDict(this.firstSourceShift.employeeId, false).then(x => {
            this.employeeChilds = x;
            if (this.employeeChilds.length > 0)
                this.selectedEmployeeChildId = this.employeeChilds[0].id;
        });
    }

    private loadSourcePlacement() {
        if (this.firstSourceShift) {
            this.sharedScheduleService.getPlacementForEmployee(this.firstSourceShift.actualStartDate, this.firstSourceShift.employeeId).then(x => {
                this.sourcePlacement = x;
            });
        }
    }

    // EVENTS

    private targetDateChanged() {
        this.$timeout(() => {
            this.moveOffsetDays = this.targetDate ? this.targetDate.date().diffDays(this.firstSourceShift.actualStartDate) : 0;
            this.targetDates = this.targetDate ? [this.targetDate] : [];
            this.reloadTargetShifts();
        });
    }

    private targetEmployeeChanged() {
        this.$timeout(() => {
            this.reloadTargetShifts();
            this.targetPlacement = this.targetEmployee && this.targetDate ? this.targetEmployee.getEmployeeSchedule(this.targetDate) : null;
        });
    }

    private reloadTargetShifts() {
        this.loadTargetShifts().then(() => {
            this.showButtons();
        });
    }

    private isWholeDayAbsenceChanged() {
        this.$timeout(() => {
            // If whole day absence is selected, all shift will be moved including on duty shifts.
            // No checks will be done on server side.
            // Therefore we set the includeOnDutyShifts checkbox and disables it, to make it show correct behaviour.
            if (this.isWholeDayAbsence && this.onDutyShifts.length > 0) {
                this.includeOnDutyShifts = true;
                this.setHeadingRow1();
            }
        });
    }

    private includeOnDutyShiftsChanged() {
        this.$timeout(() => {
            this.setHeadingRow1();
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private close() {
        this.$uibModalInstance.close({ success: true, action: this.selectedAction, targetShifts: this.targetShifts, targetEmployeeId: this.targetEmployee ? this.getTargetEmployeeIdentifier() : 0 });
    }

    private save() {
        this.executing = true;
        this.startWork();

        this.initSave().then(save => {
            if (save) {
                this.startSave();
                this.performDragAction();
            } else {
                this.executing = false;
                this.failedSave(null, true);
            }
        });
    }

    private initSave(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        this.validateSkills().then(passedSkills => {
            this.showValidateSkillsResult(passedSkills).then(proceed => {
                if (proceed) {
                    // Validate work rules
                    this.startWork("time.schedule.planning.evaluateworkrules.executing");
                    this.validateWorkRules().then(passedWorkRules => {
                        deferral.resolve(passedWorkRules);
                    });
                } else {
                    deferral.resolve(false);
                }
            });
        });

        return deferral.promise;
    }

    private performDragAction() {
        let action: DragShiftAction = this.selectedAction;
        if (this.isStandbyView && this.sourceShiftIsStandby) {
            if (action === DragShiftAction.Copy)
                action = DragShiftAction.CopyWithCycle;
            else if (action === DragShiftAction.Move)
                action = DragShiftAction.MoveWithCycle;
        }

        let sourceTemplate: TimeScheduleTemplateHeadSmallDTO;
        let targetTemplate: TimeScheduleTemplateHeadSmallDTO;
        if (this.isTemplateView || this.isEmployeePostView) {
            sourceTemplate = this.templateHelper.getTemplateSchedule(this.getSourceEmployeeIdentifier(), this.firstSourceShift.actualStartDate);
            targetTemplate = this.templateHelper.getTemplateSchedule(this.getTargetEmployeeIdentifier(), this.targetDate);
        }

        const onDutyShiftIds: number[] = this.onDutyShifts.map(s => s.timeScheduleTemplateBlockId);

        if (!this.multipleSourceGuids) {
            // Single day

            // Get target link (Guid on target shift) or create new if target shift's link is empty (old data)
            let targetLink: string = null;
            let updateLinkOnTarget = false;
            let target: ShiftDTO = this.targetShift != null ? this.targetShift : _.find(_.filter(this.targetShifts, t => !this.firstSourceShift.accountId || t.accountId === this.firstSourceShift.accountId), s => s.link && s.type === this.firstSourceShift.type);
            if (target) {
                targetLink = target.link;
                if (!targetLink) {
                    // If no Guid exists on target (old data or no shifts in target slot),
                    // create a new Guid and set 'updateLinkOnTarget' flag so the target shift(s) will be updated with the new link on the server
                    targetLink = Guid.newGuid();
                    updateLinkOnTarget = true;
                }
            }

            let targetBlockId: number = this.targetShift != null ? this.targetShift.timeScheduleTemplateBlockId : (this.firstTargetShift?.timeScheduleTemplateBlockId || 0);
            let start = this.targetDate.mergeTime(this.targetShift != null ? this.targetShift.actualStartTime : this.firstSourceShift.actualStartTime);
            let end = this.targetShift != null ? start.addMinutes(this.targetShift.actualStopTime.diffMinutes(this.targetShift.actualStartTime)) : start.addMinutes(this.firstSourceShift.actualStopTime.diffMinutes(this.firstSourceShift.actualStartTime));

            if (this.isTemplateView || this.isEmployeePostView) {
                this.scheduleService.dragTemplateShift(action, this.firstSourceShift.timeScheduleTemplateBlockId, sourceTemplate.timeScheduleTemplateHeadId, this.firstSourceShift.actualStartDate, targetBlockId, targetTemplate.timeScheduleTemplateHeadId, start, end, this.targetEmployee.employeeId, this.targetEmployee.employeePostId, targetLink, updateLinkOnTarget, this.copyTaskWithShift).then(result => {
                    this.showPerformDragActionResult(result);
                }).catch(reason => {
                    this.failedWork(null, null, true);
                    this.notificationService.showServiceError(reason);
                });
            } else {
                this.sharedScheduleService.dragShift(action, this.firstSourceShift.timeScheduleTemplateBlockId, targetBlockId, start, end, this.targetEmployee.employeeId, targetLink, updateLinkOnTarget, this.selectedDeviationCause ? this.selectedDeviationCause.timeDeviationCauseId : 0, this.getEmployeeChildId(), this.isWholeDayAbsence, this.skipXEMailOnChanges, this.copyTaskWithShift, this.isStandbyView, this.timeScheduleScenarioHeadId, this.standbyCycleWeek, this.standbyCycleDateFrom, this.standbyCycleDateTo, this.includeOnDutyShifts, this.includeOnDutyShifts ? onDutyShiftIds : []).then(result => {
                    this.showPerformDragActionResult(result);
                }).catch(reason => {
                    this.failedWork(null, null, true);
                    this.notificationService.showServiceError(reason);
                });
            }
        } else {
            // Multiple days

            if (this.isTemplateView || this.isEmployeePostView) {
                this.scheduleService.dragTemplateShifts(action, _.map(this.sourceShifts, s => s.timeScheduleTemplateBlockId), sourceTemplate.timeScheduleTemplateHeadId, this.firstSourceShift.actualStartDate, this.moveOffsetDays, this.targetDate, this.targetEmployee.employeeId, this.targetEmployee.employeePostId, targetTemplate.timeScheduleTemplateHeadId, this.copyTaskWithShift).then(result => {
                    this.showPerformDragActionResult(result);
                }).catch(reason => {
                    this.failedWork(null, null, true);
                    this.notificationService.showServiceError(reason);
                });
            } else {
                this.scheduleService.dragShifts(action, _.map(this.sourceShifts, s => s.timeScheduleTemplateBlockId), this.moveOffsetDays, this.targetEmployee.employeeId, this.skipXEMailOnChanges, this.copyTaskWithShift, this.isStandbyView, this.timeScheduleScenarioHeadId, this.standbyCycleWeek, this.standbyCycleDateFrom, this.standbyCycleDateTo, this.includeOnDutyShifts, this.includeOnDutyShifts ? onDutyShiftIds : []).then(result => {
                    this.showPerformDragActionResult(result);
                }).catch(reason => {
                    this.failedWork(null, null, true);
                    this.notificationService.showServiceError(reason);
                });
            }
        }
    }

    // VALIDATION

    private validateSkills(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        let nbrOfShiftsChecked = 0;
        let nbrOfShiftsToCheck = 0;
        this.hasInvalidSkills = false;

        if (this.selectedAction == DragShiftAction.SwapEmployee) {
            nbrOfShiftsToCheck = 0;
            if (!this.sourceEmployeeIsHidden)
                nbrOfShiftsToCheck += this.targetShifts.length;
            if (!this.targetEmployeeIsHidden)
                nbrOfShiftsToCheck += this.sourceShifts.length;

            // Source shifts
            if (!this.targetEmployeeIsHidden) {
                // Do not validate skills for hidden employee
                _.forEach(this.sourceShifts, shift => {
                    this.employeeHasSkill(this.getTargetEmployeeIdentifier(), shift, this.moveOffsetDays).then(() => {
                        nbrOfShiftsChecked++;
                        if (nbrOfShiftsToCheck === nbrOfShiftsChecked)
                            deferral.resolve(!this.hasInvalidSkills);
                    });
                });
            }

            // Target shifts
            if (!this.sourceEmployeeIsHidden) {
                // Do not validate skills for hidden employee
                _.forEach(this.targetShifts, shift => {
                    this.employeeHasSkill(this.getSourceEmployeeIdentifier(), shift, -this.moveOffsetDays).then(() => {
                        nbrOfShiftsChecked++;
                        if (nbrOfShiftsToCheck === nbrOfShiftsChecked)
                            deferral.resolve(!this.hasInvalidSkills);
                    });
                });
            }
        } else {
            // Source shifts
            if (this.targetEmployeeIsHidden) {
                // Do not validate skills for hidden employee
                deferral.resolve(true);
            } else {
                nbrOfShiftsToCheck = this.sourceShifts.length;
                _.forEach(this.sourceShifts, shift => {
                    this.employeeHasSkill(this.getTargetEmployeeIdentifier(), shift, this.moveOffsetDays).then(() => {
                        nbrOfShiftsChecked++;
                        if (nbrOfShiftsToCheck === nbrOfShiftsChecked)
                            deferral.resolve(!this.hasInvalidSkills);
                    });
                });
            }
        }

        return deferral.promise;
    }

    private employeeHasSkill(employeeIdentifier: number, shift: ShiftDTO, daysOffset: number): ng.IPromise<any> {
        if (this.isEmployeePostView) {
            return this.scheduleService.employeePostHasSkill(employeeIdentifier, shift.shiftTypeId, shift.actualStartTime.addDays(daysOffset)).then(hasSkill => {
                if (!hasSkill)
                    this.hasInvalidSkills = true;
            });
        } else {
            return this.scheduleService.employeeHasSkill(employeeIdentifier, shift.shiftTypeId, shift.actualStartTime.addDays(daysOffset)).then(hasSkill => {
                if (!hasSkill)
                    this.hasInvalidSkills = true;
            });
        }
    }

    private showValidateSkillsResult(passed: boolean): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        if (!passed) {
            this.failedWork(null, null, true);
            let message = this.terms["time.schedule.planning.editshift.missingskills"].format(this.shiftUndefined);
            if (!this.skillCantBeOverridden)
                message += "\n" + this.terms["time.schedule.planning.editshift.missingskillsoverride"];

            const modal = this.notificationService.showDialog(this.terms["common.obs"], message, SOEMessageBoxImage.Forbidden, this.skillCantBeOverridden ? SOEMessageBoxButtons.OK : SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                deferral.resolve(val && !this.skillCantBeOverridden);
            }, (reason) => {
                deferral.resolve(false);
            });
        } else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private setOverlapping() {
        if (this.targetShifts.length === 0)
            return;

        // Validate that source and target times do not overlap (except for hidden employee or on duty shifts)
        if (!this.targetEmployeeIsHidden && !this.sourceShiftIsOnDuty) {
            // Check all source shifts against all target shifts
            _.forEach(this.sourceShifts, source => {
                this.isOverlappingTargets(source, false);
                this.isOverlappingTargets(source, true);
            });
        }
    }

    private isOverlappingTargets(source: ShiftDTO, checkForMove: boolean) {
        _.forEach(this.targetShifts, target => {
            if (target.timeScheduleTemplateBlockId !== source.timeScheduleTemplateBlockId) {
                let sourceStart: Date = source.actualStartTime.addDays(this.moveOffsetDays);
                let sourceStop: Date = source.actualStopTime.addDays(this.moveOffsetDays);
                let targetStart: Date = target.actualStartTime;
                let targetStop: Date = target.actualStopTime;

                if (checkForMove && _.includes(this.sourceShiftIds, target.timeScheduleTemplateBlockId)) {
                    targetStart = targetStart.addDays(this.moveOffsetDays);
                    targetStop = targetStop.addDays(this.moveOffsetDays);
                }

                if (CalendarUtility.getIntersectingDuration(sourceStart, sourceStop, targetStart, targetStop) > 0) {
                    if (this.planningMode === TimeSchedulePlanningMode.SchedulePlanning) {
                        if (checkForMove)
                            this.isOverlappingForMove = true;
                        else
                            this.isOverlappingForCopy = true;
                        if (target.isLended)
                            this.isOverlappingLended = true;
                    } else if (this.planningMode === TimeSchedulePlanningMode.OrderPlanning) {
                        switch (source.type) {
                            case TermGroup_TimeScheduleTemplateBlockType.Schedule:
                                if (target.isSchedule || target.isStandby) {
                                    if (checkForMove)
                                        this.isOverlappingForMove = true;
                                    else
                                        this.isOverlappingForCopy = true;
                                }
                                break;
                            case TermGroup_TimeScheduleTemplateBlockType.Order:
                            case TermGroup_TimeScheduleTemplateBlockType.Booking:
                                if (target.isOrder || target.isBooking) {
                                    if (checkForMove)
                                        this.isOverlappingForMove = true;
                                    else
                                        this.isOverlappingForCopy = true;
                                }
                                break;
                        }
                    }
                }
            }
        });
    }

    private validateWorkRules(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        let action: DragShiftAction = this.selectedAction;
        if (this.isStandbyView && this.sourceShiftIsStandby) {
            if (action === DragShiftAction.Copy)
                action = DragShiftAction.CopyWithCycle;
            else if (action === DragShiftAction.Move)
                action = DragShiftAction.MoveWithCycle;
        }

        let rules: SoeScheduleWorkRules[] = null;
        if (this.skipWorkRules) {
            // The following rules should always be evaluated
            rules = [];
            rules.push(SoeScheduleWorkRules.OverlappingShifts);
            if (!this.isTemplateView)
                rules.push(SoeScheduleWorkRules.AttestedDay);
        }

        let sourceTemplate: TimeScheduleTemplateHeadSmallDTO;
        let targetTemplate: TimeScheduleTemplateHeadSmallDTO;
        if (this.isTemplateView || this.isEmployeePostView) {
            sourceTemplate = this.templateHelper.getTemplateSchedule(this.getSourceEmployeeIdentifier(), this.firstSourceShift.actualStartDate);
            targetTemplate = this.templateHelper.getTemplateSchedule(this.getTargetEmployeeIdentifier(), this.targetDate);
        }

        if (!this.multipleSourceGuids) {
            // Single day

            let targetBlockId: number = this.targetShift?.timeScheduleTemplateBlockId ? this.targetShift.timeScheduleTemplateBlockId : (this.firstTargetShift?.timeScheduleTemplateBlockId || 0);
            let start = this.targetDate.mergeTime(this.targetShift?.actualStartTime ? this.targetShift.actualStartTime : this.firstSourceShift.actualStartTime);
            let end = this.targetDate.mergeTime(this.targetShift?.actualStopTime ? this.targetShift.actualStopTime : this.firstSourceShift.actualStopTime);

            if (this.isTemplateView || this.isEmployeePostView) {
                this.scheduleService.evaluateDragTemplateShiftAgainstWorkRules(action, this.firstSourceShift.timeScheduleTemplateBlockId, sourceTemplate.timeScheduleTemplateHeadId, this.firstSourceShift.actualStartDate, targetBlockId, targetTemplate.timeScheduleTemplateHeadId, start, end, this.targetEmployee.employeeId, this.targetEmployee.employeePostId, rules).then(result => {
                    if (!result.allRulesSucceded)
                        this.failedWork(null, null, true);

                    this.notificationService.showValidateWorkRulesResult(this.getWorkruleActionFromDragAction(), result, this.targetEmployee.employeeId).then(passed => {
                        deferral.resolve(passed);
                    });
                }).catch(reason => {
                    this.notificationService.showServiceError(reason);
                    deferral.resolve(false);
                });
            } else {
                this.sharedScheduleService.evaluateDragShiftAgainstWorkRules(action, this.firstSourceShift.timeScheduleTemplateBlockId, targetBlockId, start, end, this.targetEmployee.employeeId, this.isTemplateView, this.isWholeDayAbsence, rules, this.isStandbyView, this.timeScheduleScenarioHeadId, this.standbyCycleWeek, this.standbyCycleDateFrom, this.standbyCycleDateTo, false, this.planningPeriodStartDate, this.planningPeriodStopDate).then(result => {
                    if (!result.allRulesSucceded)
                        this.failedWork(null, null, true);

                    this.notificationService.showValidateWorkRulesResult(this.getWorkruleActionFromDragAction(), result, this.targetEmployee.employeeId).then(passed => {
                        deferral.resolve(passed);
                    });
                }).catch(reason => {
                    this.notificationService.showServiceError(reason);
                    deferral.resolve(false);
                });
            }
        } else {
            // Multiple days

            if (this.isTemplateView || this.isEmployeePostView) {
                this.scheduleService.evaluateDragTemplateShiftsAgainstWorkRules(action, _.map(this.sourceShifts, s => s.timeScheduleTemplateBlockId), sourceTemplate.timeScheduleTemplateHeadId, this.firstSourceShift.actualStartDate, this.moveOffsetDays, this.targetEmployee.employeeId, this.targetEmployee.employeePostId, targetTemplate.timeScheduleTemplateHeadId, this.targetDate, rules).then(result => {
                    if (!result.allRulesSucceded)
                        this.failedWork(null, null, true);

                    this.notificationService.showValidateWorkRulesResult(this.getWorkruleActionFromDragAction(), result, this.targetEmployee.employeeId).then(passed => {
                        deferral.resolve(passed);
                    });
                }).catch(reason => {
                    this.notificationService.showServiceError(reason);
                    deferral.resolve(false);
                });
            } else {
                this.scheduleService.evaluateDragShiftsAgainstWorkRules(action, _.map(this.sourceShifts, s => s.timeScheduleTemplateBlockId), this.moveOffsetDays, this.targetEmployee.employeeId, false, rules, this.isStandbyView, this.timeScheduleScenarioHeadId, this.standbyCycleWeek, this.standbyCycleDateFrom, this.standbyCycleDateTo, this.planningPeriodStartDate, this.planningPeriodStopDate).then(result => {
                    if (!result.allRulesSucceded)
                        this.failedWork(null, null, true);

                    this.notificationService.showValidateWorkRulesResult(this.getWorkruleActionFromDragAction(), result, this.targetEmployee.employeeId).then(passed => {
                        deferral.resolve(passed);
                    });
                }).catch(reason => {
                    this.notificationService.showServiceError(reason);
                    deferral.resolve(false);
                });
            }
        }

        return deferral.promise;
    }

    private showPerformDragActionResult(result: IActionResult) {
        this.failedSave(null, true);

        if (result.success) {
            // Success
            this.close();
        } else {
            // Failure
            this.translationService.translate("time.schedule.planning.dragshift.dragfailed").then(term => {
                this.notificationService.showDialog(term.format(this.shiftUndefined), result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
            this.executing = false;
        }
    }

    protected validate() {
        if (this.selectedAction == DragShiftAction.Absence) {
            if (!this.selectedDeviationCause || !this.selectedDeviationCause.timeDeviationCauseId)
                this.mandatoryFieldKeys.push("time.schedule.planning.dragshift.deviationcause");

            if (this.selectedDeviationCause && this.selectedDeviationCause.specifyChild && !this.selectedEmployeeChildId)
                this.mandatoryFieldKeys.push("time.schedule.planning.dragshift.child");
        }
    }

    // HELP-METHODS

    private getSourceEmployeeIdentifier(): number {
        return this.isEmployeePostView ? this.firstSourceShift.employeePostId : this.firstSourceShift.employeeId;
    }

    private getTargetEmployeeIdentifier(): number {
        return this.isEmployeePostView ? this.targetEmployee.employeePostId : this.targetEmployee.employeeId;
    }

    private getUniqueSourceDates(): Date[] {
        let dates: Date[] = [];
        _.forEach(_.orderBy(this.sourceShifts, 'actualStartTime'), shift => {
            if (!CalendarUtility.includesDate(dates, shift.actualStartDate))
                dates.push(shift.actualStartDate);
        });

        return dates;
    }

    private createTargetDates() {
        const sourceDates: Date[] = this.getUniqueSourceDates();
        this.targetDates = [];
        _.forEach(sourceDates, date => {
            this.targetDates.push(date.addDays(this.moveOffsetDays));
        });
    }

    private showButtons() {
        this.setOverlapping();

        this.showMove = !this.isOverlappingForMove && this.sameAccount && !this.sourceShiftIsAbsence;
        this.showCopy = !this.isOverlappingForCopy && this.sameAccount;
        this.showReplace = !this.sourceShiftIsStandby && !this.sourceShiftIsOnDuty && !this.isOverlappingLended && this.sameAccount && !this.isTemplateView && !this.isEmployeePostView && !this.sourceShiftIsBooking && !this.multipleSourceGuids && !this.targetShiftIsAbsence && !this.targetShiftIsLended && !this.targetSlotIsEmpty;
        this.showReplaceAndFree = !this.sourceShiftIsStandby && !this.sourceShiftIsOnDuty && !this.isOverlappingLended && this.sameAccount && !this.isTemplateView && !this.isEmployeePostView && !this.isScenarioView && !this.sourceShiftIsBooking && !this.multipleSourceGuids && !this.targetShiftIsAbsence && !this.targetShiftIsLended && !this.targetSlotIsEmpty && !this.useVacant;
        this.showSwapEmployee = !this.sourceShiftIsStandby && !this.sourceShiftIsOnDuty && !this.isOverlappingLended && this.sameAccount && !this.sourceShiftIsBooking && !this.multipleSourceGuids && !this.targetShiftIsAbsence && !this.targetShiftIsLended && !this.targetSlotIsEmpty && !this.sourceAndTargetIsSameEmployee;
        this.showAbsence = !this.sourceShiftIsStandby && !this.sourceShiftIsOnDuty && !this.isOverlappingForCopy && !this.isOverlappingForMove && this.sameAccount && !this.isTemplateView && !this.isEmployeePostView && !this.isScenarioView && !this.sourceShiftIsBooking && !this.multipleSourceGuids && !this.targetShiftIsAbsence && !this.sourceAndTargetIsSameEmployee;

        if (!this.defaultAction && this.sourceShiftIsStandby && !this.sourceShiftIsAbsence)
            this.defaultAction = DragShiftAction.Move;

        // Copy as default (if not passed as init parameter)
        if (this.defaultAction) {
            if (this.defaultAction === DragShiftAction.Copy && this.showCopy)
                this.selectedAction = DragShiftAction.Copy;
            else if (this.defaultAction === DragShiftAction.Move && this.showMove)
                this.selectedAction = DragShiftAction.Move
            else if (this.defaultAction === DragShiftAction.Replace && this.showReplace)
                this.selectedAction = DragShiftAction.Replace
            else if (this.defaultAction === DragShiftAction.ReplaceAndFree && this.showReplaceAndFree)
                this.selectedAction = DragShiftAction.ReplaceAndFree
            else if (this.defaultAction === DragShiftAction.SwapEmployee && this.showSwapEmployee)
                this.selectedAction = DragShiftAction.SwapEmployee
            else if (this.defaultAction === DragShiftAction.Absence && this.showAbsence)
                this.selectedAction = DragShiftAction.Absence
            else
                this.selectedAction = DragShiftAction.Cancel;
        } else {
            if (this.showCopy)
                this.selectedAction = DragShiftAction.Copy;
            else if (this.showMove)
                this.selectedAction = DragShiftAction.Move;
            else if (this.showReplace)
                this.selectedAction = DragShiftAction.Replace;
            else if (this.showReplaceAndFree)
                this.selectedAction = DragShiftAction.ReplaceAndFree;
            else if (this.showSwapEmployee)
                this.selectedAction = DragShiftAction.SwapEmployee;
            else if (this.showAbsence)
                this.selectedAction = DragShiftAction.Absence;
            else
                this.selectedAction = DragShiftAction.Cancel;
        }
    }

    private getEmployeeChildId(): number {
        // Must do these checks since a child might be selected and action has changed into something else after
        if (this.selectedDeviationCause) {
            if (this.selectedDeviationCause.specifyChild)
                return this.selectedEmployeeChildId;
        }

        return null;
    }

    private getWorkruleActionFromDragAction(): TermGroup_ShiftHistoryType {
        switch (this.selectedAction) {
            case DragShiftAction.Move:
                return TermGroup_ShiftHistoryType.DragShiftActionMove;
            case DragShiftAction.Copy:
                return TermGroup_ShiftHistoryType.DragShiftActionCopy;
            case DragShiftAction.Replace:
                return TermGroup_ShiftHistoryType.DragShiftActionReplace;
            case DragShiftAction.ReplaceAndFree:
                return TermGroup_ShiftHistoryType.DragShiftActionReplaceAndFree;
            case DragShiftAction.SwapEmployee:
                return TermGroup_ShiftHistoryType.DragShiftActionSwapEmployee;
            case DragShiftAction.Absence:
                return TermGroup_ShiftHistoryType.DragShiftActionAbsence;
            case DragShiftAction.Delete:
                return TermGroup_ShiftHistoryType.DragShiftActionDelete;
            default:
                return TermGroup_ShiftHistoryType.Unknown;
        }
    }

    private showOrderRemainingTimeWarning() {
        const remaining = this.firstSourceShift.order.remainingTime - this.firstSourceShift.getShiftLength();
        if (remaining < 0) {
            const keys: string[] = [
                "common.obs",
                "time.schedule.planning.dragshift.orderremainingtimecopywarning"
            ];

            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialog(terms["common.obs"], terms["time.schedule.planning.dragshift.orderremainingtimecopywarning"].format(CalendarUtility.minutesToTimeSpan(Math.abs(remaining))), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Large);
            });
        }
    }
}
