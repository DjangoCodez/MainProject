import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { StockPurchaseService } from './stock-purchase.service';

describe('StockPurchaseService', () => {
  let service: StockPurchaseService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(StockPurchaseService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
