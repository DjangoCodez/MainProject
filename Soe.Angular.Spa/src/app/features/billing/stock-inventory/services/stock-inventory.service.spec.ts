import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { StockInventoryService } from './stock-inventory.service';

describe('StockInventoryService', () => {
  let service: StockInventoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(StockInventoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
