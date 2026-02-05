import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SupplierInvoicesArrivalHallService } from './supplier-invoices-arrival-hall.service';

describe('SupplierInvoicesService', () => {
  let service: SupplierInvoicesArrivalHallService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SupplierInvoicesArrivalHallService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
