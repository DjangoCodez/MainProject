import { Injectable } from '@angular/core';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IHandleBillingRowDTO,
  IProjectTimeBlockDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  batchSplitTimeRows,
  getCustomers,
  getOrders,
  getProjects,
  orderRowChangeAttestState,
  searchCustomerInvoiceRows,
  transferOrdersToInvoice,
  getExpenseRows,
  getProjectTimeBlocksForInvoiceRow,
} from '@shared/services/generated-service-endpoints/billing/HandleBilling.endpoints';
import { Observable, of } from 'rxjs';
import {
  HandleBillingRowDTO,
  SearchCustomerInvoiceRowModel,
} from '../models/handle-billing.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class HandleBillingService {
  constructor(private http: SoeHttpClient) {}

  getOrders(): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getOrders());
  }

  getProjects(): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getProjects());
  }

  getCustomers(): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getCustomers());
  }

  getExpenseRows(
    customerInvoiceId: number,
    customerInvoiceRowId: number
  ): Observable<any[]> {
    return this.http.get<any[]>(
      getExpenseRows(customerInvoiceId, customerInvoiceRowId)
    );
  }

  getProjectTimeBlocksForInvoiceRow(
    invoiceId: number,
    customerInvoiceRowId: number
  ): Observable<IProjectTimeBlockDTO[]> {
    console.log('get project time blocks', invoiceId, customerInvoiceRowId);
    return this.http.get<IProjectTimeBlockDTO[]>(
      getProjectTimeBlocksForInvoiceRow(invoiceId, customerInvoiceRowId)
    );
  }

  getGrid(
    id?: number,
    additionalProps?: {
      model: SearchCustomerInvoiceRowModel;
    }
  ): Observable<HandleBillingRowDTO[]> {
    return this.http.post<HandleBillingRowDTO[]>(
      searchCustomerInvoiceRows(),
      additionalProps?.model
    );
  }

  changeAttestState(
    items: any[],
    attestStateId: number
  ): Observable<BackendResponse[]> {
    return this.http.post(orderRowChangeAttestState(), {
      items: items,
      attestStateId: attestStateId,
    });
  }

  transferToInvoice(
    ids: number[],
    merge: boolean,
    setStatusToOrigin: boolean,
    accountYearId: number
  ): Observable<BackendResponse[]> {
    return this.http.post(transferOrdersToInvoice(), {
      ids: ids,
      merge: merge,
      setStatusToOrigin: setStatusToOrigin,
      accountYearId: accountYearId,
    });
  }

  batchSplitTimeRows(
    items: any[],
    from: Date,
    to: Date
  ): Observable<BackendResponse[]> {
    return this.http.post(batchSplitTimeRows(), {
      items: items,
      from: from,
      to: to,
    });
  }
}
