import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { DistributionCodesService } from './distribution-codes.service';

describe('DistributionCodesService', () => {
  let service: DistributionCodesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(DistributionCodesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
