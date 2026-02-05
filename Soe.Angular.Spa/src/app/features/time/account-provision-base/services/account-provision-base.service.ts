import { Injectable } from '@angular/core';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { SoeHttpClient } from '@shared/services/http.service';
import {  Observable } from 'rxjs';
import { getTimePeriodHeadsDict, getTimePeriodsDict } from '@shared/services/generated-service-endpoints/time/TimePeriod.endpoints';
import { getAccountProvisionBaseColumns, getAccountProvisionBase, saveAccountProvisionBase, lockAccountProvisionBase, unLockAccountProvisionBase } from '@shared/services/generated-service-endpoints/time/AccountProvision.endpoints';
import { IAccountProvisionBaseDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Injectable({
  providedIn: 'root'
})
export class AccountProvisionBaseService {

  constructor(private http: SoeHttpClient) {}
  getGrid(id?: number): Observable<any[]> {
      return new Observable(observer => {
        observer.next([]);
        observer.complete();
      });
  }
  
  getTimePeriodHeadsDict(type: number, addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getTimePeriodHeadsDict(type, 0, addEmptyRow)
    );
  }
  getTimePeriodsDict(timePeriodHeadId: number, addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getTimePeriodsDict(timePeriodHeadId, addEmptyRow)
    );
  }
  getAccountProvisionBaseColumns(timePeriodId: number): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getAccountProvisionBaseColumns(timePeriodId)
    );
  }
  getAccountProvisionBase(timePeriodId: number): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getAccountProvisionBase(timePeriodId)
    );
  }
  lockAccountProvisionBase(timePeriodId: number): Observable<any> {
    return this.http.get<any>(
      lockAccountProvisionBase(timePeriodId)
    );
  }
  unLockAccountProvisionBase(timePeriodId: number): Observable<any> {
    return this.http.get<any>(
      unLockAccountProvisionBase(timePeriodId)
    );
  }
  saveAccountProvisionBase(provisions: IAccountProvisionBaseDTO[]): Observable<any> {
    return this.http.post<any>(
      saveAccountProvisionBase(),
      provisions
    );
  } 
}