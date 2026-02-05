import { Injectable } from '@angular/core';
import {
  ISkillTypeDTO,
  ISkillTypeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteSkillType,
  getSkillType,
  getSkillTypesDict,
  getSkillTypesGrid,
  saveSkillType,
  updateSkillTypesState,
} from '@shared/services/generated-service-endpoints/time/Skill.endpoints';
import { Observable } from 'rxjs';
import { IUpdateEntityStatesModel } from '@shared/models/generated-interfaces/CoreModels';
import { SmallGenericType } from '@shared/models/generic-type.model';

@Injectable({
  providedIn: 'root',
})
export class SkillTypesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ISkillTypeGridDTO[]> {
    return this.http.get<ISkillTypeGridDTO[]>(getSkillTypesGrid(id));
  }

  get(id: number): Observable<ISkillTypeDTO> {
    return this.http.get<ISkillTypeDTO>(getSkillType(id));
  }

  getSkillTypesDict(addEmptyRow: boolean): Observable<SmallGenericType[]> {
    return this.http.get(getSkillTypesDict(addEmptyRow));
  }

  save(model: ISkillTypeDTO): Observable<any> {
    return this.http.post<ISkillTypeDTO>(saveSkillType(), model);
  }

  updateSkillTypesState(model: IUpdateEntityStatesModel): Observable<any> {
    return this.http.post<IUpdateEntityStatesModel>(
      updateSkillTypesState(),
      model
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteSkillType(id));
  }
}
