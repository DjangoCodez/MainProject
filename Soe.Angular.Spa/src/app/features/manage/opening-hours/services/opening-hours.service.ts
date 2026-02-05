import { inject, Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service'
import { ProgressService } from '@shared/services/progress/progress.service'
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, tap } from 'rxjs';
import {
  deleteOpeningHours,
  getOpeningHour,
  getOpeningHours,
  getOpeningHoursDict,
  saveOpeningHours,
} from '@shared/services/generated-service-endpoints/manage/OpeningHour.endpoints';
import { TermGroup } from '@shared/models/generated-interfaces/Enumerations';
import {
  IOpeningHoursDTO,
  IOpeningHoursGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { Perform } from '@shared/util/perform.class';

@Injectable({
  providedIn: 'root',
})
export class OpeningHoursService {
  constructor(private http: SoeHttpClient) {}

  // Cached data
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  performWeekDays = new Perform<SmallGenericType[]>(this.progressService);

  getGridAdditionalProps = {
    fromDate: '',
    toDate: '',
  };
  getGrid(
    id?: number,
    additionalProps?: { fromDate: string; toDate: string }
  ): Observable<IOpeningHoursGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IOpeningHoursGridDTO[]>(
      getOpeningHours(
        this.getGridAdditionalProps.fromDate,
        this.getGridAdditionalProps.toDate,
        id
      )
    );
  }

  get(id: number): Observable<IOpeningHoursDTO> {
    return this.http.get<IOpeningHoursDTO>(getOpeningHour(id));
  }

  save(model: IOpeningHoursDTO): Observable<any> {
    return this.http.post<IOpeningHoursDTO>(saveOpeningHours(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteOpeningHours(id));
  }

  getOpeningHoursDict(
    addEmptyRow: boolean,
    includeDateInName: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getOpeningHoursDict(addEmptyRow, includeDateInName)
    );
  }

  getWeekdaysDict(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.StandardDayOfWeek, true, true, true)
      .pipe(
        tap(x => {
          this.performWeekDays.data = x;
        })
      );
  }
}
