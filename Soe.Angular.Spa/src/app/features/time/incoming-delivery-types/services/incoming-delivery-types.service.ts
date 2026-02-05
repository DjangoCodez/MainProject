import { Injectable } from '@angular/core';
import {
  IIncomingDeliveryTypeDTO,
  IIncomingDeliveryTypeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  deleteIncomingDeliveryType,
  getIncomingDeliveryType,
  getIncomingDeliveryTypesGrid,
  saveIncomingDeliveryType,
} from '@shared/services/generated-service-endpoints/time/IncomingDelivery.endpoints';

import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, map } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class IncomingDeliveryTypesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IIncomingDeliveryTypeGridDTO[]> {
    return this.http.get<IIncomingDeliveryTypeGridDTO[]>(
      getIncomingDeliveryTypesGrid(id)
    );
  }

  get(id: number): Observable<IIncomingDeliveryTypeDTO> {
    return this.http.get<IIncomingDeliveryTypeDTO>(getIncomingDeliveryType(id));
  }

  save(model: IIncomingDeliveryTypeDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveIncomingDeliveryType(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteIncomingDeliveryType(id));
  }
}
