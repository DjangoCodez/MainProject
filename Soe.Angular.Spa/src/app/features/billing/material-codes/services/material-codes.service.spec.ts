import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { MaterialCodesService } from './material-codes.service';

describe('MaterialCodesService', () => {
  let service: MaterialCodesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(MaterialCodesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
