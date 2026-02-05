import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SupplierInvoiceHistoryService } from './supplier-invoice-history.service';

describe('SupplierInvoiceHistoryService', () => {
  let service: SupplierInvoiceHistoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SupplierInvoiceHistoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
