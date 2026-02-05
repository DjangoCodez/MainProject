import { TestBed } from '@angular/core/testing';

import { TimeCodePayrollProductsService } from './time-code-payroll-products.service';

describe('TimeCodePayrollProductsService', () => {
  let service: TimeCodePayrollProductsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimeCodePayrollProductsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
