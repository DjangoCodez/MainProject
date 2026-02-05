import { TestBed } from '@angular/core/testing';

import { TimeScheduleEventsService } from './time-schedule-events.service';

describe('TimeScheduleEvents', () => {
  let service: TimeScheduleEventsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimeScheduleEventsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
