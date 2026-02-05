import { IEmployeeAccumulatorDTO } from "../../Scripts/TypeLite.Net4";
import { SoeTimeAccumulatorComparison } from "../../Util/CommonEnumerations";

export class EmployeeAccumulatorDTO implements IEmployeeAccumulatorDTO {
    accumulatorAccTodayDates: string;
    accumulatorAccTodayValue: number;
	accumulatorAmount: number;
	accumulatorDiff: number;
	accumulatorId: number;
	accumulatorName: string;
	accumulatorPeriodDates: string;
	accumulatorPeriodValue: number;
	accumulatorRuleMaxMinutes: number;
	accumulatorRuleMaxWarningMinutes: number;
	accumulatorRuleMinMinutes: number;
	accumulatorRuleMinWarningMinutes: number;
	accumulatorShowError: boolean;
	accumulatorShowWarning: boolean;
	accumulatorStatus: SoeTimeAccumulatorComparison;
	accumulatorStatusName: string;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	ownLimitDiff: number;
	ownLimitMax: number;
	ownLimitMin: number;
	ownLimitShowError: boolean;
	ownLimitStatus: SoeTimeAccumulatorComparison;
	ownLimitStatusName: string;


	// Extensions
	public get employeeNrAndName(): string {
		return "({0}) {1}".format(this.employeeNr, this.employeeName);
	}
}