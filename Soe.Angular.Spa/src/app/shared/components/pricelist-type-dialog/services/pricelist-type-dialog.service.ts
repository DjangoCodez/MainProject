import { Injectable } from '@angular/core';
import { IPriceListTypeDTO } from '@shared/models/generated-interfaces/PriceListTypeDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { getPriceListType } from '@shared/services/generated-service-endpoints/billing/PriceList.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PricelistTypeDialogService {

  constructor(private http: SoeHttpClient) { }

  getPriceListType(priceListTypeId: number): Observable<IPriceListTypeDTO> {
    return this.http.get<IPriceListTypeDTO>(getPriceListType(priceListTypeId))
  }
}
