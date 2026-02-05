import { Injectable } from '@angular/core';
import {
  IAccountDimSmallDTO,
  ITimePeriodHeadDTO,
  ITimePeriodHeadGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISaveTimePeriodHeadModel } from '@shared/models/generated-interfaces/TimeModels';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getTimePeriodHead,
  getPlanningPeriodGrid,
  saveTimePeriod,
  deleteTimePeriodHead,
  getTimePeriodHeadsIncludingPeriodsForType,
} from '@shared/services/generated-service-endpoints/time/TimePeriod.endpoints';
import { accountDimByAccountDimIdSmall } from '@shared/services/generated-service-endpoints/economy/Account.endpoints';
import { Observable } from 'rxjs';
import { TermGroup_TimePeriodType } from '@shared/models/generated-interfaces/Enumerations';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class PlanningPeriodsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ITimePeriodHeadGridDTO[]> {
    return this.http.get<ITimePeriodHeadGridDTO[]>(getPlanningPeriodGrid(id));
  }

  get(id: number): Observable<ITimePeriodHeadDTO> {
    return this.http.get<ITimePeriodHeadDTO>(getTimePeriodHead(id, true));
  }

  save(
    head: ITimePeriodHeadDTO | any,
    removePeriodLinks: boolean = false
  ): Observable<BackendResponse> {
    const model = {
      timePeriodHead: head,
      removePeriodLinks: removePeriodLinks,
    } as ISaveTimePeriodHeadModel;

    return this.http.post<BackendResponse>(saveTimePeriod(), model);
  }

  delete(
    id: number,
    removePeriodLinks: boolean = false
  ): Observable<BackendResponse> {
    return this.http.delete(deleteTimePeriodHead(id, removePeriodLinks));
  }

  getDim(
    accountDimId: number,
    loadAccounts: boolean,
    loadInternalAccounts: boolean
  ): Observable<IAccountDimSmallDTO> {
    return this.http.get<IAccountDimSmallDTO>(
      accountDimByAccountDimIdSmall(
        accountDimId,
        false,
        false,
        loadAccounts,
        loadInternalAccounts,
        false,
        false,
        false,
        false
      )
    );
  }

  getTimePeriodHeadsIncludingPeriodsForType(
    type: number = TermGroup_TimePeriodType.RuleWorkTime // Planning period
  ): Observable<ITimePeriodHeadDTO[]> {
    return this.http.get<ITimePeriodHeadDTO[]>(
      getTimePeriodHeadsIncludingPeriodsForType(type)
    );
  }
}
