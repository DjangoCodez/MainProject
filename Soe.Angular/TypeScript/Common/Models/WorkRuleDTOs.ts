import { IEvaluateAllWorkRulesResultDTO, IEvaluateWorkRuleResultDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_ShiftHistoryType, SoeScheduleWorkRules } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class EvaluateWorkRuleResultDTO implements IEvaluateWorkRuleResultDTO {
    action: TermGroup_ShiftHistoryType;
    canUserOverrideRuleForMinorsViolation: boolean;
    canUserOverrideRuleViolation: boolean;
    date: Date;
    employeeId: number;
    errorMessage: string;
    errorNumber: number;
    evaluatedWorkRule: SoeScheduleWorkRules;
    isRuleForMinors: boolean;
    isRuleRestTimeDayMandatory: boolean;
    isRuleRestTimeWeekMandatory: boolean;
    restTimeDayReachedDateFrom: Date;
    restTimeDayReachedDateTo: Date;
    success: boolean;
    workTimeReachedDateFrom: Date;
    workTimeReachedDateTo: Date;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
        this.restTimeDayReachedDateFrom = CalendarUtility.convertToDate(this.restTimeDayReachedDateFrom);
        this.restTimeDayReachedDateTo = CalendarUtility.convertToDate(this.restTimeDayReachedDateTo);
        this.workTimeReachedDateFrom = CalendarUtility.convertToDate(this.workTimeReachedDateFrom);
        this.workTimeReachedDateFrom = CalendarUtility.convertToDate(this.workTimeReachedDateFrom);
    }
}

export class EvaluateAllWorkRulesResultDTO implements IEvaluateAllWorkRulesResultDTO {
    employeeId: number;
    violations: string[];

    // Extensions
    label: string;
    sort: number;
}
