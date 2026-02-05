import { SoeEntityState, TermGroup_IOType, TermGroup_IOStatus, TermGroup_IOSource, TermGroup_IOImportHeadType } from "../../Util/CommonEnumerations";

export class VoucherRowIODTO {

    voucherRowIOId: number;
    voucherHeadIOId: number;
    actorCompanyId: number;
    import: boolean;
    type: TermGroup_IOType;
    status: TermGroup_IOStatus;
    source: TermGroup_IOSource;
    importHeadType: TermGroup_IOImportHeadType;
    batchId: string;
    errorMessage: string;
    created: Date; 
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    voucherRowId: number;
    text: string;
    accountId: number;
    accountNr: string;
    accountName: string;
    accountDim2Id: number;
    accountDim2Nr: string;
    accountDim2Name: string;
    accountDim3Id: number;
    accountDim3Nr: string;
    accountDim3Name: string;
    accountDim4Id: number;
    accountDim4Nr: string;
    accountDim4Name: string;
    accountDim5Id: number;
    accountDim5Nr: string;
    accountDim5Name: string;
    accountDim6Id: number;
    accountDim6Nr: string;
    accountDim6Name: string;
    amount: number;
    debetAmount: number;
    creditAmount: number;
    quantity: number;
    statusName: string;

}