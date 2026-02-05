import { Injectable } from '@angular/core';
import { IPaymentMethodCustomerGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deletePaymentMethod,
  getPaymentMethod,
  getPaymentMethodsCustomerGrid,
  savePaymentMethod,
  getSysPaymentMethodsDict,
  getPaymentInformationViewsDict,
} from '@shared/services/generated-service-endpoints/economy/CustomerPaymentMethod.endpoints';
import { Observable } from 'rxjs';
import { PaymentMethodDTO } from '../models/customer-payment-methods.model';
import { SmallGenericType } from '@shared/models/generic-type.model';

@Injectable({
  providedIn: 'root',
})
export class CustomerPaymentMethodsService {
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
  ): Observable<IPaymentMethodCustomerGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IPaymentMethodCustomerGridDTO[]>(
      getPaymentMethodsCustomerGrid(
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

  getPaymentInformationViewsDict(
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getPaymentInformationViewsDict(addEmptyRow)
    );
  }

  save(model: PaymentMethodDTO): Observable<any> {
    return this.http.post<PaymentMethodDTO>(savePaymentMethod(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deletePaymentMethod(id));
  }
}
