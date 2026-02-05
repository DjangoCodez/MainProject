import { ISysPositionDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class SysPositionDTO implements ISysPositionDTO {
    code: string;
    created: Date;
    createdBy: string;
    description: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    sysCountryCode: string;
    sysCountryId: number;
    sysLanguageCode: string;
    sysLanguageId: number;
    sysPositionId: number;
}