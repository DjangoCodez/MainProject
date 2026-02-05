import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  deleteSkill,
  getSkill,
  getSkillsGrid,
  saveSkill,
} from '@shared/services/generated-service-endpoints/time/Skill.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  ISkillDTO,
  ISkillGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Injectable({
  providedIn: 'root',
})
export class SkillsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ISkillGridDTO[]> {
    return this.http.get<ISkillGridDTO[]>(getSkillsGrid(id));
  }

  get(id: number): Observable<ISkillDTO> {
    return this.http.get<ISkillDTO>(getSkill(id));
  }

  save(model: ISkillDTO): Observable<any> {
    return this.http.post<ISkillDTO>(saveSkill(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteSkill(id));
  }
}
