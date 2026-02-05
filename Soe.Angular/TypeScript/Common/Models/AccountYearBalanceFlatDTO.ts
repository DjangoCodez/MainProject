import { IAccountYearBalanceFlatDTO } from "../../Scripts/TypeLite.Net4";
import { AccountInternalDTO } from "./AccountInternalDTO";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class AccountYearBalanceFlatDTO implements IAccountYearBalanceFlatDTO {
    accountYearBalanceHeadId: number;
    accountYearId: number;
    balance: number;
    balanceEntCurrency: number;
    created: Date;
    createdBy: string;
    creditAmount: number;
    debitAmount: number;
    dim1Id: number;
    dim1Name: string;
    dim1Nr: string;
    dim1TypeName: string;
    dim2Id: number;
    dim2Name: string;
    dim2Nr: string;
    dim3Id: number;
    dim3Name: string;
    dim3Nr: string;
    dim4Id: number;
    dim4Name: string;
    dim4Nr: string;
    dim5Id: number;
    dim5Name: string;
    dim5Nr: string;
    dim6Id: number;
    dim6Name: string;
    dim6Nr: string;
    isModified: boolean;
    modified: Date;
    modifiedBy: string;
    quantity: number;
    rowNr: number;

    // Extensions
    isDeleted: boolean;
    isDiffRow: boolean;
}
