import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { CommodityCodesService } from './commodity-codes.service';

describe('CommodityCodesService', () => {
  let service: CommodityCodesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(CommodityCodesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
