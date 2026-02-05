import { IEmployeeVacationSEDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class EmployeeVacationSEDTO implements IEmployeeVacationSEDTO {
    adjustmentDate: Date;
    created: Date;
    createdBy: string;
    debtInAdvanceAmount: number;
    debtInAdvanceDelete: boolean;
    debtInAdvanceDueDate: Date;
    earnedDaysAdvance: number;
    earnedDaysPaid: number;
    earnedDaysRemainingHoursAdvance: number;
    earnedDaysRemainingHoursOverdue: number;
    earnedDaysRemainingHoursPaid: number;
    earnedDaysRemainingHoursUnpaid: number;
    earnedDaysRemainingHoursYear1: number;
    earnedDaysRemainingHoursYear2: number;
    earnedDaysRemainingHoursYear3: number;
    earnedDaysRemainingHoursYear4: number;
    earnedDaysRemainingHoursYear5: number;
    earnedDaysUnpaid: number;
    employeeId: number;
    employeeVacationSEId: number;
    employmentRateOverdue: number;
    employmentRatePaid: number;
    employmentRateYear1: number;
    employmentRateYear2: number;
    employmentRateYear3: number;
    employmentRateYear4: number;
    employmentRateYear5: number;
    modified: Date;
    modifiedBy: string;
    paidVacationAllowance: number;
    paidVacationVariableAllowance: number;
    prelPayedDaysYear1: number;
    remainingDaysAdvance: number;
    remainingDaysAllowanceYear1: number;
    remainingDaysAllowanceYear2: number;
    remainingDaysAllowanceYear3: number;
    remainingDaysAllowanceYear4: number;
    remainingDaysAllowanceYear5: number;
    remainingDaysAllowanceYearOverdue: number;
    remainingDaysVariableAllowanceYear1: number;
    remainingDaysVariableAllowanceYear2: number;
    remainingDaysVariableAllowanceYear3: number;
    remainingDaysVariableAllowanceYear4: number;
    remainingDaysVariableAllowanceYear5: number;
    remainingDaysVariableAllowanceYearOverdue: number;
    remainingDaysOverdue: number;
    remainingDaysPaid: number;
    remainingDaysUnpaid: number;
    remainingDaysYear1: number;
    remainingDaysYear2: number;
    remainingDaysYear3: number;
    remainingDaysYear4: number;
    remainingDaysYear5: number;
    savedDaysOverdue: number;
    savedDaysYear1: number;
    savedDaysYear2: number;
    savedDaysYear3: number;
    savedDaysYear4: number;
    savedDaysYear5: number;
    state: SoeEntityState;
    totalRemainingDays: number;
    totalRemainingHours: number;
    usedDaysAdvance: number;
    usedDaysOverdue: number;
    usedDaysPaid: number;
    usedDaysUnpaid: number;
    usedDaysYear1: number;
    usedDaysYear2: number;
    usedDaysYear3: number;
    usedDaysYear4: number;
    usedDaysYear5: number;

    constructor() {
        this.remainingDaysPaid = 0;
        this.remainingDaysUnpaid = 0;
        this.remainingDaysAdvance = 0;
        this.remainingDaysYear1 = 0;
        this.remainingDaysYear2 = 0;
        this.remainingDaysYear3 = 0;
        this.remainingDaysYear4 = 0;
        this.remainingDaysYear5 = 0;
        this.remainingDaysOverdue = 0;

        this.earnedDaysRemainingHoursPaid = 0;
        this.earnedDaysRemainingHoursUnpaid = 0;
        this.earnedDaysRemainingHoursAdvance = 0;
        this.earnedDaysRemainingHoursYear1 = 0;
        this.earnedDaysRemainingHoursYear2 = 0;
        this.earnedDaysRemainingHoursYear3 = 0;
        this.earnedDaysRemainingHoursYear4 = 0;
        this.earnedDaysRemainingHoursYear5 = 0;
        this.earnedDaysRemainingHoursOverdue = 0;
    }

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.modified = CalendarUtility.convertToDate(this.modified);
        this.debtInAdvanceDueDate = CalendarUtility.convertToDate(this.debtInAdvanceDueDate);
    }
}
