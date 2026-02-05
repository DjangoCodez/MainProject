import { Injectable } from '@angular/core';
import { IActionResult } from '@shared/models/generated-interfaces/ActionResult';
import {
  SoeModule,
  TermGroup_AttestEntity,
  TermGroup_TemplateScheduleActivateFunctions,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IActivateScheduleControlDTO,
  IActivateScheduleGridDTO,
  ITimeScheduleTemplateHeadSmallDTO,
  ITimeScheduleTemplatePeriodDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISaveEmployeeScheduleModel } from '@shared/models/generated-interfaces/TimeModels';
import { SoeHttpClient } from '@shared/services/http.service';
import { hasInitialAttestState } from '@shared/services/generated-service-endpoints/manage/AttestState.endpoints';
import {
  controlActivations,
  deletePlacement,
  employeeSchedule,
  getPlacementsForGrid,
  getTimeScheduleTemplateHeadsForActivate,
  getTimeScheduleTemplatePeriodsForActivate,
} from '@shared/services/generated-service-endpoints/time/EmployeeSchedule.endpoints';
import { getHiddenEmployeeId } from '@shared/services/generated-service-endpoints/time/EmployeeV2.endpoints';
import { Observable } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class PlacementsService {
  constructor(private http: SoeHttpClient) {}

  private getGridAdditionalProps = {
    onlyLatest: false,
    addEmptyPlacement: false,
  };

  getGrid(
    id?: number,
    additionalProps?: {
      onlyLatest: boolean;
      addEmptyPlacement: boolean;
      dateFrom?: string;
      dateTo?: string;
    }
  ): Observable<IActivateScheduleGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IActivateScheduleGridDTO[]>(
      getPlacementsForGrid(
        this.getGridAdditionalProps.onlyLatest,
        this.getGridAdditionalProps.addEmptyPlacement
      )
    );
  }

  activateSchedule(
    control: IActivateScheduleControlDTO,
    items: IActivateScheduleGridDTO[],
    func: TermGroup_TemplateScheduleActivateFunctions,
    timeScheduleTemplateHeadId: number,
    timeScheduleTemplatePeriodId: number,
    startDate: Date,
    stopDate: Date,
    preliminary: boolean
  ): Observable<IActionResult> {
    const model: ISaveEmployeeScheduleModel = {
      control: control,
      items: items,
      function: func,
      timeScheduleTemplateHeadId: timeScheduleTemplateHeadId,
      timeScheduleTemplatePeriodId: timeScheduleTemplatePeriodId,
      startDate: startDate,
      stopDate: stopDate,
      preliminary: preliminary,
    };
    return this.http.post<IActionResult>(employeeSchedule(), model);
  }

  deletePlacement(
    control: IActivateScheduleControlDTO,
    item: IActivateScheduleGridDTO
  ): Observable<BackendResponse> {
    const model = {
      control: control,
      item: item,
    };
    return this.http.post(deletePlacement(), model);
  }

  controlActivations(
    items: IActivateScheduleGridDTO[],
    startDate?: Date,
    stopDate?: Date,
    isDelete?: boolean
  ): Observable<IActivateScheduleControlDTO> {
    const model = {
      items: items,
      startDate: startDate,
      stopDate: stopDate,
      isDelete: isDelete,
    };
    return this.http.post(controlActivations(), model);
  }

  getTimeScheduleTemplateHeadsForActivate(): Observable<
    ITimeScheduleTemplateHeadSmallDTO[]
  > {
    return this.http.get<ITimeScheduleTemplateHeadSmallDTO[]>(
      getTimeScheduleTemplateHeadsForActivate()
    );
  }

  getTimeScheduleTemplatePeriodsForActivate(
    timeScheduleTemplateHeadId: number
  ): Observable<ITimeScheduleTemplatePeriodDTO[]> {
    return this.http.get<ITimeScheduleTemplatePeriodDTO[]>(
      getTimeScheduleTemplatePeriodsForActivate(timeScheduleTemplateHeadId)
    );
  }

  getHasInitialAttestState(
    entity: TermGroup_AttestEntity,
    module: SoeModule
  ): Observable<boolean> {
    return this.http.get<boolean>(hasInitialAttestState(entity, module));
  }

  getHiddenEmployeeId(): Observable<number> {
    return this.http.get<number>(getHiddenEmployeeId());
  }
}
