import { Injectable } from '@angular/core';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import {
  ITimeScheduleTaskTypeDTO,
  ITimeScheduleTaskTypeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  deleteTimeScheduleTaskType,
  getTimeScheduleTaskType,
  getTimeScheduleTaskTypesGrid,
  saveTimeScheduleTaskType,
} from '@shared/services/generated-service-endpoints/time/TimeScheduleTask.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TimeScheduleTaskTypesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ITimeScheduleTaskTypeGridDTO[]> {
    return this.http.get<ITimeScheduleTaskTypeGridDTO[]>(
      getTimeScheduleTaskTypesGrid(id)
    );
  }

  get(id: number): Observable<ITimeScheduleTaskTypeDTO> {
    return this.http.get<ITimeScheduleTaskTypeDTO>(getTimeScheduleTaskType(id));
  }

  save(model: ITimeScheduleTaskTypeDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveTimeScheduleTaskType(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteTimeScheduleTaskType(id));
  }
}
