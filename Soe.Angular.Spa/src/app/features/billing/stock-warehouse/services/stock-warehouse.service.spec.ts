import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { StockWarehouseService } from './stock-warehouse.service';

describe('WarehouseCodeService', () => {
  let service: StockWarehouseService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
    });
    service = TestBed.inject(StockWarehouseService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
