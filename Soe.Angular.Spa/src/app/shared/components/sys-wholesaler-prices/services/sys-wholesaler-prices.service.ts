import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, of } from 'rxjs';
import {
  InvoiceProductPriceSearchViewDTO,
  SearchProductPricesModel,
} from '../models/sys-wholesaler-prices.models';
import { searchInvoiceProductPrices } from '@shared/services/generated-service-endpoints/billing/BillingProduct.endpoints';

@Injectable({
  providedIn: 'root',
})
export class SysWholesalerPricesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<InvoiceProductPriceSearchViewDTO[]> {
    return of([]);
  }

  searchInvoiceProductPrices(
    model: SearchProductPricesModel
  ): Observable<InvoiceProductPriceSearchViewDTO[]> {
    return this.http.post<InvoiceProductPriceSearchViewDTO[]>(
      searchInvoiceProductPrices(),
      model
    );
  }
}
