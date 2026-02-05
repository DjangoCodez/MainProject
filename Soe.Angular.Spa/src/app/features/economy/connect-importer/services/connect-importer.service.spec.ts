import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ConnectImporterService } from './connect-importer.service';

describe('ConnectImporterService', () => {
  let service: ConnectImporterService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ConnectImporterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
