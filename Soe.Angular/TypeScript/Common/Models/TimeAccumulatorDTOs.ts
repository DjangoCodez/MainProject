import { ITimeAccumulatorItem, ITimeAccumulatorDTO, ITimeAccumulatorInvoiceProductDTO, ITimeAccumulatorPayrollProductDTO, ITimeAccumulatorTimeCodeDTO, ITimeAccumulatorEmployeeGroupRuleDTO, ITimeAccumulatorRuleItem, System, ITimeAccumulatorGridDTO, ITimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO, ITimeWorkReductionEarningDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_AccumulatorTimePeriodType, TermGroup_TimeAccumulatorType, SoeTimeAccumulatorComparison } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class TimeAccumulatorDTO implements ITimeAccumulatorDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    description: string;
    employeeGroupRules: TimeAccumulatorEmployeeGroupRuleDTO[];
    finalSalary: boolean;
    invoiceProducts: TimeAccumulatorInvoiceProductDTO[];
    modified: Date;
    modifiedBy: string;
    name: string;
    payrollProducts: TimeAccumulatorPayrollProductDTO[];
    showInTimeReports: boolean;
    state: SoeEntityState;
    timeAccumulatorId: number;
    timeCodeId: number;
    timeCodes: TimeAccumulatorTimeCodeDTO[];
    timeWorkReductionEarningId: number;
    timePeriodHeadId: number;
    timePeriodHeadName: string;
    timeWorkReductionEarning: TimeWorkReductionEarningDTO;
    type: TermGroup_TimeAccumulatorType;
    typeName: string;
    useTimeWorkAccount: boolean;
    useTimeWorkReductionWithdrawal: boolean;

    get isActive(): boolean {
        return (this.state === SoeEntityState.Active);
    }
    set isActive(active: boolean) {
        this.state = (active ? SoeEntityState.Active : SoeEntityState.Inactive);
    }

    public setTypes() {
        if (this.invoiceProducts) {
            this.invoiceProducts = this.invoiceProducts.map(i => {
                let iObj = new TimeAccumulatorInvoiceProductDTO();
                angular.extend(iObj, i);
                return iObj;
            });
        } else {
            this.invoiceProducts = [];
        }

        if (this.payrollProducts) {
            this.payrollProducts = this.payrollProducts.map(p => {
                let pObj = new TimeAccumulatorPayrollProductDTO();
                angular.extend(pObj, p);
                return pObj;
            });
        } else {
            this.payrollProducts = [];
        }

        if (this.timeCodes) {
            this.timeCodes = this.timeCodes.map(t => {
                let tObj = new TimeAccumulatorTimeCodeDTO();
                angular.extend(tObj, t);
                return tObj;
            });
        } else {
            this.timeCodes = [];
        }

        if (this.employeeGroupRules) {
            this.employeeGroupRules = this.employeeGroupRules.map(e => {
                let eObj = new TimeAccumulatorEmployeeGroupRuleDTO();
                angular.extend(eObj, e);
                return eObj;
            });
        } else {
            this.employeeGroupRules = [];
        }
    }
}

export class TimeAccumulatorGridDTO implements ITimeAccumulatorGridDTO {
    description: string;
    isSelected: boolean;
    name: string;
    state: SoeEntityState;
    timeAccumulatorId: number;
    type: TermGroup_TimeAccumulatorType;
    typeName: string;

    get isActive(): boolean {
        return (this.state === SoeEntityState.Active);
    }
    set isActive(active: boolean) {
        this.state = (active ? SoeEntityState.Active : SoeEntityState.Inactive);
    }
}

export class TimeAccumulatorInvoiceProductDTO implements ITimeAccumulatorInvoiceProductDTO {
    factor: number;
    invoiceProductId: number;

    // Extensions
    productName: string;
}

export class TimeAccumulatorPayrollProductDTO implements ITimeAccumulatorPayrollProductDTO {
    factor: number;
    payrollProductId: number;

    // Extensions
    productName: string;
}

export class TimeAccumulatorTimeCodeDTO implements ITimeAccumulatorTimeCodeDTO {
    factor: number;
    importDefault: boolean;
    isHeadTimeCode: boolean;
    timeCodeId: number;

    // Extensions
    timeCodeName: string;
}

