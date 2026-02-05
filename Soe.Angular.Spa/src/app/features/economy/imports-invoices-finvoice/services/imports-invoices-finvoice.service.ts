import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getEdiEntrysCountWithStateCheck,
  getEdiEntrysWithStateCheck,
  getFinvoiceEntrys,
  importFinvoiceAttachments,
} from '@shared/services/generated-service-endpoints/billing/FInvoice.endpoints';
import { Observable } from 'rxjs';
import { importFinvoiceFiles } from '@shared/services/generated-service-endpoints/economy/ImportPayment.endpoints';
import {
  EdiEntryViewDTO,
  FInvoiceModel,
} from '../models/imports-invoices-finvoice.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ImportsInvoicesFinvoiceService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    classification: 0,
    allItemsSelection: 0,
    onlyUnHandled: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      classification: number;
      allItemsSelection: number;
      onlyUnHandled: boolean;
    }
  ): Observable<EdiEntryViewDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<EdiEntryViewDTO[]>(
      getFinvoiceEntrys(
        this.getGridAdditionalProps.classification,
        this.getGridAdditionalProps.allItemsSelection,
        this.getGridAdditionalProps.onlyUnHandled
      )
    );
  }

  getEdiEntrysCountWithStateCheck(
    classification: number,
    originType: number
  ): Observable<EdiEntryViewDTO> {
    return this.http.get<EdiEntryViewDTO>(
      getEdiEntrysCountWithStateCheck(classification, originType)
    );
  }
  getEdiEntrysWithStateCheck(
    classification: number,
    originType: number
  ): Observable<EdiEntryViewDTO[]> {
    return this.http.get<EdiEntryViewDTO[]>(
      getEdiEntrysWithStateCheck(classification, originType)
    );
  }

  attacheFile(model: FInvoiceModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(importFinvoiceAttachments(), model);
  }

  fileUpload(dataStorageIds: number[]): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      importFinvoiceFiles(dataStorageIds),
      dataStorageIds
    );
  }
}
