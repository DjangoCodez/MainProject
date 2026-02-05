import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { DrillDownReportsService } from './drill-down-reports.service';

describe('DrillDownReportsService', () => {
  let service: DrillDownReportsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(DrillDownReportsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
