import {
  PriceListOrigin,
  SoeSysPriceListProviderType,
} from '@shared/models/generated-interfaces/Enumerations';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class SearchInvoiceProductDialogData implements DialogData {
  size?: DialogSize | undefined;
  title: string = '';
  content?: string | undefined;
  primaryText?: string | undefined;
  secondaryText?: string | undefined;
  disableClose?: boolean | undefined;
  disableContentScroll?: boolean | undefined;
  noToolbar?: boolean | undefined;
  hideFooter?: boolean | undefined;
  callbackAction?: (() => unknown) | undefined;

  //Extensions
  hideProducts: boolean = false;
  hidePrices?: boolean = false;
  priceListTypeId: number = 0;
  customerId: number = 0;
  currencyId: number = 0;
  sysWholesellerId?: number;
  number: string = '';
  name: string = '';
  quantity: number = 0;
  info: string = '';

  constructor() {}
}

export class ProductSearchResult {
  productId: number = 0;
  priceListTypeId: number = 0;
  purchasePrice: number = 0.0;
  salesPrice: number = 0.0;
  productName: string = '';
  productUnit: string = '';
  sysPriceListHeadId: number = 0;
  sysWholesalerName: string = '';
  sysWholesalerId?: number = 0;
  priceListOrigin: PriceListOrigin = PriceListOrigin.Unknown;
  quantity: number = 0;
}

export class ProductPriceSearchResult {
  productId: number = 0;
  productNr: string = '';
  productName: string = '';
  productInfo: string = '';
  quantity: number = 0;
  imageUrl: string = '';
  externalId?: number;
  type!: SoeSysPriceListProviderType;
}
