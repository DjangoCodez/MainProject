import { inject, Injectable } from '@angular/core';
import { Observable, of as observableOf } from 'rxjs';
import {
  SupplierInvoiceHistoryDetailsDTO,
  SupplierInvoiceHistoryGridDTO,
} from '../models/supplier-invoice-history.model';
import { SoeHttpClient } from '@shared/services/http.service';
import { IApiServiceGrid } from '@shared/directives/grid-base/grid-base.directive';

import { getSupplierInvoice20Latest } from '@shared/services/generated-service-endpoints/economy/SupplierInvoiceArrival.endpoints';

@Injectable({
  providedIn: 'root',
})
export class SupplierInvoiceHistoryService
  implements IApiServiceGrid<SupplierInvoiceHistoryGridDTO>
{
  private readonly http = inject(SoeHttpClient);

  getGrid(supplierId?: number): Observable<SupplierInvoiceHistoryGridDTO[]> {
    if (!supplierId) return observableOf([]);
    return this.http.get(getSupplierInvoice20Latest(supplierId));
  }

  save() {
    // No plan to implement, history component is only intended to be used for viewing.
    return observableOf();
  }

  delete() {
    // No plan to implement, history component is only intended to be used for viewing.
    return observableOf();
  }
}
