import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { TimeScheduleTypeService } from './time-schedule-type.service';

describe('TimeScheduleTypeService', () => {
  let service: TimeScheduleTypeService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(TimeScheduleTypeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
