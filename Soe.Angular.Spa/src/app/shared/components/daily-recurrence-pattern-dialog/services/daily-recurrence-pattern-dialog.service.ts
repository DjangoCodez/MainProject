import { inject, Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { TermGroup } from '@shared/models/generated-interfaces/Enumerations';
import { ProgressService } from '@shared/services/progress/progress.service'
import { SoeHttpClient } from '@shared/services/http.service';
import { getTermGroupContent } from '@shared/services/generated-service-endpoints/core/Term.endpoints';
import { getHolidayTypesDict } from '@shared/services/generated-service-endpoints/time/Holiday.endpoints';
import { Perform } from '@shared/util/perform.class';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class DailyRecurrencePatternDialogService {
  constructor(private http: SoeHttpClient) {}

  // Cached data
  progressService = inject(ProgressService);
  performTypes = new Perform<SmallGenericType[]>(this.progressService);
  performHolidayTypes = new Perform<SmallGenericType[]>(this.progressService);
  performWeekendIndexes = new Perform<SmallGenericType[]>(this.progressService);
  performRangeTypes = new Perform<SmallGenericType[]>(this.progressService);

  getTypes(): Observable<SmallGenericType[]> {
    return this.http
      .get<
        SmallGenericType[]
      >(getTermGroupContent(TermGroup.DailyRecurrencePatternType, false, false, false))
      .pipe(
        tap(x => {
          this.performTypes.data = x;
        })
      );
  }

  getHolidayTypes(): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getHolidayTypesDict()).pipe(
      tap(x => {
        this.performHolidayTypes.data = x;
      })
    );
  }

  getWeekendIndexes(): Observable<SmallGenericType[]> {
    return this.http
      .get<
        SmallGenericType[]
      >(getTermGroupContent(TermGroup.DailyRecurrencePatternWeekIndex, false, false, true))
      .pipe(
        tap(x => {
          this.performWeekendIndexes.data = x;
        })
      );
  }

  getRangeTypes(): Observable<SmallGenericType[]> {
    return this.http
      .get<
        SmallGenericType[]
      >(getTermGroupContent(TermGroup.DailyRecurrenceRangeType, false, false, false))
      .pipe(
        tap(x => {
          // Different sort
          this.performRangeTypes.data = [];
          this.performRangeTypes.data.push(x[1]);
          this.performRangeTypes.data.push(x[0]);
          this.performRangeTypes.data.push(x[2]);
        })
      );
  }
}
