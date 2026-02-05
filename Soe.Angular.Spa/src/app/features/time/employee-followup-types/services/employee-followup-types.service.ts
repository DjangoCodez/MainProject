import { Injectable } from '@angular/core';
import {
  IFollowUpTypeDTO,
  IFollowUpTypeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import {
  deleteFollowUpType,
  getFollowUpType,
  getFollowUpTypes,
  saveFollowUpType,
  updateFollowUpTypesState,
} from '@shared/services/generated-service-endpoints/time/FollowupType.endpoints';
import { IUpdateEntityStatesModel } from '@shared/models/generated-interfaces/CoreModels';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class EmployeeFollowupTypesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IFollowUpTypeGridDTO[]> {
    return this.http.get<IFollowUpTypeGridDTO[]>(getFollowUpTypes(id));
  }

  get(id: number): Observable<IFollowUpTypeDTO> {
    return this.http.get<IFollowUpTypeDTO>(getFollowUpType(id));
  }

  save(model: IFollowUpTypeDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveFollowUpType(), model);
  }

  updateFollowUpTypesState(model: IUpdateEntityStatesModel): Observable<any> {
    return this.http.post<IUpdateEntityStatesModel>(
      updateFollowUpTypesState(),
      model
    );
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteFollowUpType(id));
  }
}
