import { Injectable } from '@angular/core';
import {
  ISysPriceListHeadGridDTO,
  ISysPricelistProviderDTO,
} from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getSysPriceListHeads,
  getSysPriceProvider,
  sysPriceListImport,
} from '@shared/services/generated-service-endpoints/manage/SysPriceList.endpoints';
import { Observable } from 'rxjs';
import { SysPriceListImportDTO } from '../models/import-price-list.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ImportPriceListService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ISysPriceListHeadGridDTO[]> {
    return this.http.get<ISysPriceListHeadGridDTO[]>(getSysPriceListHeads());
  }
  uploadImportPriceListFile(
    model: SysPriceListImportDTO
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(sysPriceListImport(), model);
  }

  getSysPricelistProvider(): Observable<ISysPricelistProviderDTO[]> {
    return this.http.get<ISysPricelistProviderDTO[]>(getSysPriceProvider());
  }
}