export class TimeAccumulatorEmployeeGroupRuleDTO implements ITimeAccumulatorEmployeeGroupRuleDTO {
    employeeGroupId: number;
    maxMinutes: number;
    maxMinutesWarning: number;
    maxTimeCodeId: number;
    minMinutes: number;
    minMinutesWarning: number;
    minTimeCodeId: number;
    type: TermGroup_AccumulatorTimePeriodType;
    scheduledJobHeadId: number;
    showOnPayrollSlip: boolean;
    thresholdMinutes: number;

    // Extensions
    employeeGroupName: string;
    maxTimeCodeName: string;
    minTimeCodeName: string;
    typeName: string;

    get minMinutesFormatted(): string {
        return this.minMinutes || this.minMinutes === 0 ? CalendarUtility.minutesToTimeSpan(this.minMinutes) : null;
    }
    set minMinutesFormatted(time: string) {
        if (time) {
            var span = CalendarUtility.parseTimeSpan(time);
            this.minMinutes = CalendarUtility.timeSpanToMinutes(span);
        } else {
            this.minMinutes = null;
        }
    }

    get maxMinutesFormatted(): string {
        return this.maxMinutes || this.maxMinutes === 0 ? CalendarUtility.minutesToTimeSpan(this.maxMinutes) : null;
    }
    set maxMinutesFormatted(time: string) {
        if (time) {
            var span = CalendarUtility.parseTimeSpan(time);
            this.maxMinutes = CalendarUtility.timeSpanToMinutes(span);
        } else {
            this.maxMinutes = null;
        }
    }

    get minMinutesWarningFormatted(): string {
        return this.minMinutesWarning || this.minMinutesWarning === 0 ? CalendarUtility.minutesToTimeSpan(this.minMinutesWarning) : null;
    }
    set minMinutesWarningFormatted(time: string) {
        if (time) {
            var span = CalendarUtility.parseTimeSpan(time);
            this.minMinutesWarning = CalendarUtility.timeSpanToMinutes(span);
        } else {
            this.minMinutesWarning = null;
        }
    }

    get maxMinutesWarningFormatted(): string {
        return this.maxMinutesWarning || this.maxMinutesWarning === 0 ? CalendarUtility.minutesToTimeSpan(this.maxMinutesWarning) : null;
    }
    set maxMinutesWarningFormatted(time: string) {
        if (time) {
            var span = CalendarUtility.parseTimeSpan(time);
            this.maxMinutesWarning = CalendarUtility.timeSpanToMinutes(span);
        } else {
            this.maxMinutesWarning = null;
        }
    }


    get thresholdMinutesFormatted(): string {
        return this.thresholdMinutes || this.thresholdMinutes === 0 ? CalendarUtility.minutesToTimeSpan(this.thresholdMinutes) : null;
    }
    set thresholdMinutesFormatted(time: string) {
        if (time) {
            var span = CalendarUtility.parseTimeSpan(time);
            this.thresholdMinutes = CalendarUtility.timeSpanToMinutes(span);
        } else {
            this.thresholdMinutes = null;
        }
    }

    get isTypePlanningPeriod(): boolean {
        return this.type == TermGroup_AccumulatorTimePeriodType.PlanningPeriod;
    }
}

export class TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO implements ITimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO {
    timeAccumulatorTimeWorkReductionEarningEmployeeGroupId: number;
    employeeGroupId: number;
    employeeGroupName: string;
    timeWorkReductionEarningId: number;
    dateFrom?: Date;
    dateTo?: Date;
    state: SoeEntityState;
}
export class TimeWorkReductionEarningDTO implements ITimeWorkReductionEarningDTO {
    timeWorkReductionEarningId: number;
    minutesWeight: number;
    periodType: number
    state: SoeEntityState;

