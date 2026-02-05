import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { SupplierInvoiceCostAllocationDTO } from '../models/supplier-invoice.model';

@Injectable({
  providedIn: 'root',
})
export class SupplierInvoiceReBilledService {
  getGrid(id?: number): Observable<SupplierInvoiceCostAllocationDTO[]> {
    return of([]);
  }
}
