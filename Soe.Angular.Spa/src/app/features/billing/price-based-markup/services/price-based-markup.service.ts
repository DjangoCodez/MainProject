import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import { PriceBasedMarkupDTO } from '../models/price-based-markup.model';
import {
  getPriceBasedMarkup,
  getPriceBasedMarkupGrid,
  getPriceLists,
  savePriceBasedMarkup,
} from '@shared/services/generated-service-endpoints/billing/PriceBasedMarkup.endpoints';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class PriceBasedMarkupService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<PriceBasedMarkupDTO[]> {
    return this.http.get<PriceBasedMarkupDTO[]>(getPriceBasedMarkupGrid(id));
  }

  get(id: number): Observable<PriceBasedMarkupDTO> {
    return this.http.get<PriceBasedMarkupDTO>(getPriceBasedMarkup(id));
  }

  save(model: PriceBasedMarkupDTO[]): Observable<BackendResponse> {
    return this.http.post(savePriceBasedMarkup(), model);
  }

  getPriceList(): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getPriceLists());
  }
}
