import { ITimeRuleGridDTO, ITimeCodeDTO, System, ITimeRuleExpressionDTO, ITimeRuleOperandDTO, ITimeRuleEditDTO, ITimeRuleEditIwhDTO, ISmallGenericType, ITimeRuleExportImportUnmatchedDTO, ITimeRuleExportImportDTO, ITimeRuleExportImportDayTypeDTO, ITimeRuleExportImportEmployeeGroupDTO, ITimeRuleExportImportTimeCodeDTO, ITimeRuleExportImportTimeDeviationCauseDTO, ITimeRuleExportImportTimeScheduleTypeDTO, ITimeRuleImportedDetailsDTO } from "../../Scripts/TypeLite.Net4";
import { SoeTimeRuleType, SoeTimeRuleDirection, SoeEntityState, SoeTimeRuleOperatorType, SoeTimeRuleComparisonOperator, SoeTimeRuleValueType, TimeRuleExportImportUnmatchedType, TermGroup_SysDayType, TermGroup_TimeDeviationCauseType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class TimeRuleEditDTO implements ITimeRuleEditDTO {
    adjustStartToTimeBlockStart: boolean;
    breakIfAnyFailed: boolean;
    created: Date;
    createdBy: string;
    dayTypeIds: number[];
    description: string;
    employeeGroupIds: number[];
    exportImportUnmatched: ITimeRuleExportImportUnmatchedDTO[];
    exportStartExpression: string;
    exportStopExpression: string;
    factor: number;
    imported: boolean;
    importStartExpression: string;
    importStopExpression: string;
    inconvenientWorkHourRule: TimeRuleEditIwhDTO;
    isInconvenientWorkHours: boolean;
    isStandby: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    ruleStartDirection: number;
    ruleStopDirection: number;
    sort: number;
    standardMinutes: number;
    startDate: Date;
    state: SoeEntityState;
    stopDate: Date;
    timeCodeId: number;
    timeCodeMaxLength: number;
    timeCodeMaxPerDay: boolean;
    timeDeviationCauseIds: number[];
    timeRuleExpressions: TimeRuleExpressionDTO[];
    timeRuleId: number;
    timeScheduleTypeIds: number[];
    type: SoeTimeRuleType;

    // Extensions
    selectedEmployeeGroups: ISmallGenericType[] = [];
    selectedScheduleTypes: ISmallGenericType[] = [];
    selectedTimeDeviationCauses: ISmallGenericType[] = [];
    selectedDayTypes: ISmallGenericType[] = [];

    get isActive(): boolean {
        return (this.state === SoeEntityState.Active);
    }
    set isActive(active: boolean) {
        this.state = (active ? SoeEntityState.Active : SoeEntityState.Inactive);
    }

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
    }

    public setTypes() {
        if (this.inconvenientWorkHourRule) {
            let iObj = new TimeRuleEditIwhDTO();
            angular.extend(iObj, this.inconvenientWorkHourRule);
            iObj.fixDates();
            this.inconvenientWorkHourRule = iObj;
        }

        if (this.timeRuleExpressions) {
            this.timeRuleExpressions = this.timeRuleExpressions.map(e => {
                let eObj = new TimeRuleExpressionDTO();
                angular.extend(eObj, e);

                if (e.timeRuleOperands) {
                    eObj.timeRuleOperands = e.timeRuleOperands.map(o => {
                        let oObj = new TimeRuleOperandDTO(o.operatorType);
                        angular.extend(oObj, o);
                        return oObj;
                    });
                } else {
                    eObj.timeRuleOperands = [];
                }

                return eObj;
            });
        } else {
            this.timeRuleExpressions = [];
        }

        if (this.exportImportUnmatched) {
            this.exportImportUnmatched = this.exportImportUnmatched.map(e => {
                let eObj = new TimeRuleExportImportUnmatchedDTO();
                angular.extend(eObj, e);
                return eObj;
            });
        } else {
            this.exportImportUnmatched = [];
        }
    }

    // Export/Import extensions
    nbrOfExportedEmployeeGroups: number;
    nbrOfExportedTimeScheduleTypes: number;
    nbrOfExportedTimeDeviationCauses: number;
    nbrOfExportedDayTypes: number;

    public get hasUnmatchedTimeCode(): boolean {
        return this.hasUnmatchedOfType(TimeRuleExportImportUnmatchedType.TimeCode);
    }

    public get hasUnmatchedEmployeeGroup(): boolean {
        return this.hasUnmatchedOfType(TimeRuleExportImportUnmatchedType.EmployeeGroup);
    }

    public get nbrOfUnmatchedEmployeeGroups(): number {
        return this.getUnmatchedOfType(TimeRuleExportImportUnmatchedType.EmployeeGroup).length;
    }

    public get hasUnmatchedTimeScheduleType(): boolean {
        return this.hasUnmatchedOfType(TimeRuleExportImportUnmatchedType.TimeScheduleType);
    }

    public get nbrOfUnmatchedTimeScheduleTypes(): number {
        return this.getUnmatchedOfType(TimeRuleExportImportUnmatchedType.TimeScheduleType).length;
    }

    public get hasUnmatchedTimeDeviationCause(): boolean {
        return this.hasUnmatchedOfType(TimeRuleExportImportUnmatchedType.TimeDeviationCause);
    }

    public get nbrOfUnmatchedTimeDeviationCauses(): number {
        return this.getUnmatchedOfType(TimeRuleExportImportUnmatchedType.TimeDeviationCause).length;
    }

    public get hasUnmatchedDayType(): boolean {
        return this.hasUnmatchedOfType(TimeRuleExportImportUnmatchedType.DayType);
    }

    public get nbrOfUnmatchedDayTypes(): number {
        return this.getUnmatchedOfType(TimeRuleExportImportUnmatchedType.DayType).length;
    }

    private hasUnmatchedOfType(type: TimeRuleExportImportUnmatchedType): boolean {
        return this.exportImportUnmatched && _.some(this.exportImportUnmatched, e => e.type === type);
    }

    public getUnmatchedOfType(type: TimeRuleExportImportUnmatchedType): TimeRuleExportImportUnmatchedDTO[] {
        return _.filter(this.exportImportUnmatched, e => e.type === type);
    }

    public get hasUnmatchedStartExpression(): boolean {
        return this.exportStartExpression !== this.importStartExpression;
    }

    public get hasUnmatchedStopExpression(): boolean {
        return this.exportStopExpression !== this.importStopExpression;
    }

    public get hasUnmatchedExpression(): boolean {
        return this.hasUnmatchedStartExpression || this.hasUnmatchedStopExpression;
    }
}

