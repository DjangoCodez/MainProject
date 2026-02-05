import { Injectable } from '@angular/core';
import {
  IHolidayDTO,
  IHolidayGridDTO,
  IHolidaySmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  deleteHoliday,
  getHoliday,
  getHolidaysGrid,
  getHolidaysSmall,
  onAddHoliday,
  onDeleteHoliday,
  saveHoliday,
} from '@shared/services/generated-service-endpoints/time/Holiday.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { map, Observable } from 'rxjs';
import { onUpdateHalfDay } from '@shared/services/generated-service-endpoints/time/Halfday.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class HolidaysService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IHolidayGridDTO[]> {
    return this.http.get<IHolidayGridDTO[]>(getHolidaysGrid(id));
  }

  get(id: number): Observable<IHolidayDTO> {
    return this.http.get<IHolidayDTO>(getHoliday(id));
  }

  save(model: IHolidayDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveHoliday(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteHoliday(id));
  }

  getHolidaysSmall(
    dateFrom: string,
    dateTo: string
  ): Observable<IHolidaySmallDTO[]> {
    return this.http.get<IHolidaySmallDTO[]>(
      getHolidaysSmall(dateFrom, dateTo)
    );
  }

  // Modal Methods

  onAddHoliday(
    holidayId: number,
    dayTypeId: number
  ): Observable<BackendResponse> {
    return this.http
      .post<any>(onAddHoliday(), {
        id1: holidayId,
        id2: dayTypeId,
      })
      .pipe(map(response => response.result));
  }

  onUpdateHoliday(
    holidayId: number,
    dayTypeId: number,
    oldDateToDelete: string
  ): Observable<BackendResponse> {
    return this.http
      .post<any>(onUpdateHalfDay(), {
        id1: holidayId,
        id2: dayTypeId,
        id3: oldDateToDelete,
      })
      .pipe(map(response => response.result));
  }

  onDeleteHoliday(
    holidayId: number,
    dayTypeId: number
  ): Observable<BackendResponse> {
    return this.http
      .post<any>(onDeleteHoliday(), {
        id1: holidayId,
        id2: dayTypeId,
      })
      .pipe(map(response => response.result));
  }
}
