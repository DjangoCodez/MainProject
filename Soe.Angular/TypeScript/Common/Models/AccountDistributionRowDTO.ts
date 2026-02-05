import { IAccountDistributionRowDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";


export class AccountDistributionRowDTO implements IAccountDistributionRowDTO {
    accountDistributionRowId: number;
    accountDistributionHeadId: number;
    rowNbr: number;
    calculateRowNbr: number;
    sameBalance: number;
    oppositeBalance: number;
    description: string;
    state: SoeEntityState;
    dim1Id: number;
    dim1Nr: string;
    dim1Name: string;
    dim1Disabled: boolean;
    dim1Mandatory: boolean;
    previousRowNbr: number;
    dim2Id: number;
    dim2Nr: string;
    dim2Name: string;
    dim2Disabled: boolean;
    dim2Mandatory: boolean;
    dim2KeepSourceRowAccount: boolean;
    dim3Id: number;
    dim3Nr: string;
    dim3Name: string;
    dim3Disabled: boolean;
    dim3Mandatory: boolean;
    dim3KeepSourceRowAccount: boolean;
    dim4Id: number;
    dim4Nr: string;
    dim4Name: string;
    dim4Disabled: boolean;
    dim4Mandatory: boolean;
    dim4KeepSourceRowAccount: boolean;
    dim5Id: number;
    dim5Nr: string;
    dim5Name: string;
    dim5Disabled: boolean;
    dim5Mandatory: boolean;
    dim5KeepSourceRowAccount: boolean;
    dim6Id: number;
    dim6Nr: string;
    dim6Name: string;
    dim6Disabled: boolean;
    dim6Mandatory: boolean;
    dim6KeepSourceRowAccount: boolean;

    //used to show errors in the grid.
    dim1Error: string;
    dim2Error: string;
    dim3Error: string;
    dim4Error: string;
    dim5Error: string;
    dim6Error: string;
}
