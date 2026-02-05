import { Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IInvoiceProductSearchViewDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISysProductGroupSmallDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getVVSProductGroupsForSearch,
  searchInvoiceProducts,
  searchInvoiceProductsExtended,
} from '@shared/services/generated-service-endpoints/billing/BillingProduct.endpoints';
import { getPriceLists } from '@shared/services/generated-service-endpoints/billing/InvoicePriceLists.endpoints';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SearchInvoiceProductDialogServiceService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IInvoiceProductSearchViewDTO[]> {
    return of([]);
  }

  searchInvoiceProduct(
    number: string,
    name: string
  ): Observable<IInvoiceProductSearchViewDTO[]> {
    return this.http.get<IInvoiceProductSearchViewDTO[]>(
      searchInvoiceProducts(number, name)
    );
  }

  searchInvoiceProductsExtended(
    searchText: string | undefined,
    group: string | undefined
  ): Observable<IInvoiceProductSearchViewDTO[]> {
    return this.http.get<IInvoiceProductSearchViewDTO[]>(
      searchInvoiceProductsExtended(
        'null',
        'null',
        group ?? 'null',
        searchText ?? 'null'
      )
    );
  }

  getPriceLists(addEmptyRow: boolean): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getPriceLists(addEmptyRow));
  }

  VVSGroupsForSearch(): Observable<ISysProductGroupSmallDTO[]> {
    return this.http.get<ISysProductGroupSmallDTO[]>(
      getVVSProductGroupsForSearch()
    );
  }
}
