import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { EarnedHolidayService } from './earned-holiday.service';

describe('EarnedHolidayService', () => {
  let service: EarnedHolidayService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(EarnedHolidayService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
