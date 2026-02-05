import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { TimeDeviationCausesService } from './time-deviation-causes.service';

describe('TimeDeviationCausesService', () => {
  let service: TimeDeviationCausesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(TimeDeviationCausesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
