import { inject, Injectable } from '@angular/core';
import {
  ITimeHalfdayEditDTO,
  ITimeHalfdayGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ProgressService } from '@shared/services/progress/progress.service';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getHalfday,
  getHalfdaysGrid,
  getHalfDayTypesDict,
  getDayTypesByCompanyDict,
  getTimeCodeBrakeDict,
  saveHalfday,
  deleteHalfday,
  onAddHalfDay,
  onUpdateHalfDay,
  onDeleteHalfDay,
} from '@shared/services/generated-service-endpoints/time/Halfday.endpoints';
import { map, Observable, tap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { Perform } from '@shared/util/perform.class';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class HalfdaysService {
  constructor(private http: SoeHttpClient) {}

  // Cached data
  progressService = inject(ProgressService);
  performHalfdayGrid = new Perform<ITimeHalfdayGridDTO[]>(this.progressService);

  getGrid(id?: number): Observable<ITimeHalfdayGridDTO[]> {
    return this.http.get<ITimeHalfdayGridDTO[]>(getHalfdaysGrid(id)).pipe(
      tap(x => {
        this.performHalfdayGrid.data = x;
      })
    );
  }

  get(id: number): Observable<ITimeHalfdayEditDTO> {
    return this.http.get<ITimeHalfdayEditDTO>(getHalfday(id));
  }

  save(model: ITimeHalfdayEditDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveHalfday(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteHalfday(id));
  }

  getHalfdayTypesDict(addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getHalfDayTypesDict(addEmptyRow));
  }

  getDayTypesByCompanyDict(
    addEmptyRow: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getDayTypesByCompanyDict(addEmptyRow)
    );
  }

  getTimeCodeBreaks(addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getTimeCodeBrakeDict(addEmptyRow)
    );
  }

  // Modal Methods
  onAddHalfDay(id: number): Observable<BackendResponse> {
    const model = { id: id };
    return this.http
      .post<any>(onAddHalfDay(), model)
      .pipe(map(response => response.result));
  }

  onUpdateHalfDay(
    halfdayId: number,
    dayTypeId: number
  ): Observable<BackendResponse> {
    return this.http
      .post<any>(onUpdateHalfDay(), {
        id1: halfdayId,
        id2: dayTypeId,
      })
      .pipe(map(response => response.result));
  }

  onDeleteHalfDay(id: number): Observable<BackendResponse> {
    return this.http
      .post<any>(onDeleteHalfDay(), { id: id })
      .pipe(map(response => response.result));
  }
}
