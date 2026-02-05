import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { IDeliveryConditionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CacheSettingsFactory, SoeHttpClient } from '@shared/services/http.service';
import {
  deleteDeliveryCondition,
  getDeliveryCondition,
  getDeliveryConditions,
  getDeliveryConditionsGrid,
  saveDeliveryCondition,
} from '@shared/services/generated-service-endpoints/billing/DeliveryCondition.endpoints';
import { DeliveryConditionDTO } from '../models/delivery-condition.model';

@Injectable({
  providedIn: 'root',
})
export class DeliveryConditionService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IDeliveryConditionDTO[]> {
    return this.http.get<IDeliveryConditionDTO[]>(
      getDeliveryConditionsGrid(id)
    );
  }

  get(id: number): Observable<DeliveryConditionDTO> {
    return this.http.get<DeliveryConditionDTO>(getDeliveryCondition(id));
  }

  getDeliveryConditionsDict(
    addEmptyRow: boolean,
    useCache: boolean = false
  ): Observable<DeliveryConditionDTO[]> {
    const options = useCache ? CacheSettingsFactory.long() : {};
    return this.http.get<DeliveryConditionDTO[]>(
      getDeliveryConditions(addEmptyRow),
      options
    );
  }

  save(model: DeliveryConditionDTO): Observable<any> {
    return this.http.post<DeliveryConditionDTO>(saveDeliveryCondition(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteDeliveryCondition(id));
  }
}
