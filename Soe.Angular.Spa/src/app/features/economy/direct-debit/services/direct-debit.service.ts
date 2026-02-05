import { Injectable } from '@angular/core';
import {
  InvoiceExportDTO,
  InvoiceExportIODTO,
} from '../models/direct-debit.model';
import { Observable } from 'rxjs';
import { SoeHttpClient } from '@shared/services/http.service';
import { IInvoiceExportDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  getExportedIOInvoices,
  getInvoicesForPaymentService,
  getPaymentServiceRecords,
  saveCustomerInvoicePaymentService,
} from '@shared/services/generated-service-endpoints/economy/ExportFiles.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class DirectDebitService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IInvoiceExportDTO[]> {
    return this.http.get<IInvoiceExportDTO[]>(getPaymentServiceRecords(id));
  }

  get(id: number): Observable<InvoiceExportIODTO[]> {
    return this.http.get<InvoiceExportIODTO[]>(getExportedIOInvoices(id));
  }

  save(model: InvoiceExportDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>('', model);
  }

  saveCustomerPaymentService(
    invoices: InvoiceExportIODTO[],
    paymentServiceId: number
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveCustomerInvoicePaymentService(paymentServiceId),
      invoices
    );
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete('deleteStock(id)');
  }

  getInvoicesForPaymentService(
    paymentServiceId: number
  ): Observable<InvoiceExportIODTO[]> {
    return this.http.get<InvoiceExportIODTO[]>(
      getInvoicesForPaymentService(paymentServiceId)
    );
  }
}
