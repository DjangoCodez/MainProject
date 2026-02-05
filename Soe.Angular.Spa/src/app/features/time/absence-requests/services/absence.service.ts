import { computed, inject, Injectable, signal } from '@angular/core';
import {
  EmployeeListEmploymentDTO,
  PlanningEmployeeDTO,
} from '@features/time/schedule-planning/models/employee.model';
import { TimeDeviationCausesService } from '@features/time/time-deviation-causes/services/time-deviation-causes.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import {
  SoeScheduleWorkRules,
  TermGroup_TimeScheduleTemplateBlockShiftUserStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { IEvaluateWorkRulesActionResult } from '@shared/models/generated-interfaces/EvaluateWorkRuleResultDTO';
import {
  IEmployeeRequestDTO,
  IEmployeeRequestGridDTO,
  IExtendedAbsenceSettingDTO,
  IShiftHistoryDTO,
  ITimeDeviationCauseDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  IEmployeeListDTO,
  IShiftDTO,
} from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  deleteAbsenceRequest,
  evaluateAbsenceRequestPlannedShiftsAgainstWorkRules,
  getAbsenceAffectedShifts,
  getAbsenceRequest,
  getAbsenceRequestAffectedShifts,
  getAbsenceRequestGrid,
  getAbsenceRequestHistory,
  getEmployeesForAbsencePlanning,
  getShiftsForQuickAbsence,
  performAbsencePlanningAction,
  saveAbsenceRequest,
} from '@shared/services/generated-service-endpoints/time/AbsenceRequest.endpoints';
import { getEmployeeChildsSmall } from '@shared/services/generated-service-endpoints/time/EmployeeV2.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { map, Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AbsenceService {
  private readonly http = inject(SoeHttpClient);

  // Services
  private readonly progressService = inject(ProgressService);
  private readonly timeDeviationCausesService = inject(
    TimeDeviationCausesService
  );

  // Data
  public performAbsenceDeviationCauses = new Perform<SmallGenericType[]>(
    this.progressService
  );

  //Flags
  public loadPreliminary = signal(true);
  public loadDefinitive = signal(true);

  private readonly getGridAdditionalProps = computed(() => ({
    employeeId: 0,
    loadPreliminary: this.loadPreliminary(),
    loadDefinitive: this.loadDefinitive(),
  }));

  readonly NO_REPLACEMENT_EMPLOYEEID = -1;

  // #region CRUD
  getGrid(
    id?: number,
    additionalProps?: {
      employeeId: number;
      loadPreliminary: boolean;
      loadDefinitive: boolean;
    }
  ): Observable<IEmployeeRequestGridDTO[]> {
    const baseProps = this.getGridAdditionalProps();
    const props = { ...baseProps, ...additionalProps };
    return this.http.get<IEmployeeRequestGridDTO[]>(
      getAbsenceRequestGrid(
        props.employeeId,
        props.loadPreliminary,
        props.loadDefinitive,
        id
      )
    );
  }

  get(id: number): Observable<IEmployeeRequestDTO> {
    return this.http.get<IEmployeeRequestDTO>(getAbsenceRequest(id));
  }

  save(model: IEmployeeRequestDTO): Observable<BackendResponse> {
    return this.http.post(saveAbsenceRequest(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteAbsenceRequest(id));
  }

  // #region DATA
  getTimeDeviationCausesAbsenceDict(
    addEmptyRow: boolean = false
  ): Observable<SmallGenericType[]> {
    return this.timeDeviationCausesService
      .getTimeDeviationCausesAbsenceDict(addEmptyRow)
      .pipe(tap(x => (this.performAbsenceDeviationCauses.data = x)));
  }

  getTimeDeviationCausesAbsenceFromEmployeeId(
    employeeId: number,
    date: Date,
    onlyUseInTimeTerminal: boolean
  ): Observable<ITimeDeviationCauseDTO[]> {
    return this.timeDeviationCausesService.getTimeDeviationCausesAbsenceFromEmployeeId(
      employeeId,
      date,
      onlyUseInTimeTerminal
    );
  }

  getTimeDeviationCausesAbsenceFromEmployeeIdSmall(
    employeeId: number,
    date: Date,
    onlyUseInTimeTerminal: boolean
  ): Observable<SmallGenericType[]> {
    return this.getTimeDeviationCausesAbsenceFromEmployeeId(
      employeeId,
      date,
      onlyUseInTimeTerminal
    ).pipe(
      map(causes =>
        causes.map(cause => {
          return new SmallGenericType(cause.timeDeviationCauseId, cause.name);
        })
      )
    );
  }

  getAbsenceRequestHistory(
    absenceRequestId: number
  ): Observable<IShiftHistoryDTO> {
    return this.http.get(getAbsenceRequestHistory(absenceRequestId));
  }

  // Gets shifts pending absence request
  getAbsenceRequestAffectedShifts(
    request: IEmployeeRequestDTO,
    extendedSettings: IExtendedAbsenceSettingDTO,
    shiftUserStatus: TermGroup_TimeScheduleTemplateBlockShiftUserStatus,
    timeScheduleScenarioHeadId?: number
  ): Observable<IShiftDTO[]> {
    const model = {
      request: request,
      extendedSettings: extendedSettings,
      shiftUserStatus: shiftUserStatus,
      timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
    };
    return this.http.post(getAbsenceRequestAffectedShifts(), model);
  }

  // Get shifts absence request not pedning
  getAbsenceAffectedShifts(
    employeeId: number,
    dateFrom: Date,
    dateTo: Date,
    timeDeviationCauseId: number,
    extendedAbsenceSettings: IExtendedAbsenceSettingDTO,
    includeAlreadyAbsence: boolean,
    timeScheduleScenarioHeadId?: number
  ): Observable<IShiftDTO[]> {
    const model = {
      employeeId: employeeId,
      dateFrom: dateFrom,
      dateTo: dateTo,
      timeDeviationCauseId: timeDeviationCauseId,
      extendedAbsenceSettings: extendedAbsenceSettings,
      timeScheduleScenarioHeadId: timeScheduleScenarioHeadId ?? 0,
      includeAlreadyAbsence: includeAlreadyAbsence,
    };
    return this.http.post(getAbsenceAffectedShifts(), model);
  }

  getEmployeesForAbsencePlanning(
    dateFrom: string | null,
    dateTo: string | null,
    mandatoryEmployeeId: number,
    excludeCurrentUserEmployee: boolean,
    timeScheduleScenarioHeadId?: number
  ): Observable<PlanningEmployeeDTO[]> {
    return this.http
      .get<
        IEmployeeListDTO[]
      >(getEmployeesForAbsencePlanning(dateFrom ?? 'null', dateTo ?? 'null', mandatoryEmployeeId, excludeCurrentUserEmployee, timeScheduleScenarioHeadId ?? 0))
      .pipe(
        // Convert into PlanningEmployeeDTO
        map((data: IEmployeeListDTO[]) => {
          return data.map(item => {
            const obj = new PlanningEmployeeDTO();
            Object.assign(obj, item);

            obj.setEmployeeNrSort();

            // Map employments to correct type
            obj.employments = obj.employments.map(ep => {
              const empl = new EmployeeListEmploymentDTO();
              Object.assign(empl, ep);
              return empl;
            });
            return obj;
          });
        })
      );
  }

  getShiftsForQuickAbsence(params: {
    employeeId: number;
    shiftIds: number[];
    includeLinkedShifts: boolean;
    timeScheduleScenarioHeadId?: number;
  }): Observable<IShiftDTO[]> {
    const model = {
      employeeId: params.employeeId,
      shiftIds: params.shiftIds,
      includeLinkedShifts: params.includeLinkedShifts,
      timeScheduleScenarioHeadId: params.timeScheduleScenarioHeadId ?? 0,
    };
    return this.http.post(getShiftsForQuickAbsence(), model);
  }

  performAbsencePlanningAction(params: {
    employeeRequest: IEmployeeRequestDTO;
    shifts: IShiftDTO[];
    isScheduledAbsence: boolean;
    skipGOMailOnShiftChanges: boolean;
    timeScheduleScenarioHeadId?: number;
  }): Observable<BackendResponse> {
    const model = {
      employeeRequest: params.employeeRequest,
      shifts: params.shifts,
      isScheduledAbsence: params.isScheduledAbsence,
      skipGOMailOnShiftChanges: params.skipGOMailOnShiftChanges,
      timeScheduleScenarioHeadId: params.timeScheduleScenarioHeadId ?? 0,
    };
    return this.http.post(performAbsencePlanningAction(), model);
  }

  evaluateAbsenceRequestPlannedShiftsAgainstWorkRules(params: {
    employeeId: number;
    shifts: IShiftDTO[];
    rules: SoeScheduleWorkRules[] | null;
    timeScheduleScenarioHeadId?: number;
  }): Observable<IEvaluateWorkRulesActionResult> {
    const model = {
      employeeId: params.employeeId,
      shifts: params.shifts,
      rules: params.rules,
      timeScheduleScenarioHeadId: params.timeScheduleScenarioHeadId ?? 0,
    };
    return this.http.post(
      evaluateAbsenceRequestPlannedShiftsAgainstWorkRules(),
      model
    );
  }

  GetEmployeeChildsSmall(
    employeeId: number,
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getEmployeeChildsSmall(employeeId, addEmptyRow)
    );
  }

  //#region Helper Methods
  // getEligibleReplacementEmployees(
  //   employees: PlanningEmployeeDTO[],
  //   currentEmployeeId: number,
  //   dateFrom: Date,
  //   dateTo: Date,
  //   hiddenEmployeeId: number,
  //   onlyNoReplacementAllowed: boolean
  // )

  // getFilteredReplacementEmployees(
  //   employeeList: PlanningEmployeeDTO[],
  //   onlyNoReplacementIsSelectable: boolean,
  //   currentEmployeeId: number,
  //   hiddenEmployeeId: number,
  //   dateFrom: Date,
  //   dateTo: Date
  // ): PlanningEmployeeDTO[] {
  //   return onlyNoReplacementIsSelectable
  //     ? []
  //     : employeeList.filter(emp => {
  //         return (
  //           emp.employeeId !== currentEmployeeId &&
  //           (emp.hasEmployment(dateFrom, dateTo) ||
  //             emp.employeeId === this.NO_REPLACEMENT_EMPLOYEEID ||
  //             emp.employeeId === hiddenEmployeeId)
  //         );
  //       });
  // }

  formatEmployeeListForDisplay(
    employeeList: PlanningEmployeeDTO[],
    hiddenEmployeeId?: number
  ): SmallGenericType[] {
    return employeeList.map(emp => {
      const isSpecialEmployee =
        emp.employeeId === this.NO_REPLACEMENT_EMPLOYEEID ||
        emp.employeeId === hiddenEmployeeId;
      return new SmallGenericType(
        emp.employeeId,
        isSpecialEmployee
          ? emp.name
          : '({0}) {1}'.format(emp.employeeNr, emp.name)
      );
    });
  }

  // return this.service
  //       .getEmployeesForAbsencePlanning(
  //         DateUtil.toDateString(this.dateFrom),
  //         DateUtil.toDateString(this.dateTo),
  //         this.employeeId,
  //         true
  //       )
  //       .pipe(
  //         tap(x => {
  //           this.employeeList = this.onlyNoReplacementIsSelectable()
  //             ? []
  //             : x.filter(emp => {
  //                 return (
  //                   emp.employeeId !== this.employeeId &&
  //                   (emp.hasEmployment(this.dateFrom, this.dateTo) ||
  //                     emp.employeeId === this.service.NO_REPLACEMENT_EMPLOYEEID ||
  //                     emp.employeeId === this.hiddenEmployeeId())
  //                 );
  //               });

  //           this.replaceWithAllEmployees = this.employeeList.map(emp => {
  //             const isSpecialEmployee =
  //               emp.employeeId === this.service.NO_REPLACEMENT_EMPLOYEEID ||
  //               emp.employeeId === this.hiddenEmployeeId();

  //             return new SmallGenericType(
  //               emp.employeeId,
  //               isSpecialEmployee
  //                 ? emp.name
  //                 : '({0}) {1}'.format(emp.employeeNr, emp.name)
  //             );
  //           });
  //         })
  //       );

  //#region Employments
  // getEmploymentFromEmploymentList(
  //   //TODO: Where should this be put
  //   employments: IEmployeeListEmploymentDTO[],
  //   date: Date
  // ): IEmployeeListEmploymentDTO {
  //   // if (!employments) return null
  //   const beginningOfDay = date.beginningOfDay();
  //   const employmentsTempPrimary = employments.filter(
  //     e => e.isTemporaryPrimary === true
  //   );
  //   const employmentsRegular = employments.filter(
  //     e => e.isTemporaryPrimary === false
  //   );

  //   var filteredList: IEmployeeListEmploymentDTO[] = [];

  //   for (let employment of employmentsTempPrimary) {
  //     if (
  //       (!employment.dateFrom ||
  //         employment.dateFrom.beginningOfDay() <= beginningOfDay) &&
  //       (!employment.dateTo || employment.dateTo.endOfDay() >= beginningOfDay)
  //     ) {
  //       filteredList.push(employment);
  //     }
  //   }

  //   if (filteredList.length == 0) {
  //     for (let employment of employmentsRegular) {
  //       if (
  //         (!employment.dateFrom ||
  //           employment.dateFrom.beginningOfDay() <= beginningOfDay) &&
  //         (!employment.dateTo || employment.dateTo.endOfDay() >= beginningOfDay)
  //       ) {
  //         filteredList.push(employment);
  //       }
  //     }
  //   }

  //   function descDateFromSort(
  //     a: IEmployeeListEmploymentDTO,
  //     b: IEmployeeListEmploymentDTO
  //   ) {
  //     if ((a?.dateFrom ?? new Date(0)) < (b?.dateFrom ?? new Date(0))) return 1;
  //     else if ((a?.dateFrom ?? new Date(0)) > (b?.dateFrom ?? new Date(0)))
  //       return -1;
  //     else return 0;
  //   }
  //   return filteredList.sort(descDateFromSort)[0];
  // }
}
