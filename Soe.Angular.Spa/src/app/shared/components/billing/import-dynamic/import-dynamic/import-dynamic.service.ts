import { Injectable } from '@angular/core';
import { IActionResult } from '@shared/models/generated-interfaces/ActionResult';
import {
  IImportDynamicDTO,
  IImportDynamicFileUploadDTO,
} from '@shared/models/generated-interfaces/ImportDynamicDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import { getSupplierPricelistImport } from '@shared/services/generated-service-endpoints/billing/SupplierProductPriceList.endpoints';
import {
  getFileContent,
  parseRows,
} from '@shared/services/generated-service-endpoints/core/ImportDynamic.endpoints';
import { Observable } from 'rxjs';
import { ParseRowsModel, ParseRowsResult } from './import-dynamic.model';

@Injectable({ providedIn: 'root' })
export class ImportDynamicService {
  constructor(private http: SoeHttpClient) {}

  getSupplierPricelistImport(
    importToPriceList: boolean,
    importPrices: boolean,
    multipleSuppliers: boolean
  ): Observable<IImportDynamicDTO> {
    return this.http.get<IImportDynamicDTO>(
      getSupplierPricelistImport(
        importToPriceList,
        importPrices,
        multipleSuppliers
      )
    );
  }

  getFileContent(
    fileType: number,
    model: IImportDynamicFileUploadDTO
  ): Observable<IActionResult> {
    return this.http.post(getFileContent(fileType), model);
  }

  parseRows(model: ParseRowsModel): Observable<ParseRowsResult[]> {
    return this.http.post(parseRows(), model);
  }
}
