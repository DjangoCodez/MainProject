import { Injectable } from '@angular/core';
import { IIntrastatTransactionExportDTO } from '@shared/models/generated-interfaces/CommodityCodeDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  createIntrastatExport,
  getIntrastatTransactionsForExport,
} from '@shared/services/generated-service-endpoints/billing/OrderIntrastat.endpoints';
import { Observable } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class IntrastatExportService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    intrastatReportingType: 0,
    fromDate: '',
    toDate: '',
  };
  getGrid(
    id?: number,
    additionalProps?: {
      intrastatReportingType: number;
      fromDate: string;
      toDate: string;
    }
  ): Observable<IIntrastatTransactionExportDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IIntrastatTransactionExportDTO[]>(
      getIntrastatTransactionsForExport(
        this.getGridAdditionalProps.intrastatReportingType,
        this.getGridAdditionalProps.fromDate,
        this.getGridAdditionalProps.toDate
      )
    );
  }
  createIntrastatExport(data: any) {
    return this.http.post<BackendResponse>(createIntrastatExport(), data);
  }
}
