import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { StaffingNeedsLocationGroupDTO } from '@src/app/features/time/models/staffing-needs.model';
import { IStaffingNeedsLocationGroupGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  deleteStaffingNeedsLocationGroup,
  getStaffingNeedsLocationGroup,
  getStaffingNeedsLocationGroupsGrid,
  saveStaffingNeedsLocationGroup,
} from '@shared/services/generated-service-endpoints/time/StaffingNeeds.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { StaffingNeedsLocationGroupsForm } from '../models/staffing-needs-location-group-form.model';
import { ValidationHandler } from '@shared/handlers';

@Injectable({
  providedIn: 'root',
})
export class StaffingNeedsLocationGroupsService {
  constructor(
    private http: SoeHttpClient,
    private validationHandler: ValidationHandler
  ) {}

  getGrid(id?: number): Observable<IStaffingNeedsLocationGroupGridDTO[]> {
    return this.http.get<IStaffingNeedsLocationGroupGridDTO[]>(
      getStaffingNeedsLocationGroupsGrid(id)
    );
  }

  get(id: number): Observable<StaffingNeedsLocationGroupDTO> {
    return this.http
      .get<StaffingNeedsLocationGroupDTO>(getStaffingNeedsLocationGroup(id))
      .pipe(
        map(data => {
          const obj = new StaffingNeedsLocationGroupDTO();
          Object.assign(obj, data);
          return obj;
        })
      );
  }

  save(
    dto: StaffingNeedsLocationGroupDTO,
    shiftTypeIds: number[]
  ): Observable<any> {
    const model = { dto: dto, shiftTypeIds: shiftTypeIds };
    return this.http.post<StaffingNeedsLocationGroupDTO>(
      saveStaffingNeedsLocationGroup(),
      model
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteStaffingNeedsLocationGroup(id));
  }

  createForm(
    element?: IStaffingNeedsLocationGroupGridDTO
  ): StaffingNeedsLocationGroupsForm {
    const form = new StaffingNeedsLocationGroupsForm({
      validationHandler: this.validationHandler,
      element,
    });
    return form;
  }
}
