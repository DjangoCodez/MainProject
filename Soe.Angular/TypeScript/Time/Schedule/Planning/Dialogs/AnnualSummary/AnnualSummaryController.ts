import { IScheduleService } from "../../../ScheduleService";
import { ITimeService } from "../../../../Time/TimeService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { EmployeeListDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { TimeScheduledTimeSummaryType, CompanySettingType } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { TimeAccumulatorDTO, TimeAccumulatorItem } from "../../../../../Common/Models/TimeAccumulatorDTOs";
import { Constants } from "../../../../../Util/Constants";
import { GraphicsUtility } from "../../../../../Util/GraphicsUtility";

export class AnnualSummaryController {

    private weekString: string;

    private planningPeriodMissing = false;

    private employmentDateFrom: Date;
    private employmentDateTo: Date;

    private annualScheduledTime = 0;
    private annualScheduledTimePlaced = 0;
    private annualScheduledTimeTotal = 0;
    private annualWorkTime = 0;
    private annualScheduledTimeMinutesWeek = 0;
    private annualScheduledTimePlacedWeek = 0;
    private annualScheduledTimeWeek = 0;
    private annualWorkTimeMinutesWeek = 0;
    private diff = 0;
    private diffWeek = 0;

    private annualTimeOver = false;
    private annualTimeUnder = false;
    private annualTimeEquals = false;

    private balanceBackgroundColor: string;
    private balanceColor: string;

    private timeAccumulators: TimeAccumulatorItem[] = [];

    private recalcing = false;
    private loadingAccumulators = false;

