/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { SysWholesalerPricesService } from './sys-wholesaler-prices.service';

describe('Service: SysWholesalerPrices', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [SysWholesalerPricesService]
    });
  });

  it('should ...', inject([SysWholesalerPricesService], (service: SysWholesalerPricesService) => {
    expect(service).toBeTruthy();
  }));
});
