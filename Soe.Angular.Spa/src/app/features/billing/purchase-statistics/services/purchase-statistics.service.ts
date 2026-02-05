import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { getPurchaseStatistics } from '@shared/services/generated-service-endpoints/billing/PurchaseStatisticsController.endpoints';
import {
  PurchaseStatisticsDTO,
  PurchaseStatisticsFilterDTO,
} from '../models/purchase-statistics.model';
import { SoeHttpClient } from '@shared/services/http.service';

@Injectable({
  providedIn: 'root',
})
export class PurchaseStatisticsService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    model: new PurchaseStatisticsFilterDTO(),
  };
  getGrid(
    id?: number,
    additionalProps?: {
      model: PurchaseStatisticsFilterDTO;
    }
  ): Observable<PurchaseStatisticsDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.post<PurchaseStatisticsDTO[]>(
      getPurchaseStatistics(),
      this.getGridAdditionalProps.model
    );
  }
}
