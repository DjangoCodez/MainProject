import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { AnnualLeaveBalanceService } from './annual-leave-balance.service';

describe('AnnualLeaveBalanceService', () => {
  let service: AnnualLeaveBalanceService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(AnnualLeaveBalanceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
