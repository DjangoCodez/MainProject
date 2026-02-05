import { ISysHelpDTO, ISysHelpSmallDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class SysHelpDTO implements ISysHelpDTO {
    sysHelpId: number;
    sysLanguageId: number;
    sysFeatureId: number;
    versionNr: number;
    title: string;
    text: string;
    plainText: string;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    language: string;
}

export class SysHelpSmallDTO implements ISysHelpSmallDTO {
    sysHelpId: number;
    title: string;
    text: string;
    plainText: string;
    sysFeatureId: number;
}
