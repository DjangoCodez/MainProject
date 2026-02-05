import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ImportsInvoicesFinvoiceService } from './imports-invoices-finvoice.service';

describe('ImportsInvoicesFinvoiceService', () => {
  let service: ImportsInvoicesFinvoiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ImportsInvoicesFinvoiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
