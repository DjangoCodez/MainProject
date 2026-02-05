import { Injectable } from '@angular/core';
import { IEmployeeEarnedHolidayDTO } from '@shared/models/generated-interfaces/EmployeeEarnedHolidayDTO';
import { ActionResultDelete, ActionResultSave } from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IHolidayDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IEarnedHolidayModel, IManageTransactionsForEarnedHolidayModel } from '@shared/models/generated-interfaces/TimeModels';
import { SoeHttpClient } from '@shared/services/http.service';
import { getYears, getHolidays, loadEarnedHolidaysContent, createTransactionsForEarnedHoliday, deleteTransactionsForEarnedHolidayContent } from '@shared/services/generated-service-endpoints/time/TimeEarnedDays.endpoints';
import {  Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EarnedHolidayService {

  constructor(private http: SoeHttpClient) {}
  getGrid(id?: number): Observable<IEmployeeEarnedHolidayDTO[]> {
      return new Observable(observer => {
        observer.next([]);
        observer.complete();
      });
  }
  
  getYears(yearsBack: number): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getYears(yearsBack)
    );
  }
  
  getHolidays(year: number, onlyRedDays: boolean, onlyHistorical: boolean): Observable<any[]> {
    return this.http.get<IHolidayDTO[]>(
      getHolidays(year, onlyRedDays, onlyHistorical)
    );
  }
  
  loadEarnedHolidaysContent(model: IEarnedHolidayModel): Observable<any[]> {
    return this.http.post<IEmployeeEarnedHolidayDTO[]>(
      loadEarnedHolidaysContent(), model);
  }
  
  createTransactionsForEarnedHoliday(model: IManageTransactionsForEarnedHolidayModel): Observable<ActionResultSave> {
    return this.http.post<ActionResultSave>(
      createTransactionsForEarnedHoliday(), model);
  }
  
  deleteTransactionsForEarnedHolidayContent(model: IManageTransactionsForEarnedHolidayModel): Observable<ActionResultDelete> {
    return this.http.post<ActionResultDelete>(
      deleteTransactionsForEarnedHolidayContent(), model);
  }
  
}