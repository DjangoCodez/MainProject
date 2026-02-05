import { IImportDTO, System } from "../../Scripts/TypeLite.Net4";
import { TermGroup_IOImportHeadType, TermGroup_SysImportDefinitionType, SoeEntityState } from "../../Util/CommonEnumerations";

export class ImportDTO implements IImportDTO {

    importId: number;
    actorCompanyId: number;
    accountYearId?: number;
    voucherSeriesId?: number;
    importDefinitionId: number;
    module: number;
    name: string;
    headName: string;
    state: SoeEntityState;
    importHeadType: TermGroup_IOImportHeadType;
    type: TermGroup_SysImportDefinitionType;
    typeText: string;
    useAccountDistribution: boolean;
    useAccountDimensions: boolean;
    updateExistingInvoice: boolean;
    guid: System.IGuid;
    specialFunctionality: string;

    dim1AccountId?: number;
    dim2AccountId?: number;
    dim3AccountId?: number;
    dim4AccountId?: number;
    dim5AccountId?: number;
    dim6AccountId?: number;

    isStandard: boolean;
    isStandardText: string;

    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;

    constructor(init?: Partial<ImportDTO>) {
        Object.assign(this, init);
    }
}