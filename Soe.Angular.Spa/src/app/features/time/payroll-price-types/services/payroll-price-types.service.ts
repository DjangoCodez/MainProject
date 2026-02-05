import { Injectable } from '@angular/core';
import {
  IPayrollPriceTypeDTO,
  IPayrollPriceTypeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deletePayrollPriceType,
  getPayrollPriceType,
  getPayrollPriceTypesGrid,
  savePayrollPriceType,
} from '@shared/services/generated-service-endpoints/time/PayrollPriceType.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PayrollPriceTypesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IPayrollPriceTypeGridDTO[]> {
    return this.http.get<IPayrollPriceTypeGridDTO[]>(
      getPayrollPriceTypesGrid(id)
    );
  }

  get(id: number): Observable<IPayrollPriceTypeDTO> {
    return this.http.get<IPayrollPriceTypeDTO>(getPayrollPriceType(id));
  }

  save(model: IPayrollPriceTypeDTO): Observable<any> {
    return this.http.post<IPayrollPriceTypeDTO>(savePayrollPriceType(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deletePayrollPriceType(id));
  }
}
