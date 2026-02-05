import { IPurchaseRowDTO } from "../../Scripts/TypeLite.Net4";


export class PurchaseFromOrderDTO {
    createNewPurchase: boolean;
    purchaseId: number;
    supplierId: number;
    orderId: number;
    copyProject: boolean;
    copyInternalAccounts: boolean;
    purchaseRows: IPurchaseRowDTO[];
    copyDeliveryAddress: boolean;
}

