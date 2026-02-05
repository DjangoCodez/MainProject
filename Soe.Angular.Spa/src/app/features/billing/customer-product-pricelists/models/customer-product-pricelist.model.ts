import { SoeEntityState } from '../../../../shared/models/generated-interfaces/Enumerations';
import { IProductSmallDTO } from '../../../../shared/models/generated-interfaces/ProductDTOs';
import { IInvoiceProductSmallDTO } from '../../../../shared/models/generated-interfaces/InvoiceProductDTOs';
import {
  IPriceListTypeDTO,
  IPriceListTypeGridDTO,
} from '../../../../shared/models/generated-interfaces/PriceListTypeDTOs';
import { PriceListDTO } from '@features/billing/models/pricelist.model';

export class PriceListTypeDTO implements IPriceListTypeDTO {
  priceListTypeId: number;
  currencyId: number;
  name: string;
  currency: string;
  description: string;
  discountPercent: number;
  inclusiveVat: boolean;
  isProjectPriceList: boolean;
  state: SoeEntityState;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  showHistoricPrices?: boolean;
  priceLists?: PriceListDTO[];

  constructor() {
    this.priceListTypeId = 0;
    this.currencyId = 0;
    this.name = '';
    this.currency = '';
    this.description = '';
    this.discountPercent = 0;
    this.inclusiveVat = false;
    this.isProjectPriceList = false;
    this.state = SoeEntityState.Active;
    this.showHistoricPrices = false;
    this.priceLists = [];
  }

  public fromPriceListTypeGridDTO(
    priceListType?: IPriceListTypeGridDTO
  ): PriceListTypeDTO {
    if (priceListType) {
      this.priceListTypeId = priceListType.priceListTypeId;
      this.currencyId = priceListType.currencyId;
      this.name = priceListType.name;
      this.currency = priceListType.currency;
      this.description = priceListType.description;
      this.discountPercent = 0;
      this.inclusiveVat = priceListType.inclusiveVat;
      this.isProjectPriceList = false;
      this.state = SoeEntityState.Active;
      this.showHistoricPrices = false;
      this.priceLists = [];
    }

    return this;
  }
}

export class PriceListTypeGridDTO implements IPriceListTypeGridDTO {
  priceListTypeId: number;
  currencyId: number;
  sysCurrencyId?: number;
  name: string;
  description: string;
  inclusiveVat: boolean;
  currency: string;
  isProjectPriceList: boolean;
  constructor() {
    this.priceListTypeId = 0;
    this.currencyId = 0;
    this.name = '';
    this.currency = '';
    this.description = '';
    this.inclusiveVat = false;
    this.isProjectPriceList = false;
  }
}

export class InvoiceProductSmallDTO
  implements IInvoiceProductSmallDTO, IProductSmallDTO
{
  productId: number;
  number: string;
  name: string;
  numberName: string;
  calculationType: number;
  productUnitId?: number;
  productGroupId?: number;
  guaranteePercentage?: number;
  useCalculatedCost?: boolean;
  purchasePrice: number;

  constructor() {
    this.productId = 0;
    this.number = '';
    this.name = '';
    this.numberName = '';
    this.calculationType = 0;
    this.purchasePrice = 0;
  }
}
