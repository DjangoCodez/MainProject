import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { CustomerProductPricelistsTypeService } from './customer-product-priceliststype.service';

describe('CustomerProductPricelistsService', () => {
  let service: CustomerProductPricelistsTypeService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(CustomerProductPricelistsTypeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
