import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ImportConnectService } from './import-connect.service';

describe('ImportConnectService', () => {
  let service: ImportConnectService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ImportConnectService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
