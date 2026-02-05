import { IVatCodeDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class VatCodeDTO implements IVatCodeDTO {
    vatCodeId: number;
    actorCompanyId: number;
    accountId: number;
    purchaseVATAccountId: number;
    code: string;
    name: string;
    percent: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    accountNr: string;
    purchaseVATAccountNr: string;
}
