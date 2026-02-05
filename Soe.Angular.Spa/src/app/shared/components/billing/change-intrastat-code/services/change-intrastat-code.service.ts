import { Injectable } from '@angular/core';
import { ISaveIntrastatTransactionModel } from '@shared/models/generated-interfaces/BillingModels';
import { SoeHttpClient } from '@shared/services/http.service';
import { saveIntrastatTransactions } from '@shared/services/generated-service-endpoints/billing/OrderIntrastat.endpoints';
import { Observable } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ChangeIntrastatCodeService {
  constructor(private http: SoeHttpClient) {}

  save(model: ISaveIntrastatTransactionModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveIntrastatTransactions(), model);
  }
}
