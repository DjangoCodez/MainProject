import { Injectable } from '@angular/core';
import { ITrackChangesLogDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { getShiftHistory } from '@shared/services/generated-service-endpoints/time/SchedulePlanning.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ShiftHistoryService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number, shiftIds?: number[]): Observable<ITrackChangesLogDTO[]> {
    let stringShiftIds = shiftIds?.join(', ');
    return this.http.get<ITrackChangesLogDTO[]>(
      getShiftHistory(stringShiftIds || '')
    );
  }
}
