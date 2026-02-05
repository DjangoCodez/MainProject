import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getCustomerStatistics,
  getSalesStatisticsGridData,
} from '@shared/services/generated-service-endpoints/shared/CustomerV2.endpoints';

import { Observable } from 'rxjs';
import { GeneralProductStatisticsDTO } from '../models/sales-statistics.model';
import { ICustomerStatisticsDTO } from '@shared/models/generated-interfaces/CustomerStatisticsDTO';

@Injectable({
  providedIn: 'root',
})
export class SalesStatisticsService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    model: new GeneralProductStatisticsDTO(),
  };
  getGrid(
    id?: number,
    additionalProps?: {
      model: GeneralProductStatisticsDTO;
    }
  ): Observable<ICustomerStatisticsDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.post<ICustomerStatisticsDTO[]>(
      getSalesStatisticsGridData(),
      this.getGridAdditionalProps.model
    );
  }

  GetProductStatisticsPerCustomer(
    model: GeneralProductStatisticsDTO
  ): Observable<ICustomerStatisticsDTO> {
    return this.http.post<ICustomerStatisticsDTO>(
      getCustomerStatistics(),
      model
    );
  }
}
