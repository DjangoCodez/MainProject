import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import { SearchVoucherFilterDTO } from '../models/voucher-search.model';
import { getSearchedVoucherRows } from '@shared/services/generated-service-endpoints/economy/VoucherSearch.endpoints';
import { ISearchVoucherRowDTO } from '@shared/models/generated-interfaces/SearchVoucherRowDTO';

@Injectable({
  providedIn: 'root',
})
export class VoucherSearchService {
  constructor(private http: SoeHttpClient) {}

  getSearchedVoucherRows(
    model: SearchVoucherFilterDTO
  ): Observable<ISearchVoucherRowDTO[]> {
    return this.http.post<ISearchVoucherRowDTO[]>(
      getSearchedVoucherRows(),
      model
    );
  }
}
