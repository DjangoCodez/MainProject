import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SchoolHolidayService } from './school-holiday.service';

describe('SchoolHolidayService', () => {
  let service: SchoolHolidayService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SchoolHolidayService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
