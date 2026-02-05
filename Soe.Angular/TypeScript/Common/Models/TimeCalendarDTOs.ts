import { ITimeCalendarPeriodDTO, ITimeCalendarPeriodPayrollProductDTO, ITimeCalendarSummaryDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { TermGroup_SysPayrollType } from "../../Util/CommonEnumerations";

export class TimeCalendarPeriodDTO implements ITimeCalendarPeriodDTO {
    date: Date;
    dayDescription: string;
    payrollProducts: TimeCalendarPeriodPayrollProductDTO[];
    type1: number;
    type1ToolTip: string;
    type2: number;
    type2ToolTip: string;
    type3: number;
    type3ToolTip: string;
    type4: number;
    type4ToolTip: string;

    // Extensions
    type1Amount: number;
    type2Amount: number;
    type3Amount: number;
    type4Amount: number;
    typesAmount: number;

    typesToolTip: string;

    type1Color: string;
    type2Color: string;
    type3Color: string;
    type4Color: string;
    typesColor: string;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }

    public setTypeToolTip(typeNr: number, toolTip: string) {
        if (typeNr >= 1 && typeNr <= 4)
            this[`type${typeNr}ToolTip`] += (this[`type${typeNr}ToolTip`] ? '\n' : '') + toolTip;
        else
            this.typesToolTip += (this.typesToolTip ? '\n' : '') + toolTip;
    }

    public setTypeColor(typeNr: number, product: TimeCalendarPeriodPayrollProductDTO) {
        // Default is green
        let color: string = "#98EF5D";

        // Red for absence and minus time
        if (product.sysPayrollTypeLevel2 == TermGroup_SysPayrollType.SE_GrossSalary_Absence ||
            product.sysPayrollTypeLevel4 == TermGroup_SysPayrollType.SE_Time_Accumulator_MinusTime ||
            product.sysPayrollTypeLevel4 == TermGroup_SysPayrollType.SE_Time_Accumulator_Withdrawal)
            color = "#ED8D6C";

        // Blue for overtime and plus time
        if (product.sysPayrollTypeLevel2 == TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation ||
            product.sysPayrollTypeLevel2 == TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition ||
            product.sysPayrollTypeLevel4 == TermGroup_SysPayrollType.SE_Time_Accumulator_PlusTime ||
            product.sysPayrollTypeLevel4 == TermGroup_SysPayrollType.SE_Time_Accumulator_OverTime)
            color = "#FF00008B";

        if (typeNr >= 1 && typeNr <= 4)
            this[`type${typeNr}Color`] = color;
        else
            this.typesColor = color;
    }
}

export class TimeCalendarPeriodPayrollProductDTO implements ITimeCalendarPeriodPayrollProductDTO {
    amount: number;
    name: string;
    number: string;
    payrollProductId: number;
    sysPayrollTypeLevel1: number;
    sysPayrollTypeLevel2: number;
    sysPayrollTypeLevel3: number;
    sysPayrollTypeLevel4: number;
}

export class TimeCalendarSummaryDTO implements ITimeCalendarSummaryDTO {
    amount: number;
    days: number;
    name: string;
    number: string;
    occations: number;
    payrollProductId: number;

    // Extensions
    isGroup: boolean;
    sysPayrollTypeLevel1: number;
    sysPayrollTypeLevel2: number;
    sysPayrollTypeLevel3: number;
    sysPayrollTypeLevel4: number;}
