import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { StaffingNeedsLocationDTO } from '@src/app/features/time/models/staffing-needs.model';
import { IStaffingNeedsLocationGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  deleteStaffingNeedsLocation,
  getStaffingNeedsLocation,
  getStaffingNeedsLocationsGrid,
  saveStaffingNeedsLocation,
} from '@shared/services/generated-service-endpoints/time/StaffingNeeds.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { StaffingNeedsLocationsForm } from '../models/staffing-needs-location-form.model';
import { ValidationHandler } from '@shared/handlers';

@Injectable({
  providedIn: 'root',
})
export class StaffingNeedsLocationsService {
  validationHandler = inject(ValidationHandler);

  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IStaffingNeedsLocationGridDTO[]> {
    return this.http.get<IStaffingNeedsLocationGridDTO[]>(
      getStaffingNeedsLocationsGrid(id)
    );
  }

  get(id: number): Observable<StaffingNeedsLocationDTO> {
    return this.http
      .get<StaffingNeedsLocationDTO>(getStaffingNeedsLocation(id))
      .pipe(
        map(data => {
          const obj = new StaffingNeedsLocationDTO();
          Object.assign(obj, data);
          return obj;
        })
      );
  }

  save(model: StaffingNeedsLocationDTO): Observable<any> {
    return this.http.post<StaffingNeedsLocationDTO>(
      saveStaffingNeedsLocation(),
      model
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteStaffingNeedsLocation(id));
  }

  createForm(element?: StaffingNeedsLocationDTO): StaffingNeedsLocationsForm {
    return new StaffingNeedsLocationsForm({
      validationHandler: this.validationHandler,
      element,
    });
  }
}
