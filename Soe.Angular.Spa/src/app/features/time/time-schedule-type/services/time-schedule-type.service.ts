import { Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IUpdateEntityStatesModel } from '@shared/models/generated-interfaces/CoreModels';
import {
  ITimeScheduleTypeDTO,
  ITimeScheduleTypeGridDTO,
  ITimeScheduleTypeSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { getTimeDeviationCausesDict } from '@shared/services/generated-service-endpoints/time/TimeDeviationCause.endpoints';
import {
  getTimeScheduleTypesDict,
  getScheduleTypesForGrid,
  getScheduleType,
  saveScheduleType,
  deleteScheduleType,
  updateScheduleTypesState,
  getTimeScheduleTypes,
} from '@shared/services/generated-service-endpoints/time/TimeScheduleType.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TimeScheduleTypeService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ITimeScheduleTypeGridDTO[]> {
    return this.http.get<ITimeScheduleTypeGridDTO[]>(
      getScheduleTypesForGrid(id)
    );
  }

  get(
    id: number,
    loadFactors: boolean = true
  ): Observable<ITimeScheduleTypeDTO> {
    return this.http.get<ITimeScheduleTypeDTO>(
      getScheduleType(id, loadFactors)
    );
  }

  save(model: ITimeScheduleTypeDTO): Observable<any> {
    return this.http.post<ITimeScheduleTypeDTO>(saveScheduleType(), model);
  }

  delete(timeScheduleTypeId: number): Observable<any> {
    return this.http.delete(deleteScheduleType(timeScheduleTypeId));
  }

  updateTimeScheduleTypesState(
    model: IUpdateEntityStatesModel
  ): Observable<any> {
    return this.http.post<IUpdateEntityStatesModel>(
      updateScheduleTypesState(),
      model
    );
  }

  getTimeScheduleTypesDict(
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get(getTimeScheduleTypesDict(true, addEmptyRow));
  }

  getTimeScheduleTypesSmall(
    getAll: boolean = true,
    onlyActive: boolean = true,
    loadFactors: boolean = false
  ): Observable<ITimeScheduleTypeSmallDTO[]> {
    return this.http.get<ITimeScheduleTypeSmallDTO[]>(
      getTimeScheduleTypes(getAll, onlyActive, loadFactors)
    );
  }

  getTimeDeviationCausesDict(
    addEmptyRow: boolean,
    removeAbsence: boolean,
    removePresence: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get(
      getTimeDeviationCausesDict(addEmptyRow, removeAbsence, removePresence)
    );
  }
}
