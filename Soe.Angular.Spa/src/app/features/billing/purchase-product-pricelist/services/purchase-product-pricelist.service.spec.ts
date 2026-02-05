import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { PurchaseProductPricelistService } from './purchase-product-pricelist.service';

describe('PurchaseProductPricelistService', () => {
  let service: PurchaseProductPricelistService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(PurchaseProductPricelistService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