    timeAccumulatorTimeWorkReductionEarningEmployeeGroup: TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO[];


}
export class TimeAccumulatorItem implements ITimeAccumulatorItem {
    accTodayStartDate: Date;
    accTodayStopDate: Date;
    employeeGroupRuleBoundaries: string;
    employeeGroupRules: TimeAccumulatorItemRule[];
    hasPlanningPeriod: boolean;
    hasTimePeriod: boolean;
    name: string;
    planningPeriodDatesText: string;
    planningPeriodName: string;
    planningPeriodStartDate: Date;
    planningPeriodStopDate: Date;
    sumAccToday: number;
    sumAccTodayIsQuantity: boolean;
    sumAccTodayValue: number;
    sumInvoiceAccToday: number;
    sumInvoicePeriod: number;
    sumInvoicePlanningPeriod: number;
    sumInvoiceToday: number;
    sumInvoiceYear: number;
    sumPayrollAccToday: number;
    sumPayrollPeriod: number;
    sumPayrollPlanningPeriod: number;
    sumPayrollToday: number;
    sumPayrollYear: number;
    sumPeriod: number;
    sumPeriodIsQuantity: boolean;
    sumPlanningPeriod: number;
    sumPlanningPeriodIsQuantity: boolean;
    sumTimeCodeAccToday: number;
    sumTimeCodePeriod: number;
    sumTimeCodePlanningPeriod: number;
    sumTimeCodeToday: number;
    sumTimeCodeYear: number;
    sumToday: number;
    sumTodayIsQuantity: boolean;
    sumYear: number;
    sumYearIsQuantity: boolean;
    sumYearWithIB: number;
    timeAccumulatorBalanceYear: number;
    timeAccumulatorId: number;
    timePeriodDatesText: string;
    timePeriodName: string;

    //extensions
    rulePlanningPeriod: TimeAccumulatorItemRule;
    rulePlanningPeriodAccToday: TimeAccumulatorItemRule;
    rulePeriod: TimeAccumulatorItemRule;
    ruleYear: TimeAccumulatorItemRule;
    ruleAccToday: TimeAccumulatorItemRule;

    public createRules() {
        if (this.employeeGroupRules) {
            this.rulePlanningPeriod = _.find(this.employeeGroupRules, { periodType: TermGroup_AccumulatorTimePeriodType.PlanningPeriod });
            this.rulePlanningPeriodAccToday = _.find(this.employeeGroupRules, { periodType: TermGroup_AccumulatorTimePeriodType.PlanningPeriodRunning });
            this.rulePeriod = _.find(this.employeeGroupRules, { periodType: TermGroup_AccumulatorTimePeriodType.Period });
            this.ruleYear = _.find(this.employeeGroupRules, { periodType: TermGroup_AccumulatorTimePeriodType.Year });
            this.ruleAccToday = _.find(this.employeeGroupRules, { periodType: TermGroup_AccumulatorTimePeriodType.AccToday });
        }

        if (!this.rulePlanningPeriod) {
            this.rulePlanningPeriod = new TimeAccumulatorItemRule();
            this.rulePlanningPeriod.periodType = TermGroup_AccumulatorTimePeriodType.PlanningPeriod;
            this.rulePlanningPeriod.comparison = 0;
        }
        if (!this.rulePlanningPeriodAccToday) {
            this.rulePlanningPeriodAccToday = new TimeAccumulatorItemRule();
            this.rulePlanningPeriodAccToday.periodType = TermGroup_AccumulatorTimePeriodType.PlanningPeriodRunning;
            this.rulePlanningPeriodAccToday.comparison = 0;
        }
        if (!this.rulePeriod) {
            this.rulePeriod = new TimeAccumulatorItemRule();
            this.rulePeriod.periodType = TermGroup_AccumulatorTimePeriodType.Period;
            this.rulePeriod.comparison = 0;
        }
        if (!this.ruleYear) {
            this.ruleYear = new TimeAccumulatorItemRule();
            this.ruleYear.periodType = TermGroup_AccumulatorTimePeriodType.Year;
            this.ruleYear.comparison = 0;
        }
        if (!this.ruleAccToday) {
            this.ruleAccToday = new TimeAccumulatorItemRule();
            this.ruleAccToday.periodType = TermGroup_AccumulatorTimePeriodType.AccToday;
            this.ruleAccToday.comparison = 0;
        }
    }

}

export class TimeAccumulatorItemRule implements ITimeAccumulatorRuleItem {
    comparison: SoeTimeAccumulatorComparison;
    diff: string;
    diffMinutes: number;
    diffValue: System.ITimeSpan;
    periodType: TermGroup_AccumulatorTimePeriodType;
    showError: boolean;
    showWarning: boolean;
    valueMinutes: number;
    warningMinutes: number;
}
