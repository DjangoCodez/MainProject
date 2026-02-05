import {
  SoeEntityState,
  SoeProductType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IProductDTO,
  IProductSmallDTO,
} from '@shared/models/generated-interfaces/ProductDTOs';
import {
  ISupplierProductSearchDTO,
  ISupplierProductDTO,
  ISupplierProductPriceDTO,
  ISupplierProductSmallDTO,
} from '@shared/models/generated-interfaces/SupplierProductDTOs';

export class SupplierProductGridHeaderDTO implements ISupplierProductSearchDTO {
  supplierIds: number[];
  supplierProduct: string;
  supplierProductName: string;
  product: string;
  productName: string;
  invoiceProductId!: number;

  constructor() {
    this.supplierIds = [];
    this.supplierProduct = '';
    this.supplierProductName = '';
    this.product = '';
    this.productName = '';
  }
}

export class SupplierProductPriceDTO implements ISupplierProductPriceDTO {
  supplierProductPriceId: number;
  supplierProductPriceListId?: number;
  supplierProductId: number;
  quantity: number;
  price: number;
  sysCurrencyId!: number;
  currencyId!: number;
  currencyCode: string;
  startDate?: Date;
  endDate?: Date;
  state!: SoeEntityState;

  //Extension
  isModified!: boolean;
  entityState: SoeEntityState;

  constructor() {
    this.supplierProductPriceId = 0;
    this.supplierProductId = 0;
    this.quantity = 0;
    this.price = 0;
    this.currencyCode = '';
    this.startDate = new Date();
    this.endDate = new Date();
    this.state = SoeEntityState.Active;
    this.entityState = SoeEntityState.Active;
  }
}

export class SupplierProductDTO implements ISupplierProductDTO {
  supplierProductId: number;
  supplierId: number;
  supplierProductUnitId: number;
  supplierProductNr: string;
  supplierProductName: string;
  supplierProductCode: string;
  productId: number;
  sysCountryId?: number;
  packSize?: number;
  deliveryLeadTimeDays?: number;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  intrastatCodeId?: number;
  priceRows: SupplierProductPriceDTO[];

  //Form Helper Properties
  itemName!: string;
  itemUnit!: string;

  constructor() {
    this.supplierProductId = 0;
    this.supplierId = 0;
    this.supplierProductUnitId = 0;
    this.supplierProductNr = '';
    this.supplierProductName = '';
    this.supplierProductCode = '';
    this.productId = 0;
    this.createdBy = '';
    this.modifiedBy = '';
    this.priceRows = [];
  }
}

export class ProductTypeheadDTO implements IProductSmallDTO, IProductDTO {
  id: number;
  productId: number;
  number: string;
  name: string;
  numberName: string;
  productUnitId?: number;
  productGroupId?: number;
  type!: SoeProductType;
  description: string;
  accountingPrio: string;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state!: SoeEntityState;
  productUnitCode: string;

  constructor() {
    this.id = 0;
    this.productId = 0;
    this.number = '';
    this.name = '';
    this.numberName = '';
    this.description = '';
    this.accountingPrio = '';
    this.createdBy = '';
    this.modifiedBy = '';
    this.productUnitCode = '';
  }
}

export class SupplierProductSmallDTO implements ISupplierProductSmallDTO {
  supplierProductId: number;
  number: string;
  name: string;
  numberName: string;

  constructor() {
    this.supplierProductId = 0;
    this.number = '';
    this.name = '';
    this.numberName = '';
  }
}
