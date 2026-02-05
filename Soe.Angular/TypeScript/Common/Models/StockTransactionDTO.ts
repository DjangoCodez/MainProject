
export class StockTransactionDTO {
    stockTransactionId: number;
    stockProductId: number;
    invoiceRowId: number;

    actionType: number;

    quantity: number;
    price: number;
    note: string;

    created: Date;
    createdBy: string;
    voucherId: number;

    transactionDate: Date;

    productUnitConvertId: number;

    targetStockId: number;

    //Extensions
    productNumber: string;
    productName: string;
    stockName: string;
    actionTypeName: string;
    reservedQuantity: number;

    productId: number;
    stockId: number;
    stockShelfId: number;
    stockShelfName: string;

    constructor() {
        this.actionType = 0;
    }
}

