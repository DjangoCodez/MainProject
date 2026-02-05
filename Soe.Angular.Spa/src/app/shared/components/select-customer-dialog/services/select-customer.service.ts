import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { CustomerSearchModelDTO } from '../models/select-customer-dialog.model';
import { Observable } from 'rxjs';
import { ICustomerSearchModel } from '@shared/models/generated-interfaces/CoreModels';
import { getCustomersBySearch } from '@shared/services/generated-service-endpoints/shared/CustomerV2.endpoints';

@Injectable({
  providedIn: 'root',
})
export class SelectCustomerService {
  constructor(private http: SoeHttpClient) {}

  getCustomersBySearch(
    searchDto: CustomerSearchModelDTO
  ): Observable<ICustomerSearchModel[]> {
    return this.http.post<ICustomerSearchModel[]>(
      getCustomersBySearch(),
      searchDto
    );
  }
}
