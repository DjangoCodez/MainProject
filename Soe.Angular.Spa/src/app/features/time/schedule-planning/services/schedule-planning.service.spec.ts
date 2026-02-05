import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SchedulePlanningService } from './schedule-planning.service';

describe('SchedulePlanningService', () => {
  let service: SchedulePlanningService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SchedulePlanningService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
