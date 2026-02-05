import {
  IInvoiceProductDTO,
  IInvoiceProductGridDTO,
} from '@shared/models/generated-interfaces/InvoiceProductDTOs';
import { ProductDTO } from './product.model';
import {
  TermGroup_InvoiceProductVatType,
  TermGroup_InvoiceProductCalculationType,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { AccountingSettingsRowDTO } from '@shared/components/accounting-settings/accounting-settings/accounting-settings.models';
import { StockDTO } from './stock.model';
import { CompTermDTO } from '@shared/features/language-translations/models/language-translations.model';
import { IExtraFieldRecordDTO } from '@shared/models/generated-interfaces/ExtraFieldDTO';
import { PriceListDTO } from '@features/billing/models/pricelist.model';

export class InvoiceProductDTO
  extends ProductDTO
  implements IInvoiceProductDTO
{
  sysProductId?: number;
  sysPriceListHeadId?: number;
  vatType: TermGroup_InvoiceProductVatType;
  vatFree: boolean;
  ean: string;
  purchasePrice: number;
  sysWholesellerName: string;
  calculationType: TermGroup_InvoiceProductCalculationType;
  guaranteePercentage?: number;
  timeCodeId?: number;
  priceListOrigin: number;
  showDescriptionAsTextRow: boolean;
  showDescrAsTextRowOnPurchase: boolean;
  dontUseDiscountPercent: boolean;
  useCalculatedCost: boolean;
  vatCodeId?: number;
  householdDeductionType?: number;
  householdDeductionPercentage?: number;
  isStockProduct: boolean;
  weight?: number;
  intrastatCodeId?: number;
  sysCountryId?: number;
  defaultGrossMarginCalculationType?: number;
  isExternal?: boolean;
  salesPrice: number;
  isSupplementCharge: boolean;
  priceLists: PriceListDTO[];
  categoryIds: number[];
  accountingSettings: AccountingSettingsRowDTO[];
  sysProductType?: number;

  //Extensions
  stocks: StockDTO[] = [];
  translations: CompTermDTO[] = [];
  extraFields: IExtraFieldRecordDTO[] = [];

  get active(): boolean {
    return this.state === SoeEntityState.Active;
  }

  constructor() {
    super();
    this.vatType = TermGroup_InvoiceProductVatType.None;
    this.vatFree = false;
    this.ean = '';
    this.purchasePrice = 0.0;
    this.sysWholesellerName = '';
    this.calculationType = TermGroup_InvoiceProductCalculationType.Regular;
    this.priceListOrigin = 0;
    this.showDescriptionAsTextRow = false;
    this.showDescrAsTextRowOnPurchase = false;
    this.dontUseDiscountPercent = false;
    this.useCalculatedCost = false;
    this.isStockProduct = false;
    this.salesPrice = 0.0;
    this.isSupplementCharge = false;
    this.priceLists = [];
    this.categoryIds = [];
    this.accountingSettings = [];
  }
}

export interface InvoiceProductGridDTO extends IInvoiceProductGridDTO {
  isExternalId: number;
  isExternal: string;
}

export type InvoiceProductExtendedGridDTO = InvoiceProductGridDTO & {
  productCategoriesArray?: string[];
};
