import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../../Core/Services/NotificationService";
import { IScheduleService as ISharedScheduleService } from "../../../../Schedule/ScheduleService";
import { EditControllerBase } from "../../../../../../Core/Controllers/EditControllerBase";
import { ShiftDTO } from "../../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { ITimeCodeBreakSmallDTO, ITimeScheduleTypeSmallDTO, ITimeScheduleShiftQueueDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { EmployeeListDTO } from "../../../../../../Common/Models/EmployeeListDTO";
import { AttestEmployeeDaySmallDTO, SaveAttestEmployeeDayDTO } from "../../../../../../Common/Models/TimeEmployeeTreeDTO";
import { TimeScheduleTemplateBlockTaskDTO } from "../../../../../../Common/Models/StaffingNeedsDTOs";
import { ICoreService } from "../../../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../../../Core/Services/UrlHelperService";
import { TimeScheduleTemplateHeadSmallDTO } from "../../../../../../Common/Models/TimeScheduleTemplateDTOs";
import { EditShiftFunctions, SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize, AbsenceRequestViewMode, AbsenceRequestGuiMode, AbsenceRequestParentMode, SOEMessageBoxButton, SaveShiftFunctions } from "../../../../../../Util/Enumerations";
import { CalendarUtility } from "../../../../../../Util/CalendarUtility";
import { Guid } from "../../../../../../Util/StringUtility";
import { EditController as MessageEditController } from "../../../../../../Core/RightMenu/MessageMenu/EditController";
import { EditController as AbsenceRequestsEditController } from "../../../Absencerequests/EditController";
import { EditShiftDebugController } from "./EditShiftDebugController";
import { SplitShiftController } from "../SplitShift/SplitShiftController";
import { ShiftAccountingController } from "../ShiftAccounting/ShiftAccountingController";
import { Feature, TermGroup_TimeScheduleTemplateBlockType, SoeTimeAttestFunctionOption, DragShiftAction, SettingMainType, UserSettingType, SoeScheduleWorkRules, TermGroup_ShiftHistoryType, TermGroup_MessageType, TermGroup_TimeScheduleTemplateBlockShiftStatus, TermGroup_TimeScheduleTemplateBlockShiftUserStatus, SoeEntityState, XEMailType } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ShiftHistoryController } from "../ShiftHistory/ShiftHistoryController";
import { AccountDTO } from "../../../../../../Common/Models/AccountDTO";
import { AccountDimDTO, AccountDimSmallDTO } from "../../../../../../Common/Models/AccountDimDTO";
import { TemplateHelper } from "../../../../../../Time/Schedule/Planning/TemplateHelper";
import { EmployeeAccountDTO } from "../../../../../../Common/Models/EmployeeUserDTO";
import { SaveAndActivateController } from "../SaveAndActivate/SaveAndActivateController";
import { DateRangeDTO } from "../../../../../../Common/Models/DateRangeDTO";
import { ShiftTypeDTO } from "../../../../../../Common/Models/ShiftTypeDTO";
import { CoreUtility } from "../../../../../../Util/CoreUtility";

export class EditShiftController extends EditControllerBase {

    // Data
    private deletedShifts: ShiftDTO[] = [];
    private tempIdCounter = 0;
    private initiallyHiddenOrVacant = false;

    // Lookups
    private breakTimeCodes: ITimeCodeBreakSmallDTO[];
    private hasTimeBreakTemplates = false;
    private placement: DateRangeDTO;

    // Terms
    private terms: any = [];
    private shiftDefined: string;
    private shiftUndefined: string;
    private shiftsDefined: string;
    private shiftsUndefined: string;
    private missingTimeCodeLabel: string;
    private queueTitle: string;
    private noValidAccountLabel: string;
    private oneDayLabel: string;

    // Properties
    private dateOptions = {
        minDate: this.scenarioDateFrom,
        maxDate: this.scenarioDateTo
    };

    private canDecreaseDate = false;
    private canIncreaseDate = false;

    private _selectedDate: Date;
    get selectedDate() {
        return this._selectedDate;
    }
    set selectedDate(date: Date) {
        if (!this.isInsideScenario(date))
            return;

        this._selectedDate = date;
        if (!date || !(date instanceof Date))
            return;

        _.forEach(this.shifts, shift => {
            const length = shift.actualStopTime.diffMinutes(shift.actualStartTime);
            shift.actualStartTime = date.mergeTime(shift.actualStartTime);
            shift.actualStopTime = shift.actualStartTime.addMinutes(length);
        });

        this.setValidAccountsForEmployee();
        this.setUnavailable();
        this.loadBreakTimeCodes();

        if (this.isTemplateView)
            this.placement = this.selectedEmployee && this.selectedDate ? this.selectedEmployee.getEmployeeSchedule(this.selectedDate) : null;
    }

    private shiftDate: Date;
    private shiftDateLabel: string;

    private _selectedEmployee: EmployeeListDTO;
    get selectedEmployee() {
        return this._selectedEmployee;
    }
    set selectedEmployee(item: EmployeeListDTO) {
        this._selectedEmployee = item;

        if (item) {
            if (this.isEmployeePostView) {
                if (this.employeeId != item.employeePostId)
                    this.employeeId = item.employeePostId;
            } else {
                if (this.employeeId != item.employeeId)
                    this.employeeId = item.employeeId;
            }
        }
        _.forEach(this.shifts, shift => {
            shift.employeeId = item ? (this.isEmployeePostView ? item.employeePostId : item.employeeId) : 0;
            shift.employeeName = item ? item.name : '';
        });

        this.setValidAccountsForEmployee();
        this.setUnavailable();
        this.loadBreakTimeCodes();

        if (this.isTemplateView)
            this.placement = this.selectedEmployee && this.selectedDate ? this.selectedEmployee.getEmployeeSchedule(this.selectedDate) : null;
    }

    private userAccountId: number = undefined;
    private userAccountName: string;
    private selectedAccountId: number;
    private selectedAccountName: string;
    private validAccountsForEmployee: AccountDTO[] = [];
    private isDefaultAccountDimLevel = false;
    private usingShiftTypeHierarchyAccounts = false;
    private allValidAccountIds: number[] = [];
    private hadShifts = false;

    private get showAccountSelector(): boolean {
        if (this.isReadonly || !this.isAdmin)
            return false;

        if (this.validAccountsForEmployee.length > 0 && (this.isValidAccount(this.selectedAccountId) || this.isValidAccount(this.userAccountId)))
            return true;

        return false;
    }

    private get allowShiftsWithoutAccount(): boolean {
        let allowShiftsWithoutAccount = false;

        if (this.useAccountHierarchy) {
            let employment = this.selectedEmployee && this.selectedDate ? this.selectedEmployee.getEmployment(this.selectedDate, this.selectedDate) : null;
            if (employment)
                allowShiftsWithoutAccount = employment.allowShiftsWithoutAccount;
        } else {
            allowShiftsWithoutAccount = true;
        }

        return allowShiftsWithoutAccount;
    }

    private isValidAccount(accountId: number): boolean {
        if (!this.accountDim)
            return false;

        return _.includes(_.map(this.accountDim.accounts, a => a.accountId), accountId);
    }

    private get breakDayMinutesAfterMidnight(): number {
        return (this.selectedEmployee && this.selectedDate ? this.selectedEmployee.getBreakDayMinutesAfterMidnight(this.selectedDate, this.selectedDate) : 0);
    }

    private get breakDayTime(): string {
        return (this.selectedDate || new Date()).beginningOfDay().addMinutes(this.breakDayMinutesAfterMidnight).toFormattedTime();
    }

    private get isHidden(): boolean {
        const employeeId: number = this.selectedEmployee ? this.selectedEmployee.employeeId : 0;
        return employeeId === this.hiddenEmployeeId;
    }

    private get isHiddenOrVacant(): boolean {
        const employeeId: number = this.selectedEmployee ? this.selectedEmployee.employeeId : 0;
        return employeeId === this.hiddenEmployeeId || _.includes(this.vacantEmployeeIds, employeeId);
    }

    private get hasShifts(): boolean {
        return this.shifts && this.shifts.length > 0;
    }

    private get hasSwapRequest(): boolean {
        return this.shifts.filter(s => s.hasSwapRequest).length > 0;
    }

    private get hasMissingTimeCodes(): boolean {
        return this.shifts.filter(s => s.isBreak && s['missingTimeCode']).length > 0;
    }

    private isFullyUnavailable = false;
    private isPartlyUnavailable = false;
    private setUnavailable() {
        this.isFullyUnavailable = this.selectedEmployee && this.selectedDate && this.selectedEmployee.isFullyUnavailableInRange(this.selectedDate.beginningOfDay(), this.selectedDate.endOfDay());
        this.isPartlyUnavailable = this.selectedEmployee && this.selectedDate && this.selectedEmployee.isPartlyUnavailableInRange(this.selectedDate.beginningOfDay(), this.selectedDate.endOfDay());
    }

    private selectedShift: ShiftDTO;
    private selectedQueue: ITimeScheduleShiftQueueDTO;

    private summaryRow1: string;
    private summaryRow2: string;

    // Flags
    private loadingShifts = false;
    private hasAbsence = false;
    private dayIsAttested = false;
    private showAccounts = false;
    private showScheduleTypes = false;
    private maxNbrOfBreaksReached = true;
    private executing = false;
    private allTasks: TimeScheduleTemplateBlockTaskDTO[] = [];
    private needToCalculateDayNumber = false;
    private hasOverlappingShifts = false;
    private hasUnsortedShifts = false;

    // Skills
    private shiftTypeIds: number[] = [];
    private _invalidSkills = false;
    get invalidSkills(): boolean {
        return this._invalidSkills;
    }
    set invalidSkills(value: boolean) {
        this._invalidSkills = value;
        if (value) {
            this.skillsOpen = true;
        }
    }
    private skillsOpen = false;
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

    private standbyShiftTypes: ShiftTypeDTO[] = [];

