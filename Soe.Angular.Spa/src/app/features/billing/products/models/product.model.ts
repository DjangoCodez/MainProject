import { ICopyInvoiceProductModel } from '@shared/models/generated-interfaces/BillingModels';
import {
  SoeProductType,
  SoeEntityState,
  PriceListOrigin,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IProductComparisonDTO,
  IProductDTO,
  IProductPriceListDTO,
  IProductSmallDTO,
} from '@shared/models/generated-interfaces/ProductDTOs';
import { IProductUnitConvertDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class ProductSmallDTO implements IProductSmallDTO {
  productId: number;
  number: string;
  name: string;
  numberName: string;

  constructor() {
    this.productId = 0;
    this.number = '';
    this.name = '';
    this.numberName = '';
  }
}

export class ProductPriceListDTO
  extends ProductSmallDTO
  implements IProductPriceListDTO
{
  priceListId: number;
  purchasePrice: number;
  price: number;
  startDate: Date;
  stopDate: Date;

  constructor() {
    super();
    this.priceListId = 0;
    this.purchasePrice = 0.0;
    this.price = 0.0;
    this.startDate = new Date('1901-01-02');
    this.stopDate = new Date('9998-12-31');
  }
}

export class ProductComparisonDTO
  extends ProductSmallDTO
  implements IProductComparisonDTO
{
  purchasePrice: number;
  comparisonPrice: number;
  price: number;
  startDate: Date;
  stopDate: Date;

  constructor() {
    super();
    this.comparisonPrice = 0.0;
    this.purchasePrice = 0.0;
    this.price = 0.0;
    this.startDate = new Date('1901-01-02');
    this.stopDate = new Date('9998-12-31');
  }
}

export class ProductDTO extends ProductSmallDTO implements IProductDTO {
  productUnitId?: number;
  productGroupId?: number;
  type: SoeProductType;
  description: string;
  accountingPrio: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state: SoeEntityState;
  productUnitCode: string;

  constructor() {
    super();
    this.type = SoeProductType.Unknown;
    this.description = '';
    this.accountingPrio = '';
    this.state = SoeEntityState.Active;
    this.productUnitCode = '';
  }
}

export class CopyInvoiceProductModel implements ICopyInvoiceProductModel {
  productId!: number;
  purchasePrice!: number;
  salesPrice!: number;
  productUnit!: string;
  priceListTypeId!: number;
  priceListHeadId!: number;
  sysWholesellerName!: string;
  customerId!: number;
  origin!: PriceListOrigin;

  constructor(
    productId: number,
    purchasePrice: number,
    salesPrice: number,
    productUnit: string,
    priceListTypeId: number,
    priceListHeadId: number,
    sysWholesellerName: string,
    customerId: number,
    origin: PriceListOrigin
  ) {
    this.productId = productId;
    this.purchasePrice = purchasePrice;
    this.salesPrice = salesPrice;
    this.productUnit = productUnit;
    this.priceListTypeId = priceListTypeId;
    this.priceListHeadId = priceListHeadId;
    this.sysWholesellerName = sysWholesellerName;
    this.customerId = customerId;
    this.origin = origin;
  }
}

export class PriorityAccountRow {
  dimNr: number;
  dimName: string;
  prioNr: number;
  prioName: string;

  constructor(
    dimNr: number,
    dimName: string,
    prioNr: number,
    prioName: string
  ) {
    this.dimNr = dimNr;
    this.dimName = dimName;
    this.prioNr = prioNr;
    this.prioName = prioName;
  }
}

export class ProductUnitConvertDTO implements IProductUnitConvertDTO {
  productUnitConvertId: number;
  productId!: number;
  productNr: string;
  productName: string;
  productUnitId!: number;
  productUnitName: string;
  convertFactor: number;
  baseProductUnitId?: number;
  baseProductUnitName: string;
  isModified: boolean;
  isDeleted: boolean;

  constructor() {
    this.productUnitConvertId = 0;
    this.productNr = '';
    this.productName = '';
    this.productUnitName = '';
    this.convertFactor = 0.0;
    this.baseProductUnitName = '';
    this.isModified = false;
    this.isDeleted = false;
  }
}

export interface IProductBasicInfo {
  productId: number;
  number: string;
  name: string;
  productUnitId: number;
}
