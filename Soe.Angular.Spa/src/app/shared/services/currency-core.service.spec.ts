import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { CurrencyCoreService } from './currency-core.service';

describe('CurrencyCoreService', () => {
  let service: CurrencyCoreService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(CurrencyCoreService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
