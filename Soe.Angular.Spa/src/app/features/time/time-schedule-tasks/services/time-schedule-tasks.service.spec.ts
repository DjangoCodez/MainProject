import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { TimeScheduleTasksService } from './time-schedule-tasks.service';

describe('TimeScheduleTasksService', () => {
  let service: TimeScheduleTasksService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(TimeScheduleTasksService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
