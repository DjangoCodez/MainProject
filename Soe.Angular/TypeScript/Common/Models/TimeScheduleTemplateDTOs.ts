import { ITimeScheduleTemplateHeadSmallDTO, ITimeScheduleTemplatePeriodDTO, ITimeScheduleTemplateBlockDTO, ITimeScheduleTemplatePeriodSmallDTO, ITimeScheduleTemplateChangeDTO, ITimeScheduleTemplateHeadDTO, ITimeScheduleTemplateGroupDTO, ITimeScheduleTemplateGroupGridDTO, ITimeScheduleTemplateGroupRowDTO, ITimeScheduleTemplateGroupEmployeeDTO, ITimeScheduleTemplateHeadRangeDTO, ITimeScheduleTemplateHeadsRangeDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SoeEntityState } from "../../Util/CommonEnumerations";
import { EmployeeScheduleDTO } from "./EmployeeScheduleDTOs";
import { EvaluateWorkRuleResultDTO } from "./WorkRuleDTOs";

export class TimeScheduleTemplateGroupDTO implements ITimeScheduleTemplateGroupDTO {
    employees: TimeScheduleTemplateGroupEmployeeDTO[];
    rows: TimeScheduleTemplateGroupRowDTO[];
    created: Date;
    createdBy: string;
    description: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    templateNames: string;
    timeScheduleTemplateGroupId: number;

    public setTypes() {
        if (this.employees) {
            this.employees = this.employees.map(e => {
                let eObj = new TimeScheduleTemplateGroupEmployeeDTO();
                angular.extend(eObj, e);
                eObj.fixDates();
                return eObj;
            });
        } else {
            this.employees = [];
        }

        if (this.rows) {
            this.rows = this.rows.map(r => {
                let rObj = new TimeScheduleTemplateGroupRowDTO();
                angular.extend(rObj, r);
                rObj.fixDates();
                return rObj;
            });
        } else {
            this.rows = [];
        }
    }

    // Extensions
    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }
}

export class TimeScheduleTemplateGroupGridDTO implements ITimeScheduleTemplateGroupGridDTO {
    description: string;
    name: string;
    nbrOfEmployees: number;
    nbrOfRows: number;
    timeScheduleTemplateGroupId: number;
}

export class TimeScheduleTemplateGroupRowDTO implements ITimeScheduleTemplateGroupRowDTO {
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    nextStartDate: Date;
    recurrencePattern: string;
    startDate: Date;
    state: SoeEntityState;
    stopDate: Date;
    timeScheduleTemplateGroupId: number;
    timeScheduleTemplateGroupRowId: number;
    timeScheduleTemplateHeadId: number;

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
        this.nextStartDate = CalendarUtility.convertToDate(this.nextStartDate);
    }
}

export class TimeScheduleTemplateGroupEmployeeDTO implements ITimeScheduleTemplateGroupEmployeeDTO {
    created: Date;
    createdBy: string;
    employeeId: number;
    employeeName: string;
    employeeNr: string;
    fromDate: Date;
    group: TimeScheduleTemplateGroupDTO;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    timeScheduleTemplateGroupEmployeeId: number;
    timeScheduleTemplateGroupId: number;
    toDate: Date;

    // Extensions
    readOnly: boolean;
    errorMessage: string;
    warningMessage: string;
    infoMessage: string;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.toDate = CalendarUtility.convertToDate(this.toDate);
    }

    public setTypes() {
        if (this.group) {
            let obj = new TimeScheduleTemplateGroupDTO();
            angular.extend(obj, this.group);
            obj.setTypes();
            this.group = obj;
        }
    }
}

export class TimeScheduleTemplateHeadsRangeDTO implements ITimeScheduleTemplateHeadsRangeDTO {
    heads: TimeScheduleTemplateHeadRangeDTO[];

    public setTypes() {
        this.heads = this.heads.map(x => {
            let obj = new TimeScheduleTemplateHeadRangeDTO();
            angular.extend(obj, x);
            obj.fixDates();
            return obj;
        });
    }
}

export class TimeScheduleTemplateHeadRangeDTO implements ITimeScheduleTemplateHeadRangeDTO {
    employeeId: number;
    employeeScheduleId: number;
    employeeScheduleStartDate: Date;
    employeeScheduleStopDate: Date;
    firstMondayOfCycle: Date;
    noOfDays: number;
    startDate: Date;
    stopDate: Date;
    templateName: string;
    timeScheduleTemplateGroupId: number;
    timeScheduleTemplateGroupName: string;
    timeScheduleTemplateHeadId: number;

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
    }
}

