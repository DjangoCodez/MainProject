import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { TermGroup_TimePeriodType, CompanySettingType, TermGroup_TimeSchedulePlanningViews, TermGroup } from "../../../../../Util/CommonEnumerations";
import { AccountDTO } from "../../../../../Common/Models/AccountDTO";
import { TimeSchedulePlanningSettingsDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { ITimeService } from "../../../../Time/TimeService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";

export class SelectableInformationController {

    private settings: TimeSchedulePlanningSettingsDTO;

    // Properties

    private get isCalendarView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.Calendar;
    }

    private get isDayView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.Day;
    }

    private get isScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.Schedule;
    }

    private get isTemplateDayView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TemplateDay;
    }

    private get isTemplateScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TemplateSchedule;
    }

    private get isTemplateView(): boolean {
        return this.isTemplateDayView || this.isTemplateScheduleView;
    }

    private get isEmployeePostDayView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.EmployeePostsDay;
    }

    private get isEmployeePostScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule;
    }

    private get isEmployeePostView(): boolean {
        return this.isEmployeePostDayView || this.isEmployeePostScheduleView;
    }

    private get isScenarioDayView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioDay;
    }

    private get isScenarioScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioSchedule || this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioComplete;
    }

    public get isScenarioView(): boolean {
        return this.isScenarioDayView || this.isScenarioScheduleView;
    }

    private get isStandbyDayView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.StandbyDay;
    }

    private get isStandbyScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.StandbySchedule;
    }

    public get isStandbyView(): boolean {
        return this.isStandbyDayView || this.isStandbyScheduleView;
    }

    public get isCommonDayView(): boolean {
        return this.isDayView || this.isTemplateDayView || this.isEmployeePostDayView || this.isScenarioDayView || this.isStandbyDayView;
    }

    public get isCommonScheduleView(): boolean {
        return this.isScheduleView || this.isTemplateScheduleView || this.isEmployeePostScheduleView || this.isScenarioScheduleView || this.isStandbyScheduleView;
    }

    private get showColumn1(): boolean {
        return !this.showFollowUpOnly && (this.isDayView || this.isScheduleView || this.isTemplateView || this.isScenarioView || this.isStandbyDayView || this.isStandbyScheduleView || this.showGrossTimeSetting || this.showTotalCostPermission);
    }

    private get showColumn2(): boolean {
        return this.isSchedulePlanningMode && !this.isCalendarView;
    }

    private get nbrOfColumns(): number {
        let cols = 0;

        if (this.showColumn1)
            cols++;

        if (this.showColumn2)
            cols++;

        return cols;
    }

    private get showFollowUp(): boolean {
        return this.showDashboardPermission;
    }

    private userSelections: SmallGenericType[] = [];
    private planningPeriodHeads: ISmallGenericType[];
    private annualLeaveBalanceFormats: ISmallGenericType[];
    private shiftTypePositions: ISmallGenericType[];
    private timePositions: ISmallGenericType[];
    private breakVisibilities: ISmallGenericType[];
    private followUpAccountDim: AccountDimSmallDTO;
    private followUpAccounts: AccountDTO[];

    private chartAccordionInitiallyOpen = false;
    private tableAccordionInitiallyOpen = false;
    private showSavedAccountingUnavailable = false;
    private showSavedAccountingMismatch = false;
    private showWeekendSalary = false;

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private timeService: ITimeService,
        settings: TimeSchedulePlanningSettingsDTO,
        private useAccountHierarchy: boolean,
        private accountHierarchyId: string,
        private userAccountId: number,
        private allAccountsSelected: boolean,
        private isAdmin: boolean,
        private isSchedulePlanningMode: boolean,
        private viewDefinition: TermGroup_TimeSchedulePlanningViews,
        private modifyPermission: boolean,
        private minutesLabel: string,
        private showGrossTimeSetting: boolean,
        private showTotalCostPermission: boolean,
        private showDashboardPermission: boolean,
        private showStaffingNeedsPermission: boolean,
        private showBudgetPermission: boolean,
        private showForecastPermission: boolean,
        private calculatePlanningPeriodScheduledTime: boolean,
        private showFollowUpOnly: boolean,
        private sendXEMailOnChange: boolean,
        private followUpCalculationTypes: ISmallGenericType[],
        private accountDims: AccountDimSmallDTO[],
        private useAnnualLeave: boolean) {

        this.settings = new TimeSchedulePlanningSettingsDTO(false);
        angular.extend(this.settings, settings);

        this.chartAccordionInitiallyOpen = this.isDayView || this.isTemplateDayView;

        this.loadUserSelections();
        this.loadPlanningPeriods();
        if (this.useAnnualLeave)
            this.loadAnnualLeaveBalanceFormats();
        this.loadShiftTypePositions();
        this.loadTimePositions();
        this.loadBreakVisibilities();
        this.loadWeekendSalarySetting();
        this.accountDimChanged(false);
    }

    // SERVICE CALLS

    private loadUserSelections(): ng.IPromise<any> {
        return this.coreService.getUserSelections(this.viewDefinition + 100).then(x => {
            this.userSelections = x;
            this.userSelections.splice(0, 0, new SmallGenericType(0, ''));
        });
    }

    private loadPlanningPeriods(): ng.IPromise<any> {
        let accountId: number = 0;
        if (this.accountHierarchyId) {
            let accounts: string[] = this.accountHierarchyId.split('-');
            if (accounts.length > 0)
                accountId = parseInt(_.last(accounts), 10)
        }

        return this.timeService.getTimePeriodHeadsDict(TermGroup_TimePeriodType.RuleWorkTime, false, accountId ? accountId : null).then(x => {
            this.planningPeriodHeads = x;

            if (!this.settings.planningPeriodHeadId)
                this.loadDefaultPlanningPeriodHead();
        });
    }

    private loadDefaultPlanningPeriodHead(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeDefaultPlanningPeriod);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.settings.planningPeriodHeadId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultPlanningPeriod);
        });
    }

    private loadAnnualLeaveBalanceFormats(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeSchedulePlanningAnnualLeaveBalanceFormat, false, false, true).then(x => {
            this.annualLeaveBalanceFormats = x;
        });
    }

    private loadShiftTypePositions(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeSchedulePlanningShiftTypePosition, false, false, true).then(x => {
            this.shiftTypePositions = x;
        });
    }

    private loadTimePositions(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeSchedulePlanningTimePosition, false, false, true).then(x => {
            this.timePositions = x;
        });
    }

    private loadBreakVisibilities(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeSchedulePlanningBreakVisibility, false, false, true).then(x => {
            this.breakVisibilities = x;
        });
    }

    private loadWeekendSalarySetting(): ng.IPromise<any> {
        return this.timeService.getUsesWeekendSalary().then(x => {
            this.showWeekendSalary = x;
        });
    }

    // EVENTS

    private toggleShowTotalCost() {
        this.$timeout(() => {
            if (!this.settings.showTotalCost) {
                this.settings.showTotalCostIncEmpTaxAndSuppCharge = false;
                this.settings.showWeekendSalary = false;
            }
        });
    }

    private toggleShowTotalCostIncEmpTaxAndSuppCharge() {
        this.$timeout(() => {
            if (this.settings.showTotalCostIncEmpTaxAndSuppCharge)
                this.settings.showTotalCost = true;
        });
    }

    private toggleShowWeekendSalary() {
        this.$timeout(() => {
            if (this.settings.showWeekendSalary)
                this.settings.showTotalCost = true;
        });
    }

    private accountDimChanged(manually: boolean) {
        this.$timeout(() => {
            if (this.settings.followUpAccountDimId)
                this.followUpAccountDim = _.find(this.accountDims, a => a.accountDimId === this.settings.followUpAccountDimId);
            else if (this.showFollowUpOnly && this.accountDims && this.accountDims.length === 1) {
                this.followUpAccountDim = this.accountDims[0];
                this.settings.followUpAccountDimId = this.followUpAccountDim.accountDimId;
            }

            if (this.followUpAccountDim)
                this.followUpAccounts = this.useAccountHierarchy ? this.followUpAccountDim.filteredAccounts : this.followUpAccountDim.accounts;
            else
                this.followUpAccounts = [];

            if (!this.settings.followUpAccountId && this.followUpAccounts.length === 1)
                this.settings.followUpAccountId = this.followUpAccounts[0].accountId;

            if (this.settings.followUpAccountId && this.followUpAccounts.length > 0 && !this.followUpAccounts.find(a => a.accountId === this.settings.followUpAccountId))
                this.showSavedAccountingUnavailable = true;
            else if (this.settings.followUpAccountId && this.userAccountId && this.allAccountsSelected || (this.settings.followUpAccountId !== this.userAccountId))
                this.showSavedAccountingMismatch = true;

            if (manually) {
                if (!this.chartAccordionInitiallyOpen)
                    this.chartAccordionInitiallyOpen = true;
                if (!this.tableAccordionInitiallyOpen)
                    this.tableAccordionInitiallyOpen = true;
            }
        });
    }

    private accountChanged() {
        if (!this.chartAccordionInitiallyOpen)
            this.chartAccordionInitiallyOpen = true;
        if (!this.tableAccordionInitiallyOpen)
            this.tableAccordionInitiallyOpen = true;
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok(saveSettings: boolean) {
        if ((this.settings.showTotalCostIncEmpTaxAndSuppCharge || this.settings.showWeekendSalary) && !this.settings.showTotalCost)
            this.settings.showTotalCost = true;

        this.$uibModalInstance.close({ saveSettings: saveSettings, settings: this.settings });
    }
}
