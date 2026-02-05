import { ISearchProductPricesModel } from '@shared/models/generated-interfaces/BillingModels';
import { SoeSysPriceListProviderType } from '@shared/models/generated-interfaces/Enumerations';
import { IInvoiceProductPriceSearchViewDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class InvoiceProductPriceSearchViewDTO
  implements IInvoiceProductPriceSearchViewDTO
{
  productId: number = 0;
  companyWholesellerPriceListId?: number;
  number: string = '';
  name: string = '';
  code: string = '';
  gnp: number = 0;
  nettoNettoPrice?: number;
  customerPrice?: number;
  priceStatus: number = 0;
  sysPriceListHeadId: number = 0;
  sysWholesellerId: number = 0;
  wholeseller: string = '';
  priceListType: string = '';
  priceListOrigin: number = 0;
  purchaseUnit: string = '';
  salesUnit: string = '';
  type: number = 0;
  productType: number = 0;
  productProviderType: SoeSysPriceListProviderType =
    SoeSysPriceListProviderType.Unknown;
  priceFormula: string = '';
  marginalIncome: number = 0;
  marginalIncomeRatio: number = 0.0;
  wholsellerNetPriceId: number = 0;
  // Extensions
  productProviderTypeText: string = '';
}

export class SearchProductPricesModel implements ISearchProductPricesModel {
  priceListTypeId: number;
  customerId: number;
  currencyId: number;
  number: string;
  providerType: SoeSysPriceListProviderType;

  constructor(
    priceListTypeId: number,
    customerId: number,
    currencyId: number,
    number: string,
    providerType: SoeSysPriceListProviderType
  ) {
    this.priceListTypeId = priceListTypeId;
    this.customerId = customerId;
    this.currencyId = currencyId;
    this.number = number;
    this.providerType = providerType;
  }
}
