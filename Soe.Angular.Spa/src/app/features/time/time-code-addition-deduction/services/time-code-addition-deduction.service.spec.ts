import { TestBed } from '@angular/core/testing';

import { TimeCodeAdditionDeductionService } from './time-code-addition-deduction.service';

describe('TimeCodeAdditionDeductionService', () => {
  let service: TimeCodeAdditionDeductionService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimeCodeAdditionDeductionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
