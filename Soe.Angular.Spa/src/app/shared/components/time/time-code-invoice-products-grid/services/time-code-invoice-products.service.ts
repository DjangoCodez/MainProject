import { Injectable } from '@angular/core';
import { IProductTimeCodeDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { getTimeCodeInvoiceProducts } from '@shared/services/generated-service-endpoints/time/TimeCode.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TimeCodeInvoiceProductsService {
  constructor(private http: SoeHttpClient) {}
  getTimeCodeInvoiceProducts(): Observable<IProductTimeCodeDTO[]> {
    return this.http.get<IProductTimeCodeDTO[]>(getTimeCodeInvoiceProducts());
  }
}
