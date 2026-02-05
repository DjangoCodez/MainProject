import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { AccountingLiquidityPlanningService } from './accounting-liquidity-planning.service';

describe('AccountingLiquidityPlanningService', () => {
  let service: AccountingLiquidityPlanningService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(AccountingLiquidityPlanningService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
