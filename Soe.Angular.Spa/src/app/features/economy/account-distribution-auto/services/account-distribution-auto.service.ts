import { Injectable } from '@angular/core';
import { AccountDistributionHeadSmallDTO } from '@features/economy/account-distribution/models/account-distribution.model';
import { AccountDistributionService } from '@features/economy/account-distribution/services/account-distribution.service';
import { SoeHttpClient } from '@shared/services/http.service';
import { getAccountDistributionHeadsAuto } from '@shared/services/generated-service-endpoints/economy/AccountDistribution.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AccountDistributionAutoService extends AccountDistributionService {
  constructor(protected http: SoeHttpClient) {
    super(http);
  }

  override getGrid(id?: number): Observable<AccountDistributionHeadSmallDTO[]> {
    return this.http.get<AccountDistributionHeadSmallDTO[]>(
      getAccountDistributionHeadsAuto(id)
    );
  }
}
