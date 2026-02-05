import { IShiftTypeDTO, IShiftTypeEmployeeStatisticsTargetDTO, IShiftTypeSkillDTO, IShiftTypeGridDTO, IShiftTypeLinkDTO, IShiftTypeHierarchyAccountDTO } from "../../Scripts/TypeLite.Net4";
import { AccountingSettingsRowDTO } from "./AccountingSettingsRowDTO";
import { SoeEntityState, TermGroup_TimeScheduleTemplateBlockType, TermGroup_EmployeeStatisticsType, TermGroup_AttestRoleUserAccountPermissionType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { GraphicsUtility } from "../../Util/GraphicsUtility";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class ShiftTypeDTO implements IShiftTypeDTO {
    accountId: number;
    accountingSettings: AccountingSettingsRowDTO;
    accountInternalIds: number[];
    actorCompanyId: number;
    categoryIds: number[];
    childHierarchyAccountIds: number[];
    color: string;
    created: Date;
    createdBy: string;
    defaultLength: number;
    description: string;
    employeeStatisticsTargets: IShiftTypeEmployeeStatisticsTargetDTO[];
    externalCode: string;
    externalId: number;
    handlingMoney: boolean;
    hierarchyAccounts: IShiftTypeHierarchyAccountDTO[];
    linkedShiftTypeIds: number[];
    modified: Date;
    modifiedBy: string;
    name: string;
    needsCode: string;
    shiftTypeId: number;
    shiftTypeSkills: IShiftTypeSkillDTO[];
    startTime: Date;
    state: SoeEntityState;
    stopTime: Date;
    timeScheduleTemplateBlockType: TermGroup_TimeScheduleTemplateBlockType;
    timeScheduleTypeId: number;
    accountIsNotActive: boolean
    accountNrAndName: string;

    // Extensions
    get defaultLengthFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.defaultLength);
    }
    set defaultLengthFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.defaultLength = CalendarUtility.timeSpanToMinutes(span);
    }

    get hasHierarchyAccounts(): boolean {
        return this.hierarchyAccounts && this.hierarchyAccounts.length > 0;
    }

    public fixColors() {
        // Remove alpha values in color property
        this.color = GraphicsUtility.removeAlphaValue(this.color, Constants.SHIFT_TYPE_UNSPECIFIED_COLOR);
    }
}

export class ShiftTypeGridDTO implements IShiftTypeGridDTO {
    accountId: number;
    accountingStringAccountNames: string;
    categoryNames: string;
    color: string;
    defaultLength: number;
    description: string;
    externalCode: string;
    name: string;
    needsCode: string;
    needsCodeName: string;
    shiftTypeId: number;
    skillNames: string;
    timeScheduleTemplateBlockType: TermGroup_TimeScheduleTemplateBlockType;
    timeScheduleTemplateBlockTypeName: string;
    timeScheduleTypeId: number;
    timeScheduleTypeName: string;
    accountIsNotActive: boolean;
}

export class ShiftTypeHierarchyAccountDTO implements IShiftTypeHierarchyAccountDTO {
    accountId: number;
    accountPermissionType: TermGroup_AttestRoleUserAccountPermissionType;
    shiftTypeHierarchyAccountId: number;

    // Extensions
    accountDimName: string;
    accountName: string;
    accountPermissionTypeName: string;
}

export class ShiftTypeLinkDTO implements IShiftTypeLinkDTO {
    actorCompanyId: number;
    guid: string;
    shiftTypes: ShiftTypeDTO[];
    get shiftTypeNames(): string {
        if (this.shiftTypes && this.shiftTypes.length > 0)
            return _.map(_.sortBy(_.filter(this.shiftTypes, s => s), 'name'), s => s.name).join(', ');
        else
            return '';
    }
    get nrOfShiftTypes(): number {
        return this.shiftTypes.length;
    }
}

export class ShiftTypeSkillDTO implements IShiftTypeSkillDTO {
    missing: boolean;
    shiftTypeId: number;
    shiftTypeSkillId: number;
    skillId: number;
    skillLevel: number;
    skillLevelStars: number;
    skillName: string;
    skillTypeName: string;
}

export class ShiftTypeEmployeeStatisticsTargetDTO implements IShiftTypeEmployeeStatisticsTargetDTO {
    created: Date;
    createdBy: string;
    employeeStatisticsType: TermGroup_EmployeeStatisticsType;
    employeeStatisticsTypeName: string;
    fromDate: Date;
    modified: Date;
    modifiedBy: string;
    shiftTypeEmployeeStatisticsTargetId: number;
    shiftTypeId: number;
    state: SoeEntityState;
    targetValue: number;
}

