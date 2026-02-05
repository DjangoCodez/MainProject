import { Injectable } from '@angular/core';
import { IShiftAccountingRowDTO } from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { getShiftAccountingRows } from '@shared/services/generated-service-endpoints/time/SchedulePlanning.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ShiftAccountingService {
  constructor(private http: SoeHttpClient) {}

  getGrid(
    id?: number,
    shiftIds?: number[]
  ): Observable<IShiftAccountingRowDTO[]> {
    const stringShiftIds = shiftIds?.join(', ');
    return this.http.get<IShiftAccountingRowDTO[]>(
      getShiftAccountingRows(stringShiftIds || '')
    );
  }
}
