import { Injectable, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { IUpdateEntityStatesModel } from '@shared/models/generated-interfaces/CoreModels';
import {
  IEmploymentTypeDTO,
  IEmploymentTypeGridDTO,
} from '@shared/models/generated-interfaces/EmploymentTypeDTO';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteEmploymentType,
  getEmploymentType,
  getEmploymentTypesGrid,
  getStandardEmploymentTypes,
  saveEmploymentType,
  updateEmploymentTypesState,
} from '@shared/services/generated-service-endpoints/time/EmploymentType.endpoints';
import { Observable, map } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class EmploymentTypesService {
  translate = inject(TranslateService);

  constructor(private http: SoeHttpClient) {}

  getGrid(employmentTypeId?: number): Observable<IEmploymentTypeGridDTO[]> {
    const standard = this.translate.instant(
      'time.employee.employmenttype.standard'
    );
    const own = this.translate.instant('time.employee.employmenttype.own');

    return this.http
      .get<IEmploymentTypeGridDTO[]>(getEmploymentTypesGrid(employmentTypeId))
      .pipe(
        map(x => {
          x.forEach(y => {
            y.standardText = y.standard ? standard : own;
            if (y.standard) y.employmentTypeId = y.type;
          });
          return x;
        })
      );
  }

  get(employmentTypeId: number): Observable<IEmploymentTypeDTO> {
    return this.http.get<IEmploymentTypeDTO>(
      getEmploymentType(employmentTypeId)
    );
  }

  save(model: IEmploymentTypeDTO): Observable<any> {
    model.active = model.state === SoeEntityState.Active;
    return this.http.post<IEmploymentTypeDTO>(saveEmploymentType(), model);
  }

  delete(employmentTypeId: number): Observable<any> {
    return this.http.delete(deleteEmploymentType(employmentTypeId));
  }

  getStandardEmploymentTypes(): Observable<any> {
    return this.http.get(getStandardEmploymentTypes());
  }

  updateEmploymentTypesState(model: IUpdateEntityStatesModel): Observable<any> {
    return this.http.post<IUpdateEntityStatesModel>(
      updateEmploymentTypesState(),
      model
    );
  }
}
