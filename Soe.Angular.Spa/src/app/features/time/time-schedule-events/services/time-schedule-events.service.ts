import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  ITimeScheduleEventDTO,
  ITimeScheduleEventGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  getTimeScheduleEventsGrid,
  getTimeScheduleEvent,
  saveTimeScheduleEvent,
  deleteTimeScheduleEvent,
} from '@shared/services/generated-service-endpoints/time/TimeScheduleEvent.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class TimeScheduleEventsService {
  http = inject(SoeHttpClient);

  getGrid(id?: number): Observable<ITimeScheduleEventGridDTO[]> {
    return this.http.get<ITimeScheduleEventGridDTO[]>(
      getTimeScheduleEventsGrid(id)
    );
  }

  get(id: number): Observable<ITimeScheduleEventDTO> {
    return this.http.get<ITimeScheduleEventDTO>(getTimeScheduleEvent(id));
  }

  save(item: ITimeScheduleEventDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveTimeScheduleEvent(), item);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteTimeScheduleEvent(id));
  }
}
