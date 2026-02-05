import { IEmployeePostDTO, ISmallGenericType, IEmployeeGroupDTO, IEmployeePostSkillDTO, IScheduleCycleDTO, IShiftTypeDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { TermGroup_EmployeePostWeekendType, SoeEntityState, SoeEmployeePostStatus } from "../../Util/CommonEnumerations";
import { DayOfWeek } from "../../Util/Enumerations";

export class EmployeePostDTO implements IEmployeePostDTO {
    accountId: number;
    accountName: string;
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    dateFrom: Date;
    dateTo: Date;
    dayOfWeeks: string;
    dayOfWeeksGenericType: ISmallGenericType[];
    description: string;
    employeeGroupDTO: IEmployeeGroupDTO;
    employeeGroupId: number;
    employeeGroupName: string;
    employeePostId: number;
    employeePostSkillDTOs: IEmployeePostSkillDTO[];
    employeePostWeekendType: TermGroup_EmployeePostWeekendType;
    freeDays: DayOfWeek[];
    hasMinMaxTimeSpan: boolean;
    ignoreDaysOfWeekIds: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    overDayOfWeekIds: number[];
    overWriteDayOfWeekIds: number[];
    remainingWorkDaysWeek: number;
    scheduleCycleDTO: IScheduleCycleDTO;
    scheduleCycleId: number;
    skillNames: string;
    state: SoeEntityState;
    status: SoeEmployeePostStatus;
    validShiftTypes: IShiftTypeDTO[];
    workDaysWeek: number;
    workTimeCycle: number;
    workTimePercent: number;
    workTimePerDay: number;
    workTimeWeek: number;
    workTimeWeekMax: number;
    workTimeWeekMin: number;

    // Extensions
    dayOfWeekIds: number[];
    dayOfWeeksGridString: string;
    isLocked: boolean;

    get workTimeWeekFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.workTimeWeek);
    }
    set workTimeWeekFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.workTimeWeek = CalendarUtility.timeSpanToMinutes(span);
    }

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }
}
