import {
  ISupplierProductPriceComparisonDTO,
  ISupplierProductPriceDTO,
  ISupplierProductPriceListSaveDTO,
  ISupplierProductPriceSearchDTO,
  ISupplierProductPricelistDTO,
} from '@shared/models/generated-interfaces/SupplierProductDTOs';
import { SupplierProductPriceDTO } from '../../purchase-products/models/purchase-product.model';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';

export class SupplierProductPricelistDTO
  implements ISupplierProductPricelistDTO
{
  supplierProductPriceListId: number;
  startDate: Date;
  endDate?: Date;
  supplierId!: number;
  supplierNr: string;
  supplierName: string;
  sysWholeSellerId?: number;
  sysWholeSellerName: string;
  sysWholeSellerType!: number;
  sysWholeSellerTypeName: string;
  created!: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  sysCurrencyId!: number;
  currencyId!: number;
  currencyCode: string;

  // Extends
  priceRows!: SupplierProductPriceComparisonDTO[];

  constructor() {
    this.supplierProductPriceListId = 0;
    this.startDate = new Date();
    this.supplierNr = '';
    this.supplierName = '';
    this.sysWholeSellerName = '';
    this.sysWholeSellerTypeName = '';
    this.createdBy = '';
    this.modifiedBy = '';
    this.currencyCode = '';
  }
}

export class SupplierProductPriceListGridHeaderDTO
  implements ISupplierProductPriceSearchDTO
{
  supplierId: number;
  currencyId!: number;
  compareDate!: Date;
  includePricelessProducts!: boolean;

  constructor() {
    this.supplierId = 0;
  }
}

export class SupplierProductPriceComparisonDTO
  extends SupplierProductPriceDTO
  implements ISupplierProductPriceComparisonDTO
{
  compareSupplierProductPriceId: number;
  comparePrice: number;
  compareQuantity: number;
  compareEndDate?: Date;
  compareStartDate?: Date;
  ourProductName: string;
  productName: string;
  productNr: string;

  state!: SoeEntityState;

  constructor() {
    super();
    this.compareSupplierProductPriceId = 0;
    this.comparePrice = 0;
    this.compareQuantity = 0;
    this.compareEndDate = undefined;
    this.compareStartDate = undefined;
    this.ourProductName = '';
    this.productName = '';
    this.productNr = '';
  }
}

export class SupplierProductPriceListSaveDTO
  implements ISupplierProductPriceListSaveDTO
{
  priceList!: ISupplierProductPricelistDTO;
  priceRows: ISupplierProductPriceDTO[] = [];
}
