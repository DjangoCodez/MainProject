import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { EmployeeCsrExportService } from './employee-csr-export.service';

describe('EmployeeCsrExportService', () => {
  let service: EmployeeCsrExportService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(EmployeeCsrExportService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
