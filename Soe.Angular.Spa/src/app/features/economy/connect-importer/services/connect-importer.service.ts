import { Injectable } from '@angular/core';
import { IImportBatchDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getImportBatches,
  getImportGridColumns,
  getImportIOResult,
  importIO,
  saveCustomerInvoiceIODTO,
  saveCustomerInvoiceRowIODTO,
  saveCustomerIODTO,
  saveProjectIODTO,
  saveSupplieInvoiceIODTO,
  saveSupplierIODTO,
  saveVoucherIODTO,
} from '@shared/services/generated-service-endpoints/core/Connect.endpoints';
import { BehaviorSubject, Observable, of } from 'rxjs';
import {
  ConnectImporterGridFilterDTO,
  ImportBatchDTO,
  ImportIOModel,
} from '../models/connect-importer.model';
import { ImportGridColumnDTO } from '@features/economy/import-connect/models/import-grid-columns-dto.model';
import {
  CustomerInvoiceIODTO,
  CustomerInvoiceRowIODTO,
  CustomerIODTO,
  ProjectIODTO,
  SupplierInvoiceHeadIODTO,
  SupplierIODTO,
  VoucherHeadIODTO,
} from '@shared/components/import-rows/models/import-rows.model';

@Injectable({
  providedIn: 'root',
})
export class ConnectImporterService {
  constructor(private http: SoeHttpClient) {}

  filter = new ConnectImporterGridFilterDTO();

  private gridFilterSubject = new BehaviorSubject<ConnectImporterGridFilterDTO>(
    this.filter
  );
  readonly gridFilter$ = this.gridFilterSubject.asObservable();

  setFilterSubject(filter: ConnectImporterGridFilterDTO) {
    this.filter = filter;
    this.gridFilterSubject.next(filter);
  }

  getGridAdditionalProps = {
    importHeadType: 0,
    allItemsSelection: 0,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      importHeadType: number;
      allItemsSelection: number;
    }
  ): Observable<ImportBatchDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<ImportBatchDTO[]>(
      getImportBatches(
        this.getGridAdditionalProps.importHeadType,
        this.getGridAdditionalProps.allItemsSelection
      )
    );
  }

  get(id?: number): Observable<IImportBatchDTO[]> {
    return of();
  }

  save(model: any): Observable<any> {
    return of();
  }

  delete(id: number): Observable<any> {
    return of();
  }

  getImportGridColumns(
    importHeadType: number
  ): Observable<ImportGridColumnDTO[]> {
    return this.http.get<ImportGridColumnDTO[]>(
      getImportGridColumns(importHeadType)
    );
  }
  getImportIOResult(
    importHeadType: number,
    batchId: string
  ): Observable<any[]> {
    return this.http.get<any[]>(getImportIOResult(importHeadType, batchId));
  }

  importIO(model: ImportIOModel): Observable<any> {
    return this.http.post<any>(importIO(), model);
  }

  saveCustomerIODTO(model: CustomerIODTO[]): Observable<any> {
    return this.http.post<any>(saveCustomerIODTO(), model);
  }

  saveCustomerInvoiceHeadIODTO(model: CustomerInvoiceIODTO[]): Observable<any> {
    return this.http.post<any>(saveCustomerInvoiceIODTO(), model);
  }

  saveCustomerInvoiceRowIODTO(
    model: CustomerInvoiceRowIODTO[]
  ): Observable<any> {
    return this.http.post<any>(saveCustomerInvoiceRowIODTO(), model);
  }

  saveSupplierIODTO(model: SupplierIODTO[]): Observable<any> {
    return this.http.post<any>(saveSupplierIODTO(), model);
  }

  saveSupplierInvoiceHeadIODTO(
    model: SupplierInvoiceHeadIODTO[]
  ): Observable<any> {
    return this.http.post<any>(saveSupplieInvoiceIODTO(), model);
  }

  saveVoucherHeadIODTO(model: VoucherHeadIODTO[]): Observable<any> {
    return this.http.post<any>(saveVoucherIODTO(), model);
  }

  saveProjectIODTO(model: ProjectIODTO[]): Observable<any> {
    return this.http.post<any>(saveProjectIODTO(), model);
  }
}
