import { inject, Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, map } from 'rxjs';
import {
  ClientGridDTO,
  SysMultiCompanyConnectionRequest,
  SysMultiCompanyConnectionRequestStatus,
} from '../models/clients.model';
import {
  getClients,
  getRequestStatus,
  initRequest,
} from '@shared/services/generated-service-endpoints/shared/ClientManagement.endpoints';
import { DateUtil } from '@shared/util/date-util';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

export interface ConnectionRequestStatusResponse {
  companyName?: string;
  connectedAt?: Date;
}

@Injectable({
  providedIn: 'root',
})
export class ClientsService {
  private http = inject(SoeHttpClient);

  getGrid(): Observable<ClientGridDTO[]> {
    return this.http.get(getClients());
  }

  createConnectionRequest(): Observable<SysMultiCompanyConnectionRequest | null> {
    return this.http.post<BackendResponse>(initRequest(), {}).pipe(
      map(res =>
        res.success
          ? {
              sysMultiCompanyConnectionRequestId: ResponseUtil.getEntityId(res),
              code: ResponseUtil.getStringValue(res),
              expiresAtUTC: DateUtil.getLocalDateFromUTCDate(
                ResponseUtil.getDateTimeValue(res)
              ),
            }
          : null
      )
    );
  }
  checkConnectionRequestStatus(
    requestId: number
  ): Observable<SysMultiCompanyConnectionRequestStatus | null> {
    return this.http.get<BackendResponse>(getRequestStatus(requestId)).pipe(
      map(res =>
        res?.success
          ? {
              sysMultiCompanyConnectionRequestId: requestId,
              linkedCompanyName: ResponseUtil.getStringValue(res),
            }
          : null
      )
    );
  }
}
