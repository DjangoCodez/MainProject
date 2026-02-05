import { Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IDayTypeDTO,
  IDayTypeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteDayType,
  getDaysOfWeekDict,
  getDayType,
  getDayTypesByCompanyDict,
  getDayTypesGrid,
  saveDayType,
} from '@shared/services/generated-service-endpoints/time/DayType.endpoints';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class DayTypesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IDayTypeGridDTO[]> {
    return this.http.get<IDayTypeGridDTO[]>(getDayTypesGrid(id));
  }

  get(id: number): Observable<IDayTypeDTO> {
    return this.http.get<IDayTypeDTO>(getDayType(id));
  }

  getDaysOfWeek(addEmptyRow: boolean): Observable<SmallGenericType[]> {
    return this.http
      .get<SmallGenericType[]>(getDaysOfWeekDict(addEmptyRow))
      .pipe(
        tap(x => {
          // TermGroup values are stored as 1-7 (sun-sat) but we want to use 0-6 for sunday to saturday
          x.forEach(y => {
            if (y.id > 0) y.id--;
          });
        })
      );
  }

  save(model: IDayTypeDTO): Observable<any> {
    return this.http.post<IDayTypeDTO>(saveDayType(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteDayType(id));
  }

  getDayTypesByCompanyDict(
    addEmptyRow: boolean,
    onlyHolidaySalary: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getDayTypesByCompanyDict(addEmptyRow, onlyHolidaySalary)
    );
  }
}
