import { IPositionDTO, IPositionSkillDTO, IPositionGridDTO, ISysPositionGridDTO, IEmployeePositionDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class SysPositionGridDTO implements ISysPositionGridDTO {
    code: string;
    description: string;
    isLinked: boolean;
    name: string;
    selected: boolean;
    sysCountryCode: string;
    sysLanguageCode: string;
    sysPositionId: number;

    public get codeAndName(): string {
        return "{0} {1}".format(this.code, this.name);
    }
}

export class PositionDTO implements IPositionDTO {
    actorCompanyId: number;
    code: string;
    created: Date;
    createdBy: string;
    description: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    nameAndCode: string;
    positionId: number;
    positionSkills: IPositionSkillDTO[];
    state: SoeEntityState;
    sysPositionId: number;
    //extension
    selected: boolean;
}

export class EmployeePositionGridDTO implements IPositionGridDTO {
    code: string;
    description: string;
    name: string;
    positionId: number;
    sysPositionId: number;
    //link
    isLinked: boolean;
}

export class SysEmployeePositonGridDTO implements ISysPositionGridDTO {
    code: string;
    description: string;
    isLinked: boolean;
    name: string;
    selected: boolean;
    sysCountryCode: string;
    sysLanguageCode: string;
    sysPositionId: number;
}

export class EmployeePositionDTO implements IEmployeePositionDTO {
    default: boolean;
    employeeId: number;
    employeePositionId: number;
    employeePositionName: string;
    positionId: number;
    sysPositionCode: string;
    sysPositionDescription: string;
    sysPositionName: string;
    //Extension for directive
    selected: boolean;
}