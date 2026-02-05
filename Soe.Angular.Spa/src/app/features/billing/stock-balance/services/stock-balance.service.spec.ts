import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { StockBalanceService } from './stock-balance.service';

describe('StockBalanceService', () => {
  let service: StockBalanceService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(StockBalanceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
