import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ProjectWeekReportService } from './project-week-report.service';

describe('ProjectWeekReportService', () => {
  let service: ProjectWeekReportService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ProjectWeekReportService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
