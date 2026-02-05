import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { performPriceUpdate } from '../../../../../../shared/services/generated-service-endpoints/billing/SupplierPurchaseProduct.endpoints';
import { PriceUpdateModel } from './price-update-form.model';
import { SoeHttpClient } from '@shared/services/http.service';
import { TranslateService } from '@ngx-translate/core';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class PriceUpdateModalService {
  private readonly http = inject(SoeHttpClient);
  private readonly translateService = inject(TranslateService);

  getRoundingOptions() {
    return this.translateService
      .get([
        'common.noone',
        'common.customer.contract.one',
        'common.customer.contract.tens',
        'common.customer.contract.hundreds',
        'common.customer.contract.thousands',
      ])
      .pipe(
        map(terms => {
          return [
            { id: 0, name: terms['common.noone'] },
            { id: 1, name: terms['common.customer.contract.one'] },
            { id: 10, name: terms['common.customer.contract.tens'] },
            { id: 100, name: terms['common.customer.contract.hundreds'] },
            { id: 1000, name: terms['common.customer.contract.thousands'] },
          ];
        })
      );
  }

  performPriceUpdate(data: PriceUpdateModel) {
    return this.http.post<BackendResponse>(performPriceUpdate(), data);
  }
}
