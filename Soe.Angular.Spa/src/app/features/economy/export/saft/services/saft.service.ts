import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  createSAFTExport,
  getSAFTTransactionsForExport,
} from '@shared/services/generated-service-endpoints/economy/ExportFiles.endpoints';
import { ISaftExportDTO } from '../models/SaftExportDTO.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class SaftService implements ISaftService {
  private http = inject(SoeHttpClient);

  getGridAdditionalProps = {
    dateFrom: new Date(),
    dateTo: new Date(),
  };
  public getGrid(
    id?: number,
    additionalProps?: { dateFrom: Date; dateTo: Date }
  ): Observable<ISaftExportDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    const dateFromString: string =
      this.getGridAdditionalProps.dateFrom.toDateTimeString();
    const dateToString: string =
      this.getGridAdditionalProps.dateTo.toDateTimeString();

    return this.http.get(
      getSAFTTransactionsForExport(dateFromString, dateToString)
    );
  }

  public getSAFTExportFile(
    dateFrom: Date,
    dateTo: Date
  ): Observable<BackendResponse> {
    const dateFromString: string = dateFrom.toDateTimeString();
    const dateToString: string = dateTo.toDateTimeString();

    return this.http.get(createSAFTExport(dateFromString, dateToString));
  }
}
export interface ISaftService {
  getSAFTExportFile(dateFrom: Date, dateTo: Date): Observable<BackendResponse>;
  getGrid(
    id?: number,
    additionalProps?: { dateFrom: Date; dateTo: Date }
  ): Observable<ISaftExportDTO[]>;
}
