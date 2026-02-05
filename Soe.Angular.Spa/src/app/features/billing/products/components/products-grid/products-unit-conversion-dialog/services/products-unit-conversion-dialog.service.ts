import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { ProductUnitFileModel } from '../models/products-unit-conversion-dialog.models';
import {
  parseProductUnitConversionFile,
  saveProductUnitConvert,
} from '@shared/services/generated-service-endpoints/billing/ProductUnitConvert.endpoints';
import { Observable } from 'rxjs';
import { IProductUnitConvertDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ProductsUnitConversionService {
  constructor(private http: SoeHttpClient) {}

  parseUnitConversionFile(
    ids: number[],
    fileData: unknown
  ): Observable<IProductUnitConvertDTO[]> {
    const obj = new ProductUnitFileModel(ids, [fileData]);
    return this.http.post<IProductUnitConvertDTO[]>(
      parseProductUnitConversionFile(),
      obj
    );
  }

  saveProductUnitConvert(
    rows: IProductUnitConvertDTO[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveProductUnitConvert(), rows);
  }
}
