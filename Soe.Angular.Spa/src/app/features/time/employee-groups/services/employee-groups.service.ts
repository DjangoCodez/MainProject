import { inject, Injectable } from '@angular/core';
import { DayTypesService } from '@features/time/day-types/services/day-types.service';
import { TimeDeviationCausesService } from '@features/time/time-deviation-causes/services/time-deviation-causes.service';
import { TimeScheduleTypeService } from '@features/time/time-schedule-type/services/time-schedule-type.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  SoeModule,
  TermGroup,
  TermGroup_AttestEntity,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IEmployeeGroupDTO,
  IEmployeeGroupGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getAttestEntitiesGenericList,
  getAttestStatesGenericList,
} from '@shared/services/generated-service-endpoints/manage/AttestState.endpoints';
import { getAttestTransitionsDict } from '@shared/services/generated-service-endpoints/manage/AttestTransition.endpoints';
import {
  deleteEmployeeGroup,
  getEmployeeGroup,
  getEmployeeGroupsDict,
  getEmployeeGroupsGrid,
  getTimeAccumulatorsDict,
  saveEmployeeGroup,
} from '@shared/services/generated-service-endpoints/time/EmployeeGroup.endpoints';
import { Perform } from '@shared/util/perform.class';
import { Observable, tap } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class EmployeeGroupsService {
  constructor(private http: SoeHttpClient) {}

  coreService = inject(CoreService);
  daytypeService = inject(DayTypesService);
  timeDeviationCausesService = inject(TimeDeviationCausesService);
  scheduleTypeService = inject(TimeScheduleTypeService);
  progressService = inject(ProgressService);

  performTimeReportTypes = new Perform<SmallGenericType[]>(
    this.progressService
  );

  performDeviationCauses = new Perform<SmallGenericType[]>(
    this.progressService
  );

  performDeviationCausesAbsence = new Perform<SmallGenericType[]>(
    this.progressService
  );

  performWeekDays = new Perform<SmallGenericType[]>(this.progressService);

  performQualifyingDayCalculationRule = new Perform<SmallGenericType[]>(
    this.progressService
  );

  performTimeWorkReductionCalculationRule = new Perform<SmallGenericType[]>(
    this.progressService
  );

  performTimeCodes = new Perform<SmallGenericType[]>(this.progressService);

  getGrid(id?: number): Observable<IEmployeeGroupGridDTO[]> {
    return this.http.get<IEmployeeGroupGridDTO[]>(getEmployeeGroupsGrid(id));
  }

  get(id: number): Observable<IEmployeeGroupDTO> {
    return this.http.get<IEmployeeGroupDTO>(
      getEmployeeGroup(
        id,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      )
    );
  }

  save(model: IEmployeeGroupDTO): Observable<any> {
    return this.http.post<IEmployeeGroupDTO>(saveEmployeeGroup(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteEmployeeGroup(id));
  }

  getEmployeeGroupsDict(addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getEmployeeGroupsDict(addEmptyRow)
    );
  }

  getTimeReportTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.TimeReportType, false, false)
      .pipe(tap(x => (this.performTimeReportTypes.data = x)));
  }

  getTimeDeviationCausesDict(
    removeAbsence: boolean = false,
    removePresence: boolean = false,
    addEmptyRow: boolean = false
  ): Observable<SmallGenericType[]> {
    return this.timeDeviationCausesService
      .getTimeDeviationCausesDict(addEmptyRow, removeAbsence, removePresence)
      .pipe(tap(x => (this.performDeviationCauses.data = x)));
  }

  getTimeDeviationCausesAbsenceDict(
    addEmptyRow: boolean = false
  ): Observable<SmallGenericType[]> {
    return this.timeDeviationCausesService
      .getTimeDeviationCausesAbsenceDict(addEmptyRow)
      .pipe(tap(x => (this.performDeviationCausesAbsence.data = x)));
  }

  getDaysOfWeek(addEmptyRow: boolean = false): Observable<SmallGenericType[]> {
    return this.daytypeService
      .getDaysOfWeek(addEmptyRow)
      .pipe(tap(days => (this.performWeekDays.data = days)));
  }

  getQualifyingDayCalculationRule(
    addEmptyRow: boolean,
    skipUnknown: boolean
  ): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.QualifyingDayCalculationRule,
        addEmptyRow,
        skipUnknown
      )
      .pipe(tap(x => (this.performQualifyingDayCalculationRule.data = x)));
  }

  getTimeWorkReductionCalculationRule(
    addEmptyRow: boolean,
    skipUnknown: boolean
  ): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeWorkReductionCalculationRule,
        addEmptyRow,
        skipUnknown
      )
      .pipe(tap(x => (this.performTimeWorkReductionCalculationRule.data = x)));
  }

  getTimeCodesDict(
    addEmptyRow: boolean = true,
    concatCodeAndName: boolean = false,
    includeType: boolean = true
  ): Observable<SmallGenericType[]> {
    return this.timeDeviationCausesService
      .getTimeCodesDict(addEmptyRow, concatCodeAndName, includeType)
      .pipe(tap(x => (this.performTimeCodes.data = x)));
  }

  getTimeAccumulatorsDict(
    addEmptyRow: boolean,
    includeVacationBalance: boolean,
    includeTimeAccountBalance: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getTimeAccumulatorsDict(
        addEmptyRow,
        includeVacationBalance,
        includeTimeAccountBalance
      )
    );
  }

  getAttestStates(): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getAttestStatesGenericList(
        TermGroup_AttestEntity.PayrollTime,
        SoeModule.Time,
        true,
        false
      )
    );
  }

  getAttestTransitionsDict(
    entity: TermGroup_AttestEntity
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getAttestTransitionsDict(entity, SoeModule.Time, false)
    );
  }

  getAttestEntitiesGenericList(
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getAttestEntitiesGenericList(addEmptyRow, true, SoeModule.Time)
    );
  }
}
