import { EmployeeListDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { TimeAccumulatorItem } from "../../../../../Common/Models/TimeAccumulatorDTOs";
import { TimePeriodDTO } from "../../../../../Common/Models/TimePeriodDTO";
import { PlanningPeriod, PlanningPeriodHead } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { CompanySettingType } from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { GraphicsUtility } from "../../../../../Util/GraphicsUtility";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { ITimeService } from "../../../../Time/TimeService";

export class EmployeePeriodTimeSummaryController {

    private planningPeriodHeadId: number;
    private employmentDateFrom: Date;
    private employmentDateTo: Date;
    private timeAccumulators: TimeAccumulatorItem[] = [];

    private balanceBackgroundColor: string;
    private balanceColor: string;

    // Flags
    private recalcing = false;
    private loadingAccumulators = false;

    //@ngInject
    constructor(private $uibModalInstance,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private timeService: ITimeService,
        private messagingService: IMessagingService,
        private dateFrom: Date,
        private dateTo: Date,
        private planningPeriodHead: PlanningPeriodHead,
        private planningPeriodChild: PlanningPeriod,
        private currentPlanningPeriod: TimePeriodDTO,
        private employee: EmployeeListDTO,
        private planningPeriodColorOver: string,
        private planningPeriodColorEqual: string,
        private planningPeriodColorUnder: string) {

        if (this.planningPeriodHead) {
            this.populate();
        } else {
            this.loadPeriod().then(() => {
                if (this.planningPeriodHead)
                    this.recalc();
            });
        }

        this.messagingService.subscribe('employeePeriodTimeSummaryUpdated', data => {
            if (data.employeeId === this.employee.employeeId) {
                this.employee.annualScheduledTimeMinutes = data.childScheduledTimeMinutes;
                this.employee.annualWorkTimeMinutes = data.childWorkedTimeMinutes;
                this.employee.parentScheduledTimeMinutes = data.parentScheduledTimeMinutes;
                this.employee.parentWorkedTimeMinutes = data.parentWorkedTimeMinutes;
                this.employee.parentPeriodBalanceTimeMinutes = data.parentPeriodBalanceTimeMinutes;
                this.employee.childPeriodBalanceTimeMinutes = data.childPeriodBalanceTimeMinutes;
                this.populate();
            }
        }, this.$scope);
    }

    // SERVICE CALLS

    private loadPeriod(): ng.IPromise<any> {
        return this.loadDefaultPlanningPeriodHead().then(() => {
            return this.loadCurrentPlanningPeriod();
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
        return this.timeService.getPlanningPeriodHeadWithPeriods(this.planningPeriodHeadId, this.currentPlanningPeriod.startDate).then(x => {
            this.planningPeriodHead = x;
            if (this.planningPeriodHead)
                this.planningPeriodChild = this.planningPeriodHead.getChildByDate(this.currentPlanningPeriod.startDate);
        })
    }

    private loadTimeAccumulators(): ng.IPromise<any> {
        this.loadingAccumulators = true;
        return this.timeService.getTimeAccumulatorsForEmployee(this.employee.employeeId, this.currentPlanningPeriod.startDate, this.currentPlanningPeriod.stopDate, false, false, false, true, false, false, false).then(x => {
            this.timeAccumulators = x;
            this.loadingAccumulators = false;
        });
    }

    // ACTIONS

    private populate() {
        if (this.employee.employments) {
            this.employmentDateFrom = _.head(_.orderBy(this.employee.employments, 'dateFrom')).dateFrom;
            if (this.employmentDateFrom.isSameOrBeforeOnDay(this.currentPlanningPeriod.startDate))
                this.employmentDateFrom = null;

            let hasEnded = _.filter(this.employee.employments, e => !e.dateTo).length === 0;
            if (hasEnded) {
                this.employmentDateTo = _.head(_.orderBy(this.employee.employments, 'dateTo', 'desc')).dateTo;
                if (this.employmentDateTo.isAfterOnDay(this.currentPlanningPeriod.stopDate))
                    this.employmentDateTo = null;
            }
        }

        this.setBalanceColors();

        this.recalcing = false;
    }

    // HELP-METHODS

    private setBalanceColors() {
        if (this.employee.parentPeriodBalanceTimeMinutes > 0)
            this.balanceBackgroundColor = this.planningPeriodColorOver;
        else if (this.employee.parentPeriodBalanceTimeMinutes < 0)
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
