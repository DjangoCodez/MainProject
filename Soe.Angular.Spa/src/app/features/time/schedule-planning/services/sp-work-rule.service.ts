import { inject, Injectable } from '@angular/core';
import { IEvaluateWorkRulesActionResult } from '@shared/models/generated-interfaces/EvaluateWorkRuleResultDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  evaluateDragShiftAgainstWorkRules,
  evaluateDragShiftsAgainstWorkRules,
  evaluatePlannedShiftsAgainstWorkRules,
  evaluateSplitShiftAgainstWorkRules,
} from '@shared/services/generated-service-endpoints/time/SchedulePlanning.endpoints';
import { PlanningShiftDTO } from '../models/shift.model';
import { catchError, map, Observable, of, switchMap, take, tap } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import {
  DragShiftAction,
  SoeScheduleWorkRules,
  TermGroup_ShiftHistoryType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IEvaluateWorkRulesDragModel,
  IEvaluateWorkRulesDragMultipleModel,
  IEvaluateWorkRulesModelV2,
  IEvaluateWorkRulesSplitModelV2,
} from '@shared/models/generated-interfaces/TimeModels';
import { IActionResult } from '@shared/models/generated-interfaces/ActionResult';
import { ToasterService } from '@ui/toaster/services/toaster.service';

@Injectable({
  providedIn: 'root',
})
export class SpWorkRuleService {
  private readonly messageboxService = inject(MessageboxService);
  private readonly toasterService = inject(ToasterService);
  private readonly translate = inject(TranslateService);

  constructor(private http: SoeHttpClient) {}

  evaluateDragShiftAgainstWorkRules(
    action: DragShiftAction,
    sourceShiftId: number,
    targetShiftId: number,
    start: Date,
    end: Date,
    employeeId: number,
    isPersonalScheduleTemplate: boolean,
    wholeDayAbsence: boolean,
    rules: SoeScheduleWorkRules[] | null,
    isStandByView: boolean,
    timeScheduleScenarioHeadId?: number,
    standbyCycleWeek?: number,
    standbyCycleDateFrom?: Date,
    standbyCycleDateTo?: Date,
    fromQueue?: boolean,
    planningPeriodStartDate?: Date,
    planningPeriodStopDate?: Date
  ): Observable<IEvaluateWorkRulesActionResult> {
    return this.http
      .post<IEvaluateWorkRulesActionResult>(
        evaluateDragShiftAgainstWorkRules(),
        {
          action,
          sourceShiftId,
          targetShiftId,
          start,
          end,
          employeeId,
          isPersonalScheduleTemplate,
          wholeDayAbsence,
          rules,
          isStandByView,
          timeScheduleScenarioHeadId,
          standbyCycleWeek,
          standbyCycleDateFrom,
          standbyCycleDateTo,
          fromQueue,
          planningPeriodStartDate,
          planningPeriodStopDate,
        } as IEvaluateWorkRulesDragModel
      )
      .pipe(
        catchError(error => {
          return this.createDefaultErrorMessage(error);
        })
      );
  }

  evaluateDragShiftsAgainstWorkRules(
    action: DragShiftAction,
    employeeId: number,
    sourceShiftIds: number[],
    rules: SoeScheduleWorkRules[] | null,
    offsetDays = 0,
    isPersonalScheduleTemplate = false,
    isStandByView = false,
    timeScheduleScenarioHeadId?: number,
    standbyCycleWeek?: number,
    standbyCycleDateFrom?: Date,
    standbyCycleDateTo?: Date,
    planningPeriodStartDate?: Date,
    planningPeriodStopDate?: Date
  ): Observable<IEvaluateWorkRulesActionResult> {
    return this.http
      .post<IEvaluateWorkRulesActionResult>(
        evaluateDragShiftsAgainstWorkRules(),
        {
          action,
          employeeId,
          sourceShiftIds,
          rules,
          offsetDays,
          isPersonalScheduleTemplate,
          isStandByView,
          timeScheduleScenarioHeadId,
          standbyCycleWeek,
          standbyCycleDateFrom,
          standbyCycleDateTo,
          planningPeriodStartDate,
          planningPeriodStopDate,
        } as IEvaluateWorkRulesDragMultipleModel
      )
      .pipe(
        catchError(error => {
          return this.createDefaultErrorMessage(error);
        })
      );
  }