export class TimeRuleEditIwhDTO implements ITimeRuleEditIwhDTO {
    information: string;
    length: string;
    payrollProductFactor: number;
    payrollProductName: string;
    startTime: Date;
    stopTime: Date;

    public fixDates() {
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);
    }
}

export class TimeRuleExportImportUnmatchedDTO implements ITimeRuleExportImportUnmatchedDTO {
    code: string;
    id: number;
    name: string;
    type: TimeRuleExportImportUnmatchedType;
}

export class TimeRuleExportImportDTO implements ITimeRuleExportImportDTO {
    dayTypes: TimeRuleExportImportDayTypeDTO[];
    employeeGroups: TimeRuleExportImportEmployeeGroupDTO[];
    exportedFromCompany: string;
    filename: string;
    originalJson: string;
    timeCodes: TimeRuleExportImportTimeCodeDTO[];
    timeDeviationCauses: TimeRuleExportImportTimeDeviationCauseDTO[];
    timeRules: TimeRuleEditDTO[];
    timeScheduleTypes: TimeRuleExportImportTimeScheduleTypeDTO[];

    public setTypes() {
        if (this.timeRules) {
            this.timeRules = this.timeRules.map(r => {
                let obj = new TimeRuleEditDTO();
                angular.extend(obj, r);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        } else {
            this.timeRules = [];
        }

        if (this.dayTypes) {
            this.dayTypes = this.dayTypes.map(d => {
                let obj = new TimeRuleExportImportDayTypeDTO();
                angular.extend(obj, d);
                return obj;
            });
        } else {
            this.dayTypes = [];
        }

        if (this.employeeGroups) {
            this.employeeGroups = this.employeeGroups.map(e => {
                let obj = new TimeRuleExportImportEmployeeGroupDTO();
                angular.extend(obj, e);
                return obj;
            });
        } else {
            this.employeeGroups = [];
        }

        if (this.timeCodes) {
            this.timeCodes = this.timeCodes.map(t => {
                let obj = new TimeRuleExportImportTimeCodeDTO();
                angular.extend(obj, t);
                return obj;
            });
        } else {
            this.timeCodes = [];
        }

        if (this.timeDeviationCauses) {
            this.timeDeviationCauses = this.timeDeviationCauses.map(t => {
                let obj = new TimeRuleExportImportTimeDeviationCauseDTO();
                angular.extend(obj, t);
                return obj;
            });
        } else {
            this.timeDeviationCauses = [];
        }

        if (this.timeScheduleTypes) {
            this.timeScheduleTypes = this.timeScheduleTypes.map(t => {
                let obj = new TimeRuleExportImportTimeScheduleTypeDTO();
                angular.extend(obj, t);
                return obj;
            });
        } else {
            this.timeScheduleTypes = [];
        }
    }
}

export class TimeRuleExportImportDayTypeDTO implements ITimeRuleExportImportDayTypeDTO {
    dayTypeId: number;
    matchedDayTypeId: number;
    name: string;
    sysDayTypeId: number;
    type: TermGroup_SysDayType;

    // Extensions
    originallyUnmatched: boolean;
}

export class TimeRuleExportImportEmployeeGroupDTO implements ITimeRuleExportImportEmployeeGroupDTO {
    employeeGroupId: number;
    matchedEmployeeGroupId: number;
    name: string;

    // Extensions
    originallyUnmatched: boolean;
}

export class TimeRuleExportImportTimeCodeDTO implements ITimeRuleExportImportTimeCodeDTO {
    code: string;
    matchedTimeCodeId: number;
    name: string;
    timeCodeId: number;

    // Extensions
    originallyUnmatched: boolean;
}

export class TimeRuleExportImportTimeDeviationCauseDTO implements ITimeRuleExportImportTimeDeviationCauseDTO {
    matchedTimeDeviationCauseId: number;
    name: string;
    timeDeviationCauseId: number;
    type: TermGroup_TimeDeviationCauseType;

    // Extensions
    originallyUnmatched: boolean;
}

export class TimeRuleExportImportTimeScheduleTypeDTO implements ITimeRuleExportImportTimeScheduleTypeDTO {
    code: string;
    matchedTimeScheduleTypeId: number;
    name: string;
    timeScheduleTypeId: number;

    // Extensions
    originallyUnmatched: boolean;
}

export class TimeRuleImportedDetailsDTO implements ITimeRuleImportedDetailsDTO {
    companyName: string;
    imported: Date;
    importedBy: string;
    json: string;
    originalJson: string;

    public fixDates() {
        this.imported = CalendarUtility.convertToDate(this.imported);
    }
}

export class TimeRuleGridDTO implements ITimeRuleGridDTO {
    actorCompanyId: number;
    adjustStartToTimeBlockStart: string;
    breakIfAnyFailed: string;
    dayTypeNames: string;
    description: string;
    employeeGroupNames: string;
    imported: boolean;
    internal: boolean;
    isActive: boolean;
    isInconvenientWorkHours: string;
    isStandby: string;
    name: string;
    sort: number;
    standardMinutes: string;
    startDate: Date;
    startDirection: SoeTimeRuleDirection;
    startDirectionName: string;
    startExpression: string;
    stopDate: Date;
    stopExpression: string;
    timeCodeId: number;
    timeCodeMaxLength: number;
    timeCodeName: string;
    timeDeviationCauseNames: string;
    timeRuleId: number;
    timeScheduleTypesNames: string;
    type: SoeTimeRuleType;
    typeName: string;

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
    }
}

export class TimeRuleExpressionDTO implements ITimeRuleExpressionDTO {
    isStart: boolean;
    timeRuleExpressionId: number;
    timeRuleId: number;
    timeRuleOperands: TimeRuleOperandDTO[];
}

export class TimeRuleOperandDTO implements ITimeRuleOperandDTO {
    comparisonOperator: SoeTimeRuleComparisonOperator;
    leftValueId: number;
    leftValueType: SoeTimeRuleValueType;
    minutes: number;
    operatorType: SoeTimeRuleOperatorType;
    orderNbr: number;
    rightValueId: number;
    rightValueType: SoeTimeRuleValueType;
    timeRuleExpressionId: number;
    timeRuleExpressionRecursive: TimeRuleExpressionDTO;
    timeRuleExpressionRecursiveId: number;
    timeRuleOperandId: number;

    constructor(operatorType: SoeTimeRuleOperatorType) {
        this.operatorType = operatorType;
    }
}

