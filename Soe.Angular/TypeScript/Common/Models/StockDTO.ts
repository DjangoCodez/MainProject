import { IStockDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class StockDTO implements IStockDTO {

    actorCompanyId: number;
    avgPrice: number;
    code: string;
    created: Date;
    createdBy: string;
    isExternal: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    saldo: number;
    state: SoeEntityState;
    stockId: number;
    stockProductId: number;
    stockShelfId: number;
    stockShelfName: string;
    deliveryLeadTimeDays: number;
    purchaseQuantity: number;
    purchaseTriggerQuantity: number;
}
