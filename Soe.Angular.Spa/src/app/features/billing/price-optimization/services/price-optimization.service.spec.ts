import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { PriceOptimizationService } from './price-optimization.service';

describe('PriceOptimizationService', () => {
  let service: PriceOptimizationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
    });
    service = TestBed.inject(PriceOptimizationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
