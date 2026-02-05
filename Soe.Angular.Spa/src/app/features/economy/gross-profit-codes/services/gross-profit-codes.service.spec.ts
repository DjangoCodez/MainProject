import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { GrossProfitCodesService } from './gross-profit-codes.service';

describe('GrossProfitCodesService', () => {
  let service: GrossProfitCodesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(GrossProfitCodesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
