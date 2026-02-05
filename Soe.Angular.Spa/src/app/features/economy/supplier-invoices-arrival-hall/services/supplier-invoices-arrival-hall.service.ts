import { inject, Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { map, Observable } from 'rxjs';
import { SupplierInvoicesArrivalHallDTO } from '../models/supplier-invoices-arrival-hall.model';
import { getSupplierInvoiceArrivalHall } from '@shared/services/generated-service-endpoints/economy/SupplierInvoiceArrival.endpoints';
import { deleteInvoices } from '@shared/services/generated-service-endpoints/economy/SupplierInvoice.endpoints';
import { constructIdField } from '@features/economy/shared/supplier-invoice/models/supplier-invoice-form.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class SupplierInvoicesArrivalHallService {
  private readonly http = inject(SoeHttpClient);

  getGrid(): Observable<SupplierInvoicesArrivalHallDTO[]> {
    return this.http
      .get<SupplierInvoicesArrivalHallDTO[]>(getSupplierInvoiceArrivalHall())
      .pipe(
        map(rows => {
          return rows.map(r => {
            r.idField = constructIdField(r);
            return r;
          });
        })
      );
  }

  deleteDraftInvoices(invoiceIds: number[]): Observable<BackendResponse> {
    const invoiceIdsString = invoiceIds.join(',');
    return this.http.delete<BackendResponse>(deleteInvoices(invoiceIdsString));
  }
}
