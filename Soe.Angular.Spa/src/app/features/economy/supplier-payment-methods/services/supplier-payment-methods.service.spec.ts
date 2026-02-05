import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SupplierPaymentMethodsService } from './supplier-payment-methods.service';

describe('SupplierPaymentMethodsService', () => {
  let service: SupplierPaymentMethodsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SupplierPaymentMethodsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
