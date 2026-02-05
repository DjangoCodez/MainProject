import { ICompanyGroupMappingHeadDTO, ICompanyGroupMappingRowDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";
import { AccountDTO } from "./AccountDTO";

export class CompanyGroupMappingHeadDTO implements ICompanyGroupMappingHeadDTO {
    actorCompanyId: number;
    companyGroupMappingHeadId: number;
    created: Date;
    createdBy: string;
    description: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    number: number;
    rows: ICompanyGroupMappingRowDTO[];
    state: SoeEntityState;
    type: number;
}

export class CompanyGroupMappingRowDTO implements ICompanyGroupMappingRowDTO {
    childAccountFrom: number;
    childAccountFromName: string;
    childAccountTo: number;
    childAccountToName: string;
    companyGroupMappingHeadId: number;
    companyGroupMappingRowId: number;
    created: Date;
    createdBy: string;
    groupCompanyAccount: number;
    groupCompanyAccountName: string;
    isDeleted: boolean;
    isModified: boolean;
    isProcessed: boolean;
    modified: Date;
    modifiedBy: string;
    rowNr: number;
    state: SoeEntityState;
}
