import { Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IDistributionCodeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, map } from 'rxjs';
import {
  deleteDistributionCode,
  getDistributionCode,
  getDistributionCodes,
  getDistributionCodesByType,
  getDistributionCodesDict,
  getDistributionCodesGrid,
  saveDistributionCode,
} from '@shared/services/generated-service-endpoints/economy/DistributionCode.endpoints';
import { DistributionCodeHeadDTO } from '../models/distribution-codes.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class DistributionCodesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IDistributionCodeGridDTO[]> {
    return this.http.get<IDistributionCodeGridDTO[]>(
      getDistributionCodesGrid(id)
    );
  }

  get(id: number): Observable<DistributionCodeHeadDTO> {
    return this.http.get<DistributionCodeHeadDTO>(getDistributionCode(id)).pipe(
      map(data => {
        const obj = new DistributionCodeHeadDTO();
        Object.assign(obj, data);
        return obj;
      })
    );
  }

  getDistributionCodes(
    includePeriods: boolean,
    budgetType?: number,
    fromDate?: number,
    toDate?: number
  ): Observable<DistributionCodeHeadDTO[]> {
    return this.http.get<DistributionCodeHeadDTO[]>(
      getDistributionCodes(includePeriods, budgetType, fromDate, toDate)
    );
  }

  getDistributionCodesByType(
    distributionCodeType: number,
    loadPeriods: boolean
  ): Observable<DistributionCodeHeadDTO> {
    return this.http.get<DistributionCodeHeadDTO>(
      getDistributionCodesByType(distributionCodeType, loadPeriods)
    );
  }

  getDistributionCodesDict(
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getDistributionCodesDict(addEmptyRow)
    );
  }

  save(model: DistributionCodeHeadDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveDistributionCode(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteDistributionCode(id));
  }
}
