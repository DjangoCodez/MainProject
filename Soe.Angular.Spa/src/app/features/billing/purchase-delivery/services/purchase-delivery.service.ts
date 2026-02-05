import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import {
  PurchaseDeliveryDTO,
  PurchaseDeliverySaveDTO,
} from '../models/purchase-delivery.model';
import {
  getDelivery,
  getDeliveryList,
  getDeliveryRows,
  getDeliveryRowsFromPurchase,
  getPurchaseDeliveryRowsByPurchaseId,
  saveDelivery,
} from '@shared/services/generated-service-endpoints/billing/PurchaseDelivery.endpoints';
import { IPurchaseDeliveryGridDTO } from '@shared/models/generated-interfaces/PurchaseDeliveryDTOs ';
import { PurchaseDeliveryRowDTO } from '../models/purchase-delivery.model';

@Injectable({
  providedIn: 'root',
})
export class PurchaseDeliveryService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    selectedId: 0,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      selectedId: number;
    }
  ): Observable<IPurchaseDeliveryGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IPurchaseDeliveryGridDTO[]>(
      getDeliveryList(this.getGridAdditionalProps.selectedId, id)
    );
  }

  get(id: number): Observable<PurchaseDeliveryDTO> {
    return this.http.get<PurchaseDeliveryDTO>(getDelivery(id));
  }

  getRowsFromPurchase(
    purchaseId: number,
    supplierId: number
  ): Observable<PurchaseDeliveryRowDTO[]> {
    return this.http.get<PurchaseDeliveryRowDTO[]>(
      getDeliveryRowsFromPurchase(purchaseId, supplierId)
    );
  }

  // getPurchaseDeliveryRowsById(
  //   purchaseId: number
  // ): Observable<PurchaseDeliveryDTO> {
  //   return this.http.get<PurchaseDeliveryDTO>(
  //     getPurchaseDeliveryRowsByPurchaseId(purchaseId)
  //   );
  // }

  getDeliveryRows(purchaseId: number): Observable<PurchaseDeliveryRowDTO[]> {
    return this.http.get<PurchaseDeliveryRowDTO[]>(getDeliveryRows(purchaseId));
  }

  getPurchaseDeliveryRowsByPurchaseId(
    purchaseId: number
  ): Observable<PurchaseDeliveryRowDTO[]> {
    return this.http.get<PurchaseDeliveryRowDTO[]>(
      getPurchaseDeliveryRowsByPurchaseId(purchaseId)
    );
  }

  delete(id: number): Observable<any> {
    return new Observable();
  }

  save(model: PurchaseDeliverySaveDTO): Observable<any> {
    return this.http.post<PurchaseDeliverySaveDTO>(saveDelivery(), model);
  }
}
