import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { PayrollPriceTypesService } from './payroll-price-types.service';

describe('PayrollPriceTypesService', () => {
  let service: PayrollPriceTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(PayrollPriceTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
