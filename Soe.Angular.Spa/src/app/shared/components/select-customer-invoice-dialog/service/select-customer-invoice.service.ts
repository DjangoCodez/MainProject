import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { map, Observable } from 'rxjs';
import {
  ICustomerInvoiceSearchResultDTO,
  SearchCustomerInvoiceDTO,
} from '../model/customer-invoice-search.model';
import { getInvoicesBySearch } from '@shared/services/generated-service-endpoints/core/CustomerInvoices.endpoints';

@Injectable({
  providedIn: 'root',
})
export class SelectCustomerInvoiceService {
  constructor(private http: SoeHttpClient) {}

  getInvoicesBySearch(
    searchDto: SearchCustomerInvoiceDTO
  ): Observable<ICustomerInvoiceSearchResultDTO[]> {
    return this.http
      .post<ICustomerInvoiceSearchResultDTO[]>(getInvoicesBySearch(), searchDto)
      .pipe(
        map(rows =>
          rows.map(row => {
            row.balance = row.totalAmount - row.paidAmount;
            return row;
          })
        )
      );
  }
}
