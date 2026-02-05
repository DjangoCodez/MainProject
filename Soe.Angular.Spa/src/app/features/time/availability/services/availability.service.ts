import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import { IEmployeeRequestGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { getEmployeeRequestsGrid } from '@shared/services/generated-service-endpoints/time/EmployeeRequest.endpoints';
import { DateUtil } from '@shared/util/date-util';

@Injectable({
  providedIn: 'root',
})
export class AvailabilityService {
  constructor(private http: SoeHttpClient) {}

  getGrid(
    id?: number,
    additionalProps?: { fromDate?: Date; toDate?: Date }
  ): Observable<IEmployeeRequestGridDTO[]> {
    const fromDate = additionalProps?.fromDate || DateUtil.getToday();
    const toDate = additionalProps?.toDate || DateUtil.getToday();
    const fromDateStr = DateUtil.format(fromDate, 'yyyy-MM-dd');
    const toDateStr = DateUtil.format(toDate, 'yyyy-MM-dd');
    return this.http.get<IEmployeeRequestGridDTO[]>(
      getEmployeeRequestsGrid(fromDateStr as any, toDateStr as any, id)
    );
  }
}
