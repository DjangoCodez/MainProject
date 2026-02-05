import { Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ICommodityCodeDTO } from '@shared/models/generated-interfaces/CommodityCodeDTO';
import { IUpdateEntityStatesModel } from '@shared/models/generated-interfaces/CoreModels';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getCustomerCommodyCodes,
  getCustomerCommodyCodesDict,
  saveCustomerCommodityCodes,
} from '@shared/services/generated-service-endpoints/billing/CommodityCodes.endpoints';
import { Observable } from 'rxjs';
import { CommodityCodeDTO } from '../../../manage/commodity-codes/models/commodity-codes.model';

@Injectable({
  providedIn: 'root',
})
export class StatisticalCommodityCodesService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    onlyActive: false,
  };
  getGrid(
    id?: number,
    additionalProps?: { onlyActive: boolean }
  ): Observable<CommodityCodeDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<CommodityCodeDTO[]>(
      getCustomerCommodyCodes(this.getGridAdditionalProps.onlyActive)
    );
  }

  getDict(addEmpty: boolean): Observable<SmallGenericType> {
    return this.http.get<SmallGenericType>(
      getCustomerCommodyCodesDict(addEmpty)
    );
  }

  save(model: IUpdateEntityStatesModel): Observable<any> {
    return this.http.post<IUpdateEntityStatesModel>(
      saveCustomerCommodityCodes(),
      model
    );
  }
}
