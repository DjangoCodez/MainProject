import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { TimeWorkReductionService } from './time-work-reduction.service';

describe('TimeWorkReductionService', () => {
  let service: TimeWorkReductionService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(TimeWorkReductionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
