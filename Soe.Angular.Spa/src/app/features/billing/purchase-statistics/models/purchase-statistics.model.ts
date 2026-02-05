import { IPurchaseStatisticsDTO } from '@shared/models/generated-interfaces/PurchaseStatisticsDTO';

export class PurchaseStatisticsDTO implements IPurchaseStatisticsDTO {
  supplierNr!: string;
  supplierName!: string;
  supplierNumberName!: string;
  purchaseNr!: string;
  purchaseDate?: Date;
  status!: number;
  sysCurrencyId!: number;
  productUnitCode!: string;
  statusName!: string;
  currencyCode!: string;
  code!: string;
  projectNumber!: string;
  productNumber!: string;
  productName!: string;
  supplierItemNumber!: string;
  supplierItemName!: string;
  supplierItemCode!: string;
  stockPlace!: string;
  customerOrderNumber!: string;
  quantity!: number;
  deliveredQuantity?: number;
  purchasePrice!: number;
  purchasePriceCurrency!: number;
  discountAmount!: number;
  discountAmountCurrency!: number;
  sumAmount!: number;
  sumAmountCurrency!: number;
  wantedDeliveryDate?: Date;
  acknowledgeDeliveryDate?: Date;
  deliveryDate?: Date;
  rowStatus!: number;
  rowStatusName!: string;
  unit!: string;
}
export class PurchaseStatisticsFilterDTO {
  fromDate: Date;
  toDate: Date;

  constructor() {
    this.fromDate = new Date(
      new Date().getFullYear(),
      new Date().getMonth() - 1,
      1
    );
    this.toDate = new Date();
  }
}
