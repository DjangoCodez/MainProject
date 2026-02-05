import { inject, Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { SupplierInvoiceCostAllocationDTO } from '../models/supplier-invoice.model';
import { SoeHttpClient } from '@shared/services/http.service';

@Injectable({
  providedIn: 'root',
})
export class SupplierInvoiceChargedToProjectService {
  private readonly http = inject(SoeHttpClient);

  getGrid(id?: number): Observable<SupplierInvoiceCostAllocationDTO[]> {
    return of([]);
  }
}
