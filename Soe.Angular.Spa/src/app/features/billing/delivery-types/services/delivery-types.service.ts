import { Injectable } from '@angular/core';
import { CacheSettingsFactory, SoeHttpClient } from '@shared/services/http.service';
import { IDeliveryTypeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { Observable } from 'rxjs';
import { DeliveryTypeDTO } from '../models/delivery-types.model';
import {
  deleteDeliveryType,
  getDeliveryType,
  getDeliveryTypesDict,
  getDeliveryTypesGrid,
  saveDeliveryType,
} from '@shared/services/generated-service-endpoints/billing/DeliveryType.endpoints';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';

@Injectable({
  providedIn: 'root',
})
export class DeliveryTypesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IDeliveryTypeGridDTO[]> {
    return this.http.get<IDeliveryTypeGridDTO[]>(getDeliveryTypesGrid(id));
  }

  get(id: number): Observable<DeliveryTypeDTO> {
    return this.http.get<DeliveryTypeDTO>(getDeliveryType(id));
  }

  save(model: DeliveryTypeDTO): Observable<any> {
    return this.http.post<DeliveryTypeDTO>(saveDeliveryType(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteDeliveryType(id));
  }
  getDeliveryTypesDict(
    addEmptyRow: boolean,
    useCache: boolean = false
  ): Observable<ISmallGenericType[]> {
    const options = useCache ? CacheSettingsFactory.long() : {};
    return this.http.get<ISmallGenericType[]>(
      getDeliveryTypesDict(addEmptyRow),
      options
    );
  }
}
