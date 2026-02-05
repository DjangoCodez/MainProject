import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { getTimeScheduleTasksDict } from '@shared/services/generated-service-endpoints/time/TimeScheduleTask.endpoints';
import { tap } from 'rxjs/operators';
import {
  getDayTypesAndWeekdays,
  getDayTypesDict,
} from '@shared/services/generated-service-endpoints/time/DayType.endpoints';
import { IDayTypeAndWeekdayDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';

import { SoeHttpClient } from '@shared/services/http.service';
import { getHolidayTypesDict } from '@shared/services/generated-service-endpoints/time/Holiday.endpoints';
import { getEmployeeGroupsDict } from '@shared/services/generated-service-endpoints/time/EmployeeGroup.endpoints';
import { getPayrollGroupsDict } from '@shared/services/generated-service-endpoints/time/PayrollGroup.endpoints';
import { getVacationGroupsDict } from '@shared/services/generated-service-endpoints/time/VacationGroup.endpoints';
import { getTimeScheduleTypesDict } from '@shared/services/generated-service-endpoints/time/TimeScheduleType.endpoints';
import { getTimeCodesDictByType } from '@shared/services/generated-service-endpoints/time/TimeCode.endpoints';

@Injectable({
  providedIn: 'root',
})
export class TimeService {
  private timeScheduleTasksBS$ = new BehaviorSubject<
    SmallGenericType[] | undefined
  >(undefined);
  timeScheduleTasks$ = this.timeScheduleTasksBS$.asObservable();
  timeScheduleTasks: SmallGenericType[] = [];

  constructor(private http: SoeHttpClient) {}

  setTimeScheduleTasks(timeScheduleTasks: SmallGenericType[]) {
    this.timeScheduleTasksBS$.next(timeScheduleTasks);
    this.timeScheduleTasks = timeScheduleTasks;
  }

  getTimeScheduleTasksDict(
    addEmptyRow: boolean,
    forceReload = false
  ): Observable<SmallGenericType[]> {
    if (!forceReload && this.timeScheduleTasksBS$.value)
      return of(this.timeScheduleTasksBS$.value);

    return this.http
      .get<SmallGenericType[]>(getTimeScheduleTasksDict(addEmptyRow))
      .pipe(
        tap(timeScheduleTasks => this.setTimeScheduleTasks(timeScheduleTasks))
      );
  }

  getDayTypesAndWeekdays(): Observable<IDayTypeAndWeekdayDTO[]> {
    return this.http.get(getDayTypesAndWeekdays());
  }

  getHolidayTypesDict(): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getHolidayTypesDict());
  }

  getDayTypes(addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getDayTypesDict(addEmptyRow));
  }

  getTimeCodesDictByType(
    timeCodeType: number,
    onlyActive: boolean,
    addEmptyRow: boolean,
    concatCodeAndName: boolean,
    loadPayrollProducts: boolean,
    onlyWithInvoiceProduct: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getTimeCodesDictByType(
        timeCodeType,
        onlyActive,
        addEmptyRow,
        concatCodeAndName,
        loadPayrollProducts,
        onlyWithInvoiceProduct
      )
    );
  }

  getTimeScheduleTypesDict(
    getAll: boolean,
    addEmptyRow: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getTimeScheduleTypesDict(getAll, addEmptyRow)
    );
  }

  // EmployeeGroup - Move this inside 'EmployeeGroup' component, once it is implemented
  getEmployeeGroups(addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getEmployeeGroupsDict(addEmptyRow)
    );
  }
  // End EmployeeGroup

  // PayrollGroup - Move this inside 'PayrollGroup' component, once it is implemented
  getPayrollGroups(addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getPayrollGroupsDict(addEmptyRow)
    );
  }
  // End PayrollGroup

  // VacationGroup - Move this inside 'VacationGroup' component, once it is implemented
  getVacationGroups(addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getVacationGroupsDict(addEmptyRow)
    );
  }
  // End VacationGroup
}
