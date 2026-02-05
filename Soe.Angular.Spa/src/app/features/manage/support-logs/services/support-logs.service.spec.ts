import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SupportLogsService } from './support-logs.service';

describe('SupportLogsService', () => {
  let service: SupportLogsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SupportLogsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
