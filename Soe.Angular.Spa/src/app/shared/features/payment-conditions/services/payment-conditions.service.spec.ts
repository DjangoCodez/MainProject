import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { PaymentConditionsService } from './payment-conditions.service';

describe('PaymentConditionsService', () => {
  let service: PaymentConditionsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(PaymentConditionsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
