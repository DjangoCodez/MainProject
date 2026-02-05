import { IStockProductDTO } from "../../Scripts/TypeLite.Net4";

export class StockProductDTO implements IStockProductDTO {

    stockProductId: number;
    stockId: number;
    invoiceProductId: number;

    quantity: number;
    orderedQuantity: number;
    reservedQuantity: number;

    isInInventory: boolean;
    warningLevel: number;

    avgPrice: number;

    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    stockShelfId: number;

    deliveryLeadTimeDays: number;
    purchaseQuantity: number;
    purchaseTriggerQuantity: number;
    purchasedQuantity: number;

    //Extensions
    productNumber: string;
    productName: string;
    productUnit: string;
    stockName: string;
    stockShelfCode: string;
    stockShelfName: string;
    stockValue: number;
}
