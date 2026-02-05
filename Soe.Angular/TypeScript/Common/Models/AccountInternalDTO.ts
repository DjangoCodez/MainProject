import { IAccountInternalDTO } from "../../Scripts/TypeLite.Net4";
import { AccountDTO } from "./AccountDTO";


export class AccountInternalDTO implements IAccountInternalDTO {
    account: AccountDTO;
    accountDimId: number;
    accountDimNr: number;
    accountId: number;
    accountNr: string;
    mandatoryLevel: number;
    name: string;
    sysSieDimNr: number;
    sysSieDimNrOrAccountDimNr: number;
    useVatDeduction: boolean;
    vatDeduction: number;
}
