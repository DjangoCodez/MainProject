import { TestBed } from '@angular/core/testing';

import { TimeCodeInvoiceProductsService } from './time-code-invoice-products.service';

describe('TimeCodeInvoiceProductsService', () => {
  let service: TimeCodeInvoiceProductsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimeCodeInvoiceProductsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
