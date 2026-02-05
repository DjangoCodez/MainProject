import { TermGroup_StockPurchaseGenerationOptions } from '@shared/models/generated-interfaces/Enumerations';
import { IPurchaseRowFromStockDTO } from '@shared/models/generated-interfaces/PurchaseDTOs';
import { IGenerateStockPurchaseSuggestionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class StockPurchaseDTO implements IPurchaseRowFromStockDTO {
  //grid & save obj
  tempId: number;
  productId: number;
  productNr: string;
  productName: string;
  productUnitCode: string;
  stockId: number;
  stockName: string;
  stockPurchaseTriggerQuantity: number;
  stockPurchaseQuantity: number;
  totalStockQuantity: number;
  reservedStockQauntity: number;
  availableStockQuantity: number;
  purchasedQuantity: number;
  supplierId: number;
  supplierName: string;
  supplierNr: string;
  supplierProductId: number;
  supplierUnitId: number;
  supplierUnitCode: string;
  multipleSupplierMatches: boolean;
  unitId: number;
  unitCode: string;
  packSize?: number;
  deliveryLeadTimeDays: number;
  quantity: number;
  price: number;
  sum: number;
  currencyId: number;
  currencyCode: string;
  discountPercentage: number;
  requestedDeliveryDate: Date;
  deliveryAddress: string;
  exclusivePurchase: boolean;
  referenceOur: string;
  vatCodeId: number;
  vatAmount: number;
  vatRate: number;
  purchaseId: number;
  purchaseNr: string;

  constructor() {
    this.tempId = 0;
    this.productId = 0;
    this.productNr = '';
    this.productName = '';
    this.productUnitCode = '';
    this.stockId = 0;
    this.stockName = '';
    this.stockPurchaseTriggerQuantity = 0;
    this.stockPurchaseQuantity = 0;
    this.totalStockQuantity = 0;
    this.reservedStockQauntity = 0;
    this.availableStockQuantity = 0;
    this.purchasedQuantity = 0;
    this.supplierId = 0;
    this.supplierName = '';
    this.supplierNr = '';
    this.supplierProductId = 0;
    this.supplierUnitId = 0;
    this.supplierUnitCode = '';
    this.multipleSupplierMatches = false;
    this.unitId = 0;
    this.unitCode = '';
    this.deliveryLeadTimeDays = 0;
    this.quantity = 0;
    this.price = 0;
    this.sum = 0;
    this.currencyId = 0;
    this.currencyCode = '';
    this.discountPercentage = 0;
    this.requestedDeliveryDate = new Date();
    this.deliveryAddress = '';
    this.exclusivePurchase = false;
    this.referenceOur = '';
    this.vatCodeId = 0;
    this.vatAmount = 0;
    this.vatRate = 0;
    this.purchaseId = 0;
    this.purchaseNr = '';
  }
}

export class StockPurchaseFilterDTO
  implements IGenerateStockPurchaseSuggestionDTO
{
  // filter form
  purchaseGenerationType!: TermGroup_StockPurchaseGenerationOptions;
  productNrFrom: string;
  productNrTo: string;
  triggerQuantityPercent: number;
  excludeMissingTriggerQuantity: boolean;
  excludeMissingPurchaseQuantity: boolean;
  excludePurchaseQuantityZero: boolean;
  stockPlaceIds: number[];
  defaultDeliveryAddress: string;
  purchaser: string;

  constructor() {
    this.productNrFrom = '';
    this.productNrTo = '';
    this.triggerQuantityPercent = 0;
    this.excludeMissingTriggerQuantity = false;
    this.excludeMissingPurchaseQuantity = false;
    this.excludePurchaseQuantityZero = false;
    this.stockPlaceIds = [];
    this.defaultDeliveryAddress = '';
    this.purchaser = '';
  }
}
