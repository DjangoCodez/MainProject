import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { VatCodeDTO } from '../../models/vat-code.model';
import {
  deleteVatCode,
  getVatCode,
  getVatCodesDict,
  getVatCodesGrid,
  saveVatCode,
} from '@shared/services/generated-service-endpoints/economy/VatCode.endpoints';
import { CacheSettingsFactory, SoeHttpClient } from '@shared/services/http.service';
import { IVatCodeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';

@Injectable({
  providedIn: 'root',
})
export class VatCodeService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IVatCodeGridDTO[]> {
    return this.http.get<IVatCodeGridDTO[]>(getVatCodesGrid(id));
  }

  getDict(
    addEmptyRow: boolean,
    useCache: boolean = false
  ): Observable<ISmallGenericType[]> {
    const options = useCache ? CacheSettingsFactory.long() : {};
    return this.http.get<ISmallGenericType[]>(
      getVatCodesDict(addEmptyRow),
      options
    );
  }

  get(id: number): Observable<VatCodeDTO> {
    return this.http.get<VatCodeDTO>(getVatCode(id)).pipe(
      map(data => {
        const obj = new VatCodeDTO();
        Object.assign(obj, data);
        return obj;
      })
    );
  }

  save(model: VatCodeDTO): Observable<any> {
    return this.http.post<VatCodeDTO>(saveVatCode(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteVatCode(id));
  }
}
