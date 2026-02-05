import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { OpeningHoursService } from './opening-hours.service';

describe('OpeningHoursService', () => {
  let service: OpeningHoursService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(OpeningHoursService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
