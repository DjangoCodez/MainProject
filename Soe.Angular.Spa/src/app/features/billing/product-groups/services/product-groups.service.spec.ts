import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ProductGroupsService } from './product-groups.service';

describe('ProductGroupsService', () => {
  let service: ProductGroupsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ProductGroupsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
