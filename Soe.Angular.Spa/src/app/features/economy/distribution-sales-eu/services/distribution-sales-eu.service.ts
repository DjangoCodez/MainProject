import { Injectable } from '@angular/core';
import {
  IAccountYearDTO,
  ISalesEUDetailDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  salesEU,
  salesEUDetails,
  salesEUExportFile,
} from '@shared/services/generated-service-endpoints/report/SalesEU.endpoints';
import { map, Observable } from 'rxjs';
import { SalesEUGridDTO } from '../models/distribution-sales-eu.model';
import {
  getAccountYearId,
  getAccountYears,
  getCurrentAccountYear,
} from '@shared/services/generated-service-endpoints/economy/AccountYear.endpoints';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';

@Injectable({
  providedIn: 'root',
})
export class DistributionSalesEuService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    startDate: new Date(),
    stopDate: new Date(),
  };
  getGrid(
    id?: number,
    additionalProps?: { startDate: Date; stopDate: Date }
  ): Observable<SalesEUGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http
      .get<
        SalesEUGridDTO[]
      >(salesEU(this.getGridAdditionalProps.startDate.toDateTimeString(), this.getGridAdditionalProps.stopDate.toDateTimeString()))
      .pipe(
        map(data => {
          return data.sort((a, b) => {
            return a.customerName.localeCompare(b.customerName);
          });
        })
      );
  }

  getDetailGrid(
    actorId: number,
    startDate: Date,
    stopDate: Date
  ): Observable<ISalesEUDetailDTO[]> {
    return this.http
      .get<
        ISalesEUDetailDTO[]
      >(salesEUDetails(actorId, startDate.toDateTimeString(), stopDate.toDateTimeString()))
      .pipe(
        map(data => {
          return data.sort((a, b) => {
            const aInvoice = parseInt(a.invoiceNr || '0', 10);
            const bInvoice = parseInt(b.invoiceNr || '0', 10);
            return bInvoice - aInvoice;
          });
        })
      );
  }

  getExportFile(
    periodType: number,
    startDate: Date,
    stopDate: Date
  ): Observable<Blob[]> {
    return this.http.get<Blob[]>(
      salesEUExportFile(
        periodType,
        startDate.toDateTimeString(),
        stopDate.toDateTimeString()
      )
    );
  }

  getCurrentAccountYear(): Observable<IAccountYearDTO> {
    return this.http.get<IAccountYearDTO>(getCurrentAccountYear());
  }

  getAccountYears(
    addEmptyRow: boolean = false,
    excludeNew: boolean = false
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getAccountYears(addEmptyRow, excludeNew)
    );
  }

  getAccountYearIntervals(
    accountYearId: number,
    loadPeriods: boolean = true
  ): Observable<IAccountYearDTO> {
    return this.http.get<IAccountYearDTO>(
      getAccountYearId(accountYearId, loadPeriods)
    );
  }
}
