import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteTimeCode,
  getTimeCode,
  getTimeCodes,
  getTimeCodesGrid,
  saveTimeCode,
  updateTimeCodeState,
} from '@shared/services/generated-service-endpoints/time/TimeCode.endpoints';
import { SoeTimeCodeType } from '@shared/models/generated-interfaces/Enumerations';
import { TimeCodeMaterialDTO } from '../models/material-codes.model';
import {
  ITimeCodeDTO,
  ITimeCodeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IUpdateEntityStatesModel } from '@shared/models/generated-interfaces/CoreModels';
import { getInvoiceProducts } from '@shared/services/generated-service-endpoints/billing/InvoiceProduct.endpoints';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class MaterialCodesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ITimeCodeGridDTO[]> {
    return this.http.get<ITimeCodeGridDTO[]>(
      getTimeCodesGrid(SoeTimeCodeType.Material, false, false, id)
    );
  }

  get(timeCodeId: number): Observable<TimeCodeMaterialDTO> {
    return this.http.get<TimeCodeMaterialDTO>(
      getTimeCode(
        SoeTimeCodeType.Material,
        timeCodeId,
        true,
        false,
        false,
        false
      )
    );
  }

  getTimeCodes(
    timeCodeType: number,
    onlyActive: boolean,
    loadPayrollProducts: boolean,
    onlyWithInvoiceProduct: boolean
  ): Observable<ITimeCodeDTO[]> {
    return this.http.get<ITimeCodeDTO[]>(
      getTimeCodes(
        timeCodeType,
        onlyActive,
        loadPayrollProducts,
        onlyWithInvoiceProduct
      )
    );
  }

  getInvoiceProducts(
    invoiceProductVatType: number,
    addEmptyRow: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getInvoiceProducts(invoiceProductVatType, addEmptyRow)
    );
  }

  save(model: TimeCodeMaterialDTO): Observable<BackendResponse> {
    return this.http.post(saveTimeCode(), model);
  }
  updateTimeCodeState(
    model: IUpdateEntityStatesModel
  ): Observable<BackendResponse> {
    return this.http.post(updateTimeCodeState(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteTimeCode(id));
  }
}
