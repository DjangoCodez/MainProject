import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { CustomerPaymentMethodsService } from './customer-payment-methods.service';

describe('CustomerPaymentMethodsService', () => {
  let service: CustomerPaymentMethodsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(CustomerPaymentMethodsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
