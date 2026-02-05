import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ExcelImportService } from './excel-import.service';

describe('ExcelImportService', () => {
  let service: ExcelImportService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ExcelImportService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
