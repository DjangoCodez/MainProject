import { StaffingStatisticsIntervalRow } from "../../../../../Common/Models/StaffingNeedsDTOs";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { TimeSchedulePlanningSettingsDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { IScheduleService } from "../../../ScheduleService";
import { TermGroup_TimeSchedulePlanningFollowUpCalculationType } from "../../../../../Util/CommonEnumerations";

export class AdjustFollowUpDataController {

    private row: StaffingStatisticsIntervalRow;

    private get showBudget(): boolean {
        return this.selectableInformationSettings.followUpShowCalculationTypeSalesBudget || this.selectableInformationSettings.followUpShowCalculationTypeHoursBudget || this.selectableInformationSettings.followUpShowCalculationTypePersonelCostBudget || this.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentBudget || this.selectableInformationSettings.followUpShowCalculationTypeLPATBudget || this.selectableInformationSettings.followUpShowCalculationTypeFPATBudget;
    }

    private get showForecast(): boolean {
        return this.selectableInformationSettings.followUpShowCalculationTypeSalesForecast || this.selectableInformationSettings.followUpShowCalculationTypeHoursForecast || this.selectableInformationSettings.followUpShowCalculationTypePersonelCostForecast || this.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentForecast || this.selectableInformationSettings.followUpShowCalculationTypeLPATForecast || this.selectableInformationSettings.followUpShowCalculationTypeFPATForecast;
    }

    private get isSalesIncluded(): boolean {
        return this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours ||
            this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost ||
            this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent ||
            this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT;
    }

    private get isHoursIncluded(): boolean {
        return this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales ||
            this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost ||
            this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT ||
            this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT;
    }

    private get isPersonelCostIncluded(): boolean {
        return this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales ||
            this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours ||
            this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent ||
            this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT;
    }

    private get isSalaryPercentIncluded(): boolean {
        return this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales ||
            this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost;
    }

    private get isLPATIncluded(): boolean {
        return this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours ||
            this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost;
    }

    private get isFPATIncluded(): boolean {
        return this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales ||
            this.row.targetCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours;
    }

    //@ngInject
    constructor(private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private scheduleService: IScheduleService,
        private followUpCalculationTypes: ISmallGenericType[],
        private selectableInformationSettings: TimeSchedulePlanningSettingsDTO,
        row: StaffingStatisticsIntervalRow,
        private originalRow: StaffingStatisticsIntervalRow) {

        this.row = new StaffingStatisticsIntervalRow();
        angular.extend(this.row, row);
        this.row.targetCalculationType = TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales;
    }

    // SERVICE CALLS

    private recalculateStaffingNeedsSummary(): ng.IPromise<any> {
        return this.scheduleService.recalculateStaffingNeedsSummary(this.row).then(x => {
            this.row = new StaffingStatisticsIntervalRow();
            angular.extend(this.row, x);
        });
    }

    // EVENTS

    private valueModified(type: TermGroup_TimeSchedulePlanningFollowUpCalculationType) {
        this.$timeout(() => {
            this.row.modifiedCalculationType = type;
            this.recalculateStaffingNeedsSummary();
        });
    }

    private typeModified() {
        this.$timeout(() => {
            this.recalculateStaffingNeedsSummary();
        });
    }

    private restore() {
        this.$uibModalInstance.close({ restore: true });
    }

    private ok() {
        this.$uibModalInstance.close({ row: this.row });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}
