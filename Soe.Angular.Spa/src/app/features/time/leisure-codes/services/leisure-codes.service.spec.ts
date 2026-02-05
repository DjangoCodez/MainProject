import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { LeisureCodesService } from './leisure-codes.service';

describe('LeisureCodesService', () => {
  let service: LeisureCodesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(LeisureCodesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
