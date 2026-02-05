import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { PlanningPeriodsService } from './planning-periods.service';

describe('PlanningPeriodssService', () => {
  let service: PlanningPeriodsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(PlanningPeriodsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
