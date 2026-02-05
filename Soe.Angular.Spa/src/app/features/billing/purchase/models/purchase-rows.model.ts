import { SmallGenericType } from '@shared/models/generic-type.model';
import { IProductsSimpleModel } from '@shared/models/generated-interfaces/BillingModels';
import {
  SoeEntityState,
  PurchaseRowType,
  TermGroup_InvoiceProductCalculationType,
  TermGroup_InvoiceProductVatType,
  TermGroup_GrossMarginCalculationType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IProductRowsProductDTO } from '@shared/models/generated-interfaces/InvoiceProductDTOs';
import { IPurchaseRowDTO } from '@shared/models/generated-interfaces/PurchaseDTOs';

export class PurchaseRows {}

export class PurchaseRowDTO implements IPurchaseRowDTO {
  tempRowId: number;
  purchaseRowId: number;
  purchaseId: number;
  purchaseNr: string;
  productId?: number;
  productName: string;
  productNr: string;
  stockId?: number;
  stockCode: string;
  rowNr: number;
  purchaseUnitId?: number;
  parentRowId?: number;
  quantity: number;
  deliveredQuantity?: number;
  wantedDeliveryDate?: Date;
  accDeliveryDate?: Date;
  deliveryDate?: Date;
  text: string;
  purchasePrice: number;
  purchasePriceCurrency: number;
  discountType: number;
  discountAmount: number;
  discountAmountCurrency: number;
  discountPercent: number;
  vatAmount: number;
  vatAmountCurrency: number;
  vatRate: number;
  vatCodeId?: number;
  vatCodeName: string;
  vatCodeCode: string;
  sumAmount: number;
  sumAmountCurrency: number;
  orderId?: number;
  orderNr: string;
  status: number;
  statusName: string;
  isLocked: boolean;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;
  type: PurchaseRowType;
  supplierProductId?: number;
  supplierProductNr: string;
  intrastatCodeId?: number;
  sysCountryId?: number;
  intrastatTransactionId?: number;
  customerInvoiceRowIds: number[];

  // Extensions
  isModified: boolean;
  statusIcon: string;
  purchaseProductUnitCode: string;
  discountTypeText: string;
  discountValue: number;
  stocksForProduct: SmallGenericType[];

  constructor() {
    this.tempRowId = 0;
    this.purchaseRowId = 0;
    this.purchaseId = 0;
    this.purchaseNr = '';
    this.productName = '';
    this.productNr = '';
    this.stockCode = '';
    this.rowNr = 0;
    this.purchaseUnitId = 0;
    this.quantity = 0;
    this.text = '';
    this.purchasePrice = 0;
    this.purchasePriceCurrency = 0;
    this.discountType = 0;
    this.discountAmount = 0;
    this.discountAmountCurrency = 0;
    this.discountPercent = 0;
    this.vatAmount = 0;
    this.vatAmountCurrency = 0;
    this.vatRate = 0;
    this.vatCodeName = '';
    this.vatCodeCode = '';
    this.sumAmount = 0;
    this.sumAmountCurrency = 0;
    this.orderNr = '';
    this.status = 0;
    this.statusName = '';
    this.isLocked = false;
    this.state = SoeEntityState.Active;
    this.type = PurchaseRowType.TextRow;
    this.supplierProductNr = '';
    this.customerInvoiceRowIds = [];
    this.modifiedBy = '';
    this.isModified = false;
    this.statusIcon = '';
    this.purchaseProductUnitCode = '';
    this.discountTypeText = '';
    this.discountValue = 0;
    this.stocksForProduct = [];
  }
  public static getPropertiesToSkipOnSave(): string[] {
    return [
      'created',
      'createdBy',
      'modified',
      'modifiedBy',
      'purchaseProductUnitCode',
      'productNr',
      'isModified',
      'stocksForProduct',
      'statusName',
    ];
  }
}

export class ProductsSimpleModel implements IProductsSimpleModel {
  productIds: number[];

  constructor() {
    this.productIds = [];
  }
}

export class PurchaseRowSummeryFormDTO {
  centRounding?: string = '';
  totalAmountExVatCurrency?: string = '';
  baseCurrencyCode?: string = '';
}

export class ProductRowsProductDTO implements IProductRowsProductDTO {
  productId: number;
  sysProductId?: number;
  number: string;
  name: string;
  description: string;
  showDescriptionAsTextRow: boolean;
  showDescrAsTextRowOnPurchase: boolean;
  productUnitId?: number;
  productUnitCode: string;
  vatType: TermGroup_InvoiceProductVatType;
  calculationType: TermGroup_InvoiceProductCalculationType;
  guaranteePercentage?: number;
  sysWholesellerName: string;
  dontUseDiscountPercent: boolean;
  fixedPrice?: number;
  purchasePrice: number;
  salesPrice: number;
  vatCodeId?: number;
  isStockProduct: boolean;
  isSupplementCharge: boolean;
  householdDeductionType?: number;
  householdDeductionPercentage?: number;
  isLiftProduct: boolean;
  isInactive: boolean;
  isExternal: boolean;
  intrastatCodeId?: number;
  sysCountryId?: number;
  grossMarginCalculationType!: TermGroup_GrossMarginCalculationType;

  constructor() {
    this.productId = 0;
    this.number = '';
    this.name = '';
    this.description = '';
    this.showDescriptionAsTextRow = false;
    this.showDescrAsTextRowOnPurchase = false;
    this.productUnitCode = '';
    this.vatType = TermGroup_InvoiceProductVatType.Merchandise;
    this.calculationType = TermGroup_InvoiceProductCalculationType.Clearing;
    this.sysWholesellerName = '';
    this.dontUseDiscountPercent = false;
    this.purchasePrice = 0;
    this.salesPrice = 0;
    this.isStockProduct = false;
    this.isSupplementCharge = false;
    this.isLiftProduct = false;
    this.isInactive = false;
    this.isExternal = false;
  }
}