    // Functions
    private functions: any = [];
    private saveFunctions: any = [];

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        coreService: ICoreService,
        private sharedScheduleService: ISharedScheduleService,
        translationService: ITranslationService,
        public messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private isAdmin: boolean,
        private currentEmployeeId: number,
        private templateHelper: TemplateHelper,
        private isScheduleView: boolean,
        private isTemplateView: boolean,
        private isEmployeePostView: boolean,
        private isScenarioView: boolean,
        private isStandbyView: boolean,
        private isReadonly: boolean,
        private template: TimeScheduleTemplateHeadSmallDTO,
        private standby: boolean,
        private onDuty: boolean,
        private shift: ShiftDTO,
        private shifts: ShiftDTO[],
        private loadTasks: boolean,
        date: Date,
        private employeeId: number,
        private shiftTypes: ShiftTypeDTO[],
        private shiftTypeAccountDim: AccountDimDTO,
        private timeScheduleTypes: ITimeScheduleTypeSmallDTO[],
        private allBreakTimeCodes: ITimeCodeBreakSmallDTO[],
        private singleEmployeeMode: boolean,
        private employees: EmployeeListDTO[],
        private hiddenEmployeeId: number,
        private vacantEmployeeIds: number[],
        private showSkills: boolean,
        private standbyModifyPermission: boolean,
        private onDutyModifyPermission: boolean,
        private attestPermission: boolean,
        private hasStaffingByEmployeeAccount: boolean,
        private placementPermission: boolean,
        private showTotalCost: boolean,
        private showTotalCostIncEmpTaxAndSuppCharge: boolean,
        private showGrossTime: boolean,
        private showExtraShift: boolean,
        private showSubstitute: boolean,
        private useMultipleScheduleTypes: boolean,
        private showAvailability: boolean,
        private maxNbrOfBreaks: number,
        private clockRounding: number,
        private useAccountHierarchy: boolean,
        private accountDim: AccountDimSmallDTO,
        private accountDims: AccountDimSmallDTO[],
        private accountHierarchyId: string,
        private validAccountIds: number[],
        private showSecondaryAccounts: boolean,
        private shiftTypeMandatory: boolean,
        private keepShiftsTogether: boolean,
        private disableBreaksWithinHolesWarning: boolean,
        private disableSaveAndActivateCheck: boolean,
        private autoSaveAndActivate: boolean,
        private allowHolesWithoutBreaks: boolean,
        private skillCantBeOverridden: boolean,
        private useShiftRequestPreventTooEarly: boolean,
        private skipWorkRules: boolean,
        private skipXEMailOnChanges: boolean,
        private dayHasDeviations: boolean,
        private timeScheduleScenarioHeadId: number,
        private scenarioDateFrom: Date,
        private scenarioDateTo: Date,
        private loadedRangeDateFrom: Date,
        private loadedRangeDateTo: Date,
        private inactivateLending: boolean,
        private extraShiftAsDefaultOnHidden: boolean,
        private planningPeriodStartDate: Date,
        private planningPeriodStopDate: Date) {

        super("", Feature.Time_Schedule_SchedulePlanning, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);

        this.selectedDate = date;
        this.selectedEmployee = _.find(this.employees, e => (this.isEmployeePostView ? e.employeePostId : e.employeeId) === this.employeeId);
        this.enableMoveDays();

        this.setup();
    }

    // SETUP

    private setup() {
        if (this.timeScheduleTypes && this.timeScheduleTypes.length > 0)
            this.showScheduleTypes = true;

        // Must show gross time to show total cost
        if (this.showTotalCost && !this.showGrossTime)
            this.showTotalCost = false;

        // Setup employees to ignore skill matching on
        this.ignoreSkillEmployeeIds.push(this.hiddenEmployeeId);
        _.forEach(this.vacantEmployeeIds, vacantEmployeeId => {
            this.ignoreSkillEmployeeIds.push(vacantEmployeeId);
        });

        this.standbyShiftTypes = _.filter(this.shiftTypes, s => s.timeScheduleTemplateBlockType === TermGroup_TimeScheduleTemplateBlockType.Standby);
        this.shiftTypes = _.filter(this.shiftTypes, s => s.timeScheduleTemplateBlockType !== TermGroup_TimeScheduleTemplateBlockType.Standby);

        this.setValidAccounts();
        this.setUserAccountId();

        this.$q.all([
            this.loadTerms(),
            this.getHasBreakTemplates()
        ]).then(() => {
            this.initiallyHiddenOrVacant = this.isHiddenOrVacant;

            this.setupFunctions();
            this.setupWatchers();

            // If not passing any shifts in shifts parameter, load them based on date and employeeId
            if (!this.hasShifts)
                this.loadShifts(!this.shift, !!this.shift);
            else
                this.shiftsLoaded(this.shifts, this.shift ? this.shift.timeScheduleTemplateBlockId : 0, false);
        });
    }

    private setupFunctions() {
        if (!this.isStandbyView) {
            this.functions.push({ id: EditShiftFunctions.FillHolesWithBreaks, name: this.terms["time.schedule.planning.editshift.functions.fillholeswithbreaks"], icon: "fal fa-fw fa-mug-hot", hidden: () => { return !this.modifyPermission || this.isReadonly || !this.hasShifts; } });
            if (this.hasTimeBreakTemplates)
                this.functions.push({ id: EditShiftFunctions.CreateBreaksFromTemplates, name: this.terms["time.schedule.planning.editshift.functions.createbreaksfromtemplates"], icon: "fal fa-fw fa-mug-hot", hidden: () => { return !this.modifyPermission || this.isReadonly || !this.hasShifts || this.isEmployeePostView; } });
            this.functions.push({ hidden: () => { return !this.modifyPermission || this.isReadonly; } });
            this.functions.push({ id: EditShiftFunctions.ReSort, name: this.terms["time.schedule.planning.editshift.functions.resort"].format(this.shiftsDefined), icon: "fal fa-fw fa-sort-numeric-down", hidden: () => { return !this.modifyPermission || this.isReadonly || !this.hasShifts; } });
            this.functions.push({ hidden: () => { return !this.modifyPermission || this.isReadonly; } });
            this.functions.push({ id: EditShiftFunctions.SplitShift, name: this.terms["time.schedule.planning.editshift.functions.splitshift"].format(this.shiftUndefined), icon: "fal fa-fw fa-cut", hidden: () => { return this.hideFunctionSplitShift; } });
            this.functions.push({ hidden: () => { return this.hideFunctionSplitShift; } });
            this.functions.push({ id: EditShiftFunctions.ShiftRequest, name: this.terms["time.schedule.planning.editshift.functions.shiftrequest"], icon: "fal fa-fw fa-envelope", hidden: () => { return this.hideFunctionShiftRequest } });
            this.functions.push({ hidden: () => { return this.hideFunctionShiftRequest } });
            this.functions.push({ id: EditShiftFunctions.Absence, name: this.terms["time.schedule.planning.editshift.functions.absence"], icon: "fal fa-fw fa-medkit errorColor", hidden: () => { return this.hideFunctionAbsence }, disabled: () => { return this.isHiddenOrVacant; } });
            this.functions.push({ id: EditShiftFunctions.AbsenceRequest, name: this.terms["time.schedule.planning.editshift.functions.absencerequest"], icon: "fal fa-fw fa-plane-alt errorColor", hidden: () => { return this.hideFunctionAbsenceRequest }, disabled: () => { return this.isHiddenOrVacant; } });
            this.functions.push({ id: EditShiftFunctions.RestoreToSchedule, name: this.terms["time.schedule.planning.editshift.functions.restoretoschedule"], icon: "fal fa-fw fa-undo warningColor", hidden: () => { return this.hideFunctionRestoreToSchedule }, disabled: () => { return this.isHiddenOrVacant; } });
            this.functions.push({ id: EditShiftFunctions.RemoveAbsenceInScenario, name: this.terms["time.schedule.planning.editshift.functions.removeabsence"], icon: "fal fa-fw fa-undo warningColor", hidden: () => { return this.hideFunctionRemoveAbsence }, disabled: () => { return this.isHiddenOrVacant; } });
            this.functions.push({ hidden: () => { return this.hideFunctionAbsence && this.hideFunctionAbsenceRequest && this.hideFunctionRestoreToSchedule } });
        }
        this.functions.push({ id: EditShiftFunctions.History, name: this.terms["time.schedule.planning.editshift.functions.history"], icon: "fal fa-fw fa-history", hidden: () => { return this.isTemplateView || this.isEmployeePostView || this.isScenarioView || !this.selectedShift; }, disabled: () => { return !this.selectedShift || !this.selectedShift.timeScheduleTemplateBlockId; } });
        this.functions.push({ id: EditShiftFunctions.Accounting, name: this.terms["common.accounting"], icon: "fal fa-fw fa-columns", hidden: () => { return !this.selectedShift || this.selectedShift.isOnDuty; }, disabled: () => { return !this.selectedShift || !this.selectedShift.timeScheduleTemplateBlockId; } });

        this.saveFunctions.push({ id: SaveShiftFunctions.Save, name: this.terms["core.save"], icon: "fal fa-fw fa-save accentColor" });
        if (this.isTemplateView && this.placementPermission)
            this.saveFunctions.push({ id: SaveShiftFunctions.SaveAndActivate, name: this.terms["time.schedule.planning.saveandactivate"], icon: "fal fa-fw fa-calendar-check" });
    }

    private get hideFunctionSplitShift(): boolean {
        return this.singleEmployeeMode || !this.modifyPermission || this.isReadonly || this.isTemplateView || !this.selectedShift || this.selectedShift.isReadOnly;
    }

    private get hideFunctionShiftRequest(): boolean {
        if (this.singleEmployeeMode || !this.modifyPermission || this.isReadonly || this.isTemplateView || this.isScenarioView || !this.selectedShift || this.selectedShift.isReadOnly || this.selectedShift.isOnDuty)
            return true;

        return this.selectedShift && this.hasShifts && _.first(_.sortBy(_.map(_.filter(this.shifts, s => s.link === this.selectedShift.link), s => s.actualStopTime)))?.isBeforeOnMinute(new Date);
    }

    private get hideFunctionAbsence(): boolean {
        return !this.modifyPermission || this.isReadonly || this.isTemplateView || this.isEmployeePostView || !this.selectedShift || this.selectedShift.isReadOnly || this.selectedShift.isOnDuty;
    }

    private get hideFunctionAbsenceRequest(): boolean {
        return this.isTemplateView || this.isEmployeePostView || this.isScenarioView || !this.selectedShift || !this.selectedShift.isAbsenceRequest || this.selectedShift.isReadOnly || this.selectedShift.isOnDuty;
    }

    private get hideFunctionRestoreToSchedule(): boolean {
        return !this.modifyPermission || this.isTemplateView || this.isEmployeePostView || this.isScenarioView || !this.selectedShift || this.selectedShift.isReadOnly || this.selectedShift.isOnDuty || this.selectedShift.isLended || this.shifts.filter(s => s.isLended).length > 0;
    }

    private get hideFunctionRemoveAbsence(): boolean {
        return !this.modifyPermission || !this.isScenarioView || !this.selectedShift || this.selectedShift.isReadOnly || this.selectedShift.isOnDuty || this.selectedShift.isLended || !this.selectedShift.timeDeviationCauseId;
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.shifts.length, () => {
            this.calculateDurations();

            const breaks = _.filter(this.shifts, s => s.isBreak);
            this.maxNbrOfBreaksReached = (breaks.length >= this.maxNbrOfBreaks);
        });

        this.messagingService.subscribe(Constants.EVENT_ASSIGN_EMPLOYEE_FROM_QUEUE, (params) => {
            const shift = _.find(this.shifts, s => s.timeScheduleTemplateBlockId === params.timeScheduleTemplateBlockId);
            if (shift)
                this.performDragAction(shift, params.employeeId);
        }, this.$scope);
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.belongsto",
            "core.donotshowagain",
            "core.or",
            "core.save",
            "core.time.day",
            "core.unabletosave",
            "core.warning",
            "common.accounting",
            "common.obs",
            "common.weekshort",
            "time.schedule.planning.shiftdefined",
            "time.schedule.planning.shiftundefined",
            "time.schedule.planning.shiftsundefined",
            "time.schedule.planning.shiftsdefined",
            "time.schedule.planning.shift",
            "time.schedule.planning.shifts",
            "time.schedule.planning.break",
            "time.schedule.planning.breaks",
            "time.schedule.planning.hidden",
            "time.schedule.planning.scheduletypefactortime",
            "time.schedule.planning.grosstime",
            "time.schedule.planning.cost",
            "time.schedule.planning.editshift.shiftbelongstopreviousday",
            "time.schedule.planning.editshift.shiftbelongstocurrentday",
            "time.schedule.planning.editshift.shiftbelongstonextday",
            "time.schedule.planning.editshift.breakbelongstopreviousday",
            "time.schedule.planning.editshift.breakbelongstocurrentday",
            "time.schedule.planning.editshift.breakbelongstonextday",
            "time.schedule.planning.editshift.askadjustholes",
            "time.schedule.planning.editshift.asksavewithholes",
            "time.schedule.planning.editshift.askfillholeswithbreaks",
            "time.schedule.planning.editshift.missingskills",
            "time.schedule.planning.editshift.missingskillsoverride",
            "time.schedule.planning.editshift.onlybreaks",
            "time.schedule.planning.editshift.endbeforestart",
            "time.schedule.planning.editshift.accountmandatory",
            "time.schedule.planning.editshift.shifttypemandatory",
            "time.schedule.planning.editshift.employeemandatory",
            "time.schedule.planning.editshift.overlappingshifts",
            "time.schedule.planning.editshift.maxnbrofbreakspassed",
            "time.schedule.planning.editshift.breakoutsideworktime",
            "time.schedule.planning.editshift.breaktypemissing",
            "time.schedule.planning.editshift.breaktypelengthmismatch",
            "time.schedule.planning.editshift.overlappingbreaks",
            "time.schedule.planning.editshift.daystartswithbreak",
            "time.schedule.planning.editshift.dayendswithbreak",
            "time.schedule.planning.editshift.tasksoutsideconnectedshift",
            "time.schedule.planning.editshift.ondutyoutsideschedule",
            "time.schedule.planning.editshift.novalidaccount",
            "time.schedule.planning.editshift.missingtimecode1",
            "time.schedule.planning.editshift.missingtimecode2",
            "time.schedule.planning.editshift.missingtimecode.info",
            "time.schedule.planning.editshift.functions.fillholeswithbreaks",
            "time.schedule.planning.editshift.functions.createbreaksfromtemplates",
            "time.schedule.planning.editshift.functions.resort",
            "time.schedule.planning.editshift.functions.splitshift",
            "time.schedule.planning.editshift.functions.shiftrequest",
            "time.schedule.planning.editshift.functions.absence",
            "time.schedule.planning.editshift.functions.absencerequest",
            "time.schedule.planning.editshift.functions.restoretoschedule",
            "time.schedule.planning.editshift.functions.history",
            "time.schedule.planning.editshift.functions.restoretoschedule.message",
            "time.schedule.planning.editshift.functions.removeabsence",
            "time.schedule.planning.editshift.functions.removeabsence.message",
            "time.schedule.planning.editshiftwillrestoredaywarning",
            "time.schedule.planning.shiftqueue.title",
            "time.schedule.planning.saveandactivate",
            "time.schedule.timescheduletask.task",
            "time.schedule.timescheduletask.tasks"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.shiftDefined = this.terms["time.schedule.planning.shiftdefined"];
            this.shiftUndefined = this.terms["time.schedule.planning.shiftundefined"];
            this.shiftsDefined = this.terms["time.schedule.planning.shiftsdefined"];
            this.shiftsUndefined = this.terms["time.schedule.planning.shiftsundefined"];
            this.missingTimeCodeLabel = this.terms["time.schedule.planning.editshift.missingtimecode.info"];
            this.queueTitle = this.terms["time.schedule.planning.shiftqueue.title"].format(this.shiftUndefined);
            if (this.accountDim)
                this.noValidAccountLabel = this.terms["time.schedule.planning.editshift.novalidaccount"].format(this.accountDim.name.toLocaleLowerCase());
            this.oneDayLabel = "1 {0}".format(this.terms["core.time.day"]).toLocaleLowerCase();

            // Set template description
            if (this.template) {
                let description: string = this.template.name;
                if (this.template.startDate && !this.template.name.endsWithCaseInsensitive(this.template.startDate.toFormattedDate()))
                    description += ", {0}".format(this.template.startDate.toFormattedDate());
                if (this.template.stopDate)
                    description += " - {0}".format(this.template.stopDate.toFormattedDate());
                description += ", {0}{1}".format(CalendarUtility.getWeekNr(this.template.noOfDays).toString(), this.terms["common.weekshort"]);

                this.template['description'] = description;
            }
        });
    }

    private loadBreakTimeCodes() {
        if (!this.selectedEmployee || !this.selectedDate)
            return;

        if (this.isEmployeePostView) {
            this.sharedScheduleService.getTimeCodeBreaksForEmployeePost(this.selectedEmployee.employeePostId, true).then((x) => {
                this.breakTimeCodes = x;
                this.setBreakTimeCodes();
            });
        } else {
            this.sharedScheduleService.getTimeCodeBreaksForEmployee(this.selectedEmployee.employeeId, this.selectedDate, true).then((x) => {
                this.breakTimeCodes = x;
                this.setBreakTimeCodes();
            });
        }
    }

    private setBreakTimeCodes() {
        // Reset break time codes on existing shifts
        _.forEach(this.shifts.filter(s => s.isBreak), s => {
            if (s.break1TimeCodeId)
                this.setBreakTimeCodeFromTimeCodeId(s);
            else
                this.setBreakTimeCodeFromTime(s);
        });
    }

    private getHasBreakTemplates(): ng.IPromise<any> {
        return this.sharedScheduleService.hasTimeBreakTemplates().then(x => {
            this.hasTimeBreakTemplates = x;
        });
    }

    private isDayAttested(): ng.IPromise<any> {
        return this.sharedScheduleService.isDayAttested(this.employeeId, this.selectedDate).then(result => {
            this.dayIsAttested = result;
        })
    }

    // FUNCTIONS

    private executeFunction(option) {
        switch (option.id) {
            case EditShiftFunctions.FillHolesWithBreaks:
                this.fillHolesWithBreaks();
                break;
            case EditShiftFunctions.CreateBreaksFromTemplates:
                this.initCreateBreaksFromTemplates();
                break;
            case EditShiftFunctions.ReSort:
                this.reSortShifts();
                break;
            case EditShiftFunctions.SplitShift:
                this.openSplitShift();
                break;
            case EditShiftFunctions.ShiftRequest:
                this.openShiftRequestDialog();
                break;
            case EditShiftFunctions.Absence:
                this.openAbsenceDialog();
                break;
            case EditShiftFunctions.AbsenceRequest:
                this.openAbsenceRequestDialog();
                break;
            case EditShiftFunctions.RestoreToSchedule:
                this.restoreToSchedule();
                break;
            case EditShiftFunctions.RemoveAbsenceInScenario:
                this.removeAbsenceInScenario();
                break;
            case EditShiftFunctions.History:
                let shift: ShiftDTO = this.selectedShift ? this.selectedShift : this.shifts[0];
                this.openShiftHistory(shift);
                break;
            case EditShiftFunctions.Accounting:
                this.openAccountingDialog();
                break;
        }
    }

    // ACTIONS

    private loadShifts(addShift: boolean, onlyAddIfNoShiftsLoaded: boolean = false) {
        if (!this.selectedEmployee) {
            this.shiftsLoaded([], 0, addShift);
            return;
        }

        this.loadingShifts = true;

        // Remember selected shift
        const shiftId: number = this.shift ? this.shift.timeScheduleTemplateBlockId : 0;

        // If new shift on hidden employee, do not load existing shifts on the day
        if (shiftId === 0 && this.selectedEmployee.employeeId === this.hiddenEmployeeId) {
            this.shiftsLoaded([], 0, addShift, onlyAddIfNoShiftsLoaded);
            return;
        }

        if (this.isTemplateView) {
            this.sharedScheduleService.getTemplateShiftsForDay(this.selectedEmployee.employeeId, this.selectedDate, this.selectedEmployee.employeeId === this.hiddenEmployeeId ? this.shift.link : null, false, this.showTotalCost || this.showGrossTime, this.showTotalCostIncEmpTaxAndSuppCharge, true).then(x => {
                this.shiftsLoaded(x, shiftId, addShift, onlyAddIfNoShiftsLoaded);
            });
        } else if (this.isEmployeePostView) {
            this.sharedScheduleService.getEmployeePostTemplateShiftsForDay(this.selectedEmployee.employeePostId, this.selectedDate, false, true).then(x => {
                this.shiftsLoaded(x, shiftId, addShift, onlyAddIfNoShiftsLoaded);
            });
        } else {
            this.sharedScheduleService.getShiftsForDay(this.selectedEmployee.employeeId, this.shift ? this.shift.actualStartDate : this.selectedDate, [TermGroup_TimeScheduleTemplateBlockType.Schedule, TermGroup_TimeScheduleTemplateBlockType.Standby, TermGroup_TimeScheduleTemplateBlockType.OnDuty], true, this.showTotalCost || this.showGrossTime, this.selectedEmployee.employeeId === this.hiddenEmployeeId ? this.shift.link : null, false, true, true, true, this.timeScheduleScenarioHeadId).then(x => {
                this.shiftsLoaded(x, shiftId, addShift, onlyAddIfNoShiftsLoaded);
            });
        }
    }

    private shiftsLoaded(x: ShiftDTO[], selectedShiftId: number, addShift: boolean, onlyAddIfNoShiftsLoaded: boolean = false) {
        this.shifts = x;

        this.isNew = !this.hasShifts && addShift;

        _.forEach(this.shifts, shift => {
            shift.tempTimeScheduleTemplateBlockId = shift.timeScheduleTemplateBlockId;
            if (!shift.tempTimeScheduleTemplateBlockId)
                shift.tempTimeScheduleTemplateBlockId = ++this.tempIdCounter;

            if (this.isStandbyView && !shift.isStandby)
                shift.isReadOnly = true;

            if (shift.tasks) {
                _.forEach(shift.tasks, (task) => {
                    task.tempTimeScheduleTemplateBlockId = shift.tempTimeScheduleTemplateBlockId;
                });
            }
        });

        // Dialog was called with a new shift (eg. right click "add shift" in schedule)
        if (this.shift && !selectedShiftId) {
            this.shifts.push(this.createNewShift(null, this.standby, this.onDuty));
            if (this.useAccountHierarchy)
                this.setSelectedAccount();
        }

        this.allTasks = [];
        let sortOrder: number = 0;
        _.forEach(this.shifts, shift => {
            shift.sortOrder = ++sortOrder;
            if (shift.tasks) {
                shift.tasks = shift.tasks.map(s => {
                    let obj = new TimeScheduleTemplateBlockTaskDTO();
                    angular.extend(obj, s);
                    obj.fixDates();
                    return obj;
                });
                this.allTasks = this.allTasks.concat(shift.tasks);
            } else if (this.loadTasks) {
                this.sharedScheduleService.getShiftTasks([shift.timeScheduleTemplateBlockId]).then(t => {
                    shift.tasks = t.map(y => {
                        let obj = new TimeScheduleTemplateBlockTaskDTO();
                        angular.extend(obj, y);
                        obj.fixDates();
                        obj.tempTimeScheduleTemplateBlockId = shift.tempTimeScheduleTemplateBlockId;
                        return obj;
                    });
                    this.allTasks = this.allTasks.concat(shift.tasks);
                });
            }

            if (this.useAccountHierarchy && shift.accountId) {
                if (!this.inactivateLending) {
                    // Shift account is not valid for current user, or user is looking at another account
                    if (this.validAccountIds.length > 0 && !_.includes(this.validAccountIds, shift.accountId))
                        shift.isLended = true;
                    else if (this.isDefaultAccountDimLevel && shift.accountId !== this.userAccountId && !this.showSecondaryAccounts)
                        shift.isOtherAccount = true;

                    if (shift.isLended || shift.isOtherAccount) {
                        this.isDayAttested();

                        if (!this.hasStaffingByEmployeeAccount)
                            shift.isReadOnly = true;
                    }
                }

                // Selected account not found in collection, show account name in separate textbox beside
                shift['showAccountName'] = (!this.isReadonly && !shift.isReadOnly && !shift.isAbsence && !shift.isAbsenceRequest && !this.validAccountsForEmployee.find(a => a.accountId === shift.accountId));
            }

            if (!this.useAccountHierarchy)
                this.setRowShiftTypes(shift);
        });

        this.hasAbsence = _.some(this.shifts, s => s.timeDeviationCauseId);

        this.createBreaksFromShift();

        this.shiftDate = this.hasShifts ? this.shifts[0].actualStartDate : this.selectedDate;
        this.shiftDateLabel = '';
        if (!this.shiftDate.isSameDayAs(this.selectedDate)) {
            this.shiftDateLabel = "{0} {1}".format((<string>this.terms["core.belongsto"]).toLocaleLowerCase(), this.shiftDate.toFormattedDate());
        }

        _.forEach(this.shifts, shift => {
            shift.setBelongsToBasedOnStartTime(shift.actualDateOnLoad || this.selectedDate);
            this.setBelongsToTooltip(shift);
        });

        if (this.useAccountHierarchy && !this.inactivateLending) {
            _.forEach(_.filter(this.shifts, s => s.isBreak && s.accountId), brk => {
                if (this.validAccountIds.length > 0 && !_.includes(this.validAccountIds, brk.accountId))
                    brk.isLended = true;
                else if (this.isDefaultAccountDimLevel && brk.accountId !== this.userAccountId && !this.showSecondaryAccounts)
                    brk.isOtherAccount = true;

                if ((brk.isLended || brk.isOtherAccount) && !this.hasStaffingByEmployeeAccount)
                    brk.isReadOnly = true;
            });
        }

        if (this.isNew)
            this.setDefaultAccount();
        else
            this.setSelectedAccount();

        if (addShift && (!onlyAddIfNoShiftsLoaded || !this.hasShifts))
            this.addShift(null, this.standby, this.onDuty);
        else
            this.selectedShift = _.find(this.shifts, s => s.timeScheduleTemplateBlockId === selectedShiftId);

        this.stopProgress();
        this.loadingShifts = false;

        this.hadShifts = !this.isNew && this.hasShifts;

        if (!this.hasShifts)
            this.setShiftTypeIds();

        if (this.dayHasDeviations)
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["time.schedule.planning.editshiftwillrestoredaywarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);

        // TODO: Set selected shift to the one that was clicked on in the schedule view does not work.
        // This code on the time picker in html ruins it: set-focus="$last" on-focus="ctrl.selectedShift = shift"
        // That code will always set focus on last row.
        // Tried to add a timeout here, but all that happens is that selectedShift is correct, but focus is still on last shift.
        // Maybe the focus service could be helpful.
    }

    // EVENTS

    private addShift(createFrom: ShiftDTO, standby: boolean, onDuty: boolean, keepAllFields: boolean = false): ShiftDTO {
        let newShift = this.createNewShift(createFrom, standby, onDuty, keepAllFields);

        this.shifts.push(newShift);
        if (this.useAccountHierarchy)
            this.setSelectedAccount();

        this.$timeout(() => {
            // Need timeout to set focus on new row before sorting
            //this.reSortShifts();
            this.selectedShift = newShift;
        });
        this.setShiftTypeIds();

        return newShift;
    }

    private insertShift(shiftToInsert: ShiftDTO) {
        let readOnlyShift = false;

        let dates: Date[] = this.getUniqueDates(this.shifts);
        _.forEach(dates, (date: Date) => {
            let dayShifts = _.filter(this.shifts, s => !s.isBreak && s.actualStartDate.isSameDayAs(date));
            _.forEach(_.orderBy(dayShifts, ['actualStartTime', 'actualStopTime']), shift => {
                if (shiftToInsert !== shift) {
                    let duration = CalendarUtility.getIntersectingDuration(shift.actualStartTime, shift.actualStopTime, shiftToInsert.actualStartTime, shiftToInsert.actualStopTime);
                    if (duration > 0) {
                        if (duration === shift.getShiftLength()) {
                            // Compleately overlaps shift, delete it
                            if (this.isShiftValidToModify(shift))
                                this.deleteShift(shift);
                            else
                                readOnlyShift = true;
                        } else {
                            if (shift.actualStartTime.isBeforeOnMinute(shiftToInsert.actualStartTime) && shift.actualStopTime.isAfterOnMinute(shiftToInsert.actualStopTime)) {
                                // Split shift
                                if (this.isShiftValidToModify(shift)) {
                                    let newShift = this.addShift(shift, shift.isStandby, shift.isOnDuty, true);
                                    newShift.actualStartTime = shiftToInsert.actualStopTime;
                                    newShift.actualStopTime = shift.actualStopTime;
                                    shift.actualStopTime = shiftToInsert.actualStartTime;
                                } else {
                                    readOnlyShift = true;
                                }
                            } else if (shift.actualStartTime.isBeforeOnMinute(shiftToInsert.actualStartTime) ||
                                (shift.actualStopTime.isSameMinuteAs(shiftToInsert.actualStopTime) && shift.actualStartTime.isBeforeOnMinute(shiftToInsert.actualStartTime))) {
                                // Modify shift stop
                                if (this.isShiftValidToModify(shift)) {
                                    if (shift.actualStopTime.isAfterOnMinute(shiftToInsert.actualStartTime)) {
                                        shift.actualStopTime = shiftToInsert.actualStartTime;
                                    }
                                } else {
                                    readOnlyShift = true;
                                }
                            } else if (shift.actualStartTime.isAfterOnMinute(shiftToInsert.actualStartTime) ||
                                (shift.actualStartTime.isSameMinuteAs(shiftToInsert.actualStartTime) && shift.actualStopTime.isAfterOnMinute(shiftToInsert.actualStopTime))) {
                                // Modify shift start
                                if (this.isShiftValidToModify(shift)) {
                                    if (shift.actualStartTime.isBeforeOnMinute(shiftToInsert.actualStopTime)) {
                                        shift.actualStartTime = shiftToInsert.actualStopTime;
                                    }
                                } else {
                                    readOnlyShift = true;
                                }
                            }
                        }
                    }
                }
            });
        });

        if (readOnlyShift) {
            const keys: string[] = [
                "time.schedule.planning.editshift.insertshift.readonly.title",
                "time.schedule.planning.editshift.insertshift.readonly.message"
            ];
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["time.schedule.planning.editshift.insertshift.readonly.title"], terms["time.schedule.planning.editshift.insertshift.readonly.message"], SOEMessageBoxImage.Forbidden);
            });
        } else {
            this.reSortShifts();
            this.validateOverlappingShifts();
        }
    }

    private isShiftValidToModify(shift: ShiftDTO): boolean {
        return !this.isReadonly && !shift.isReadOnly && !shift.isAbsence && !shift.isAbsenceRequest;
    }

    private deleteShift(shift: ShiftDTO) {
        // If shift to delete has been saved, store it in deleted collection
        // Must be passed when saving
        if (!shift.isBreak && shift.timeScheduleTemplateBlockId) {
            shift.startTime = shift.actualStartTime = shift.stopTime = shift.actualStopTime = shift.startTime.beginningOfDay();
            if (shift.isStandby) {
                shift.type = TermGroup_TimeScheduleTemplateBlockType.Schedule;
                shift.shiftTypeId = null;
            }
            shift.isDeleted = true;
            this.deletedShifts.push(shift);
            this.setConnectedTasksToDeleted(shift);
        }

        _.pull(this.shifts, shift);
        this.setShiftTypeIds();

        if (shift.isBreak) {
            this.setBreaksOnShifts();

            // Set shift which break belongs to as modified
            _.forEach(_.filter(this.shifts, s => !s.isBreak && !s.isStandby && !s.isOnDuty), shft => {
                const duration = CalendarUtility.getIntersectingDuration(shft.actualStartTime, shft.actualStopTime, shift.actualStartTime, shift.actualStopTime);
                if (duration > 0)
                    this.setModified(shft);
            });
        }

        this.setModified(shift);

        if (this.selectedShift === shift)
            this.selectedShift = this.shift = (this.hasShifts ? this.getLastShift() : null);
    }

    private addBreak() {
        let newBreak = this.createNewShift(null, false, false);
        newBreak.isBreak = true;

        this.shifts.push(newBreak);
        this.setBreaksOnShifts();
    }

    private startChanged(shift: ShiftDTO) {
        this.$timeout(() => {
            shift.roundTimes(this.clockRounding);
            this.timeChanged(shift);
        });
    }

    private stopChanged(shift: ShiftDTO) {
        this.$timeout(() => {
            shift.roundTimes(this.clockRounding);
            this.timeChanged(shift);
        });
    }

    private timeChanged(shift: ShiftDTO) {
        if (shift.actualStartTime) {
            shift.date = shift.actualStartTime.beginningOfDay();
            shift.setBelongsToBasedOnStartTime(this.selectedDate);
            this.setBelongsToTooltip(shift);

            if (shift.actualStopTime) {
                // Handle over midnight
                if (shift.actualStopTime.isBeforeOnMinute(shift.actualStartTime))
                    shift.actualStopTime = shift.actualStopTime.addDays(1);
                // Handle switch back (if end is set to less than start, and then back again)
                // So, if shift ends more than 24 hours after it starts, reduce end by 24 hours
                while (shift.actualStopTime.isSameOrAfterOnMinute(shift.actualStartTime.addDays(1))) {
                    shift.actualStopTime = shift.actualStopTime.addDays(-1);
                }

                // Make it possible to have a 24 hour standby shift
                if (shift.isStandby && shift.actualStartTime.isSameMinuteAs(shift.actualStopTime))
                    shift.actualStopTime = shift.actualStopTime.addDays(1);

                shift.startTime = shift.actualStartTime.beginningOfDay();
                shift.stopTime = shift.actualStopTime.endOfDay();
            }
        }

        this.setModified(shift);
        this.$timeout(() => {
            this.calculateDurations();
            if (shift.isBreak)
                this.setBreakTimeCodeFromTime(shift);

            this.setBreaksOnShifts();
            this.validateOverlappingShifts();
            this.validateSortOrder();
        });
    }

    private selectedDateChanged(addShift: boolean = true) {
        this.$timeout(() => {
            this.shift = null;
            this.shifts = [];
            this.loadShifts(addShift, true);
        });
    }

    private decreaseDate() {
        this.moveDays(-1);
    }

    private increaseDate() {
        this.moveDays(1);
    }

    private moveDays(days: number) {
        if (this.selectedDate) {
            this.loadingShifts = true;
            this.shift = null;
            this.shifts = [];
            // Set date only to private variable.
            // This will update the date in the GUI, but will not trigger code in the set property method.
            this._selectedDate = this._selectedDate.addDays(days);
            this.enableMoveDays();
            this.doMoveDays();
        }
    }

    private doMoveDays = _.debounce(() => {
        // Use debounce to enable fast clicking on increase/decrease date buttons without loading after each click.
        // Need to set selectedDate here on last click to trigger the set property method and run code in there.
        this.selectedDate = this._selectedDate;
        this.selectedDateChanged(false);
    }, 500, { leading: false, trailing: true });

    private enableMoveDays() {
        if (this._selectedDate && this.selectedEmployee && this.loadedRangeDateFrom && this.loadedRangeDateTo) {
            this.canDecreaseDate = this._selectedDate.addDays(-1).isSameOrAfterOnDay(this.loadedRangeDateFrom.beginningOfYear()) && this.selectedEmployee.hasEmployeeSchedule(this._selectedDate.addDays(-1));
            this.canIncreaseDate = this._selectedDate.addDays(1).isSameOrBeforeOnDay(this.loadedRangeDateTo.endOfYear()) && this.selectedEmployee.hasEmployeeSchedule(this._selectedDate.addDays(1));
        }
    }

    private selectedEmployeeChanged() {
        this.$timeout(() => {
            this.shift = null;
            this.shifts = [];
            this.loadShifts(true, true);
        });
    }

    private defaultAccountChangedFromGui() {
        this.$timeout(() => {
            this.defaultAccountChanged();
        });
    }

    private defaultAccountChanged() {
        _.forEach(_.filter(this.shifts, s => !s.isBreak && !s.isLended && !s.isOtherAccount), shift => {
            if (shift.accountId !== this.selectedAccountId) {
                shift.accountId = this.selectedAccountId ? this.selectedAccountId : null;
                this.setModified(shift);
            }
            this.setRowShiftTypes(shift);
        });
    }

    private accountChanged(shift: ShiftDTO) {
        this.$timeout(() => {
            this.setModified(shift);
            this.setSelectedAccount();
        });
    }
    private shiftTypeChangedFromGui(shift: ShiftDTO) {
        this.$timeout(() => {
            this.shiftTypeChanged(shift);
        });
    }

    private shiftTypeChanged(shift: ShiftDTO) {
        this.setModified(shift);
        // Set TimeScheduleType from ShiftType
        let shiftType = _.find(this.shiftTypes, s => s.shiftTypeId === shift.shiftTypeId);
        if (!shiftType)
            shiftType = _.find(this.standbyShiftTypes, s => s.shiftTypeId === shift.shiftTypeId);
        let scheduleType = (shiftType && shiftType.timeScheduleTypeId) ? _.find(this.timeScheduleTypes, t => t.timeScheduleTypeId === shiftType.timeScheduleTypeId) : null;
        this.setTimeScheduleType(shift, scheduleType);
        this.setShiftTypeTimeScheduleType(shift, scheduleType);
        this.setShiftTypeIds();
    }

    private timeScheduleTypeChanged(shift: ShiftDTO) {
        this.$timeout(() => {
            this.setModified(shift);
            let timeScheduleType = _.find(this.timeScheduleTypes, t => t.timeScheduleTypeId === shift.timeScheduleTypeId);
            this.setTimeScheduleType(shift, timeScheduleType);
        });
    }

    private breakTimeCodeChanged(shift: ShiftDTO) {
        this.$timeout(() => {
            this.setModified(shift);
            this.setTimeFromBreakTimeCode(shift, null);
            this.setBreaksOnShifts();
            this.calculateDurations();
        });
    }

    private toggleBelongsToPreviousDay(shift: ShiftDTO) {
        // Toggle order:
        // Next (starts yesterday) | Current (starts today) | Previous (starts tomorrow)

        if (shift.belongsToNextDay) {
            // Go from yesterday to today
            shift.belongsToNextDay = false;
            shift.belongsToPreviousDay = false;
            shift.actualStartTime = this.selectedDate.mergeTime(shift.actualStartTime);
        } else if (shift.belongsToPreviousDay) {
            // Go from tomorrow to yesterday

            shift.belongsToNextDay = true;
            shift.belongsToPreviousDay = false;
            shift.actualStartTime = this.selectedDate.mergeTime(shift.actualStartTime).addDays(-1);
        } else {
            // Go from today to tomorrow
            shift.belongsToNextDay = false;
            shift.belongsToPreviousDay = true;
            shift.actualStartTime = this.selectedDate.mergeTime(shift.actualStartTime).addDays(1);
        }

        shift.actualStopTime = shift.actualStartTime.addMinutes(shift.duration);
        shift.date = shift.actualStartTime.beginningOfDay();

        this.setBelongsToTooltip(shift);

        this.setBreaksOnShifts();
        this.setModified(shift);
    }

    private setBelongsToTooltip(shift: ShiftDTO) {
        let tooltip: string = '';

        if (shift.belongsToNextDay)
            tooltip = shift.isBreak ? this.terms["time.schedule.planning.editshift.breakbelongstonextday"] : this.terms["time.schedule.planning.editshift.shiftbelongstonextday"];
        else if (shift.belongsToPreviousDay)
            tooltip = shift.isBreak ? this.terms["time.schedule.planning.editshift.breakbelongstopreviousday"] : this.terms["time.schedule.planning.editshift.shiftbelongstopreviousday"];
        else
            tooltip = shift.isBreak ? this.terms["time.schedule.planning.editshift.breakbelongstocurrentday"] : this.terms["time.schedule.planning.editshift.shiftbelongstocurrentday"];

        tooltip += " ({0})".format(shift.actualStartTime.toFormattedDate());
        shift['belongsToTitle'] = tooltip;
    }

    private toggleBelongsToPreviousDayFromGui(shift: ShiftDTO) {
        this.$timeout(() => {
            this.toggleBelongsToPreviousDay(shift);
            this.reSortShifts();
        });
    }

    private toggleAccounts() {
        this.showAccounts = !this.showAccounts;
    }

    private cancel() {
        this.$uibModalInstance.close({ reload: false });
    }

    private executeSaveFunction(option) {
        switch (option.id) {
            case SaveShiftFunctions.Save:
                if (this.isTemplateView && this.placementPermission && this.autoSaveAndActivate && this.placement)
                    this.saveAndActivate();
                else
                    this.save();
                break;
            case SaveShiftFunctions.SaveAndActivate:
                this.saveAndActivate();
                break;
        }
    }

    private save() {
        this.executing = true;

        this.initSave(false).then(passed => {
            if (passed) {
                let data = this.prepareToSave();
                if (data) {
                    data.guid = Guid.newGuid().toString();
                    this.messagingService.publish(Constants.EVENT_SAVE_SHIFTS, data);
                }
            }

            this.executing = false;
        });
    }

    private initSave(activate: boolean): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        // Remove empty rows
        _.forEach(_.filter(this.shifts, s => s.getShiftLength() === 0), shift => {
            this.deleteShift(shift);
        });

        this.validateSaveActivatedTemplate(activate).then(result => {
            if (result === null) {
                // User cancelled
                deferral.resolve(false);
                return;
            } else if (result === true) {
                // Save and activate instead
                this.saveAndActivate();
                deferral.resolve(false);
                return;
            }

            // Common validation for shifts and breaks
            this.validateCommon().then(passedCommon => {
                if (!passedCommon)
                    deferral.resolve(false);
                else {
                    // Validate holes with breaks
                    this.validateHolesWithBreaks().then(passedHolesWithBreaks => {
                        if (!passedHolesWithBreaks)
                            deferral.resolve(false);
                        else {
                            // Validate holes without breaks
                            this.validateHolesWithoutBreaks().then(passedHolesWithoutBreaks => {
                                if (!passedHolesWithoutBreaks)
                                    deferral.resolve(false);
                                else {
                                    // Validate skills
                                    this.validateSkills().then(passedSkills => {
                                        if (!passedSkills)
                                            deferral.resolve(false);
                                        else {
                                            // Validate on duty shifts
                                            this.validateOnDutyShifts().then(passedOnDuty => {
                                                if (!passedOnDuty)
                                                    deferral.resolve(false);
                                                else {
                                                    // Only description can be modified on absence shifts, no need to evaluate rules
                                                    // No evaluation for hidden or vacant employee either
                                                    let modifiedShifts = _.filter(this.shifts, s => s.isModified);
                                                    const onlyAbsence: boolean = _.filter(modifiedShifts, s => s.timeDeviationCauseId).length === modifiedShifts.length;
                                                    const onlyHidden: boolean = _.filter(modifiedShifts, s => s.employeeId === this.hiddenEmployeeId).length == modifiedShifts.length;
                                                    if (this.hasShifts && modifiedShifts.length > 0 && (onlyAbsence || onlyHidden)) {
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
                        }
                    });
                }
            })
        });

        return deferral.promise;
    }

    private prepareToSave() {
        let shiftsToSave = _.filter(this.shifts, s => !s.isBreak);

        // If only one shift, make sure breaks have the same link
        if (shiftsToSave.length === 1 || (shiftsToSave.length > 0 && (this.keepShiftsTogether || this.isHidden))) {
            const link = shiftsToSave[0].link || Guid.newGuid();

            shiftsToSave.forEach(shift => {
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
        }

        // Add deleted shifts to collection
        if (this.deletedShifts.length > 0)
            shiftsToSave = shiftsToSave.concat(this.deletedShifts);

        _.forEach(shiftsToSave, s => {
            s.tasks = _.filter(this.allTasks, t => t.tempTimeScheduleTemplateBlockId === s.tempTimeScheduleTemplateBlockId)
        });

        let data: any = {};
        data.employeeIdentifier = this.selectedEmployee ? (this.isEmployeePostView ? this.selectedEmployee.employeePostId : this.selectedEmployee.employeeId) : 0;
        data.shifts = shiftsToSave;
        data.disableBreaksWithinHolesWarning = this.disableBreaksWithinHolesWarning;
        if (this.template) {
            data.timeScheduleTemplateHeadId = this.template.timeScheduleTemplateHeadId;
            data.needToCalculateDayNumber = this.needToCalculateDayNumber;
            data.disableSaveAndActivateCheck = this.disableSaveAndActivateCheck;
            data.autoSaveAndActivate = this.autoSaveAndActivate;
        }

        return data;
    }

    private saveAndActivate() {
        this.executing = true;

        this.initSave(true).then(passed => {
            if (passed) {
                let data = this.prepareToSave();
                this.openSaveAndActivateDialog(data);
            }

            this.executing = false;
        });

    }

    private performDragAction(shift: ShiftDTO, employeeId: number, evaluateWorkRules: boolean = true) {
        if (evaluateWorkRules) {
            this.validateWorkRulesForAssign(shift, employeeId).then(result => {
                if (result)
                    this.performDragAction(shift, employeeId, false);
            });
        } else {
            this.startSave();
            this.executing = true;
            this.sharedScheduleService.dragShift(DragShiftAction.Move, shift.timeScheduleTemplateBlockId, 0, this.shift.actualStartTime, this.shift.actualStopTime, employeeId, null, false, 0, null, false, this.skipXEMailOnChanges, false, this.isStandbyView, this.timeScheduleScenarioHeadId).then(result => {
                if (result.success) {
                    // Success
                    this.completedSave(null, true);

                    let employeeIds: number[] = [];
                    employeeIds.push(shift.employeeId);
                    employeeIds.push(employeeId);
                    this.$uibModalInstance.close({ reload: true, reloadEmployeeIds: employeeIds });
                } else {
                    // Failure
                    this.translationService.translate("time.schedule.planning.shiftqueue.assignfromqueue.error").then(term => {
                        this.failedSave(term.format(this.shiftUndefined));
                    });
                    this.executing = false;
                }
            });
        }
    }

    private validateSaveActivatedTemplate(activate: boolean): ng.IPromise<any> {
        let deferral = this.$q.defer<any>();

        if (!activate && !this.disableSaveAndActivateCheck && this.placement) {
            let keys: string[] = [
                "time.schedule.planning.scheduleactivated.title",
                "time.schedule.planning.scheduleactivated.message"
            ];

            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["time.schedule.planning.scheduleactivated.title"] + " " + this.placement.stop.toFormattedDate(), terms["time.schedule.planning.scheduleactivated.message"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNoCancel, { showCheckBox: true, checkBoxLabel: this.terms["core.donotshowagain"] }).result.then(val => {
                    // Remember setting
                    if (val.isChecked) {
                        this.disableSaveAndActivateCheck = true;
                        this.autoSaveAndActivate = val.result;
                    }

                    deferral.resolve(val.result);
                }, (reason) => {
                    deferral.resolve(null);
                });
            });
        } else {
            deferral.resolve(false);
        }

        return deferral.promise;
    }

    private validateOverlappingShifts() {
        if (this.isStandbyView)
            return;

        // This method is copied from part of validateCommon below, just to get a small part used to check for overlapping shifts

        this.hasOverlappingShifts = false;

        // Clear flag on all shifts
        _.filter(this.shifts, s => !s.isBreak).forEach(s => {
            s['isOverlapping'] = false;
        });

        let dates: Date[] = this.getUniqueDates(this.shifts);
        _.forEach(dates, (date: Date) => {
            let dayShifts = _.filter(this.shifts, s => !s.isBreak && !s.isOnDuty && s.actualStartDate.isSameDayAs(date));
            let prevShift: ShiftDTO = null;
            _.forEach(_.orderBy(dayShifts, ['actualStartTime', 'actualStopTime']), shift => {
                // Check for overlapping shifts
                if (prevShift) {
                    // If hidden employee only validate if shifts are linked
                    if (shift.employeeId !== this.hiddenEmployeeId || (prevShift.link && prevShift.link === shift.link)) {
                        // Overlapping
                        if (shift.actualStartTime.isBeforeOnMinute(prevShift.actualStopTime)) {
                            this.hasOverlappingShifts = true;
                            prevShift['isOverlapping'] = true;
                            shift['isOverlapping'] = true;
                        }
                    }
                }

                prevShift = shift;
            });
        });
    }

    private validateCommon(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        // Validation for all shifts
        let validationErrors: string = '';
        let isValid = true;

        // AccountId must be set on all shifts if using account hierarchy,
        // unless setting on employee group allows it
        if (this.useAccountHierarchy && !this.allowShiftsWithoutAccount) {
            if (_.filter(this.shifts, s => !s.isBreak && !s.accountId).length > 0) {
                isValid = false;
                validationErrors += this.terms["time.schedule.planning.editshift.accountmandatory"].format(this.accountDim.name.toLocaleLowerCase()) + "\n";
            }
        }

        // Only breaks not allowed
        if (_.filter(this.shifts, s => s.isBreak).length > 0 && _.filter(this.shifts, s => !s.isBreak).length === 0) {
            isValid = false;
            validationErrors += this.terms["time.schedule.planning.editshift.onlybreaks"] + "\n";
        }

        // Validate shift start/end
        if (_.filter(this.shifts, s => s.actualStartTime.isAfterOnMinute(s.actualStopTime)).length > 0) {
            isValid = false;
            validationErrors += this.terms["time.schedule.planning.editshift.endbeforestart"].format(this.shiftDefined.toUpperCaseFirstLetter()) + "\n";
        }

        // ShiftType mandatory (company setting)
        // Not mandatory on breaks or absence
        if (this.shiftTypeMandatory && _.filter(this.shifts, s => !s.isBreak && !s.timeDeviationCauseId && !s.shiftTypeId).length > 0) {
            isValid = false;
            validationErrors += this.terms["time.schedule.planning.editshift.shifttypemandatory"] + "\n";
        }

        // Employee mandatory
        if (_.filter(this.shifts, s => !s.employeeId && !s.employeePostId).length > 0) {
            isValid = false;
            validationErrors += this.terms["time.schedule.planning.editshift.employeemandatory"];
            validationErrors += " {0} '{1}'".format(this.terms["core.or"].toLocaleLowerCase(), this.terms["time.schedule.planning.hidden"].format(this.shiftUndefined));
            validationErrors += "\n";
        }

        // Validation for each shift
        let dates: Date[] = this.getUniqueDates(this.shifts);
        _.forEach(dates, (date: Date) => {
            let dayShifts = _.filter(this.shifts, s => !s.isBreak && !s.isOnDuty && s.actualStartDate.isSameDayAs(date));
            let dayBreaks = _.filter(this.shifts, s => s.isBreak && s.actualStartDate.isSameDayAs(date));

            // Prescence
            let shiftsOverlapping = false;
            let tasksOutsideConnectedShift = false;
            let prevShift: ShiftDTO = null;
            _.forEach(_.orderBy(dayShifts, ['actualStartTime', 'actualStopTime']), shift => {
                // Check for overlapping shifts
                if (prevShift) {
                    // If hidden employee only validate if shifts are linked
                    if (shift.employeeId !== this.hiddenEmployeeId || (prevShift.link && prevShift.link === shift.link)) {
                        // Overlapping
                        if (shift.actualStartTime.isBeforeOnMinute(prevShift.actualStopTime))
                            shiftsOverlapping = true;
                    }
                }

                //Check for tasks outside its connected shift
                let tasks: TimeScheduleTemplateBlockTaskDTO[] = _.filter(this.allTasks, t => t.tempTimeScheduleTemplateBlockId === shift.tempTimeScheduleTemplateBlockId)
                _.forEach(tasks, (task: TimeScheduleTemplateBlockTaskDTO) => {
                    task.startTime = CalendarUtility.convertToDate(task.startTime);
                    task.stopTime = CalendarUtility.convertToDate(task.stopTime);

                    let taskStartTime = (this.isTemplateView || this.isEmployeePostView) ? shift.date.mergeTime(task.startTime) : task.startTime;
                    let taskStopTime = (this.isTemplateView || this.isEmployeePostView) ? taskStartTime.addMinutes(task.stopTime.diffMinutes(task.startTime)) : task.stopTime;
                    if (taskStartTime.isBeforeOnMinute(shift.actualStartTime) || taskStopTime.isAfterOnMinute(shift.actualStopTime))
                        tasksOutsideConnectedShift = true;
                });
                prevShift = shift;
            });

            // Breaks
            // Check max number of breaks per day against setting
            if (dayBreaks.length > this.maxNbrOfBreaks) {
                isValid = false;
                validationErrors += this.terms["time.schedule.planning.editshift.maxnbrofbreakspassed"].format(date.toFormattedDate(), dayBreaks.length, this.maxNbrOfBreaks) + "\n";
            }

            let breaksOverlapping = false;
            let prevBreak: ShiftDTO = null;
            _.forEach(_.orderBy(dayBreaks, ['actualStartTime', 'actualStopTime']), brk => {
                // Verify that break is within start and end time of day.
                // Make sure a break is not spanning over two shifts with different links.
                let breakTimeIsValid = true;
                let dayStartsWithBreak = false;
                let dayEndsWithBreak = false;
                let previousShift: ShiftDTO = null;
                _.forEach(_.orderBy(dayShifts, ['actualStartTime', 'actualStopTime']), shift => {
                    // Break starts before first shift start
                    if (!previousShift) {
                        if (brk.actualStartTime.isSameOrBeforeOnMinute(shift.actualStartTime)) {
                            breakTimeIsValid = false;
                            if (brk.actualStartTime.isSameMinuteAs(shift.actualStartTime))
                                dayStartsWithBreak = true;
                            return false;
                        }
                    }

                    if (previousShift && previousShift.link !== shift.link) {
                        // Shift is not linked with previous shifts, check intersecting breaks
                        let prevIntersect = CalendarUtility.getIntersectingDuration(previousShift.actualStartTime, previousShift.actualStopTime, brk.actualStartTime, brk.actualStopTime);
                        let currentIntersect = CalendarUtility.getIntersectingDuration(shift.actualStartTime, shift.actualStopTime, brk.actualStartTime, brk.actualStopTime);
                        // Current break intersects with both previous shift and current shift
                        if (prevIntersect > 0 && currentIntersect > 0) {
                            breakTimeIsValid = false;
                            return false;
                        }
                    }

                    previousShift = shift;
                });

                // Break ends after last shift ends
                if (breakTimeIsValid && previousShift && brk.actualStopTime.isSameOrAfterOnMinute(previousShift.actualStopTime)) {
                    breakTimeIsValid = false;
                    if (brk.actualStopTime.isSameMinuteAs(previousShift.actualStopTime))
                        dayEndsWithBreak = true;
                }

                if (!breakTimeIsValid) {
                    isValid = false;
                    if (dayStartsWithBreak)
                        validationErrors += this.terms["time.schedule.planning.editshift.daystartswithbreak"] + "\n";
                    else if (dayEndsWithBreak)
                        validationErrors += this.terms["time.schedule.planning.editshift.dayendswithbreak"] + "\n";
                    else
                        validationErrors += this.terms["time.schedule.planning.editshift.breakoutsideworktime"] + "\n";
                }

                // Verify that break start/end times corresponds with break type length
                let breakTimeCode: ITimeCodeBreakSmallDTO = _.find(this.breakTimeCodes, t => t.timeCodeId === brk.break1TimeCodeId);
                let breakLength: number = brk.getShiftLength();
                if (!breakTimeCode && breakLength > 0) {
                    isValid = false;
                    validationErrors += this.terms["time.schedule.planning.editshift.breaktypemissing"].format(brk.actualStartTime.toFormattedTime(), brk.actualStopTime.toFormattedTime()) + "\n";
                } else if (breakTimeCode && breakLength !== breakTimeCode.defaultMinutes) {
                    isValid = false;
                    validationErrors += this.terms["time.schedule.planning.editshift.breaktypelengthmismatch"].format(brk.actualStartTime.toFormattedTime(), brk.actualStopTime.toFormattedTime(), breakLength, breakTimeCode.defaultMinutes) + "\n";
                }

                // Overlapping
                if (prevBreak && brk.actualStartTime.isBeforeOnMinute(prevBreak.actualStopTime))
                    breaksOverlapping = true;

                prevBreak = brk;
            });

            if (shiftsOverlapping) {
                isValid = false;
                validationErrors += this.terms["time.schedule.planning.editshift.overlappingshifts"].format(this.shiftsUndefined) + "\n";
            }

            if (breaksOverlapping) {
                isValid = false;
                validationErrors += this.terms["time.schedule.planning.editshift.overlappingbreaks"] + "\n";
            }

            if (tasksOutsideConnectedShift) {
                isValid = false;
                validationErrors += this.terms["time.schedule.planning.editshift.tasksoutsideconnectedshift"] + "\n";
            }

        });

        if (!isValid)
            this.notificationService.showDialog(this.terms["core.unabletosave"], validationErrors, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);

        deferral.resolve(isValid);

        return deferral.promise;
    }

    private validateHolesWithBreaks(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        if (this.hasHolesWithBreaksInside(false)) {
            if (!this.disableBreaksWithinHolesWarning) {
                this.notificationService.showDialog(this.terms["core.unabletosave"] + " " + this.shiftsUndefined, this.terms["time.schedule.planning.editshift.askadjustholes"].format(this.shiftsDefined), SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium, false, true, this.terms["core.donotshowagain"]).result.then(val => {
                    // Save user setting
                    if (val.isChecked) {
                        this.disableBreaksWithinHolesWarning = true;
                        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeSchedulePlanningDisableBreaksWithinHolesWarning, this.disableBreaksWithinHolesWarning);
                    }

                    // Adjust break to fill hole
                    this.hasHolesWithBreaksInside(true);
                    deferral.resolve(true);
                }, (reason) => {
                    deferral.resolve(false);
                });
            } else {
                // Adjust break to fill hole
                this.hasHolesWithBreaksInside(true);
                deferral.resolve(true);
            }
        } else
            deferral.resolve(true);

        return deferral.promise;
    }

    private validateHolesWithoutBreaks(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        if (this.hasHolesWithoutBreaks()) {
            if (this.allowHolesWithoutBreaks) {
                this.notificationService.showDialog(this.terms["core.warning"], this.terms["time.schedule.planning.editshift.asksavewithholes"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNoCancel, SOEMessageBoxSize.Medium, false, false, '', false, '', "core.save", "time.schedule.planning.editshift.fillholes", '', null).result.then(val => {
                    let save = true;
                    if (val === false)
                        save = this.fillHolesWithBreaks();
                    deferral.resolve(save);
                }, (reason) => {
                    deferral.resolve(false);
                });
            } else {
                this.notificationService.showDialog(this.terms["core.warning"], this.terms["time.schedule.planning.editshift.askfillholeswithbreaks"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium).result.then(val => {
                    deferral.resolve(this.fillHolesWithBreaks());
                }, (reason) => {
                    deferral.resolve(false);
                });
            }
        } else
            deferral.resolve(true);

        return deferral.promise;
    }

    private validateSkills(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        if (!this.isHiddenOrVacant && this.invalidSkills) {
            let message = this.terms["time.schedule.planning.editshift.missingskills"].format(this.shiftUndefined);
            if (!this.skillCantBeOverridden)
                message += "\n" + this.terms["time.schedule.planning.editshift.missingskillsoverride"];

            this.notificationService.showDialog(this.terms["common.obs"], message, SOEMessageBoxImage.Forbidden, this.skillCantBeOverridden ? SOEMessageBoxButtons.OK : SOEMessageBoxButtons.OKCancel).result.then(val => {
                deferral.resolve(val && !this.skillCantBeOverridden);
            }, (reason) => {
                deferral.resolve(false);
            });
        } else
            deferral.resolve(true);

        return deferral.promise;
    }

    private validateOnDutyShifts(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        let shiftsOutside = false;

        if (this.isHidden) {
            deferral.resolve(true);
        } else {
            let onDutyShifts = _.filter(this.shifts, s => s.isOnDuty);
            if (onDutyShifts.length === 0) {
                // No on duty shifts
                deferral.resolve(true);
            } else {
                const firstShift = this.getFirstShift();
                const lastShift = this.getLastShift();
                if (!firstShift || !lastShift) {
                    // No regular shifts
                    shiftsOutside = true;
                } else {
                    _.forEach(onDutyShifts, s => {
                        if (s.actualStartTime.isBeforeOnMinute(firstShift.actualStartTime) || s.actualStopTime.isAfterOnMinute(lastShift.actualStopTime)) {
                            shiftsOutside = true;
                            return false;
                        }
                    });
                }

                if (shiftsOutside) {
                    this.notificationService.showDialog(this.terms["common.obs"], this.terms["time.schedule.planning.editshift.ondutyoutsideschedule"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OKCancel).result.then(val => {
                        deferral.resolve(true);
                    }, (reason) => {
                        deferral.resolve(false);
                    });
                } else {
                    deferral.resolve(true);
                }
            }
        }

        return deferral.promise;
    }

    private validateWorkRules(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        // Collect shifts to evaluate
        let shifts: ShiftDTO[] = _.filter(this.shifts, s => !s.isBreak);
        shifts = shifts.concat(this.deletedShifts);

        _.forEach(shifts, shift => {
            shift.setTimesForSave();
        });

        let rules: SoeScheduleWorkRules[] = null;
        if (this.skipWorkRules) {
            // The following rules should always be evaluated
            rules = [];
            rules.push(SoeScheduleWorkRules.OverlappingShifts);
            if (!this.isTemplateView)
                rules.push(SoeScheduleWorkRules.AttestedDay);
        }

        if (shifts.length === 0)
            deferral.resolve(true);
        else {
            if (this.isEmployeePostView) {
                this.startWork("time.schedule.planning.evaluateworkrules.executing");
                this.sharedScheduleService.evaluateEmployeePostPlannedShiftsAgainstWorkRules(shifts, rules).then(result => {
                    this.completedWork(null, true);
                    this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.TaskSaveTimeScheduleShift, result, shifts[0].employeeId).then(passed => {
                        deferral.resolve(passed);
                    });
                });
            } else if (this.isTemplateView) {
                deferral.resolve(true);
            } else {
                const employeeId: number = shifts[0].employeeId;
                this.startWork("time.schedule.planning.evaluateworkrules.executing");
                this.sharedScheduleService.evaluatePlannedShiftsAgainstWorkRules(shifts, rules, employeeId, false, this.timeScheduleScenarioHeadId, this.planningPeriodStartDate, this.planningPeriodStopDate).then(result => {
                    this.completedWork(null, true);
                    this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.TaskSaveTimeScheduleShift, result, employeeId).then(passed => {
                        deferral.resolve(passed);
                    });
                });
            }
        }

        return deferral.promise;
    }

    private validateWorkRulesForAssign(shift: ShiftDTO, employeeId: number): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        if (shift) {
            let rules: SoeScheduleWorkRules[] = null;
            if (this.skipWorkRules) {
                // The following rules should always be evaluated
                rules = [];
                rules.push(SoeScheduleWorkRules.OverlappingShifts);
                rules.push(SoeScheduleWorkRules.AttestedDay);
            }

            this.sharedScheduleService.evaluateDragShiftAgainstWorkRules(DragShiftAction.Move, shift.timeScheduleTemplateBlockId, 0, shift.actualStartTime, shift.actualStopTime, employeeId, false, false, rules, this.isStandbyView, this.timeScheduleScenarioHeadId, null, null, null, true, this.planningPeriodStartDate, this.planningPeriodStopDate).then(result => {
                this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.AssignEmployeeFromQueue, result, employeeId).then(passed => {
                    deferral.resolve(passed);
                });
            });
        } else {
            deferral.resolve(false);
        }

        return deferral.promise;
    }

    private debug() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/EditShift/Views/editShiftDebug.html"),
            controller: EditShiftDebugController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                shifts: () => { return this.shifts },
            }
        }
        this.$uibModal.open(options);
    }

    // HELP-METHODS

    private isInsideScenario(date: Date): boolean {
        return !this.isScenarioView || (this.scenarioDateFrom && this.scenarioDateTo && date.isSameOrAfterOnDay(this.scenarioDateFrom) && date.isSameOrBeforeOnDay(this.scenarioDateTo));
    }

    private setUserAccountId() {
        // Set user account to current user setting
        if (this.useAccountHierarchy && this.accountHierarchyId && this.accountDim?.accounts) {
            const userAccountIds: number[] = this.accountHierarchyId.split('-').map(Number);
            if (userAccountIds.length > 0) {
                if (_.first(userAccountIds) === 0) {
                    // User has selected 'All accounts'
                    // Use first valid account for employee
                    this.userAccountId = (this.validAccountsForEmployee.length > 0 ? this.validAccountsForEmployee[0].accountId : 0);
                } else {
                    if (_.includes(this.accountDim.accounts.map(a => a.accountId), _.last(userAccountIds)))
                        this.userAccountId = _.last(userAccountIds);
                    else {
                        // If account selector in top menu is not on same level as accountDim
                        const intersectedAccounts = _.intersection(this.accountDim.accounts.map(a => a.accountId), userAccountIds);
                        if (intersectedAccounts.length > 0)
                            this.userAccountId = _.last(intersectedAccounts);
                    }
                }

                // Workaround for Martin & Servera
                if (this.inactivateLending && !this.userAccountId && this.validAccountsForEmployee.length === 1)
                    this.userAccountId = this.validAccountsForEmployee[0].accountId;

                const account = _.find(this.accountDim.accounts, a => a.accountId === this.userAccountId);
                this.userAccountName = account ? account.name : '';

                // Filter accounts based on current user setting
                const accounts = _.filter(this.accountDim.accounts, a => _.includes(userAccountIds, a.parentAccountId));
                if (accounts.length > 0)
                    this.accountDim.accounts = accounts;

                this.isDefaultAccountDimLevel = userAccountIds[0] !== 0 && _.includes(this.accountDim.accounts.map(a => a.accountId), this.userAccountId);
            }
        }
    }

    private setDefaultAccount() {
        if (!this.useAccountHierarchy)
            return;

        if (this.allowShiftsWithoutAccount) {
            this.selectedAccountId = 0;
            this.defaultAccountChanged();
        } else {
            // Set selected account to current user setting
            if (this.userAccountId !== undefined && this.accountDim?.accounts) {
                if (this.userAccountId !== 0 && _.includes(_.map(this.accountDim.accounts, a => a.accountId), this.userAccountId)) {
                    // Set account to the one that the user is working on
                    if (_.includes(this.validAccountsForEmployee.map(a => a.accountId), this.userAccountId)) {
                        this.selectedAccountId = this.userAccountId;
                    } else {
                        this.selectedAccountId = (this.validAccountsForEmployee.length === 1 ? this.validAccountsForEmployee[0].accountId : 0);
                    }
                    this.defaultAccountChanged();
                } else if (this.employeeId) {
                    // User working on a higher level, get account from employee
                    this.sharedScheduleService.getDefaultEmployeeAccountId(this.employeeId, this.selectedDate).then(employeeAccountId => {
                        if (employeeAccountId) {
                            if (_.includes(_.map(this.accountDim.accounts, a => a.accountId), employeeAccountId)) {
                                this.selectedAccountId = employeeAccountId;
                                this.defaultAccountChanged();
                            }
                        }
                    });
                }
            }
        }
    }

    private setSelectedAccount() {
        if (!this.useAccountHierarchy)
            return;

        // If account is same on all shifts, set account in header selector, otherwise clear it
        let accountId: number = 0;
        let accountName: string = '';
        let different = false;
        _.forEach(_.filter(this.shifts, s => !s.isBreak), shift => {
            if (accountId === 0) {
                accountId = shift.accountId;
                accountName = shift.accountName;
            }

            if (accountId !== shift.accountId)
                different = true;

            this.setRowShiftTypes(shift);
        });

        this.selectedAccountId = different ? null : accountId;
        this.selectedAccountName = different ? '' : accountName;
        let isValidAccount = _.includes(this.validAccountsForEmployee.map(a => a.accountId), this.selectedAccountId);
        if ((!this.showAccounts && (different || !isValidAccount)) ||
            (this.showAccounts && !different && isValidAccount))
            this.toggleAccounts();
    }

    private setValidAccounts() {
        this.usingShiftTypeHierarchyAccounts = this.useAccountHierarchy && _.some(this.shiftTypes, s => s.hierarchyAccounts && s.hierarchyAccounts.length > 0);

        this.allValidAccountIds = [];
        _.forEach(this.accountDims, dim => {
            if (dim.accounts) {
                this.allValidAccountIds.push(...dim.accounts.map(a => a.accountId));
            }
        });
    }

    private setRowShiftTypes(shift: ShiftDTO) {
        // Only filter if account is set
        if (!shift.accountId) {
            shift.shiftTypes = this.shiftTypes;
        } else {
            let dimHasAnyParentAccount = false;

            let validShiftTypeAccountDimAccountIds: number[] = [];
            if (this.shiftTypeAccountDim) {
                let dim = _.find(this.accountDims, d => d.accountDimId === this.shiftTypeAccountDim.accountDimId);
                if (dim) {
                    dimHasAnyParentAccount = _.filter(dim.accounts, a => a.parentAccountId && !a.hasVirtualParent).length > 0;
                    if (dimHasAnyParentAccount)
                        validShiftTypeAccountDimAccountIds = _.filter(dim.accounts, a => a.parentAccountId === shift.accountId).map(a => a.accountId);

                    // Below is code to support a hole in the hierarchy between shift account and shift type account
                    if (validShiftTypeAccountDimAccountIds.length === 0) {
                        let shiftDim: AccountDimSmallDTO;
                        _.forEach(this.accountDims, d => {
                            if (d.accounts.find(a => a.accountId === shift.accountId)) {
                                shiftDim = d;
                                return false;
                            }
                        });
                        if (shiftDim) {
                            let dimLevel: number = shiftDim.level;
                            let parentAccountIds: number[] = [shift.accountId];
                            while (dimLevel <= dim.level) {
                                dimLevel++;
                                let childDim: AccountDimSmallDTO = this.accountDims.find(d => d.level === dimLevel);
                                if (childDim) {
                                    let accountIds: number[] = _.filter(childDim.accounts, a => (a.hasVirtualParent || a.virtualParentAccountId || _.includes(parentAccountIds, a.parentAccountId))).map(a => a.accountId)
                                    if (accountIds.length > 0)
                                        validShiftTypeAccountDimAccountIds.push(...accountIds);

                                    parentAccountIds = accountIds;
                                }
                            }
                        }
                    }
                }
            }

            shift.shiftTypes = [];
            _.forEach(this.shiftTypes, shiftType => {
                let isValid = true;
                let isValidHierarchy = false;

                if ((this.shiftTypeAccountDim && dimHasAnyParentAccount) || this.usingShiftTypeHierarchyAccounts) {
                    // Linked to account dim
                    if (this.shiftTypeAccountDim && dimHasAnyParentAccount && shiftType.accountId && _.includes(validShiftTypeAccountDimAccountIds, shiftType.accountId)) {
                        isValidHierarchy = true;
                    }

                    if (!isValidHierarchy && this.usingShiftTypeHierarchyAccounts) {
                        // Hierarcy accounts, none selected
                        if (!isValidHierarchy && !this.shiftTypeAccountDim && (!shiftType.hierarchyAccounts || shiftType.hierarchyAccounts.length === 0))
                            isValidHierarchy = true;

                        // Hierarcy accounts, at least one valid account selected
                        let shiftTypeAccountIds: number[] = [];
                        if (shiftType.hierarchyAccounts && shiftType.hierarchyAccounts.length > 0)
                            shiftTypeAccountIds.push(...shiftType.hierarchyAccounts.map(a => a.accountId));
                        if (shiftType.childHierarchyAccountIds && shiftType.childHierarchyAccountIds.length > 0)
                            shiftTypeAccountIds.push(...shiftType.childHierarchyAccountIds);
                        if (!isValidHierarchy && shiftTypeAccountIds.length > 0 && _.includes(shiftTypeAccountIds, shift.accountId))
                            isValidHierarchy = true;
                    }

                    if (!isValidHierarchy)
                        isValid = false;
                } else if (shiftType.accountingSettings) {
                    let accountIds: number[] = [];
                    for (let i = 1; i <= 6; i++) {
                        if (shiftType.accountingSettings[`account{i}Id`])
                            accountIds.push(shiftType.accountingSettings[`account{i}Id`]);
                    }
                    isValid = accountIds.length === 0 || _.includes(accountIds, shift.accountId);
                }

                if (isValid)
                    shift.shiftTypes.push(shiftType);
            });
        }

        // Selected shift type not found in collection, show shift type name in separate textbox beside
        if (!this.isReadonly && !shift.isReadOnly && !shift.isAbsence && !shift.isAbsenceRequest) {
            shift['showShiftTypeName'] = shift.isStandby ? !this.standbyShiftTypes.find(s => s.shiftTypeId === shift.shiftTypeId) : !shift.shiftTypes.find(s => s.shiftTypeId === shift.shiftTypeId);
        } else {
            shift['showShiftTypeName'] = false;
        }
    }

    private validateSortOrder() {
        let correctSortOrder = true;
        let prevSortOrder = 0;
        let shifts = _.orderBy(_.filter(this.shifts, s => !s.isBreak), ['isWholeDayAbsence', 'actualStartTime', 'actualStopTime'], ['desc', 'asc', 'asc']);
        shifts.forEach(s => {
            if (s.sortOrder <= prevSortOrder)
                correctSortOrder = false;
            prevSortOrder = s.sortOrder;
        });

        this.hasUnsortedShifts = this.hasShifts && !correctSortOrder;
    }

    private reSortShifts() {
        this.shifts = _.orderBy(this.shifts, ['isWholeDayAbsence', 'actualStartTime', 'actualStopTime'], ['desc', 'asc', 'asc']);

        let sortOrder: number = 0;
        _.forEach(this.shifts, shift => {
            shift.sortOrder = ++sortOrder;
        });

        this.validateSortOrder();
    }

    private openSaveAndActivateDialog(data: any) {
        // Show save and activate dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/SaveAndActivate/Views/saveAndActivate.html"),
            controller: SaveAndActivateController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                employeeId: () => { return data.employeeIdentifier },
                date: () => { return this.selectedDate },
                timeScheduleTemplateHeadId: () => { return data.timeScheduleTemplateHeadId },
                shiftsToSave: () => { return data.shifts }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                if (result.activateDayNumber)
                    data.activateDayNumber = result.activateDayNumber;
                if (result.activateDates)
                    data.activateDates = result.activateDates;
                this.messagingService.publish(Constants.EVENT_SAVE_SHIFTS, data);
            }
        });
    }

    private openSplitShift() {
        let shift: ShiftDTO = this.selectedShift;
        if (!shift)
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
                templateHelper: () => { return this.templateHelper },
                isTemplate: () => { return this.isTemplateView },
                isEmployeePost: () => { return this.isEmployeePostView },
                showSkills: () => { return this.showSkills; },
                showExtraShift: () => { return this.showExtraShift; },
                showSubstitute: () => { return this.showSubstitute; },
                clockRounding: () => { return this.clockRounding },
                keepShiftsTogether: () => { return this.keepShiftsTogether || this.isHidden },
                skillCantBeOverridden: () => { return this.skillCantBeOverridden },
                hiddenEmployeeId: () => { return this.hiddenEmployeeId },
                vacantEmployeeIds: () => { return this.vacantEmployeeIds },
                allEmployees: () => { return this.employees },
                shift: () => { return shift },
                timeScheduleScenarioHeadId: () => { return this.timeScheduleScenarioHeadId; },
                planningPeriodStartDate: () => { return this.planningPeriodStartDate; },
                planningPeriodStopDate: () => { return this.planningPeriodStopDate; },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                if (this.isTemplateView)
                    this.performSplitTemplateShift(shift, result.splitTime, result.employeeId1, result.employeeId2);
                else
                    this.performSplitShift(shift, result.splitTime, result.employeeId1, result.employeeId2);
            }
        });
    }

    private performSplitTemplateShift(shift: ShiftDTO, splitTime: Date, selectedEmployeeId1: number, selectedEmployeeId2: number) {
        let sourceTemplate: TimeScheduleTemplateHeadSmallDTO;
        let template1: TimeScheduleTemplateHeadSmallDTO;
        let template2: TimeScheduleTemplateHeadSmallDTO;
        if (this.templateHelper) {
            sourceTemplate = this.templateHelper.getTemplateSchedule(shift.employeeId, shift.startTime);
            template1 = this.templateHelper.getTemplateSchedule(selectedEmployeeId1, shift.startTime);
            template2 = this.templateHelper.getTemplateSchedule(selectedEmployeeId2, shift.startTime);
        }
        let sourceTemplateHeadId: number = sourceTemplate ? sourceTemplate.timeScheduleTemplateHeadId : 0;
        let template1HeadId: number = template1 ? template1.timeScheduleTemplateHeadId : 0;
        let template2HeadId: number = template2 ? template2.timeScheduleTemplateHeadId : 0;
        let employeeId1 = this.isEmployeePostView ? 0 : selectedEmployeeId1;
        let employeePostId1 = this.isEmployeePostView ? selectedEmployeeId1 : 0;
        let employeeId2 = this.isEmployeePostView ? 0 : selectedEmployeeId2;
        let employeePostId2 = this.isEmployeePostView ? selectedEmployeeId2 : 0;

        this.sharedScheduleService.splitTemplateShift(shift, sourceTemplateHeadId, splitTime, employeeId1, employeePostId1, template1HeadId, employeeId2, employeePostId2, template2HeadId, this.keepShiftsTogether || this.isHidden, this.skipXEMailOnChanges).then(result => {
            if (result.success) {
                this.loadShifts(false);
                this.messagingService.publish(Constants.EVENT_RELOAD_SHIFTS_FOR_EMPLOYEE, _.uniq([shift.employeeId, selectedEmployeeId1, selectedEmployeeId2]));
            } else {
                this.notificationService.showDialogEx(this.terms["error.default_error"], result.errorMessage, SOEMessageBoxImage.Error);
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
        });
    }

    private performSplitShift(shift: ShiftDTO, splitTime: Date, employeeId1: number, employeeId2: number) {
        this.sharedScheduleService.splitShift(shift, splitTime, employeeId1, employeeId2, this.keepShiftsTogether || this.isHidden, this.isTemplateView, this.skipXEMailOnChanges, this.timeScheduleScenarioHeadId).then(result => {
            if (result.success) {
                this.loadShifts(false);

                this.messagingService.publish(Constants.EVENT_RELOAD_SHIFTS_FOR_EMPLOYEE, [employeeId1, employeeId2]);
            } else {
                this.notificationService.showDialogEx(this.terms["error.default_error"], result.errorMessage, SOEMessageBoxImage.Error);
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
        });
    }

    private openShiftRequestDialog() {
        let shift: ShiftDTO = this.selectedShift;

        this.validateSendShiftRequest(shift).then(passed => {
            if (passed) {
                this.sharedScheduleService.getShiftRequestStatus(shift.timeScheduleTemplateBlockId).then(x => {
                    let excludeEmployeeIds: number[] = [];
                    if (x?.recipients)
                        excludeEmployeeIds = _.map(x.recipients, r => r['employeeId']);

                    // Do some filtering on valid accounts for the selected date
                    let validEmployees: EmployeeListDTO[] = [];

                    if (this.useAccountHierarchy) {
                        _.forEach(this.employees, employee => {
                            if (!employee.hidden && employee.accounts) {
                                _.forEach(employee.accounts, empAccount => {
                                    if (this.isValidAccount(empAccount.accountId) && this._selectedDate.isSameOrAfterOnDay(empAccount.dateFrom) && (empAccount.dateTo !== null && this._selectedDate.isSameOrBeforeOnDay(empAccount.dateTo) || CalendarUtility.isEmptyDate(empAccount.dateTo))) {
                                        validEmployees.push(employee);
                                        return false;
                                    }
                                });
                            }
                        });
                    } else {
                        validEmployees = this.employees;
                    }

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
                            title: this.terms["time.schedule.planning.editshift.functions.shiftrequest"],
                            id: 0,
                            messageMinHeight: 250,
                            type: XEMailType.Outgoing,
                            messageType: TermGroup_MessageType.ShiftRequest,
                            shift: shift,
                            showAvailableEmployees: true,
                            showAvailability: this.showAvailability,
                            allEmployees: _.filter(validEmployees, e => e.employeeId && !e.hidden && !e.vacant && !_.includes(excludeEmployeeIds, e.employeeId))
                        });
                    });

                    modal.result.then(result => {
                        if (result && result.success) {
                            this.messagingService.publish(Constants.EVENT_RELOAD_SHIFTS_FOR_EMPLOYEE, [shift.employeeId]);
                        }
                    });
                });
            }
        });
    }

    private validateSendShiftRequest(shift: ShiftDTO): ng.IPromise<any> {
        const deferral = this.$q.defer<boolean>();

        if (this.useShiftRequestPreventTooEarly) {
            this.sharedScheduleService.checkIfShiftRequestIsTooEarlyToSend(shift.actualStartTime).then(result => {
                this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.ShiftRequest, result, shift.employeeId, false, "time.schedule.planning.contextmenu.sendshiftrequest").then(passed => {
                    deferral.resolve(passed);
                });
            }).catch(reason => {
                this.notificationService.showServiceError(reason);
                deferral.resolve(false);
            });
        } else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private openAbsenceDialog() {
        let shift: ShiftDTO = this.selectedShift ? this.selectedShift : this.shifts[0];

        let modal = this.$uibModal.open({
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
                employeeId: shift.employeeId,
                viewMode: AbsenceRequestViewMode.Attest,
                guiMode: AbsenceRequestGuiMode.AbsenceDialog,
                skipXEMailOnShiftChanges: false,
                shiftId: shift.timeScheduleTemplateBlockId,
                date: shift.date,
                hideOptionSelectedShift: false,
                parentMode: AbsenceRequestParentMode.SchedulePlanning,
                timeScheduleScenarioHeadId: this.timeScheduleScenarioHeadId,
            });
        });

        modal.result.then(reloadEmployeeIds => {
            this.$uibModalInstance.close({ reload: true, reloadEmployeeIds: reloadEmployeeIds });
            this.messagingService.publish(Constants.EVENT_RELOAD_SHIFTS_FOR_EMPLOYEE, reloadEmployeeIds);

        });
    }

    private openAbsenceRequestDialog() {
        let shift: ShiftDTO = this.selectedShift ? this.selectedShift : this.shifts[0];

        let modal = this.$uibModal.open({
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
                employeeId: shift.employeeId,
                viewMode: AbsenceRequestViewMode.Attest,
                guiMode: AbsenceRequestGuiMode.EmployeeRequest,
                loadRequestFromInterval: true,
                date: shift.date,
                skipXEMailOnShiftChanges: false,
                shiftId: 0,
                hideOptionSelectedShift: false,
                parentMode: AbsenceRequestParentMode.SchedulePlanning,
                timeScheduleScenarioHeadId: this.timeScheduleScenarioHeadId,
            });
        });

        modal.result.then(reloadEmployeeIds => {
            this.$uibModalInstance.close({ reload: true, reloadEmployeeIds: reloadEmployeeIds });
        });
    }

    private restoreToSchedule() {
        let shift: ShiftDTO = this.selectedShift ? this.selectedShift : this.shifts[0];

        this.notificationService.showDialog(this.terms["core.info"], this.terms["time.schedule.planning.editshift.functions.restoretoschedule.message"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel).result.then(val => {
            let items: AttestEmployeeDaySmallDTO[] = [];
            let item = new AttestEmployeeDaySmallDTO();
            item.employeeId = shift.employeeId;
            item.date = shift.date;
            item.timeScheduleTemplatePeriodId = shift.timeScheduleTemplatePeriodId;
            items.push(item);

            this.applyAttestCalculationFunction(items, SoeTimeAttestFunctionOption.RestoreToSchedule).then(passed => {
                if (passed) {
                    let reloadEmployeeIds: number[] = [];
                    reloadEmployeeIds.push(shift.employeeId);

                    this.$uibModalInstance.close({ reload: true, reloadEmployeeIds: reloadEmployeeIds });
                }
            });
        });
    }

    private applyAttestCalculationFunction(items: AttestEmployeeDaySmallDTO[], option: SoeTimeAttestFunctionOption): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();
        this.startSave();

        this.sharedScheduleService.applyAttestCalculationFunctionEmployee(items, option).then(result => {
            if (result.success) {
                this.stopProgress();
                this.completedSave(null);
                deferral.resolve(true);
                this.messagingService.publish(Constants.EVENT_RELOAD_SHIFTS_FOR_EMPLOYEE, true);
            } else {
                this.failedSave(result.errorMessage);
                deferral.resolve(false);
            }
        }, error => {
            this.failedSave(error.message);
            deferral.resolve(false);
        });

        return deferral.promise;
    }

    private unlockDay() {
        this.translationService.translateMany(["time.time.attest.timestamps.unlockday", "time.time.attest.timestamps.unlockday.tooltip", "time.time.attest.timestamps.unlockday.error"]).then(terms => {
            const modal = this.notificationService.showDialog(terms["time.time.attest.timestamps.unlockday"], terms["time.time.attest.timestamps.unlockday.tooltip"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.startWork();
                    this.executing = true;

                    this.sharedScheduleService.getTimeBlockDateId(this.employeeId, this.selectedDate).then(timeBlockDateId => {
                        const items: SaveAttestEmployeeDayDTO[] = [];
                        const item = new SaveAttestEmployeeDayDTO();
                        item.date = this.selectedDate;
                        item.timeBlockDateId = timeBlockDateId;
                        items.push(item);

                        this.sharedScheduleService.unlockDay(items, this.employeeId).then(result => {
                            if (result.success) {
                                this.selectedDateChanged(false);
                            } else {
                                this.notificationService.showDialogEx(terms["time.time.attest.timestamps.unlockday.error"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                            }
                            this.executing = false;
                            this.completedWork(null, true);
                        });

                    });
                }
            });
        });
    }

    private removeAbsenceInScenario() {
        if (!this.isScenarioView)
            return;

        let shift: ShiftDTO = this.selectedShift ? this.selectedShift : this.shifts[0];

        this.notificationService.showDialog(this.terms["core.info"], this.terms["time.schedule.planning.editshift.functions.removeabsence.message"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel).result.then(val => {
            let items: AttestEmployeeDaySmallDTO[] = [];
            let item = new AttestEmployeeDaySmallDTO();
            item.employeeId = shift.employeeId;
            item.date = shift.date;
            item.timeScheduleTemplatePeriodId = shift.timeScheduleTemplatePeriodId;
            items.push(item);

            this.applyRemoveAbsenceInScenario(items, this.timeScheduleScenarioHeadId).then(passed => {
                if (passed) {
                    let reloadEmployeeIds: number[] = [];
                    reloadEmployeeIds.push(shift.employeeId);

                    this.$uibModalInstance.close({ reload: true, reloadEmployeeIds: reloadEmployeeIds });
                }
            });
        });
    }

    private applyRemoveAbsenceInScenario(items: AttestEmployeeDaySmallDTO[], timeScheduleScenarioHeadId: number): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();
        this.startSave();

        this.sharedScheduleService.removeAbsenceInScenario(items, timeScheduleScenarioHeadId).then(result => {
            if (result.success) {
                this.stopProgress();
                this.completedSave(null);
                deferral.resolve(true);
            } else {
                this.failedSave(result.errorMessage);
                deferral.resolve(false);
            }
        }, error => {
            this.failedSave(error.message);
            deferral.resolve(false);
        });

        return deferral.promise;
    }

    private openShiftHistory(shift: ShiftDTO) {
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
                shiftType: () => { return shift.type },
                timeScheduleTemplateBlockId: () => { return shift.timeScheduleTemplateBlockId }
            }
        }
        this.$uibModal.open(options);
    }

    private openAccountingDialog() {
        let shifts = _.filter(this.shifts, s => !s.isBreak);
        if (shifts.length === 0)
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
                shifts: () => { return shifts },
                selectedShift: () => { return this.selectedShift }
            }
        }

        this.$uibModal.open(options);
    }

    private setBreaksOnShifts() {
        // Clear break information
        _.forEach(_.filter(this.shifts, s => !s.isBreak && !s.isStandby && !s.isOnDuty), shift => {
            shift.clearBreaks();
        });
        _.forEach(this.deletedShifts, shift => {
            shift.clearBreaks();
        });

        // Copy break information to all shifts
        // Note! Break1TimeCodeId is used as break TimeCode for all breaks, not just break 1
        let i = 0;
        let prevDate = Constants.DATETIME_DEFAULT;
        _.forEach(_.orderBy(_.filter(this.shifts, s => s.isBreak && s.actualStartTime && s.actualStopTime), 'actualStartTime'), brk => {
            // Restart break number counter when there is another date
            if (prevDate !== Constants.DATETIME_DEFAULT && !brk.actualStartTime.date().isSameDayAs(prevDate)) {
                if (_.filter(this.shifts, s => !s.isBreak && !s.isStandby && !s.isOnDuty && s.actualStartTime.isBeforeOnMinute(brk.actualStartTime) && s.actualStopTime.isAfterOnMinute(brk.actualStopTime)).length > 0) {
                    // Shift runs over midnight and this break starts after midnight.
                    // Do not restart break number.
                } else {
                    i = 0;
                }
            }

            i++;
            _.forEach(_.filter(this.shifts, s => !s.isBreak && !s.isStandby && !s.isOnDuty && (s.actualStartDate.isSameDayAs(brk.actualStartTime && s.actualStartDate))), shift => {
                brk.link = shift.link;
                shift.setBreakInformation(i, brk.timeScheduleTemplateBlockId, brk.actualStartTime, brk.break1TimeCodeId, brk.getShiftLength(), brk.link, brk.isPreliminary);
                this.setModified(shift);
            });

            _.forEach(_.filter(this.deletedShifts, s => s.actualStartDate.isSameDayAs(brk.actualStartDate)), shift => {
                shift.setBreakInformation(i, brk.timeScheduleTemplateBlockId, brk.actualStartDate, brk.break1TimeCodeId, brk.getShiftLength(), brk.link, brk.isPreliminary);
                this.setModified(shift);
            });
            prevDate = brk.actualStartDate;
        });
    }

    private setBreakTimeCodeFromTimeCodeId(shift: ShiftDTO) {
        if (shift && shift.isBreak && shift.break1TimeCodeId) {
            const timeCode = _.find(this.breakTimeCodes, t => t.timeCodeId === shift.break1TimeCodeId);
            shift.shiftTypeName = timeCode ? timeCode.name : '';
            if (!timeCode) {
                shift['missingTimeCode'] = true;
                const missingTimeCode = _.find(this.allBreakTimeCodes, t => t.timeCodeId === shift.break1TimeCodeId);
                if (missingTimeCode)
                    shift['missingTimeCodeText'] = this.terms["time.schedule.planning.editshift.missingtimecode2"].format(missingTimeCode.name);
                else
                    shift['missingTimeCodeText'] = this.terms["time.schedule.planning.editshift.missingtimecode1"];
            }
        }
    }

    private setBreakTimeCodeFromTime(shift: ShiftDTO): boolean {
        if (!shift || !shift.isBreak)
            return false;

        let minutes: number = shift.getShiftLength();
        let timeCode = _.head(_.filter(this.breakTimeCodes, t => t.defaultMinutes === minutes));
        shift.shiftTypeName = timeCode ? timeCode.name : '';
        shift.break1TimeCodeId = timeCode ? timeCode.timeCodeId : 0;

        return shift.break1TimeCodeId !== 0;
    }

    private setTimeFromBreakTimeCode(shift: ShiftDTO, timeCode: ITimeCodeBreakSmallDTO) {
        if (!shift || !shift.isBreak)
            return;

        if (!timeCode)
            timeCode = _.head(_.filter(this.breakTimeCodes, t => t.timeCodeId === shift.break1TimeCodeId));
        if (timeCode)
            shift.actualStopTime = shift.actualStartTime.addMinutes(timeCode.defaultMinutes);
    }

    private createBreaksFromShift() {
        if (!this.hasShifts)
            return;

        // Create break shifts from break information on shift DTO
        let dates: Date[] = [];
        _.forEach(this.shifts, shift => {
            let date = _.find(dates, d => d.isSameDayAs(shift.startTime));
            if (!date)
                dates.push(shift.startTime.date());
        });

        _.forEach(dates, date => {
            let shfts = _.filter(this.shifts, s => s.startTime.isSameDayAs(date) && !s.isStandby && !s.isOnDuty);
            if (shfts.length > 0) {
                let breaks = shfts[0].createBreaksFromShift(this.selectedDate);
                if (breaks.length > 0) {
                    _.forEach(breaks, breakShift => {
                        // This condition prevents night breaks to be added twice
                        if (!breakShift.timeScheduleTemplateBlockId || !_.includes(this.shifts.map(s => s.timeScheduleTemplateBlockId), breakShift.timeScheduleTemplateBlockId)) {
                            _.forEach(_.filter(this.shifts, s => !s.isBreak && !s.isStandby && !s.isOnDuty && s.actualStartTime.isSameOrBeforeOnMinute(breakShift.actualStartTime) && s.actualStopTime.isSameOrAfterOnMinute(breakShift.actualStartTime)), shift => {
                                breakShift.link = shift.link;
                                breakShift.tempTimeScheduleTemplateBlockId = breakShift.timeScheduleTemplateBlockId;
                                if (!breakShift.tempTimeScheduleTemplateBlockId)
                                    breakShift.tempTimeScheduleTemplateBlockId = ++this.tempIdCounter;
                            });

                            // Get account from overlapping shift
                            let overlappingShift = _.find(_.filter(this.shifts, s => !s.isBreak && !s.isStandby && !s.isOnDuty), s => s.actualStartTime.isSameOrBeforeOnMinute(breakShift.actualStartTime) && s.actualStopTime.isSameOrAfterOnMinute(breakShift.actualStartTime));
                            breakShift.accountId = overlappingShift ? overlappingShift.accountId : 0;
                            breakShift.accountName = overlappingShift ? overlappingShift.accountName : '';

                            // Get absence from overlapping shift
                            if (overlappingShift) {
                                breakShift.timeDeviationCauseId = overlappingShift.timeDeviationCauseId;
                                breakShift.isAbsenceRequest = overlappingShift.isAbsenceRequest;
                            }

                            this.shifts.push(breakShift);
                        }
                    });
                }
            }
        });

        if (this.breakTimeCodes)
            this.setBreakTimeCodes();
        this.reSortShifts();
        this.calculateDurations();
        this.setShiftTypeIds();
    }

    private initCreateBreaksFromTemplates() {
        const keys: string[] = [
            "time.schedule.planning.editshift.functions.createbreaksfromtemplates",
            "time.schedule.planning.editshift.functions.createbreaksfromtemplatesquestion",
        ];
        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["time.schedule.planning.editshift.functions.createbreaksfromtemplates"], terms["time.schedule.planning.editshift.functions.createbreaksfromtemplatesquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, { initialFocusButton: SOEMessageBoxButton.OK }).result.then(val => {
                this.createBreaksFromTemplates();
            });
        });
    }

    private createBreaksFromTemplates(): boolean {
        if (!this.hasShifts || !this.selectedEmployee || this.selectedEmployee.employeeId === 0)
            return;

        let allSucceded = true;
        this.startProgress();

        const shiftsToCreateBreaksFor = _.filter(this.shifts, s => !s.isBreak && !s.isStandby && !s.isOnDuty);
        _.forEach(shiftsToCreateBreaksFor, (shift) => {
            shift.startTime = shift.actualStartTime;
            shift.stopTime = shift.actualStopTime;
        });

        this.sharedScheduleService.createBreaksFromTemplatesForEmployee(shiftsToCreateBreaksFor, this.selectedEmployee.employeeId).then(result => {
            let hasValidBreakTimeCode = false;

            if (result && result.length > 0) {
                this.shifts = result;

                let sortOrder: number = 0;
                _.forEach(this.shifts, shift => {
                    shift.sortOrder = ++sortOrder;
                    this.setRowShiftTypes(shift);
                    this.setModified(shift);

                    if (!hasValidBreakTimeCode && (shift.break1TimeCodeId || shift.break2TimeCodeId || shift.break3TimeCodeId || shift.break4TimeCodeId))
                        hasValidBreakTimeCode = true;
                });

                if (hasValidBreakTimeCode)
                    this.createBreaksFromShift();
            }

            this.stopProgress();

            if (!hasValidBreakTimeCode) {
                const keys: string[] = [
                    "time.schedule.planning.editshift.functions.createbreaksfromtemplates",
                    "time.schedule.planning.editshift.functions.createbreaksfromtemplatenotfound",
                ];
                this.translationService.translateMany(keys).then(terms => {
                    this.notificationService.showDialogEx(terms["time.schedule.planning.editshift.functions.createbreaksfromtemplates"], terms["time.schedule.planning.editshift.functions.createbreaksfromtemplatenotfound"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                });
                allSucceded = false;
            }
        })

        return allSucceded;
    }

    private getBreakTimeCode(timeCodeId: number) {
        return _.find(this.breakTimeCodes, t => t.timeCodeId == timeCodeId);
    }

    private getBreakTimeCodeName(timeCodeId: number): string {
        let timeCode = this.getBreakTimeCode(timeCodeId);
        return timeCode ? timeCode.name : '';
    }

    private calculateDurations() {
        let summaryRow1 = '';
        let summaryRow2 = '';
        let factorMinutes = 0;

        // Prescence
        let duration = 0;
        let shifts = _.filter(this.shifts, s => !s.isBreak);
        _.forEach(shifts, shift => {
            shift.duration = shift.getShiftLength();
            if (shift.duration < 0)
                shift.duration += 1440;   // Add 24 hours

            // On duty shifts should not be in total sum
            if (!shift.isOnDuty) {
                duration += shift.duration;

                // ScheduleType factor
                factorMinutes += shift.getTimeScheduleTypeFactorsWithinShift();
            }
        });

        // Breaks
        let breakDuration = 0;
        let breaks = _.filter(this.shifts, s => s.isBreak);
        _.forEach(breaks, brk => {
            brk.duration = brk.getShiftLength();
            if (brk.duration < 0)
                brk.duration += 1440;   // Add 24 hours

            breakDuration += brk.duration;

            // Decrease breaks if they overlap prescence
            _.forEach(shifts, shift => {
                duration -= CalendarUtility.getIntersectingDuration(shift.actualStartTime, shift.actualStopTime, brk.actualStartTime, brk.actualStopTime);
            });
        });

        // Shift time range
        let firstTime: Date = this.getFirstTime();
        let lastTime: Date = this.getLastTime();
        if (firstTime.isSameDayAs(lastTime))
            summaryRow1 = "{0}-{1}".format(firstTime.toFormattedTime(), lastTime.toFormattedTime());
        else
            summaryRow1 = "{0}-{1}".format(firstTime.toFormattedDate(), lastTime.toFormattedDate());

        // Shift lengths
        summaryRow1 += ", {0}".format(CalendarUtility.minutesToTimeSpan(duration));
        summaryRow1 += " ({0})".format(CalendarUtility.minutesToTimeSpan(breakDuration));

        // ScheduleType factor
        if (factorMinutes !== 0)
            summaryRow1 += "  {0}: {1}".format(this.terms["time.schedule.planning.scheduletypefactortime"], CalendarUtility.minutesToTimeSpan(factorMinutes));

        // Gross time/cost
        if (this.showGrossTime) {
            let grossTime: number = _.sumBy(shifts, s => { return s.grossTime || 0; });
            summaryRow1 += ", {0}: {1}".format(this.terms["time.schedule.planning.grosstime"], CalendarUtility.minutesToTimeSpan(grossTime));
        }
        if (this.showTotalCost) {
            let totalCost: number = this.showTotalCostIncEmpTaxAndSuppCharge ? _.sumBy(shifts, s => { return s.totalCostIncEmpTaxAndSuppCharge || 0; }) : _.sumBy(shifts, s => { return s.totalCost || 0; });
            summaryRow1 += ", {0}: {1}".format(this.terms["time.schedule.planning.cost"], totalCost.round(0).toString());
        }

        // Number of shifts/breaks
        summaryRow2 = "{0} {1}, {2} {3}".format(shifts.length.toString(),
            (<string>this.terms[shifts.length === 1 ? "time.schedule.planning.shift" : "time.schedule.planning.shifts"]).toLocaleLowerCase(),
            breaks.length.toString(),
            (<string>this.terms[breaks.length === 1 ? "time.schedule.planning.break" : "time.schedule.planning.breaks"]).toLocaleLowerCase());

        if (this.allTasks.length > 0)
            summaryRow2 += ", {0} {1}".format(this.allTasks.length.toString(), (<string>this.terms[this.allTasks.length === 1 ? "time.schedule.timescheduletask.task" : "time.schedule.timescheduletask.tasks"]).toLocaleLowerCase());

        this.summaryRow1 = summaryRow1;
        this.summaryRow2 = summaryRow2;
    }

    private getFirstShift() {
        let shifts = _.filter(this.shifts, s => !s.isBreak && !s.isOnDuty && s.actualStartTime);
        return shifts.length > 0 ? _.head(_.orderBy(shifts, 'actualStartTime')) : null;
    }

    private getFirstTime(): Date {
        let shift = this.getFirstShift();
        return shift ? shift.actualStartTime : this.selectedDate.beginningOfDay();
    }

    private getLastShift() {
        let shifts = _.filter(this.shifts, s => !s.isBreak && !s.isOnDuty && !s.isWholeDayAbsence && s.actualStopTime);
        return shifts.length > 0 ? _.head(_.orderBy(shifts, 'actualStopTime', 'desc')) : null;
    }

    private getLastTime(): Date {
        let shift = this.getLastShift();
        return shift ? shift.actualStopTime : this.selectedDate.beginningOfDay();
    }

    private hasHolesWithBreaksInside(adjustShifts: boolean): boolean {
        let hasHole = false;
        let hasBreakInsideHole = false;

        let prevShift: ShiftDTO = null;
        _.forEach(_.orderBy(_.filter(this.shifts, s => !s.isBreak && !s.isStandby && !s.isOnDuty), ['actualStartTime', 'actualStopTime']), shift => {
            // Check for holes
            if (prevShift && prevShift.actualStopTime.isBeforeOnMinute(shift.actualStartTime)) {
                hasHole = true;
                // A hole found, check if a break is within the hole
                if (_.filter(this.shifts, s => s.isBreak && (s.actualStartTime.isSameOrAfterOnMinute(prevShift.actualStopTime) && s.actualStartTime.isBeforeOnMinute(shift.actualStartTime)) || (s.actualStopTime.isAfterOnMinute(prevShift.actualStopTime) && s.actualStopTime.isSameOrBeforeOnMinute(shift.actualStartTime))).length > 0) {
                    hasBreakInsideHole = true;
                    if (adjustShifts) {
                        prevShift.actualStopTime = shift.actualStartTime;
                    }
                }
            }
            prevShift = shift;
        });

        return hasHole && hasBreakInsideHole;
    }

    private hasHolesWithoutBreaks(): boolean {
        let hasHole = false;
        let prevShift: ShiftDTO = null;
        _.forEach(_.orderBy(_.filter(this.shifts, s => !s.isBreak && !s.isStandby && !s.isOnDuty), ['actualStartTime', 'actualStopTime']), shift => {
            // Check for holes
            if (prevShift && prevShift.actualStopTime.isBeforeOnMinute(shift.actualStartTime)) {
                // A hole found, check if a break is within the hole
                if (_.filter(this.shifts, s => s.isBreak && (s.actualStartTime.isSameOrAfterOnMinute(prevShift.actualStopTime) && s.actualStartTime.isBeforeOnMinute(shift.actualStartTime)) || (s.actualStopTime.isAfterOnMinute(prevShift.actualStopTime) && s.actualStopTime.isSameOrBeforeOnMinute(shift.actualStartTime))).length === 0) {
                    hasHole = true;
                }
            }
            prevShift = shift;
        });

        return hasHole;
    }

    private fillHolesWithBreaks(): boolean {
        let breaksCreated = false;
        let allSucceded = true;
        let prevShift: ShiftDTO = null;

        let dates: Date[] = this.getUniqueDates(this.shifts);
        _.forEach(dates, (date: Date) => {
            let dayShifts = _.filter(this.shifts, s => s.startTime.isSameDayAs(date) && !s.isStandby && !s.isOnDuty);
            prevShift = null;
            _.forEach(_.orderBy(_.filter(dayShifts, s => !s.isBreak), ['actualStartTime', 'actualStopTime']), shift => {
                // Check for holes
                if (prevShift && prevShift.actualStopTime.isBeforeOnMinute(shift.actualStartTime)) {
                    // A hole found, check if a break is within the hole
                    if (_.filter(this.shifts, s => s.isBreak && (s.actualStartTime.isSameOrAfterOnMinute(prevShift.actualStopTime) && s.actualStartTime.isBeforeOnMinute(shift.actualStartTime)) || (s.actualStopTime.isAfterOnMinute(prevShift.actualStopTime) && s.actualStopTime.isSameOrBeforeOnMinute(shift.actualStartTime))).length === 0) {
                        // Create break inside hole
                        let brk: ShiftDTO = this.createNewShift(prevShift, false, false);
                        brk.isBreak = true;
                        brk.actualStartTime = prevShift.actualStopTime;
                        brk.actualStopTime = shift.actualStartTime;
                        if (!this.setBreakTimeCodeFromTime(brk))
                            allSucceded = false;
                        this.shifts.push(brk);
                        breaksCreated = true;

                        // Adjust shift to span over break
                        prevShift.actualStopTime = shift.actualStartTime;
                    }
                }
                prevShift = shift;
            });
        });

        if (breaksCreated) {
            this.setBreaksOnShifts();
            this.reSortShifts();
        }

        return allSucceded;
    }

    private createNewShift(createFrom: ShiftDTO = null, standby: boolean = false, onDuty: boolean = false, keepAllFields: boolean = false): ShiftDTO {
        let type = TermGroup_TimeScheduleTemplateBlockType.Schedule;
        if (standby)
            type = TermGroup_TimeScheduleTemplateBlockType.Standby;
        else if (onDuty)
            type = TermGroup_TimeScheduleTemplateBlockType.OnDuty;
        let newShift = new ShiftDTO(type);
        if (this.shifts && _.filter(this.shifts, s => !s.isBreak && !s.isWholeDayAbsence && s.actualStopTime).length > 0) {
            if (createFrom)
                this.shift = createFrom;
            else if (!this.shift || this.shift.isBreak)
                this.shift = this.getLastShift();

            let createFromStandby = createFrom && createFrom.isStandby;
            let createFromOnDuty = createFrom && createFrom.isOnDuty;
            let keepLink: boolean = (this.keepShiftsTogether || this.isHidden) && !standby && !createFromStandby && !onDuty && !createFromOnDuty;

            newShift = this.shift.copy(keepLink, false);
            newShift.timeCodeId = 0;
            newShift.type = type;
            newShift.sortOrder++;
            newShift.isBreak = false;
            if (!keepAllFields)
                newShift.description = '';
            newShift.isLinked = keepLink;

            if (this.shift.actualStopTime.isSameMinuteAs(this.getLastTime()))
                newShift.actualStartTime = newShift.actualStopTime = this.shift.actualStopTime;
            else
                newShift.actualStartTime = newShift.actualStopTime = this.shift.actualStartTime;
            newShift.date = newShift.actualStartTime.beginningOfDay();
        } else {
            newShift.date = this.selectedDate;
            newShift.startTime = this.selectedDate;
            newShift.setDefaultTimes();
            newShift.stopTime = newShift.actualStopTime = newShift.startTime;
            newShift.belongsToPreviousDay = false;
            newShift.belongsToNextDay = false;
            newShift.dayNumber = createFrom ? createFrom.dayNumber : 1;
            if (!createFrom)
                this.needToCalculateDayNumber = true;
            newShift.employeeId = this.selectedEmployee ? this.selectedEmployee.employeeId : 0;
            newShift.employeePostId = this.selectedEmployee ? this.selectedEmployee.employeePostId : 0;
            newShift.employeeName = this.selectedEmployee ? this.selectedEmployee.name : '';
            newShift.link = Guid.newGuid();
            newShift.isPreliminary = false;
            newShift.sortOrder = 1;
        }

        if (this.timeScheduleScenarioHeadId)
            newShift.timeScheduleScenarioHeadId = this.timeScheduleScenarioHeadId;

        newShift.tempTimeScheduleTemplateBlockId = ++this.tempIdCounter;
        if (!keepAllFields) {
            newShift.shiftStatus = TermGroup_TimeScheduleTemplateBlockShiftStatus.Assigned;
            newShift.shiftStatusName = '';
            newShift.shiftUserStatus = TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted;
            newShift.shiftUserStatusName = '';
            if (this.shiftTypeMandatory && newShift.type === TermGroup_TimeScheduleTemplateBlockType.Standby && this.standbyShiftTypes.length === 1) {
                newShift.shiftTypeId = this.standbyShiftTypes[0].shiftTypeId;
                newShift.shiftTypeName = this.standbyShiftTypes[0].name;
                this.shiftTypeChanged(newShift);
            } else {
                newShift.shiftTypeId = 0;
                newShift.shiftTypeName = '';
            }
            newShift.timeDeviationCauseId = null;
            newShift.timeDeviationCauseName = '';
            newShift.employeeChildId = null;

            if (this.useAccountHierarchy) {
                if (!newShift.accountId && !this.allowShiftsWithoutAccount)
                    newShift.accountId = this.userAccountId;

                if (newShift.accountId) {
                    if (this.isValidAccount(this.userAccountId) && newShift.accountId != this.userAccountId) {
                        newShift.accountId = this.userAccountId;
                        newShift.accountName = this.userAccountName;
                        newShift.isLended = false;
                        newShift.isOtherAccount = false;
                        newShift.isReadOnly = false;
                    } else if (this.isValidAccount(this.selectedAccountId) && newShift.accountId != this.selectedAccountId) {
                        newShift.accountId = this.selectedAccountId;
                        newShift.accountName = this.selectedAccountName;
                        newShift.isLended = false;
                        newShift.isOtherAccount = false;
                        newShift.isReadOnly = false;
                    }
                }

                if (this.shift && newShift.accountId !== this.shift.accountId) {
                    // This was added for Axfood in #43286. Should not be able to link shifts that belongs to different stores.
                    // But for MatHem #64123 that is a problem because account is not same as store.
                    // TODO: Think we need a setting for this?
                    // For now this is an ugly quick workaround.
                    let isMathem = CoreUtility.actorCompanyId === 701609;
                    if (!isMathem) {
                        newShift.link = Guid.newGuid();
                        newShift.isLinked = false;
                    }
                }
            }
        }
        // get current employment
        let employment = this.selectedEmployee && this.selectedDate ? this.selectedEmployee.getEmployment(this.selectedDate, this.selectedDate) : null;

        // check setting if extrashift should be checked as default (currently only in active schedule)
        if (((employment && employment.extraShiftAsDefault) || (this.isHidden && this.extraShiftAsDefaultOnHidden)) && this.isScheduleView)
            newShift.extraShift = true;

        this.timeChanged(newShift);
        this.setRowShiftTypes(newShift);
        this.setModified(newShift);

        this.shift = newShift;

        return newShift;
    }

    private setTimeScheduleType(shift: ShiftDTO, scheduleType: ITimeScheduleTypeSmallDTO) {
        shift.timeScheduleTypeId = scheduleType ? scheduleType.timeScheduleTypeId : 0;
        shift.timeScheduleTypeCode = scheduleType ? scheduleType.code : '';
        shift.timeScheduleTypeName = scheduleType ? scheduleType.name : '';
        shift.timeScheduleTypeFactors = scheduleType ? scheduleType.factors : null;
        this.calculateDurations();
    }

    private setShiftTypeTimeScheduleType(shift: ShiftDTO, scheduleType: ITimeScheduleTypeSmallDTO) {
        shift.shiftTypeTimeScheduleTypeId = scheduleType ? scheduleType.timeScheduleTypeId : 0;
        shift.shiftTypeTimeScheduleTypeCode = scheduleType ? scheduleType.code : '';
        shift.shiftTypeTimeScheduleTypeName = scheduleType ? scheduleType.name : '';
    }

    private setModified(shift: ShiftDTO, modified: boolean = true) {
        shift.isModified = modified;
    }

    private setValidAccountsForEmployee() {
        if (this.useAccountHierarchy) {
            if (this.isHidden) {
                this.validAccountsForEmployee = this.accountDim.accounts;
            } else {
                let allAccountIds: number[] = this.accountDim ? _.map(this.accountDim.accounts, a => a.accountId) : [];
                let empAccountIds: number[] = [];

                if (this.selectedDate && this.selectedEmployee && this.selectedEmployee.accounts) {
                    _.forEach(this.selectedEmployee.accounts, account => {
                        if (this.isEmployeeAccountValid(allAccountIds, account))
                            empAccountIds.push(account.accountId);
                    });
                }

                this.validAccountsForEmployee = [];
                if (this.allowShiftsWithoutAccount) {
                    let emptyAccount = new AccountDTO();
                    emptyAccount.accountId = 0;
                    emptyAccount.name = '';
                    this.validAccountsForEmployee.push(emptyAccount);
                }

                if (this.accountDim)
                    this.validAccountsForEmployee = this.validAccountsForEmployee.concat(_.filter(this.accountDim.accounts, a => _.includes(empAccountIds, a.accountId)));
            }
            this.setDefaultAccount();
        }
    }

    private isEmployeeAccountValid(filteredAccountIds: number[], empAccount: EmployeeAccountDTO): boolean {
        // Check account
        if (_.includes(filteredAccountIds, empAccount.accountId)) {
            // Check date interval
            if (empAccount.dateFrom.isSameOrBeforeOnDay(this.selectedDate) && (!empAccount.dateTo || empAccount.dateTo.isSameOrAfterOnDay(this.selectedDate))) {
                // Check children
                if (!empAccount.children || empAccount.children.length === 0) {
                    // No children, parent was valid so it's OK
                    return true;
                } else {
                    if (_.includes(filteredAccountIds, empAccount.accountId)) {
                        // Do not check children if we are at selected level (Butik)
                        return true;
                    } else {
                        // Recursively check each child account
                        // If one is valid it's OK
                        let childValid = false;
                        _.forEach(empAccount.children, childAccount => {
                            if (this.isEmployeeAccountValid(filteredAccountIds, childAccount)) {
                                childValid = true;
                                return false;   // Exit loop
                            }
                        });
                        if (childValid)
                            return true;
                    }
                }
            }
        }

        return false;
    }

    private setShiftTypeIds() {
        this.shiftTypeIds = [];
        this.$timeout(() => {
            this.shiftTypeIds = _.uniq(_.map(_.filter(this.shifts, s => !s.isBreak && s.shiftTypeId !== 0), s => s.shiftTypeId));
        });
    }

    private getUniqueDates(shifts: ShiftDTO[]): Date[] {
        let dates: Date[] = [];
        _.forEach(shifts, shift => {
            if (!_.find(dates, d => d.isSameDayAs(shift.startTime)))
                dates.push(shift.startTime.date());
        });

        return dates;
    }

    private setConnectedTasksToDeleted(shift: ShiftDTO) {
        if (shift && shift.timeScheduleTemplateBlockId > 0) {
            _.forEach(_.filter(this.allTasks, t => t.timeScheduleTemplateBlockId === shift.timeScheduleTemplateBlockId), task => {
                task.state = SoeEntityState.Deleted;
            });
        }
    }
}
