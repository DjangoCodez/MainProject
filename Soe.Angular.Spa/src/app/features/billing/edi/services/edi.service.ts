import { Injectable } from '@angular/core';
import { ImportsInvoicesFinvoiceService } from '@features/economy/imports-invoices-finvoice/services/imports-invoices-finvoice.service';
import { SupplierService } from '@features/economy/services/supplier.service';
import { IGetFilteredEDIEntrysModel } from '@shared/models/generated-interfaces/BillingModels';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IEdiEntryViewDTO,
  IUpdateEdiEntryDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getEdiEntrysWithStateCheck,
  getFilteredEdiEntrys,
  getFinvoiceEntrys,
} from '@shared/services/generated-service-endpoints/billing/FInvoice.endpoints';
import {
  addScanningEntrys,
  generateReportForEdi,
} from '@shared/services/generated-service-endpoints/economy/SupplierInvoice.endpoints';
import { map, Observable } from 'rxjs';
import { EdiEntryViewDTO } from '../models/edi.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class EdiService {
  constructor(
    private http: SoeHttpClient,
    private supplierService: SupplierService,
    private importFinvoiceService: ImportsInvoicesFinvoiceService
  ) {}

  getGridAdditionalProps = { classification: 0, originType: 0 };
  getGrid(
    id?: number,
    additionalProps?: {
      classification: number;
      originType: number;
    }
  ): Observable<EdiEntryViewDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http
      .get<
        EdiEntryViewDTO[]
      >(getEdiEntrysWithStateCheck(this.getGridAdditionalProps.classification, this.getGridAdditionalProps.originType))
      .pipe(
        map(data => {
          const ediRows: EdiEntryViewDTO[] = data as EdiEntryViewDTO[];
          ediRows.forEach((row: EdiEntryViewDTO) => {
            row.supplierNrName = row.supplierNr + ' ' + row.supplierName;
            if (!row.supplierNr) row.supplierNrName = '';
          });
          return ediRows;
        })
      );
  }

  getEdiEntryViews(
    classification: number,
    originType: number
  ): Observable<IEdiEntryViewDTO[]> {
    return this.http.get<IEdiEntryViewDTO[]>(
      getEdiEntrysWithStateCheck(classification, originType)
    );
  }

  getSuppliersDict(
    onlyActive = false,
    addEmptyRow = false,
    useCache = false
  ): Observable<ISmallGenericType[]> {
    return this.supplierService.getSupplierDict(
      onlyActive,
      addEmptyRow,
      useCache
    );
  }

  getFinvoiceEntryViews(
    classification: number,
    allItemsSelection: number,
    onlyUnHandled: boolean
  ): Observable<IEdiEntryViewDTO[]> {
    return this.http.get<IEdiEntryViewDTO[]>(
      getFinvoiceEntrys(classification, allItemsSelection, onlyUnHandled)
    );
  }

  // POST

  generateReportForEdi(ediEntries: number[]): Observable<any> {
    return this.http.post<number[]>(
      generateReportForEdi(ediEntries),
      ediEntries
    );
  }

  transferEdiToOrders(idsToTransfer: number[]) {
    return this.supplierService.transferEdiToOrder(idsToTransfer);
  }

  transferEdiToInvoices(idsToTransfer: number[]) {
    return this.supplierService.transferEdiToInvoices(idsToTransfer);
  }

  changeEdiState(idsToTransfer: number[], stateTo: number) {
    const model = {
      idsToTransfer: idsToTransfer,
      stateTo: stateTo,
    };

    return this.supplierService.transferEdiState(model);
  }

  addEdiEntrys(ediSourceType: number): Observable<any> {
    return this.http.post<BackendResponse>(
      addScanningEntrys(ediSourceType),
      ediSourceType
    );
  }

  updateEdiEntries(ediEntries: IUpdateEdiEntryDTO[]): Observable<any> {
    return this.supplierService.updateEdiEntrys(ediEntries);
  }

  getFilteredEdiEntryViews(
    ediEntryModel: IGetFilteredEDIEntrysModel
  ): Observable<IEdiEntryViewDTO[]> {
    return this.http.post(getFilteredEdiEntrys(), ediEntryModel);
  }

  importFinvoiceItems(dataStorageIds: number[]) {
    return this.importFinvoiceService.fileUpload(dataStorageIds);
  }
}
