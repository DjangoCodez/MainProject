import { Injectable } from '@angular/core';
import { IAccountYearDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getAccountYearId,
  getAllAccountYears,
  getCurrentAccountYear,
} from '@shared/services/generated-service-endpoints/economy/AccountYear.endpoints';
import { Observable } from 'rxjs';

@Injectable()
export class AccountingPeriodSelectionService {
  constructor(private http: SoeHttpClient) {}

  getCurrentAccountYear(): Observable<IAccountYearDTO> {
    return this.http.get<IAccountYearDTO>(getCurrentAccountYear());
  }

  getAccountYears(excludeNew: boolean = false): Observable<IAccountYearDTO[]> {
    return this.http.get<IAccountYearDTO[]>(
      getAllAccountYears(false, excludeNew)
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
