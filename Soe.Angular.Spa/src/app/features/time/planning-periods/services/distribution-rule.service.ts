import { Injectable } from '@angular/core';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';

import {
  IPayrollProductDistributionRuleHeadDTO,
  IPayrollProductDistributionRuleHeadGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISavePayrollProductDistributionRuleHeadModel } from '@shared/models/generated-interfaces/TimeModels';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteDistributionRuleHead,
  getDistributionRuleHead,
  getDistributionRulesForGrid,
  saveDistributionRuleHead,
} from '@shared/services/generated-service-endpoints/time/TimePeriod.endpoints';
import { getPayrollProductsSmall } from '@shared/services/generated-service-endpoints/time/TimeWorkAccount.endpoints';
import { Observable } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class DistributionRuleService {
  constructor(private http: SoeHttpClient) {}

  getGrid(
    id?: number
  ): Observable<IPayrollProductDistributionRuleHeadGridDTO[]> {
    return this.http.get<IPayrollProductDistributionRuleHeadGridDTO[]>(
      getDistributionRulesForGrid(id)
    );
  }

  get(id: number): Observable<IPayrollProductDistributionRuleHeadDTO> {
    return this.http.get<IPayrollProductDistributionRuleHeadDTO>(
      getDistributionRuleHead(id)
    );
  }

  save(
    head: IPayrollProductDistributionRuleHeadDTO
  ): Observable<BackendResponse> {
    const model = {
      payrollProductDistributionRuleHead: head,
    } as ISavePayrollProductDistributionRuleHeadModel;

    return this.http.post<BackendResponse>(saveDistributionRuleHead(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteDistributionRuleHead(id));
  }

  getPayrollProductsSmall(): Observable<any[]> {
    return this.http.get<IProductSmallDTO[]>(getPayrollProductsSmall());
  }
}
