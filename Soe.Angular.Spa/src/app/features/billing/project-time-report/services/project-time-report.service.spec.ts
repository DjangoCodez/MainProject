import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ProjectTimeReportService } from './project-time-report.service';

describe('TimeSheetsUserService', () => {
  let service: ProjectTimeReportService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ProjectTimeReportService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
