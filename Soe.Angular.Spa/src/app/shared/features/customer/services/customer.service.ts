import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteCustomer,
  getCustomer,
  getCustomersForGrid,
  updateGrid,
  saveCustomer,
  updateCustomersState,
} from '@shared/services/generated-service-endpoints/shared/CustomerV2.endpoints';
import { CustomerDTO, CustomerGridDTO } from '../models/customer.model';
import { map, Observable, of } from 'rxjs';
import {
  ICustomerUpdateGrid,
  ISaveCustomerModel,
} from '@shared/models/generated-interfaces/CoreModels';
import { Dict } from '@ui/grid/services/selected-item.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    onlyActive: false,
  };
  getGrid(
    id?: number,
    additionalProps?: { onlyActive: boolean }
  ): Observable<CustomerGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<CustomerGridDTO[]>(
      getCustomersForGrid(this.getGridAdditionalProps.onlyActive, id)
    );
  }

  get(
    customerId: number,
    loadActor: boolean,
    loadAccount: boolean,
    loadNote: boolean,
    loadCustomerUser: boolean,
    loadContactAddresses: boolean,
    loadCategories: boolean
  ): Observable<CustomerDTO> {
    return this.http
      .get(
        getCustomer(
          customerId,
          loadActor,
          loadAccount,
          loadNote,
          loadCustomerUser,
          loadContactAddresses,
          loadCategories
        )
      )
      .pipe(
        map(c => {
          const obj = new CustomerDTO();
          Object.assign(obj, c);
          return obj;
        })
      );
  }

  save(model: ISaveCustomerModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveCustomer(), model, {});
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteCustomer(id));
  }

  updateCustomerState(selectedItems: Dict): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      updateCustomersState(),
      selectedItems
    );
  }

  updateGrid(
    customerUpdateGridDto: ICustomerUpdateGrid
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(updateGrid(), customerUpdateGridDto);
  }
}
