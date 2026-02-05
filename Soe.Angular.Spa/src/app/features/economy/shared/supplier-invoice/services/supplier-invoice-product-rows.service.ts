import { inject, Injectable } from '@angular/core';
import { Observable, of as observableOf } from 'rxjs';
import { SupplierInvoiceProductRowDTO } from '../models/supplier-invoice-product-rows.model';
import { TransferSupplierProductRowsModel } from '../models/transfer-supplier-product-rows.model';
import { SoeHttpClient } from '@shared/services/http.service';
import { IApiServiceGrid } from '@shared/directives/grid-base/grid-base.directive';
import {
  getSupplierInvoiceProductRows,
  transferSupplierProductRows,
} from '@shared/services/generated-service-endpoints/economy/SupplierInvoice.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable()
export class SupplierInvoiceProductRowsService
  implements IApiServiceGrid<SupplierInvoiceProductRowDTO>
{
  private readonly http = inject(SoeHttpClient);

  getGrid(
    supplierInvoiceId?: number
  ): Observable<SupplierInvoiceProductRowDTO[]> {
    if (!supplierInvoiceId) return observableOf([]);

    return this.http.get(getSupplierInvoiceProductRows(supplierInvoiceId));
  }

  get(id: number): Observable<SupplierInvoiceProductRowDTO> {
    // Not needed for this grid
    return observableOf();
  }

  save() {
    // No plan to implement, product rows are read-only
    return observableOf();
  }

  delete() {
    // No plan to implement, product rows are read-only
    return observableOf();
  }

  transferToOrder(
    model: TransferSupplierProductRowsModel
  ): Observable<BackendResponse> {
    return this.http.post(transferSupplierProductRows(), model);
  }
}
