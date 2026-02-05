import { IRoleDTO, IRoleEditDTO, IRoleGridDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class RoleDTO implements IRoleDTO {
    actorCompanyId: number;
    actualName: string;
    created: Date;
    createdBy: string;
    externalCodes: string[];
    externalCodesString: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    roleId: number;
    sort: number;
    state: SoeEntityState;
    termId: number;
}

export class RoleEditDTO implements IRoleEditDTO {
    created: Date;
    createdBy: string;
    externalCodesString: string;
    favoriteOption: number;
    isAdmin: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    roleId: number;
    sort: number;
    state: SoeEntityState;
    templateRoleId: number;
    updateStartPage: boolean;
    active: boolean;
    isActive: boolean;
}

export class RoleGridDTO implements IRoleGridDTO {
    name: string;
    externalCodesString: string;
    roleId: number;
    isActive: boolean;
    sort: number;
    state: SoeEntityState;
}