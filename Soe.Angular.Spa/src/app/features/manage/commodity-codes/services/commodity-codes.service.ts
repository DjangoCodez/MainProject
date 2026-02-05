import { Injectable } from '@angular/core';
import { ICommodityCodeDTO } from '@shared/models/generated-interfaces/CommodityCodeDTO';
import { ICommodityCodeUploadDTO } from '@shared/models/generated-interfaces/CoreModels';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CacheSettingsFactory, SoeHttpClient } from '@shared/services/http.service';
import {
  getCommodyCodes,
  getCustomerCommodyCodesDict,
  uploadCommodityCodesFile,
} from '@shared/services/generated-service-endpoints/manage/SysCommodityCodes.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CommodityCodesService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    langId: 0,
  };
  getGrid(
    id?: number,
    additionalProps?: { langId: number }
  ): Observable<ICommodityCodeDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<ICommodityCodeDTO[]>(
      getCommodyCodes(this.getGridAdditionalProps.langId)
    );
  }

  getCustomerCommodyCodesDict(
    addEmpty: boolean,
    useCache: boolean = false
  ): Observable<ISmallGenericType[]> {
    const options = useCache ? CacheSettingsFactory.long() : {};
    return this.http.get<ISmallGenericType[]>(
      getCustomerCommodyCodesDict(addEmpty),
      options
    );
  }

  uploadCommodityCodesFile(model: ICommodityCodeUploadDTO): Observable<any> {
    return this.http.post<any>(uploadCommodityCodesFile(), model);
  }
}