  evaluatePlannedShiftsAgainstWorkRules(
    shifts: PlanningShiftDTO[],
    rules: SoeScheduleWorkRules[] | null,
    employeeId: number,
    isPersonalScheduleTemplate: boolean,
    timeScheduleScenarioHeadId?: number,
    planningPeriodStartDate?: Date,
    planningPeriodStopDate?: Date
  ): Observable<IEvaluateWorkRulesActionResult> {
    return this.http
      .post<IEvaluateWorkRulesActionResult>(
        evaluatePlannedShiftsAgainstWorkRules(),
        {
          shifts,
          rules,
          employeeId,
          isPersonalScheduleTemplate,
          timeScheduleScenarioHeadId,
          planningPeriodStartDate,
          planningPeriodStopDate,
        } as IEvaluateWorkRulesModelV2
      )
      .pipe(
        catchError(error => {
          return this.createDefaultErrorMessage(error);
        })
      );
  }

  evaluateSplitShiftAgainstWorkRules(
    shift: PlanningShiftDTO,
    splitTime: Date,
    employeeId1: number,
    employeeId2: number,
    keepShiftsTogether: boolean,
    isPersonalScheduleTemplate: boolean,
    timeScheduleScenarioHeadId?: number,
    planningPeriodStartDate?: Date,
    planningPeriodStopDate?: Date
  ): Observable<IEvaluateWorkRulesActionResult> {
    return this.http
      .post<IEvaluateWorkRulesActionResult>(
        evaluateSplitShiftAgainstWorkRules(),
        {
          shift,
          splitTime,
          employeeId1,
          employeeId2,
          keepShiftsTogether,
          isPersonalScheduleTemplate,
          timeScheduleScenarioHeadId,
          planningPeriodStartDate,
          planningPeriodStopDate,
        } as IEvaluateWorkRulesSplitModelV2
      )
      .pipe(
        catchError(error => {
          return this.createDefaultErrorMessage(error);
        })
      );
  }

  showValidateWorkRulesResult(
    action: TermGroup_ShiftHistoryType,
    result: IEvaluateWorkRulesActionResult,
    employeeId: number,
    showCancelAll: boolean = false,
    dialogTitleKey: string = ''
  ): Observable<boolean> {
    // TODO: New term
    this.toasterService.info('Arbetstidsregler validerade');

    if (result.result.success) {
      if (result.allRulesSucceded) {
        // Success
        return of(true);
      } else {
        // Warning
        const keys: string[] = [
          'time.schedule.planning.evaluateworkrules.warning',
          'core.continue',
        ];
        if (dialogTitleKey) keys.push(dialogTitleKey);

        return this.translate.get(keys).pipe(
          switchMap(terms => {
            let text: string = result.errorMessage ? result.errorMessage : '';
            if (result.canUserOverrideRuleViolation)
              text += '\n' + terms['core.continue'];

            // TODO: Implement cancel all button in messagebox
            // if (showCancelAll) config.showButtonCancelAll = true;

            return this.messageboxService
              .warning(
                dialogTitleKey
                  ? terms[dialogTitleKey]
                  : terms['time.schedule.planning.evaluateworkrules.warning'],
                text,
                {
                  buttons: result.canUserOverrideRuleViolation
                    ? 'okCancel'
                    : 'ok',
                }
              )
              .afterClosed()
              .pipe(
                map((response: IMessageboxComponentResponse) => {
                  if (response.result) {
                    if (result.canUserOverrideRuleViolation) {
                      result.evaluatedRuleResults
                        .filter(r => r.success === false)
                        .forEach(r => {
                          r.action = action;
                        });

                      // TODO: Implement logging of override
                      // Log override warning
                      // if (employeeId) {
                      //   this.coreService.saveEvaluateAllWorkRulesByPass(
                      //     result,
                      //     employeeId
                      //   );
                      // }
                    }
                    return result.canUserOverrideRuleViolation;
                  } else {
                    return false;
                  }
                })
              );
          })
        );
      }
    } else {
      // Failure
      return this.translate
        .get(
          dialogTitleKey
            ? dialogTitleKey
            : 'time.schedule.planning.evaluateworkrules.failed'
        )
        .pipe(
          tap(term => {
            this.messageboxService.error(term, result.result.errorMessage, {
              type: 'forbidden',
            });
          }),
          map(() => false)
        );
    }
  }

  private createDefaultErrorMessage(
    error: any
  ): Observable<IEvaluateWorkRulesActionResult> {
    const result: IEvaluateWorkRulesActionResult = {
      result: {
        success: false,
        errorMessage: error?.message || 'Server error',
      } as IActionResult,
      allRulesSucceded: false,
      canUserOverrideRuleViolation: false,
      errorMessage: error?.message || 'Server error',
      evaluatedRuleResults: [],
    };
    return of(result);
  }
}
