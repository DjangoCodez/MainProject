import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SalesStatisticsService } from './sales-statistics.service';

describe('SalesStatisticsService', () => {
  let service: SalesStatisticsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SalesStatisticsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
