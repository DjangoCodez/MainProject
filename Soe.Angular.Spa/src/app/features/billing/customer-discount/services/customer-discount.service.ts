import { Injectable } from '@angular/core';
import { MarkupDTO } from '@features/billing/markup/models/markup.model';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getMarkup,
  saveMarkup,
} from '@shared/services/generated-service-endpoints/billing/Markup.endpoints';
import { Observable } from 'rxjs';
import { CustomerDiscountMarkupDTO } from '../models/customer-discount.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { getSmallGenericSysWholesellers } from '@shared/services/generated-service-endpoints/billing/SysWholeseller.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class CustomerDiscountService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = { isDiscount: false };
  getGrid(
    id?: number,
    additionalProps?: { isDiscount: boolean }
  ): Observable<MarkupDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<MarkupDTO[]>(
      getMarkup(this.getGridAdditionalProps.isDiscount)
    );
  }

  save(model: CustomerDiscountMarkupDTO[]): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveMarkup(), model);
  }

  getSysWholesellersDict(
    addEmptyRow: boolean,
    useCache: boolean = false
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getSmallGenericSysWholesellers(addEmptyRow),
      { useCache }
    );
  }
}
