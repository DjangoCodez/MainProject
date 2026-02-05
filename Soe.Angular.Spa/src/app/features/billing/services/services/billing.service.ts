import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, tap } from 'rxjs';
import { getProductUnitConverts } from '@shared/services/generated-service-endpoints/billing/ProductUnitConvert.endpoints';
import {
  ICompCurrencySmallDTO,
  IProductUnitConvertDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  getSupplierInvoiceProductPrice,
  getSupplierProductPrices,
} from '@shared/services/generated-service-endpoints/billing/SupplierProductPrice.endpoints';
import { getCompCurrenciesDictSmall } from '@shared/services/generated-service-endpoints/core/CoreCurrency.endpoints';
import { SupplierProductPriceDTO } from '../../purchase-products/models/purchase-product.model';
import { orderRowChangeAttestState } from '@shared/services/generated-service-endpoints/billing/OrderV2.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class BillingService {
  constructor(private http: SoeHttpClient) {}

  GetProductUnitConverts(
    productId: number,
    addEmptyRow: boolean
  ): Observable<IProductUnitConvertDTO[]> {
    return this.http.get<IProductUnitConvertDTO[]>(
      getProductUnitConverts(productId, addEmptyRow)
    );
  }

  //#region SupplierProductPriceController
  getSupplierProductPricesGrid(
    id: number
  ): Observable<SupplierProductPriceDTO[]> {
    return this.http
      .get<SupplierProductPriceDTO[]>(getSupplierProductPrices(id))
      .pipe(
        tap(value => {
          const startDate = new Date('1901-01-02');
          const stopDate = new Date('9998-12-31');

          value.map(row => {
            if (row.startDate && row.startDate < startDate)
              row.startDate = undefined!;
            if (row.endDate && row.endDate! > stopDate)
              row.endDate = undefined!;

            return row;
          });
        })
      );
  }

  getSupplierProductPrice(
    supplierProductId: number,
    currentDate: string,
    quantity: number,
    currencyId: number
  ): Observable<SupplierProductPriceDTO> {
    return this.http.get<SupplierProductPriceDTO>(
      getSupplierInvoiceProductPrice(
        supplierProductId,
        currentDate,
        quantity,
        currencyId
      )
    );
  }

  getProductPrice(
    productId: number,
    currentDate: string,
    quantity: number,
    currencyId: number
  ): Observable<SupplierProductPriceDTO> {
    return this.http.get<SupplierProductPriceDTO>(
      getInvoiceProductPrices(productId, currentDate, quantity, currencyId)
    );
  }

  //#endRegion

  //#region  OrderHandleBillingController
  updateOrderRowAttestState(data: any) {
    return this.http.post<BackendResponse>(orderRowChangeAttestState(), data);
  }
  //#endregion

  //#region CoreCurrencyController
  getCompCurrenciesDictSmall(): Observable<ICompCurrencySmallDTO[]> {
    return this.http.get<ICompCurrencySmallDTO[]>(getCompCurrenciesDictSmall());
  }
  //#endregion
}
function getInvoiceProductPrices(
  productId: number,
  currentDate: string,
  quantity: number,
  currencyId: number
): string {
  throw new Error('Function not implemented.');
}
