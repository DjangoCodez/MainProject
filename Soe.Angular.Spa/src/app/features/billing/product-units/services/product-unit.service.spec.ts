import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ProductUnitService } from './product-unit.service';

describe('ProductUnitService', () => {
  let service: ProductUnitService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ProductUnitService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
