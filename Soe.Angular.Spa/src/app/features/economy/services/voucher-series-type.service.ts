import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import { VoucherSeriesTypeDTO } from '../models/voucher-series-type.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  deleteVoucherSeriesType,
  getVoucherSeriesType,
  getVoucherSeriesTypes,
  getVoucherSeriesTypesByCompany,
  saveVoucherSeriesType,
} from '@shared/services/generated-service-endpoints/economy/VoucherSeriesType.endpoints';

@Injectable({
  providedIn: 'root',
})
export class VoucherSeriesTypeService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<VoucherSeriesTypeDTO[]> {
    return this.http.get<VoucherSeriesTypeDTO[]>(getVoucherSeriesTypes(id));
  }

  getVoucherSeriesTypesByCompany(addEmptyRow?: boolean, nameOnly?: boolean) {
    return this.http.get<ISmallGenericType[]>(
      getVoucherSeriesTypesByCompany(addEmptyRow, nameOnly)
    );
  }

  get(id: number): Observable<VoucherSeriesTypeDTO> {
    return this.http.get<VoucherSeriesTypeDTO>(getVoucherSeriesType(id));
  }

  save(model: VoucherSeriesTypeDTO): Observable<any> {
    return this.http.post<VoucherSeriesTypeDTO>(saveVoucherSeriesType(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteVoucherSeriesType(id));
  }
}
