import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { PaymentMethodDTO } from '../models/supplier-payment-methods.model';
import { Observable } from 'rxjs';
import {
  IPaymentInformationViewDTOSmall,
  IPaymentMethodSupplierGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  deletePaymentMethod,
  getPaymentInformationViewsSmall,
  getPaymentMethod,
  getPaymentMethodsSupplierGrid,
  getSysPaymentMethodsDict,
  savePaymentMethod,
} from '@shared/services/generated-service-endpoints/economy/SupplierPaymentMethod.endpoints';
import { SmallGenericType } from '@shared/models/generic-type.model';

@Injectable({
  providedIn: 'root',
})
export class SupplierPaymentMethodsService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    addEmptyRow: false,
    includePaymentInformationRows: false,
    includeAccount: false,
    onlyCashSales: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      addEmptyRow: boolean;
      includePaymentInformationRows: boolean;
      includeAccount: boolean;
      onlyCashSales: boolean;
    }
  ): Observable<IPaymentMethodSupplierGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IPaymentMethodSupplierGridDTO[]>(
      getPaymentMethodsSupplierGrid(
        this.getGridAdditionalProps.addEmptyRow,
        this.getGridAdditionalProps.includePaymentInformationRows,
        this.getGridAdditionalProps.includeAccount,
        this.getGridAdditionalProps.onlyCashSales,
        id
      )
    );
  }

  get(id: number): Observable<PaymentMethodDTO> {
    return this.http.get<PaymentMethodDTO>(getPaymentMethod(id, true, false));
  }

  getSysPaymentMethodsDict(
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getSysPaymentMethodsDict(addEmptyRow)
    );
  }

  getPaymentInformationViewsSmall(
    addEmptyRow: boolean
  ): Observable<IPaymentInformationViewDTOSmall[]> {
    return this.http.get<IPaymentInformationViewDTOSmall[]>(
      getPaymentInformationViewsSmall(addEmptyRow)
    );
  }

  save(model: PaymentMethodDTO): Observable<any> {
    return this.http.post<PaymentMethodDTO>(savePaymentMethod(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deletePaymentMethod(id));
  }
}
