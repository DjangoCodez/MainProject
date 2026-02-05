import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { PaymentConditionDTO } from '../models/payment-condition.model';
import { Observable } from 'rxjs';
import {
  getPaymentCondition,
  getPaymentConditionsGrid,
  savePaymentCondition,
  deletePaymentCondition,
} from '@shared/services/generated-service-endpoints/economy/PaymentCondition.endpoints';
import { IPaymentConditionGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Injectable({
  providedIn: 'root',
})
export class PaymentConditionsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IPaymentConditionGridDTO[]> {
    return this.http.get<IPaymentConditionGridDTO[]>(
      getPaymentConditionsGrid(id)
    );
  }

  get(id: number): Observable<PaymentConditionDTO> {
    return this.http.get<PaymentConditionDTO>(getPaymentCondition(id));
  }

  save(model: PaymentConditionDTO): Observable<any> {
    return this.http.post<PaymentConditionDTO>(savePaymentCondition(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deletePaymentCondition(id));
  }
}
