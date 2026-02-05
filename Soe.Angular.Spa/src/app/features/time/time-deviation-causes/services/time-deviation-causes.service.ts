import { inject, Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { TermGroup } from '@shared/models/generated-interfaces/Enumerations';
import {
  ITimeDeviationCauseDTO,
  ITimeDeviationCauseGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { SoeHttpClient } from '@shared/services/http.service';
import { getTimeCodesDict } from '@shared/services/generated-service-endpoints/time/TimeCode.endpoints';
import {
  deleteTimeDeviationCause,
  getTimeDeviationCause,
  getTimeDeviationCausesAbsenceDict,
  getTimeDeviationCausesAbsenceFromEmployeeId,
  getTimeDeviationCausesDict,
  getTimeDeviationCausesGrid,
  saveTimeDeviationCauses,
} from '@shared/services/generated-service-endpoints/time/TimeDeviationCause.endpoints';
import { Perform } from '@shared/util/perform.class';
import { Observable, tap } from 'rxjs';
import { DateUtil } from '@shared/util/date-util';

@Injectable({
  providedIn: 'root',
})
export class TimeDeviationCausesService {
  constructor(private http: SoeHttpClient) {}

  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  performTypes = new Perform<SmallGenericType[]>(this.progressService);

  getGrid(id?: number): Observable<ITimeDeviationCauseGridDTO[]> {
    return this.http.get<ITimeDeviationCauseGridDTO[]>(
      getTimeDeviationCausesGrid(id)
    );
  }

  get(id: number): Observable<ITimeDeviationCauseDTO> {
    return this.http.get<ITimeDeviationCauseDTO>(getTimeDeviationCause(id));
  }

  save(model: ITimeDeviationCauseDTO): Observable<any> {
    return this.http.post<ITimeDeviationCauseDTO>(
      saveTimeDeviationCauses(),
      model
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete<ITimeDeviationCauseDTO>(
      deleteTimeDeviationCause(id)
    );
  }

  getTimeDeviationCausesDict(
    addEmptyRow: boolean,
    removeAbsence: boolean,
    removePresence: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get(
      getTimeDeviationCausesDict(addEmptyRow, removeAbsence, removePresence)
    );
  }

  getTimeDeviationCausesAbsenceDict(
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get(getTimeDeviationCausesAbsenceDict(addEmptyRow));
  }

  getTimeCodesDict(
    addEmptyRow: boolean,
    concatCodeAndName: boolean,
    includeType: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get(
      getTimeCodesDict(addEmptyRow, concatCodeAndName, includeType)
    );
  }

  getTypesDict(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.TimeDeviationCauseType, false, true)
      .pipe(
        tap(x => {
          this.performTypes.data = x;
        })
      );
  }

  getTimeDeviationCausesAbsenceFromEmployeeId(
    employeeId: number,
    date: Date,
    onlyUseInTimeTerminal: boolean
  ): Observable<ITimeDeviationCauseDTO[]> {
    return this.http.get<ITimeDeviationCauseDTO[]>(
      getTimeDeviationCausesAbsenceFromEmployeeId(
        employeeId,
        DateUtil.toDateString(date),
        onlyUseInTimeTerminal
      )
    );
  }
}
