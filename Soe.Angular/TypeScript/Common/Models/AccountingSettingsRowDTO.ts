import { IAccountingSettingsRowDTO } from "../../Scripts/TypeLite.Net4";


export class AccountingSettingsRowDTO implements IAccountingSettingsRowDTO {
    type: number;
    account1Id: number;
    account1Nr: string;
    account1Name: string;
    account2Id: number;
    account2Nr: string;
    account2Name: string;
    account3Id: number;
    account3Nr: string;
    account3Name: string;
    account4Id: number;
    account4Nr: string;
    account4Name: string;
    account5Id: number;
    account5Nr: string;
    account5Name: string;
    account6Id: number;
    account6Nr: string;
    account6Name: string;
    accountDim1Nr: number;
    accountDim2Nr: number;
    accountDim3Nr: number;
    accountDim4Nr: number;
    accountDim5Nr: number;
    accountDim6Nr: number;
    percent: number;

    // Extensions
    typeName: string;
    baseAccount: string;

    constructor(type: number) {
        this.type = type;
    }

    public getAccountId(dimNr: number): number {
        if (dimNr === this.accountDim1Nr)
            return this.account1Id;
        else if (dimNr === this.accountDim2Nr)
            return this.account2Id;
        else if (dimNr === this.accountDim3Nr)
            return this.account3Id;
        else if (dimNr === this.accountDim4Nr)
            return this.account4Id;
        else if (dimNr === this.accountDim5Nr)
            return this.account5Id;
        else if (dimNr === this.accountDim6Nr)
            return this.account6Id;
    }
}
