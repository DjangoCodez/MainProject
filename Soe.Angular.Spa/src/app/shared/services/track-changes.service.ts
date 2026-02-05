import { ITrackChangesLogDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from './http.service';
import { Observable } from 'rxjs';
import { getTrackChangesLog } from './generated-service-endpoints/core/TrackChanges.endpoints';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class TrackChangesService {
  constructor(private http: SoeHttpClient) {}

  getTrackChangesLog(
    entityId: number,
    recordId: number,
    fromDate: string,
    toDate: string
  ): Observable<ITrackChangesLogDTO[]> {
    return this.http.get<ITrackChangesLogDTO[]>(
      getTrackChangesLog(entityId, recordId, fromDate, toDate)
    );
  }
}