    private get nbrOfWeeks(): number {
        return ((this.employmentDateTo || this.dateTo).diffDays(this.employmentDateFrom || this.dateFrom) + 1) / 7;
    }

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private timeService: ITimeService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private dateFrom: Date,
        private dateTo: Date,
        private planningPeriodHeadId: number,
        private periodName: string,
        private employee: EmployeeListDTO,
        private recalcOnOpen: boolean,
        private planningPeriodColorOver: string,
        private planningPeriodColorEqual: string,
        private planningPeriodColorUnder: string) {

        if (!this.planningPeriodHeadId) {
            this.loadPeriod().then(() => {
                if (!this.planningPeriodMissing)
                    this.checkRecalcOnLoad();
            });
        } else {
            this.checkRecalcOnLoad();
        }

        this.messagingService.subscribe('annualScheduledTimeUpdated', data => {
            if (data.employeeId === this.employee.employeeId) {
                this.employee.annualScheduledTimeMinutes = data.annualScheduledTime;
                this.loadTimes();
            }
        }, this.$scope);
    }

    private checkRecalcOnLoad() {
        if (this.recalcOnOpen) {
            this.recalcAnnualScheduledTime();
        } else {
            this.loadTimes();
        }
    }

    // SERVICE CALLS

    private loadPeriod(): ng.IPromise<any> {
        return this.loadDefaultPlanningPeriodHead().then(() => {
            return this.loadCurrentPlanningPeriod();
        });
    }

    private loadTimes() {
        this.$q.all([
            this.loadScheduledTimePlaced(),
            this.loadScheduledTimeTotal(),
            this.loadWorkTime()
        ]).then(() => {
            this.populate();
        });
    }

    private loadDefaultPlanningPeriodHead(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeDefaultPlanningPeriod);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.planningPeriodHeadId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultPlanningPeriod);
        });
    }

    private loadCurrentPlanningPeriod(): ng.IPromise<any> {
        return this.timeService.getTimePeriod(this.planningPeriodHeadId, this.dateFrom, true).then(x => {
            if (x) {
                this.dateFrom = x.startDate;
                this.dateTo = x.stopDate;
                this.periodName = x.name;
            } else {
                this.planningPeriodMissing = true;
            }
        });
    }

    private loadScheduledTimePlaced(): ng.IPromise<any> {
        return this.scheduleService.getAnnualScheduledTimeSummaryForEmployee(this.employee.employeeId, this.dateFrom, this.dateTo, TimeScheduledTimeSummaryType.ScheduledTime).then(minutes => {
            this.annualScheduledTimePlaced = minutes;
        });
    }

    private loadScheduledTimeTotal(): ng.IPromise<any> {
        return this.scheduleService.getAnnualScheduledTimeSummaryForEmployee(this.employee.employeeId, this.dateFrom, this.dateTo, TimeScheduledTimeSummaryType.Both).then(minutes => {
            this.annualScheduledTimeTotal = minutes;
        });
    }

    private loadWorkTime() {
        return this.scheduleService.getAnnualWorkTime(this.employee.employeeId, this.dateFrom, this.dateTo, this.planningPeriodHeadId).then(minutes => {
            this.annualWorkTime = minutes;
            this.employee.annualWorkTimeMinutes = minutes;
        });
    }

    private loadTimeAccumulators(): ng.IPromise<any> {
        this.loadingAccumulators = true;
        return this.timeService.getTimeAccumulatorsForEmployee(this.employee.employeeId, this.dateFrom, this.dateTo, false, false, false, true, false, false, false).then(x => {
            this.timeAccumulators = x;
            this.loadingAccumulators = false;
        });
    }

    private recalcAnnualScheduledTime(): ng.IPromise<any> {
        this.recalcing = true;
        return this.scheduleService.updateAnnualScheduledTimeSummaryForEmployee(this.employee.employeeId, this.dateFrom, this.dateTo, false).then(x => {
            return this.scheduleService.getAnnualScheduledTimeSummary([this.employee.employeeId], this.dateFrom, this.dateTo, this.planningPeriodHeadId).then(y => {
                _.forEach(y, z => {
                    this.employee.annualScheduledTimeMinutes = z.annualScheduledTimeMinutes;
                    this.employee.annualWorkTimeMinutes = z.annualWorkTimeMinutes;
                });
                this.recalcing = false;

                this.loadTimes();
            });
        });
    }

    // ACTIONS

    private populate() {
        if (this.employee.employments) {
            this.employmentDateFrom = _.head(_.orderBy(this.employee.employments, 'dateFrom')).dateFrom;
            if (this.employmentDateFrom.isSameOrBeforeOnDay(this.dateFrom))
                this.employmentDateFrom = null;

            let hasEnded = _.filter(this.employee.employments, e => !e.dateTo).length === 0;
            if (hasEnded) {
                this.employmentDateTo = _.head(_.orderBy(this.employee.employments, 'dateTo', 'desc')).dateTo;
                if (this.employmentDateTo.isAfterOnDay(this.dateTo))
                    this.employmentDateTo = null;
            }
        }

        this.weekString = '{0}.{1} - {2}.{3}'.format(this.dateFrom.week().toString(), this.dateFrom.year().toString(), this.dateTo.week().toString(), this.dateTo.endOfWeek().year().toString());

        // Period
        if (this.employee.annualScheduledTimeMinutes === 0 && this.annualScheduledTimeTotal !== 0)
            this.employee.annualScheduledTimeMinutes = this.annualScheduledTimeTotal;
        if (this.employee.annualWorkTimeMinutes === 0 && this.annualWorkTime !== 0)
            this.employee.annualWorkTimeMinutes = this.annualWorkTime;
        this.annualScheduledTime = this.annualScheduledTimeTotal - this.annualScheduledTimePlaced;
        this.diff = this.employee.annualScheduledTimeMinutes - this.employee.annualWorkTimeMinutes;

        // Week
        this.annualScheduledTimeMinutesWeek = (this.employee.annualScheduledTimeMinutes / this.nbrOfWeeks).round(0);
        this.annualScheduledTimePlacedWeek = (this.annualScheduledTimePlaced / this.nbrOfWeeks).round(0);
        this.annualScheduledTimeWeek = this.annualScheduledTimeMinutesWeek - this.annualScheduledTimePlacedWeek;
        this.annualWorkTimeMinutesWeek = (this.employee.annualWorkTimeMinutes / this.nbrOfWeeks).round(0);
        this.diffWeek = this.annualScheduledTimeMinutesWeek - this.annualWorkTimeMinutesWeek;

        // Colors
        this.annualTimeOver = false;
        this.annualTimeUnder = false;
        this.annualTimeEquals = false;

        if (this.employee.annualScheduledTimeMinutes > this.employee.annualWorkTimeMinutes)
            this.annualTimeOver = true;
        else if (this.employee.annualScheduledTimeMinutes < this.employee.annualWorkTimeMinutes)
            this.annualTimeUnder = true;
        else if (this.employee.annualScheduledTimeMinutes === this.employee.annualWorkTimeMinutes && this.employee.annualScheduledTimeMinutes !== 0)
            this.annualTimeEquals = true;

        this.setBalanceColors();

        this.recalcing = false;
    }

    // HELP-METHODS

    private setBalanceColors() {
        if (this.annualTimeOver)
            this.balanceBackgroundColor = this.planningPeriodColorOver;
        else if (this.annualTimeUnder)
            this.balanceBackgroundColor = this.planningPeriodColorUnder;
        else
            this.balanceBackgroundColor = this.planningPeriodColorEqual;

        this.balanceColor = GraphicsUtility.foregroundColorByBackgroundBrightness(this.balanceBackgroundColor);
    }

    private getAccumulatorBalanceBackgroundColor(timeAccumulator: TimeAccumulatorItem): string {
        if (timeAccumulator.sumPlanningPeriod - this.employee.annualScheduledTimeMinutes > 0)
            return this.planningPeriodColorOver;
        else if (timeAccumulator.sumPlanningPeriod - this.employee.annualScheduledTimeMinutes < 0)
            return this.planningPeriodColorUnder;
        else
            return this.planningPeriodColorEqual;
    }

    private getAccumulatorBalanceColor(timeAccumulator: TimeAccumulatorItem): string {
        let backgroundColor = this.getAccumulatorBalanceBackgroundColor(timeAccumulator);
        return GraphicsUtility.foregroundColorByBackgroundBrightness(backgroundColor);
    }

    // EVENTS

    private recalc() {
        this.recalcing = true;
        this.messagingService.publish(Constants.EVENT_UPDATE_ANNUAL_SCHEDULED_TIME, this.employee.employeeId);
        if (this.timeAccumulators.length > 0)
            this.loadTimeAccumulators();
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}
