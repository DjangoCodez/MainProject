import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { StaffingNeedsRuleDTO } from '@src/app/features/time/models/staffing-needs.model';
import {
  deleteStaffingNeedsRule,
  getStaffingNeedsRule,
  getStaffingNeedsRulesGrid,
  saveStaffingNeedsRule,
} from '@shared/services/generated-service-endpoints/time/StaffingNeeds.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { IStaffingNeedsRuleGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Injectable({
  providedIn: 'root',
})
export class StaffingNeedsRulesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IStaffingNeedsRuleGridDTO[]> {
    return this.http.get<IStaffingNeedsRuleGridDTO[]>(
      getStaffingNeedsRulesGrid(id)
    );
  }

  get(id: number): Observable<StaffingNeedsRuleDTO> {
    return this.http.get<StaffingNeedsRuleDTO>(getStaffingNeedsRule(id)).pipe(
      map(data => {
        const obj = new StaffingNeedsRuleDTO();
        Object.assign(obj, data);
        return obj;
      })
    );
  }

  save(model: StaffingNeedsRuleDTO): Observable<any> {
    return this.http.post<StaffingNeedsRuleDTO>(saveStaffingNeedsRule(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteStaffingNeedsRule(id));
  }
}
