import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  IScheduleCycleDTO,
  IScheduleCycleGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CrudResponse } from '@shared/interfaces';
import {
  getScheduleCyclesGrid,
  getScheduleCyclesDict,
  getScheduleCycle,
  saveScheduleCycle,
  deleteScheduleCycle,
} from '@shared/services/generated-service-endpoints/time/ScheduleCycle.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ScheduleCyclesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(scheduleCycleId?: number): Observable<IScheduleCycleGridDTO[]> {
    return this.http.get<IScheduleCycleGridDTO[]>(
      getScheduleCyclesGrid(scheduleCycleId)
    );
  }

  get(scheduleCycleId: number): Observable<IScheduleCycleDTO> {
    return this.http.get<IScheduleCycleDTO>(getScheduleCycle(scheduleCycleId));
  }

  getDict(addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getScheduleCyclesDict(addEmptyRow)
    );
  }

  save(model: IScheduleCycleDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveScheduleCycle(), model);
  }

  delete(scheduleCycleId: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(
      deleteScheduleCycle(scheduleCycleId)
    );
  }
}
