import { inject, Injectable } from '@angular/core';
import { getTimeCodesGrid } from '../../../../../../shared/services/generated-service-endpoints/time/TimeCode.endpoints';
import {
  SoeTimeCodeType,
  TermGroup,
} from '../../../../../../shared/models/generated-interfaces/Enumerations';
import { map } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { performPriceUpdate } from '../../../../../../shared/services/generated-service-endpoints/billing/PriceList.endpoints';
import { PriceUpdateModel } from './pricelist-update-form.model';
import { ProductGroupDTO } from '../../../../product-groups/models/product-groups.model';
import { getProductGroupsGrid } from '../../../../../../shared/services/generated-service-endpoints/billing/ProductGroup.endpoints';
import { TimeCodeMaterialDTO } from 'src/app/features/billing/material-codes/models/material-codes.model';
import { SoeHttpClient } from '@shared/services/http.service';
import { CoreService } from '@shared/services/core.service';
import { addEmptyOption } from '@shared/util/array-util';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class PricelistUpdateModalService {
  private readonly http = inject(SoeHttpClient);
  private readonly coreService = inject(CoreService);
  private readonly translateService = inject(TranslateService);

  getMaterialCodes() {
    return this.http
      .get<
        TimeCodeMaterialDTO[]
      >(getTimeCodesGrid(SoeTimeCodeType.Material, true, false))
      .pipe(
        map(data => {
          const result = data.map(item => {
            return { id: item.timeCodeId, name: item.name };
          });

          addEmptyOption(result);

          return result;
        })
      );
  }

  getProductVatTypes() {
    return this.coreService.getTermGroupContent(
      TermGroup.InvoiceProductVatType,
      true,
      true
    );
  }

  getProductGroups() {
    return this.http.get<ProductGroupDTO[]>(getProductGroupsGrid()).pipe(
      map(data => {
        const result = data.map(item => {
          return { id: item.productGroupId, name: item.name };
        });
        addEmptyOption(result);
        return result;
      })
    );
  }

  getRoundingOptions() {
    return this.translateService
      .get([
        'common.noone',
        'common.decimal',
        'common.customer.contract.fivecent',
        'common.customer.contract.one',
        'common.customer.contract.tens',
        'common.customer.contract.hundreds',
        'common.customer.contract.thousands',
      ])
      .pipe(
        map(terms => {
          return [
            { id: 0, name: terms['common.noone'] },
            { id: 0.1, name: terms['common.decimal'] },
            { id: 0.05, name: terms['common.customer.contract.fivecent'] },
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
