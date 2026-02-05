import { inject, Injectable } from '@angular/core';
import {
  DragShiftAction,
  TermGroup,
  TermGroup_TimeScheduleTemplateBlockType,
  TimeSchedulePlanningDisplayMode,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IDeleteShiftsModel,
  IDragShiftModel,
  IDragShiftsModel,
  IGetShiftsModel,
  ISaveShiftsModelV2,
  ISplitShiftModelV2,
} from '@shared/models/generated-interfaces/TimeModels';
import {
  IEmployeeListDTO,
  IShiftDTO,
} from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { CoreService } from '@shared/services/core.service';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getEmployeeAvailability,
  getEmployeeListForPlanning,
  getHiddenEmployeeId,
} from '@shared/services/generated-service-endpoints/time/EmployeeV2.endpoints';
import {
  deleteShifts,
  dragShift,
  dragShifts,
  getLinkedShifts,
  getShifts,
  getShiftsForDay,
  saveShifts,
  splitShift,
} from '@shared/services/generated-service-endpoints/time/SchedulePlanning.endpoints';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { Observable, tap } from 'rxjs';
import { PlanningShiftDTO } from '../models/shift.model';
import { ITimeCodeBreakSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { getTimeCodeBreaks } from '@shared/services/generated-service-endpoints/time/TimeCode.endpoints';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class SchedulePlanningService {
  private readonly coreService = inject(CoreService);

  currentEmployeeId: number = 0;

  startsOnItems: ISmallGenericType[] = [];
  timeCodeBreaks: ITimeCodeBreakSmallDTO[] = [];
  hiddenEmployeeId = 0;

  constructor(private http: SoeHttpClient) {
    this.currentEmployeeId = SoeConfigUtil.employeeId;

    this.getBelongsToItems().subscribe();
  }

  getBelongsToItems(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeSchedulePlanningShiftStartsOnDay,
        false,
        false,
        true
      )
      .pipe(
        tap(items => {
          this.startsOnItems = items;
        })
      );
  }

  getTimeCodeBreaks(): Observable<ITimeCodeBreakSmallDTO[]> {
    return this.http
      .get<ITimeCodeBreakSmallDTO[]>(getTimeCodeBreaks(true))
      .pipe(
        tap(x => {
          this.timeCodeBreaks = x;
        })
      );
  }

  getTimeCodeBreak(
    timeCodeId: number,
    excludeEmpty: boolean
  ): ITimeCodeBreakSmallDTO | undefined {
    // If excludeEmpty is true, we do not return the empty time code (id=0)
    return this.timeCodeBreaks.find(
      t => t.timeCodeId === timeCodeId && (t.timeCodeId !== 0 || !excludeEmpty)
    );
  }

  getTimeCodeBreakFromLength(
    length: number
  ): ITimeCodeBreakSmallDTO | undefined {
    return this.timeCodeBreaks.find(t => t.defaultMinutes === length);
  }

  getHiddenEmployeeId(): Observable<number> {
    return this.http.get<number>(getHiddenEmployeeId()).pipe(
      tap(x => {
        this.hiddenEmployeeId = x;
      })
    );
  }

  getEmployees(
    employeeIds: number[],
    getHidden: boolean,
    getInacive: boolean,
    loadSkills: boolean,
    loadAvailability: boolean,
    dateFrom: Date,
    dateTo: Date,
    includeSecondaryCategoriesOrAccounts: boolean,
    displayMode: TimeSchedulePlanningDisplayMode
  ): Observable<IEmployeeListDTO[]> {
    return this.http.get<IEmployeeListDTO[]>(
      getEmployeeListForPlanning(
        employeeIds?.length > 0 ? employeeIds.join(',') : 'null',
        getHidden,
        getInacive,
        loadSkills,
        loadAvailability,
        dateFrom.toDateTimeString(),
        dateTo.toDateTimeString(),
        includeSecondaryCategoriesOrAccounts,
        displayMode
      )
    );
  }

  getEmployeeAvailability(
    employeeIds: number[]
  ): Observable<IEmployeeListDTO[]> {
    return this.http.post(getEmployeeAvailability(), { numbers: employeeIds });
  }

  getShifts(model: IGetShiftsModel): Observable<IShiftDTO[]> {
    return this.http.post<IShiftDTO[]>(getShifts(), model);
  }

  getShiftsForDay(
    employeeId: number,
    date: Date,
    blockTypes: TermGroup_TimeScheduleTemplateBlockType[],
    includeBreaks: boolean,
    includeGrossNetAndCost: boolean,
    link: string,
    loadQueue: boolean,
    loadDeviationCause: boolean,
    loadTasks: boolean,
    includePreliminary: boolean,
    timeScheduleScenarioHeadId?: number
  ): Observable<IShiftDTO[]> {
    return this.http.get<IShiftDTO[]>(
      getShiftsForDay(
        employeeId,
        date.toDateTimeString(),
        blockTypes.join(),
        includeBreaks,
        includeGrossNetAndCost,
        link,
        loadQueue,
        loadDeviationCause,
        loadTasks,
        includePreliminary,
        timeScheduleScenarioHeadId ?? 0
      )
    );
  }

  getLinkedShifts(
    timeScheduleTemplateBlockId: number
  ): Observable<IShiftDTO[]> {
    return this.http.get<IShiftDTO[]>(
      getLinkedShifts(timeScheduleTemplateBlockId)
    );
  }

  saveShifts(
    source: string,
    shifts: PlanningShiftDTO[],
    updateBreaks: boolean,
    skipXEMailOnChanges: boolean,
    adjustTasks: boolean,
    minutesMoved: number,
    timeScheduleScenarioHeadId?: number
  ) {
    const model: ISaveShiftsModelV2 = {
      source: source,
      shifts: shifts,
      updateBreaks: updateBreaks,
      skipXEMailOnChanges: skipXEMailOnChanges,
      adjustTasks: adjustTasks,
      minutesMoved: minutesMoved,
      timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
    };

    return this.http.post<BackendResponse>(saveShifts(), model);
  }

  deleteShifts(
    shiftIds: number[],
    skipXEMailOnChanges: boolean,
    timeScheduleScenarioHeadId?: number,
    includedOnDutyShiftIds: number[] = []
  ): Observable<BackendResponse> {
    const model: IDeleteShiftsModel = {
      shiftIds: shiftIds,
      skipXEMailOnChanges: skipXEMailOnChanges,
      timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
      includedOnDutyShiftIds: includedOnDutyShiftIds,
    };

    return this.http.post<BackendResponse>(deleteShifts(), model);
  }

  dragShift(
    action: DragShiftAction,
    sourceShiftId: number,
    targetShiftId: number,
    start: Date,
    end: Date,
    employeeId: number,
    targetLink: string,
    updateLinkOnTarget: boolean,
    timeDeviationCauseId: number,
    employeeChildId: number,
    wholeDayAbsence: boolean,
    skipXEMailOnChanges: boolean,
    copyTaskWithShift: boolean,
    isStandByView: boolean,
    timeScheduleScenarioHeadId?: number,
    standbyCycleWeek?: number,
    standbyCycleDateFrom?: Date,
    standbyCycleDateTo?: Date,
    includeOnDutyShifts?: boolean,
    includedOnDutyShiftIds?: number[]
  ): Observable<BackendResponse> {
    const model: IDragShiftModel = {
      action: action,
      sourceShiftId: sourceShiftId,
      targetShiftId: targetShiftId,
      start: start,
      end: end,
      employeeId: employeeId,
      targetLink: targetLink,
      updateLinkOnTarget: updateLinkOnTarget,
      timeDeviationCauseId: timeDeviationCauseId,
      employeeChildId: employeeChildId,
      wholeDayAbsence: wholeDayAbsence,
      skipXEMailOnChanges: skipXEMailOnChanges,
      copyTaskWithShift: copyTaskWithShift,
      isStandByView: isStandByView,
      timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
      standbyCycleWeek: standbyCycleWeek,
      standbyCycleDateFrom: standbyCycleDateFrom,
      standbyCycleDateTo: standbyCycleDateTo,
      includeOnDutyShifts: includeOnDutyShifts ?? false,
      includedOnDutyShiftIds: includedOnDutyShiftIds ?? [],
    };
    return this.http.post<BackendResponse>(dragShift(), model);
  }

  dragShifts(
    action: DragShiftAction,
    sourceShiftIds: number[],
    offsetDays: number,
    targetEmployeeId: number,
    skipXEMailOnChanges: boolean,
    copyTaskWithShift: boolean,
    isStandByView: boolean,
    timeScheduleScenarioHeadId?: number,
    standbyCycleWeek?: number,
    standbyCycleDateFrom?: Date,
    standbyCycleDateTo?: Date,
    includeOnDutyShifts?: boolean,
    includedOnDutyShiftIds?: number[]
  ): Observable<BackendResponse> {
    const model: IDragShiftsModel = {
      action: action,
      sourceShiftIds: sourceShiftIds,
      offsetDays: offsetDays,
      targetEmployeeId: targetEmployeeId,
      skipXEMailOnChanges: skipXEMailOnChanges,
      copyTaskWithShift: copyTaskWithShift,
      isStandByView: isStandByView,
      timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
      standbyCycleWeek: standbyCycleWeek,
      standbyCycleDateFrom: standbyCycleDateFrom,
      standbyCycleDateTo: standbyCycleDateTo,
      includeOnDutyShifts: includeOnDutyShifts ?? false,
      includedOnDutyShiftIds: includedOnDutyShiftIds ?? [],
    };
    return this.http.post<BackendResponse>(dragShifts(), model);
  }

  splitShift(
    shift: PlanningShiftDTO,
    splitTime: Date,
    employeeId1: number,
    employeeId2: number,
    keepShiftsTogether: boolean,
    isPersonalScheduleTemplate: boolean,
    skipXEMailOnChanges: boolean,
    timeScheduleScenarioHeadId?: number
  ): Observable<BackendResponse> {
    const model: ISplitShiftModelV2 = {
      shift,
      splitTime,
      employeeId1,
      employeeId2,
      keepShiftsTogether,
      isPersonalScheduleTemplate,
      skipXEMailOnChanges,
      timeScheduleScenarioHeadId,
    };
    return this.http.post<BackendResponse>(splitShift(), model);
  }
}
