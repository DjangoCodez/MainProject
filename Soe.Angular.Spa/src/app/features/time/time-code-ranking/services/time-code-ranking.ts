import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteTimeCodeRanking,
  getTimeCodeRankingGrid,
  getTimeCodeRankings,
  getTimeCodes,
  saveTimeCodeRanking,
  validateTimeCodeRanking,
} from '../../../../shared/services/generated-service-endpoints/time/TimeCode.endpoints';

import {
  ITimeCodeDTO,
  ITimeCodeRankingGroupDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class TimeCodeRankingService {
  private readonly http = inject(SoeHttpClient);

  getTimeCodes(
    timeCodeType: number,
    onlyActive: boolean,
    loadPayrollProducts: boolean,
    onlyWithInvoiceProduct: boolean
  ): Observable<ITimeCodeDTO[]> {
    return this.http.get<ITimeCodeDTO[]>(
      getTimeCodes(
        timeCodeType,
        onlyActive,
        loadPayrollProducts,
        onlyWithInvoiceProduct
      )
    );
  }
  getGrid(id?: number): Observable<ITimeCodeRankingGroupDTO[]> {
    return this.http.get<ITimeCodeRankingGroupDTO[]>(
      getTimeCodeRankingGrid(id)
    );
  }
  get(id: number, additionalProps?: any): Observable<ITimeCodeRankingGroupDTO> {
    return this.http.get<ITimeCodeRankingGroupDTO>(getTimeCodeRankings(id));
  }
  save(model: ITimeCodeRankingGroupDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveTimeCodeRanking(), model);
  }
  delete(id: number): Observable<any> {
    return this.http.delete(deleteTimeCodeRanking(id));
  }
  validateTimeCodeRanking(
    model: ITimeCodeRankingGroupDTO,
    isDelete: boolean
  ): Observable<any> {
    return this.http.post(validateTimeCodeRanking(isDelete), model);
  }
}
