import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SelectCustomerInvoiceService } from './select-customer-invoice.service';

describe('SelectCustomerInvoiceService', () => {
  let service: SelectCustomerInvoiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SelectCustomerInvoiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