export class TimeScheduleTemplateHeadDTO implements ITimeScheduleTemplateHeadDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    description: string;
    employeeId: number;
    employeeName: string;
    employeePostId: number;
    employeeSchedules: EmployeeScheduleDTO[];
    firstMondayOfCycle: Date;
    flexForceSchedule: boolean;
    lastPlacementStartDate: Date;
    lastPlacementStopDate: Date;
    locked: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    noOfDays: number;
    simpleSchedule: boolean;
    startDate: Date;
    startOnFirstDayOfWeek: boolean;
    state: SoeEntityState;
    stopDate: Date;
    timeScheduleTemplateHeadId: number;
    timeScheduleTemplatePeriods: TimeScheduleTemplatePeriodDTO[];

    // Extensions
    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
        this.firstMondayOfCycle = CalendarUtility.convertToDate(this.firstMondayOfCycle);
        this.lastPlacementStartDate = CalendarUtility.convertToDate(this.lastPlacementStartDate, null, true);
        this.lastPlacementStopDate = CalendarUtility.convertToDate(this.lastPlacementStopDate, null, true);
    }

    public setTypes() {
        if (this.timeScheduleTemplatePeriods) {
            this.timeScheduleTemplatePeriods = this.timeScheduleTemplatePeriods.map(p => {
                let pObj = new TimeScheduleTemplatePeriodDTO()
                angular.extend(pObj, p);
                pObj.fixDates();
                return pObj;
            });
        } else {
            this.timeScheduleTemplatePeriods = [];
        }

        if (this.employeeSchedules) {
            this.employeeSchedules = this.employeeSchedules.map(e => {
                let eObj = new EmployeeScheduleDTO()
                angular.extend(eObj, e);
                eObj.fixDates();
                return eObj;
            });
        } else {
            this.employeeSchedules = [];
        }
    }
}

export class TimeScheduleTemplateHeadSmallDTO implements ITimeScheduleTemplateHeadSmallDTO {
    accountId: number;
    accountName: string;
    employeeId: number;
    firstMondayOfCycle: Date;
    locked: boolean;
    name: string;
    noOfDays: number;
    simpleSchedule: boolean;
    startDate: Date;
    stopDate: Date;
    timeScheduleTemplateGroupId: number;
    timeScheduleTemplateGroupName: string;
    timeScheduleTemplateHeadId: number;
    virtualStopDate: Date;

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
        this.virtualStopDate = CalendarUtility.convertToDate(this.virtualStopDate);
        this.firstMondayOfCycle = CalendarUtility.convertToDate(this.firstMondayOfCycle);
    }
}

export class TimeScheduleTemplatePeriodDTO implements ITimeScheduleTemplatePeriodDTO {
    created: Date;
    createdBy: string;
    date: Date;
    dayName: string;
    dayNumber: number;
    hasAttestedTransactions: boolean;
    holidayName: string;
    isHoliday: boolean;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    timeScheduleTemplateBlocks: ITimeScheduleTemplateBlockDTO[];
    timeScheduleTemplateHeadId: number;
    timeScheduleTemplatePeriodId: number;

    // Extensions
    blocks: TimeScheduleTemplateBlockSlim[];

    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}

export class TimeScheduleTemplatePeriodSmallDTO implements ITimeScheduleTemplatePeriodSmallDTO {
    dayNumber: number;
    timeScheduleTemplateHeadId: number;
    timeScheduleTemplatePeriodId: number;
}

export class TimeScheduleTemplateBlockSlim {
    timeScheduleTemplateBlockId: number;
    timeScheduleTemplatePeriodId: number;
    timeCodeId: number;
    dayNumber: number;
    startTime: Date;
    stopTime: Date;
    shiftTypeId: number;
    break1TimeCodeId: number;
    break1Length: number;
    break2TimeCodeId: number;
    break2Length: number;
    break3TimeCodeId: number;
    break3Length: number;
    break4TimeCodeId: number;
    break4Length: number;

    public get shiftLength(): number {
        return this.stopTime.diffMinutes(this.startTime);
    }
    public get breakLength(): number {
        return (this.break1Length || 0) + (this.break2Length || 0) + (this.break3Length || 0) + (this.break4Length || 0);
    }
    public get duration(): number {
        return (this.shiftLength || 0) - (this.breakLength || 0);
    }

    public roundTimes(clockRounding: number) {
        if (clockRounding === 0)
            return;

        this.startTime = this.startTime.roundMinutes(clockRounding);
        this.stopTime = this.stopTime.roundMinutes(clockRounding);
    }
}

export class TimeScheduleTemplateChangeDTO implements ITimeScheduleTemplateChangeDTO {
    date: Date;
    dayTypeName: string;
    hasAbsence: boolean;
    hasInvalidDayType: boolean;
    hasManualChanges: boolean;
    hasWarnings: boolean;
    shiftsBeforeUpdate: string;
    warnings: string;
    workRulesResults: EvaluateWorkRuleResultDTO[];

    // Extensions

    dayTypeToolTip: string;
    hasSameChanges: boolean;
    notSelectable: boolean;
    selected: boolean;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }

    public get hasWorkRuleWarnings(): boolean {
        return this.workRulesResults && _.some(this.workRulesResults, w => (w.isRuleForMinors && w.canUserOverrideRuleForMinorsViolation) || (!w.isRuleForMinors && w.canUserOverrideRuleViolation));
    }

    public get hasWorkRuleErrors(): boolean {
        return this.workRulesResults && _.some(this.workRulesResults, w => (w.isRuleForMinors && !w.canUserOverrideRuleForMinorsViolation) || (!w.isRuleForMinors && !w.canUserOverrideRuleViolation));
    }
}