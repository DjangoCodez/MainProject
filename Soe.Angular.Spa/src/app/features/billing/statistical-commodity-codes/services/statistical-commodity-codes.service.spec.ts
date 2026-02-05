import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { StatisticalCommodityCodesService } from './statistical-commodity-codes.service';

describe('StatisticalCommodityCodesService', () => {
  let service: StatisticalCommodityCodesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(StatisticalCommodityCodesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
