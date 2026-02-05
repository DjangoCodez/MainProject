import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  IScheduleCycleRuleTypeDTO,
  IScheduleCycleRuleTypeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CrudResponse } from '@shared/interfaces';
import {
  getScheduleCycleRuleTypesGrid,
  getScheduleCycleRuleTypesDict,
  getScheduleCycleRuleType,
  saveScheduleCycleRuleType,
  deleteScheduleCycleRuleType,
} from '@shared/services/generated-service-endpoints/time/ScheduleCycle.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ScheduleCycleRuleTypesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(
    scheduleCycleRuleTypeId?: number
  ): Observable<IScheduleCycleRuleTypeGridDTO[]> {
    return this.http.get<IScheduleCycleRuleTypeGridDTO[]>(
      getScheduleCycleRuleTypesGrid(scheduleCycleRuleTypeId)
    );
  }

  get(scheduleCycleRuleTypeId: number): Observable<IScheduleCycleRuleTypeDTO> {
    return this.http.get<IScheduleCycleRuleTypeDTO>(
      getScheduleCycleRuleType(scheduleCycleRuleTypeId)
    );
  }

  save(model: IScheduleCycleRuleTypeDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveScheduleCycleRuleType(), model);
  }

  delete(scheduleCycleRuleTypeId: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(
      deleteScheduleCycleRuleType(scheduleCycleRuleTypeId)
    );
  }

  getDict(addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getScheduleCycleRuleTypesDict(addEmptyRow)
    );
  }
}
