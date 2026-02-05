import { Injectable } from '@angular/core';
import { IMarkupDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getDiscount,
  getMarkup,
  saveMarkup,
} from '@shared/services/generated-service-endpoints/billing/Markup.endpoints';
import { Observable } from 'rxjs';
import { MarkupDTO } from '../models/markup.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class MarkupService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    isDiscount: false,
  };
  getGrid(
    id?: number,
    additionalProps?: { isDiscount: boolean }
  ): Observable<IMarkupDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IMarkupDTO[]>(
      getMarkup(this.getGridAdditionalProps.isDiscount)
    );
  }

  getDiscount(sysWholesellerId: number, code: string): Observable<unknown> {
    return this.http.get<unknown>(getDiscount(sysWholesellerId, code));
  }

  save(model: MarkupDTO[]): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveMarkup(), model);
  }
}
