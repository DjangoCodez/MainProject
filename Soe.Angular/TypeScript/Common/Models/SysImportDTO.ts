import { ISysImportDefinitionDTO, ISysImportHeadDTO, ISysImportDefinitionLevelDTO, ISysImportRelationDTO, ISysImportSelectDTO, ISysImportDefinitionLevelColumnSettings } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_SysImportDefinitionType, SoeModule } from "../../Util/CommonEnumerations";
import { Guid } from "../../Util/StringUtility";

export class SysImportDefinitionDTO implements ISysImportDefinitionDTO {
    created: Date;
    createdBy: string;
    guid: Guid;
    modified: Date;
    modifiedBy: string;
    module: SoeModule;
    name: string;
    separator: string;
    specialFunctionality: string;
    state: SoeEntityState;
    sysImportDefinitionId: number;
    sysImportDefinitionLevels: ISysImportDefinitionLevelDTO[];
    sysImportHeadId: number;
    type: TermGroup_SysImportDefinitionType;
    xmlTagHead: string;
}

export class SysImportDefinitionLevelDTO implements ISysImportDefinitionLevelDTO {
    columns: ISysImportDefinitionLevelColumnSettings[];
    level: number;
    sysImportDefinitionId: number;
    sysImportDefinitionLevelId: number;
    xml: string;
}

export class SysImportDefinitionLevelColumnSettings implements ISysImportDefinitionLevelColumnSettings {
    characters: number;
    column: string;
    convert: string;
    from: number;
    isModified: boolean;
    level: number;
    position: number;
    standard: string;
    sysImportDefinitionLevelColumnSettingsId: number;
    text: string;
    updateTypeId: number;
    updateTypeText: string;
    xmlTag: string;
}

export class SysImportHeadDTO implements ISysImportHeadDTO {
    created: Date;
    createdBy: string;
    description: string;
    modified: Date;
    modifiedBy: string;
    module: SoeModule;
    name: string;
    sortorder: number;
    state: SoeEntityState;
    sysImportHeadId: number;
    sysImportRelations: ISysImportRelationDTO[];
    sysImportSelects: ISysImportSelectDTO[];
}
