import { Injectable } from '@angular/core';
import { IInvoiceDistributionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { getEDistributionItems } from '@shared/services/generated-service-endpoints/report/ReportEDistribution.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class EdistributionService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = { originType: 0, type: 0, allItemsSelection: 0 };
  getGrid(
    id?: number,
    additionalProps?: {
      originType: number;
      type: number;
      allItemsSelection: number;
    }
  ): Observable<IInvoiceDistributionDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IInvoiceDistributionDTO[]>(
      getEDistributionItems(
        this.getGridAdditionalProps.originType,
        this.getGridAdditionalProps.type,
        this.getGridAdditionalProps.allItemsSelection
      )
    );
  }
}
