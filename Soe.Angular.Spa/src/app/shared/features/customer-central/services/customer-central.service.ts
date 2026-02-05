import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { CustomerCentralDTO } from '../models/customer-central.model';
import { Observable } from 'rxjs';
import {
  deleteCustomer,
  getCustomer,
  saveCustomer,
} from '@shared/services/generated-service-endpoints/shared/CustomerV2.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class CustomerCentralService {
  constructor(private http: SoeHttpClient) {}

  save(model: CustomerCentralDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveCustomer(), model, {});
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteCustomer(id));
  }

  loadCompleteCustomer(
    actorCustomerId: number,
    loadActor: boolean,
    loadAccount: boolean,
    loadNote: boolean,
    loadCustomerUser: boolean,
    loadContactAddresses: boolean,
    loadCategories: boolean
  ) {
    return this.http.get<CustomerCentralDTO>(
      getCustomer(
        actorCustomerId,
        loadActor,
        loadAccount,
        loadNote,
        loadCustomerUser,
        loadContactAddresses,
        loadCategories
      )
    );
  }
}
