import { TestBed } from '@angular/core/testing';

import { SupplierInvoiceOverviewService } from './supplier-invoice-overview.service';

describe('SupplierInvoiceOverviewService', () => {
  let service: SupplierInvoiceOverviewService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SupplierInvoiceOverviewService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
