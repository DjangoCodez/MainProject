import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  IEmployeeGridDTO,
  IEmployeeSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';

import { SoeHttpClient } from '@shared/services/http.service';
import {
  getEmployeesForGrid,
  getEmployeesForGridDict,
  getEmployeesForGridSmall,
  getEmployeeChildsDict,
} from '@shared/services/generated-service-endpoints/time/EmployeeV2.endpoints';

export interface IEmployeesForGridParams {
  date: Date;
  employeeIds: number[];
  showInactive: boolean;
  showEnded: boolean;
  showNotStarted: boolean;
  setAge: boolean;
  loadPayrollGroups: boolean;
  loadAnnualLeaveGroups: boolean;
}
export interface IEmployeesForGridSmallParams {
  dateFrom: Date;
  dateTo: Date;
  employeeIds: number[];
  showInactive: boolean;
  showEnded: boolean;
  showNotStarted: boolean;
  filterOnAnnualLeaveAgreement: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class EmployeeService {
  constructor(private http: SoeHttpClient) {}

  getEmployeesForGrid(
    params: IEmployeesForGridParams
  ): Observable<IEmployeeGridDTO[]> {
    return this.http.get<IEmployeeGridDTO[]>(
      getEmployeesForGrid(
        params.date.toDateTimeString(),
        params.employeeIds?.length > 0 ? params.employeeIds.join(',') : 'null',
        params.showInactive,
        params.showEnded,
        params.showNotStarted,
        params.setAge,
        params.loadPayrollGroups,
        params.loadAnnualLeaveGroups
      )
    );
  }

  getEmployeesForGridSmall(
    params: IEmployeesForGridSmallParams
  ): Observable<IEmployeeSmallDTO[]> {
    return this.http.get<IEmployeeSmallDTO[]>(
      getEmployeesForGridSmall(
        params.dateFrom.toDateTimeString(),
        params.employeeIds?.length > 0 ? params.employeeIds.join(',') : 'null',
        params.showInactive,
        params.showEnded,
        params.showNotStarted
      )
    );
  }

  getEmployeesForGridDict(
    params: IEmployeesForGridSmallParams
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getEmployeesForGridDict(
        params.dateFrom.toDateTimeString(),
        params.dateTo.toDateTimeString(),
        params.employeeIds?.length > 0 ? params.employeeIds.join(',') : 'null',
        params.showInactive,
        params.showEnded,
        params.showNotStarted,
        params.filterOnAnnualLeaveAgreement
      )
    );
  }

  getEmployeeChildsDict(
    employeeId: number,
    addEmptyRow: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getEmployeeChildsDict(employeeId, addEmptyRow)
    );
  }
}
