import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { IntrastatExportService } from './intrastat-export.service';

describe('IntrastatExportService', () => {
  let service: IntrastatExportService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(IntrastatExportService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
