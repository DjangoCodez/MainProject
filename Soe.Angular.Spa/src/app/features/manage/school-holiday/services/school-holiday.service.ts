import { Injectable } from '@angular/core';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import {
  ISchoolHolidayDTO,
  ISchoolHolidayGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  deleteSchoolHoliday,
  getSchoolHoliday,
  getSchoolHolidaysGrid,
  saveSchoolHoliday,
} from '@shared/services/generated-service-endpoints/manage/SchoolHoliday.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SchoolHolidayService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ISchoolHolidayGridDTO[]> {
    return this.http.get<ISchoolHolidayGridDTO[]>(getSchoolHolidaysGrid(id));
  }

  get(id: number): Observable<ISchoolHolidayDTO> {
    return this.http.get<ISchoolHolidayDTO>(getSchoolHoliday(id));
  }

  save(model: ISchoolHolidayDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveSchoolHoliday(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteSchoolHoliday(id));
  }
}
