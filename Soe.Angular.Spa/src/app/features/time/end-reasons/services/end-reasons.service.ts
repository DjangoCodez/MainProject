import { Injectable } from '@angular/core';
import {
  IEndReasonDTO,
  IEndReasonGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, map } from 'rxjs';
import {
  deleteEndReason,
  getEndReason,
  getEndReasonsGrid,
  saveEndReason,
  updateEndReasonsState,
} from '@shared/services/generated-service-endpoints/time/EndReason.endpoints';
import { IUpdateEntityStatesModel } from '@shared/models/generated-interfaces/CoreModels';

@Injectable({
  providedIn: 'root',
})
export class EndReasonsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IEndReasonGridDTO[]> {
    return this.http
      .get<IEndReasonGridDTO[]>(getEndReasonsGrid(id))
      .pipe(map(data => data.filter(er => er.endReasonId !== 0)));
  }

  get(id: number): Observable<IEndReasonDTO> {
    return this.http.get<IEndReasonDTO>(getEndReason(id));
  }

  save(model: IEndReasonDTO): Observable<any> {
    return this.http.post<IEndReasonDTO>(saveEndReason(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteEndReason(id));
  }

  updateEndReasonsState(model: IUpdateEntityStatesModel): Observable<any> {
    return this.http.post<IUpdateEntityStatesModel>(
      updateEndReasonsState(),
      model
    );
  }
}
