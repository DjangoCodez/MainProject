import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { PriceBasedMarkupService } from './price-based-markup.service';

describe('PriceBasedMarkupService', () => {
  let service: PriceBasedMarkupService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(PriceBasedMarkupService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
