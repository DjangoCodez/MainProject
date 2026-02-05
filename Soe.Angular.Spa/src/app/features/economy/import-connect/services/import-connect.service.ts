import { Injectable } from '@angular/core';
import {
  SoeModule,
  TermGroup_IOImportHeadType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IImportDTO,
  ISupplierIODTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  ISysImportDefinitionDTO,
  ISysImportHeadDTO,
} from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteImport,
  getImport,
  getImportGridColumns,
  getImportIOResult,
  getImports,
  getImportSelectionGrid,
  getSysImportDefinitions,
  getSysImportHeads,
  importFile,
  importIO,
  saveCustomerInvoiceIODTO,
  saveCustomerInvoiceRowIODTO,
  saveCustomerIODTO,
  saveImport,
  saveProjectIODTO,
  saveSupplieInvoiceIODTO,
  saveSupplierIODTO,
  saveVoucherIODTO,
} from '@shared/services/generated-service-endpoints/core/Connect.endpoints';
import { Observable } from 'rxjs';
import { ImportGridColumnDTO } from '../models/import-grid-columns-dto.model';
import { ICustomerIODTO } from '@shared/models/generated-interfaces/CustomerDTO';
import {
  ICustomerInvoiceIODTO,
  ICustomerInvoiceRowIODTO,
} from '@shared/models/generated-interfaces/CustomerInvoiceIODTOs';
import { ISupplierInvoiceHeadIODTO } from '@shared/models/generated-interfaces/SupplierInvoiceIODTOs';
import { IVoucherHeadIODTO } from '@shared/models/generated-interfaces/VoucherHeadDTOs';
import { IProjectIODTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { FilesLookupDTO } from '@shared/models/file.model';
import { IImportSelectionGridRowDTO } from '@shared/models/generated-interfaces/ImportSelectionGridRowDTO';
import { IFilesLookupDTO } from '@shared/models/generated-interfaces/FilesLookupDTO';
import { checkForDuplicates } from '@shared/services/generated-service-endpoints/core/File.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ImportConnectService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    module: SoeModule.None,
  };
  getGrid(
    id?: number,
    additionalProps?: { module: SoeModule }
  ): Observable<IImportDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IImportDTO[]>(
      getImports(this.getGridAdditionalProps.module)
    );
  }

  get(id: number): Observable<IImportDTO> {
    return this.http.get<IImportDTO>(getImport(id));
  }

  save(model: IImportDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveImport(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteImport(id));
  }

  getSysImportDefinitions(
    module: SoeModule
  ): Observable<ISysImportDefinitionDTO[]> {
    return this.http.get<ISysImportDefinitionDTO[]>(
      getSysImportDefinitions(module)
    );
  }

  getSysImportHeads(): Observable<ISysImportHeadDTO[]> {
    return this.http.get<ISysImportHeadDTO[]>(getSysImportHeads());
  }

  getImportGridColumns(
    importHeadType: number
  ): Observable<ImportGridColumnDTO[]> {
    return this.http.get<ImportGridColumnDTO[]>(
      getImportGridColumns(importHeadType)
    );
  }

  getImportSelectionGrid(
    files: FilesLookupDTO
  ): Observable<IImportSelectionGridRowDTO[]> {
    return this.http.post<IImportSelectionGridRowDTO[]>(
      getImportSelectionGrid(),
      files
    );
  }

  /* eslint-disable @typescript-eslint/no-explicit-any */
  getImportIOResult(importHeadType: number, batchId: string): Observable<any> {
    return this.http.get<any>(getImportIOResult(importHeadType, batchId));
  }
  /* eslint-enable @typescript-eslint/no-explicit-any */

  saveCustomerIODTO(
    customerDTOs: ICustomerIODTO[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveCustomerIODTO(), customerDTOs);
  }

  saveCustomerInvoiceHeadIODTO(
    customerInvoiceHeadIODTOs: ICustomerInvoiceIODTO[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveCustomerInvoiceIODTO(),
      customerInvoiceHeadIODTOs
    );
  }

  saveCustomerInvoiceRowIODTO(
    customerInvoiceRowDTOs: ICustomerInvoiceRowIODTO[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveCustomerInvoiceRowIODTO(),
      customerInvoiceRowDTOs
    );
  }

  saveSupplierIODTO(
    supplierDTOs: ISupplierIODTO[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveSupplierIODTO(), supplierDTOs);
  }

  saveSupplierInvoiceHeadIODTO(
    supplierInvoiceDTOs: ISupplierInvoiceHeadIODTO[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveSupplieInvoiceIODTO(),
      supplierInvoiceDTOs
    );
  }

  saveVoucherHeadIODTO(
    voucherDTOs: IVoucherHeadIODTO[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveVoucherIODTO(), voucherDTOs);
  }

  saveProjectIODTO(projectDTOs: IProjectIODTO[]): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveProjectIODTO(), projectDTOs);
  }

  importIO(
    importHeadType: TermGroup_IOImportHeadType,
    ioIds: number[],
    useAccountDistribution?: boolean,
    useAccountDims?: boolean,
    defaultDim2AccountId?: number,
    defaultDim3AccountId?: number,
    defaultDim4AccountId?: number,
    defaultDim5AccountId?: number,
    defaultDim6AccountId?: number
  ): Observable<BackendResponse> {
    const model = {
      importHeadType: importHeadType,
      ioIds: ioIds,
      useAccountDistribution: useAccountDistribution,
      useAccoungDims: useAccountDims,
      defaultDim2AccountId: defaultDim2AccountId,
      defaultDim3AccountId: defaultDim3AccountId,
      defaultDim4AccountId: defaultDim4AccountId,
      defaultDim5AccountId: defaultDim5AccountId,
      defaultDim6AccountId: defaultDim6AccountId,
    };
    return this.http.post<BackendResponse>(importIO(), model);
  }

  importFiles(
    importId: number,
    dataStorageIds: number[],
    accountYearId: number,
    voucherSeriesId: number,
    importDefinitionId: number
  ): Observable<BackendResponse> {
    const model = {
      importId: importId,
      dataStorageIds: dataStorageIds,
      accountYearId: accountYearId,
      voucherSeriesId: voucherSeriesId,
      importDefinitionId: importDefinitionId,
    };
    return this.http.post<BackendResponse>(importFile(), model);
  }

  checkForDuplicates(fileLookup: IFilesLookupDTO) {
    return this.http.post<string[]>(checkForDuplicates(), fileLookup);
  }
}
