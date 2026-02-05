import { Injectable } from '@angular/core';
import { PaymentConditionDTO } from '@shared/features/payment-conditions/models/payment-condition.model';
import { ISupplierInvoiceGridDTO } from '@shared/models/generated-interfaces/SupplierInvoiceDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { getPaymentCondition } from '@shared/services/generated-service-endpoints/economy/PaymentCondition.endpoints';
import { getInvoicesForSupplier } from '@shared/services/generated-service-endpoints/economy/SupplierInvoice.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SupplierCentralService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    loadOpen: false,
    loadClosed: false,
    onlyMine: false,
    allItemsSelection: 0,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      loadOpen: boolean;
      loadClosed: boolean;
      onlyMine: boolean;
      allItemsSelection: number;
    }
  ) {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<ISupplierInvoiceGridDTO[]>(
      getInvoicesForSupplier(
        this.getGridAdditionalProps.loadOpen,
        this.getGridAdditionalProps.loadClosed,
        this.getGridAdditionalProps.onlyMine,
        this.getGridAdditionalProps.allItemsSelection,
        id!
      )
    );
  }

  getPaymentCondition(id: number): Observable<PaymentConditionDTO> {
    return this.http.get(getPaymentCondition(id));
  }
}
