import { Injectable } from '@angular/core';
import { IEvaluateWorkRulesActionResult } from '@shared/models/generated-interfaces/EvaluateWorkRuleResultDTO';
import { IAvailableEmployeesDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IGetAvailableEmployeesModel } from '@shared/models/generated-interfaces/TimeModels';
import { IShiftRequestStatusDTO } from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { getAvailableEmployees } from '@shared/services/generated-service-endpoints/time/EmployeeV2.endpoints';
import {
  checkIfTooEarlyToSend,
  getShiftRequestStatus,
} from '@shared/services/generated-service-endpoints/time/SchedulePlanning.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SpShiftRequestService {
  constructor(private http: SoeHttpClient) {}

  checkIfTooEarlyToSendShiftRequest(
    startTime: Date
  ): Observable<IEvaluateWorkRulesActionResult> {
    return this.http.get<IEvaluateWorkRulesActionResult>(
      checkIfTooEarlyToSend(startTime.toDateTimeString())
    );
  }

  getShiftRequestStatus(
    timeScheduleTemplateBlockId: number
  ): Observable<IShiftRequestStatusDTO> {
    return this.http.get<IShiftRequestStatusDTO>(
      getShiftRequestStatus(timeScheduleTemplateBlockId)
    );
  }

  getAvailableEmployees(
    timeScheduleTemplateBlockIds: number[],
    employeeIds: number[],
    filterOnShiftType: boolean,
    filterOnAvailability: boolean,
    filterOnSkills: boolean,
    filterOnWorkRules: boolean,
    filterOnMessageGroupId: number | undefined
  ): Observable<IAvailableEmployeesDTO[]> {
    const model: IGetAvailableEmployeesModel = {
      timeScheduleTemplateBlockIds: timeScheduleTemplateBlockIds,
      employeeIds: employeeIds,
      filterOnShiftType: filterOnShiftType,
      filterOnAvailability: filterOnAvailability,
      filterOnSkills: filterOnSkills,
      filterOnWorkRules: filterOnWorkRules,
      filterOnMessageGroupId: filterOnMessageGroupId,
    };

    return this.http.post<IAvailableEmployeesDTO[]>(
      getAvailableEmployees(),
      model
    );
  }
}
