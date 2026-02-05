import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { PurchaseStatisticsService } from './purchase-statistics.service';

describe('PurchaseStatisticsService', () => {
  let service: PurchaseStatisticsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(PurchaseStatisticsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
