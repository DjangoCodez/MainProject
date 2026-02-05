import { inject, Injectable } from '@angular/core';
import {
  ITimeScheduleTaskDTO,
  ITimeScheduleTaskGeneratedNeedDTO,
  ITimeScheduleTaskGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ProgressService } from '@shared/services/progress/progress.service';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteTimeScheduleTask,
  getRecurrenceDescription,
  getTimeScheduleTask,
  getTimeScheduleTasksGrid,
  getTimeScheduleTaskTypesDict,
  saveTimeScheduleTask,
} from '@shared/services/generated-service-endpoints/time/TimeScheduleTask.endpoints';
import { Observable, tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import {
  deleteGeneratedNeeds,
  getTimeScheduleTaskGeneratedNeeds,
} from '@shared/services/generated-service-endpoints/time/StaffingNeeds.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class TimeScheduleTasksService {
  constructor(private http: SoeHttpClient) {}

  // Cached data
  progressService = inject(ProgressService);
  performTaskTypes = new Perform<SmallGenericType[]>(this.progressService);

  getGrid(id?: number): Observable<ITimeScheduleTaskGridDTO[]> {
    return this.http
      .get<ITimeScheduleTaskGridDTO[]>(getTimeScheduleTasksGrid(id))
      .pipe(
        tap(tasks => {
          // Set shiftTypeId to 0 on tasks without a shift type
          // Will be translated to 'Not specified' in the grid
          tasks
            .filter(task => !task.shiftTypeId)
            .map(t => {
              t.shiftTypeId = 0;
            });

          // Format start/end
          // Can be dates or text
          tasks.map(task => {
            if (
              task.recurrenceStartsOnDescription &&
              DateUtil.isValidDateOrString(task.recurrenceStartsOnDescription)
            ) {
              task.recurrenceStartsOnDescription = new Date(
                task.recurrenceStartsOnDescription
              ).toFormattedDate();
            }
            if (
              task.recurrenceEndsOnDescription &&
              DateUtil.isValidDateOrString(task.recurrenceEndsOnDescription)
            ) {
              task.recurrenceEndsOnDescription = new Date(
                task.recurrenceEndsOnDescription
              ).toFormattedDate();
            }
          });
        })
      );
  }

  get(
    id: number,
    loadAccounts: boolean,
    loadExcludedDates: boolean,
    loadAccountHierarchyAccount: boolean
  ): Observable<ITimeScheduleTaskDTO> {
    return this.http.get<ITimeScheduleTaskDTO>(
      getTimeScheduleTask(
        id,
        loadAccounts,
        loadExcludedDates,
        loadAccountHierarchyAccount
      )
    );
  }

  save(model: ITimeScheduleTaskDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveTimeScheduleTask(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteTimeScheduleTask(id));
  }

  getTaskTypesDict(addEmptyRow: boolean): Observable<SmallGenericType[]> {
    return this.http
      .get<SmallGenericType[]>(getTimeScheduleTaskTypesDict(addEmptyRow))
      .pipe(
        tap(x => {
          this.performTaskTypes.data = x;
        })
      );
  }

  getRecurrenceDescription(pattern: string): Observable<string> {
    return this.http.get(getRecurrenceDescription(pattern));
  }

  getTimeScheduleTaskGeneratedNeeds(
    timeScheduleTaskId: number
  ): Observable<ITimeScheduleTaskGeneratedNeedDTO[]> {
    return this.http.get(getTimeScheduleTaskGeneratedNeeds(timeScheduleTaskId));
  }

  deleteGeneratedNeeds(
    staffingNeedRowPeriodIds: number[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(deleteGeneratedNeeds(), {
      numbers: staffingNeedRowPeriodIds,
    });
  }
}
