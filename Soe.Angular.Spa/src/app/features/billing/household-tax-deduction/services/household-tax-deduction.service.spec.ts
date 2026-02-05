import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { HouseholdTaxDeductionService } from './household-tax-deduction.service';

describe('HouseholdTaxDeductionService', () => {
  let service: HouseholdTaxDeductionService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(HouseholdTaxDeductionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
