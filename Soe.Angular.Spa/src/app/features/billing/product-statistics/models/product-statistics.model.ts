import { SoeOriginType, TermGroup_InvoiceProductVatType } from "@shared/models/generated-interfaces/Enumerations";
import { IProductStatisticsDTO } from "@shared/models/generated-interfaces/ProductStatisticsDTO";
import { IProductStatisticsRequest } from "@shared/models/generated-interfaces/ProductStatisticsRequest";

export class ProductStatisticsDTO implements IProductStatisticsDTO {
    originType: SoeOriginType;
    originTypeName: string;
    productId: number;
    productNr: string;
    productName: string;
    year: string;
    month: string;
    invoiceDate?: Date;
    purchaseDeliveryDate?: Date;
    invoiceId: number;
    invoiceNr: string;
    orderId: number;
    orderNr: string;
    purchaseId: number;
    purchaseNr: string;
    customerNr: string;
    customerName: string;
    supplierNr: string;
    supplierName: string;
    purchaseQty: number;
    customerInvoiceQty: number;
    customerInvoiceAmount: number;
    marginalIncome: number;
    marginalRatio: number;
    vatType: TermGroup_InvoiceProductVatType;
    vatTypeName: string;

    constructor() {
        this.originType = 0;
        this.originTypeName = '';
        this.productId = 0;
        this.productNr = '';
        this.productName = '';
        this.year = '';
        this.month = '';
        this.invoiceId = 0;
        this.invoiceNr = '';
        this.orderId = 0;
        this.orderNr = '';
        this.purchaseId = 0;
        this.purchaseNr = '';
        this.customerNr = '';
        this.customerName = '';
        this.supplierNr = '';
        this.supplierName = '';
        this.purchaseQty = 0;
        this.customerInvoiceQty = 0;
        this.customerInvoiceAmount = 0;
        this.marginalIncome = 0;
        this.marginalRatio = 0;
        this.vatType = 0;
        this.vatTypeName = '';
    }
}

export class ProductStatisticsRequest implements IProductStatisticsRequest {
    productIds: number[];
    fromDate: Date;
    toDate: Date;
    originType: SoeOriginType;
    includeServiceProducts: boolean;

    constructor() {
        this.productIds = [];
        this.originType = SoeOriginType.None;
        this.fromDate = new Date(
            new Date().getFullYear(),
            new Date().getMonth() - 1,
            1
        );
        this.toDate = new Date();
        this.includeServiceProducts = false;
    }
}