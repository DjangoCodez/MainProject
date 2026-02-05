import { inject, Injectable } from '@angular/core';
import { IMultiCompanyResponseDTO } from '@features/client-management/models/client-management.models';
import { ISupplierInvoiceSummaryDTO } from '@shared/models/generated-interfaces/SupplierInvoiceDTOs';
import { getSupplierInvoiceOverview } from '@shared/services/generated-service-endpoints/clientmanagement/ClientManagement.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SupplierInvoiceOverviewService {
  #http = inject(SoeHttpClient);

  getGrid(): Observable<ISupplierInvoiceSummaryDTO[]> {
    return of([]);
  }

  getInvoicesSummary(): Observable<
    IMultiCompanyResponseDTO<ISupplierInvoiceSummaryDTO[]>
  > {
    return this.#http.get<
      IMultiCompanyResponseDTO<ISupplierInvoiceSummaryDTO[]>
    >(getSupplierInvoiceOverview());
  }
}
