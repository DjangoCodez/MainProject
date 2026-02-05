import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  IProductStatisticsModel,
  IProductsSimpleModel,
  ISaveInvoiceProductModel,
} from '@shared/models/generated-interfaces/BillingModels';
import {
  SoeTimeCodeType,
  TermGroup_ChangeStatusGridAllItemsSelection,
} from '@shared/models/generated-interfaces/Enumerations';
import { IInvoiceProductCopyResult } from '@shared/models/generated-interfaces/InvoiceProductCopyResult';
import {
  IPriceListTypeDTO,
  IPriceListTypeGridDTO,
} from '@shared/models/generated-interfaces/PriceListTypeDTOs';
import {
  IProductCleanupDTO,
  IProductSmallDTO,
} from '@shared/models/generated-interfaces/ProductDTOs';
import { IProductStatisticsDTO } from '@shared/models/generated-interfaces/ProductStatisticsDTO';
import { IProductStatisticsRequest } from '@shared/models/generated-interfaces/ProductStatisticsRequest';
import {
  IProductGroupDTO,
  ITimeCodeDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  copyExternalInvoiceProduct,
  deleteProduct,
  deleteProducts,
  getCustomerInvoiceProductStatistics,
  getHouseholdDeductionTypes,
  getProduct,
  getProductExternalUrls,
  getProductForSelect,
  getProductRowsProduct,
  getProductRowsProducts,
  getProductsDict,
  getProductsForCleanup,
  getProductsGrid,
  getProductsSmall,
  inactivateProducts,
  saveInvoiceProduct,
  updateProductState,
} from '@shared/services/generated-service-endpoints/billing/BillingProduct.endpoints';
import { getCustomerCommodyCodesDict } from '@shared/services/generated-service-endpoints/billing/CommodityCodes.endpoints';
import {
  getPriceListTypes,
  getPriceListTypesGrid,
} from '@shared/services/generated-service-endpoints/billing/PriceList.endpoints';
import { getProductGroups } from '@shared/services/generated-service-endpoints/billing/ProductGroup.endpoints';
import { getProductStatistics } from '@shared/services/generated-service-endpoints/billing/ProductStatistics.endpoints';
import { getProductUnitsDict } from '@shared/services/generated-service-endpoints/billing/ProductUnit.endpoints';
import { getTimeCodes } from '@shared/services/generated-service-endpoints/economy/SupplierInvoice.endpoints';
import { Dict } from '@ui/grid/services/selected-item.service';
import { Observable, forkJoin, map } from 'rxjs';
import { getVatCodes } from '../../../../shared/services/generated-service-endpoints/economy/VatCode.endpoints';
import { ProductStatisticsDTO } from '../../product-statistics/models/product-statistics.model';
import { ProductTypeheadDTO } from '../../purchase-products/models/purchase-product.model';
import {
  InvoiceProductDTO,
  InvoiceProductExtendedGridDTO,
} from '../models/invoice-product.model';
import {
  CopyInvoiceProductModel,
  IProductBasicInfo,
  ProductUnitConvertDTO,
} from '../models/product.model';
import { VatCodeDTO } from '@features/economy/models/vat-code.model';
import { getStocksForInvoiceProduct } from '@shared/services/generated-service-endpoints/billing/StockV2.endpoints';
import { StockDTO } from '../models/stock.model';
import { CustomerStatisticsDTO } from '@features/billing/sales-statistics/models/sales-statistics.model';
import {
  getProductUnitConverts,
  saveProductUnitConvert,
} from '@shared/services/generated-service-endpoints/billing/ProductUnitConvert.endpoints';
import { ProductRowsProductDTO } from '@features/billing/purchase/models/purchase-rows.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ProductCleanupDTO } from '../models/products-cleanup-dialog.model';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  public parentProductContext?: IProductBasicInfo;

  constructor(
    private readonly http: SoeHttpClient,
    private readonly translate: TranslateService
  ) {}

  getGrid(id?: number): Observable<InvoiceProductExtendedGridDTO[]> {
    return forkJoin({
      terms: this.translate.get(['core.yes', 'core.no']),
      products: this.http.get<InvoiceProductExtendedGridDTO[]>(
        getProductsGrid(false, true, false, true, true, id)
      ),
    }).pipe(
      map(({ terms, products }): InvoiceProductExtendedGridDTO[] => {
        products.forEach(p => {
          p.isExternal = p.external ? terms['core.yes'] : terms['core.no'];
          p.isExternalId = p.external ? 1 : 0;
          p.productCategoriesArray = p.productCategories
            .split(',')
            .flatMap(c => {
              const trimmed = c.trim();
              return trimmed ? [trimmed] : [];
            });
        });
        return products;
      })
    );
  }

  get(id: number): Observable<InvoiceProductDTO> {
    return this.http
      .get<InvoiceProductDTO>(getProduct(id))
      .pipe(map(p => this.getObject(InvoiceProductDTO, p)));
  }

  getProducts(): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getProductsDict());
  }

  getProductsSmall(): Observable<IProductSmallDTO[]> {
    return this.http.get<IProductSmallDTO[]>(getProductsSmall());
  }

  getProductForProductRows(
    productId: number
  ): Observable<ProductRowsProductDTO> {
    return this.http
      .get<ProductRowsProductDTO>(getProductRowsProduct(productId))
      .pipe(map(p => this.getObject(ProductRowsProductDTO, p)));
  }

  getProductRowsProducts(
    productIds: number[]
  ): Observable<ProductRowsProductDTO[]> {
    return this.http.post<ProductRowsProductDTO[]>(
      getProductRowsProducts(),
      productIds
    );
  }

  getProductForSelect(): Observable<ProductTypeheadDTO[]> {
    return this.http.get<ProductTypeheadDTO[]>(getProductForSelect());
  }

  getProductStatistics(
    model: IProductStatisticsRequest,
    vatTypes: SmallGenericType[]
  ): Observable<ProductStatisticsDTO[]> {
    return this.http
      .post<IProductStatisticsDTO[]>(getProductStatistics(), model)
      .pipe(
        map(records =>
          records.map(x => {
            const result: ProductStatisticsDTO = {
              ...x,
              vatTypeName: vatTypes.find(t => t.id === x.vatType)?.name || '',
            };
            return result;
          })
        )
      );
  }

  copyInvoiceProduct(
    model: CopyInvoiceProductModel
  ): Observable<IInvoiceProductCopyResult> {
    return this.http.post(copyExternalInvoiceProduct(), model);
  }

  updateProductState(selectedItems: Dict): Observable<BackendResponse> {
    return this.http.post(updateProductState(), selectedItems);
  }

  getProductExternalUrls(productIds: number[]): Observable<string[]> {
    return this.http.post(getProductExternalUrls(), <IProductsSimpleModel>{
      productIds: productIds,
    });
  }

  save(
    product: InvoiceProductDTO,
    additionalData?: any
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveInvoiceProduct(), <
      ISaveInvoiceProductModel
    >{
      invoiceProduct: product,
      priceLists: additionalData.priceLists,
      categoryRecords: additionalData.categoryRecords,
      stocks: additionalData.stocks,
      translations: additionalData.translations,
      extrafields: additionalData.extraFields,
    });
  }

  saveProductUnitConvert(
    rows: ProductUnitConvertDTO[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveProductUnitConvert(), rows);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteProduct(id));
  }

  deleteProducts(productIds: number[]): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(deleteProducts(), productIds);
  }

  getProductUnitConverts(
    productId: number,
    addEmptyRow: boolean
  ): Observable<ProductUnitConvertDTO[]> {
    return this.http
      .get<
        ProductUnitConvertDTO[]
      >(getProductUnitConverts(productId, addEmptyRow))
      .pipe(
        map((rows: ProductUnitConvertDTO[]) => {
          rows = rows.map(row => {
            row = this.getObject(ProductUnitConvertDTO, row);
            return row;
          });
          return rows;
        })
      );
  }

  getProductInvoiceStatistics(
    productId: number,
    originType: number,
    allItemSelection: TermGroup_ChangeStatusGridAllItemsSelection
  ): Observable<CustomerStatisticsDTO[]> {
    return this.http
      .post<CustomerStatisticsDTO[]>(getCustomerInvoiceProductStatistics(), <
        IProductStatisticsModel
      >{
        productId,
        originType,
        allItemSelection,
      })
      .pipe(
        map((rows: CustomerStatisticsDTO[]) => {
          rows = rows.map(row => {
            row = this.getObject(CustomerStatisticsDTO, row);
            row.fixDates(true);
            return row;
          });
          return rows;
        })
      );
  }

  getVatCodes(): Observable<VatCodeDTO[]> {
    return this.http.get<VatCodeDTO[]>(getVatCodes());
  }

  getProductUnitsDict(): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getProductUnitsDict());
  }

  getMaterialCodes(
    timeCodeType: SoeTimeCodeType,
    active: boolean,
    loadPayrollProducts: boolean
  ): Observable<ITimeCodeDTO[]> {
    return this.http.get<ITimeCodeDTO[]>(
      getTimeCodes(+timeCodeType, active, loadPayrollProducts)
    );
  }

  getProductGroups(): Observable<IProductGroupDTO[]> {
    return this.http.get<IProductGroupDTO[]>(getProductGroups());
  }

  getHouseholdDeductionTypes(
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getHouseholdDeductionTypes(addEmptyRow)
    );
  }

  getCustomerCommodityCodesDict(
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getCustomerCommodyCodesDict(addEmptyRow)
    );
  }

  getPriceListTypesGrid(): Observable<IPriceListTypeGridDTO[]> {
    return this.http.get<IPriceListTypeGridDTO[]>(
      getPriceListTypesGrid(undefined)
    );
  }

  getPriceListTypes(): Observable<IPriceListTypeDTO[]> {
    return this.http.get<IPriceListTypeDTO[]>(getPriceListTypes());
  }

  getStocksByProduct(productId: number): Observable<StockDTO[]> {
    return this.http.get<StockDTO[]>(getStocksForInvoiceProduct(productId));
  }

  private getObject<T extends object>(TType: { new (): T }, data: unknown): T {
    const obj = new TType();
    Object.assign<T, unknown>(obj, data);
    return obj;
  }

  getProductsForCleanup(lastUsedDate: string): Observable<ProductCleanupDTO[]> {
    return this.http
      .get<ProductCleanupDTO[]>(getProductsForCleanup(lastUsedDate))
      .pipe(
        map((rows: ProductCleanupDTO[]) => {
          rows = rows.map(row => {
            row = this.getObject(ProductCleanupDTO, row);
            row.externalStatus = row.isExternal ? 'Yes' : 'No';
            return row;
          });
          return rows;
        })
      );
  }

  inactivateProducts(productIds: number[]): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(inactivateProducts(), productIds);
  }
}
