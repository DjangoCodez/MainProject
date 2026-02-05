import { Injectable } from '@angular/core';
import { IProductTimeCodeDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { getTimeCodePayrollProducts } from '@shared/services/generated-service-endpoints/time/TimeCode.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TimeCodePayrollProductsService {
  constructor(private http: SoeHttpClient) {}
  getTimeCodePayrollProducts(): Observable<IProductTimeCodeDTO[]> {
    return this.http.get<IProductTimeCodeDTO[]>(getTimeCodePayrollProducts());
  }
}
