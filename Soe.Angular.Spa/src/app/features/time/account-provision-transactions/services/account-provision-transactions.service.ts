import { Injectable } from '@angular/core';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import {
  getTimePeriodHeadsDict,
  getTimePeriodsDict,
} from '@shared/services/generated-service-endpoints/time/TimePeriod.endpoints';
import {
  getAccountProvisionTransactions,
  saveAttestForAccountProvision,
  updateAccountProvisionTransactions,
} from '@shared/services/generated-service-endpoints/time/AccountProvision.endpoints';
import { IAccountProvisionTransactionGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IAccountProvisionTransactionsModel } from '@shared/models/generated-interfaces/TimeModels';

@Injectable({
  providedIn: 'root',
})
export class AccountProvisionTransactionsService {
  constructor(private http: SoeHttpClient) {}
  getGrid(id?: number): Observable<any[]> {
    return new Observable(observer => {
      observer.next([]);
      observer.complete();
    });
  }

  getTimePeriodHeadsDict(
    type: number,
    addEmptyRow: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getTimePeriodHeadsDict(type, 0, addEmptyRow)
    );
  }
  getTimePeriodsDict(
    timePeriodHeadId: number,
    addEmptyRow: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getTimePeriodsDict(timePeriodHeadId, addEmptyRow)
    );
  }
  getAccountProvisionTransactions(
    timePeriodId: number
  ): Observable<IAccountProvisionTransactionGridDTO[]> {
    return this.http.get<IAccountProvisionTransactionGridDTO[]>(
      getAccountProvisionTransactions(timePeriodId)
    );
  }
  saveAttestForAccountProvision(
    provisions: IAccountProvisionTransactionsModel
  ): Observable<any> {
    return this.http.post<any>(saveAttestForAccountProvision(), provisions);
  }
  updateAccountProvisionTransactions(
    provisions: IAccountProvisionTransactionsModel
  ): Observable<any> {
    return this.http.post<any>(
      updateAccountProvisionTransactions(),
      provisions
    );
  }
}
