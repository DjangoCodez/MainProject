import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { CustomerDiscountService } from './customer-discount.service';

describe('CustomerDiscountService', () => {
  let service: CustomerDiscountService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(CustomerDiscountService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
