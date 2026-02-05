/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { PurchaseProductsService } from './purchase-products.service';

describe('Service: PurchaseProducts', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [PurchaseProductsService]
    });
  });

  it('should ...', inject([PurchaseProductsService], (service: PurchaseProductsService) => {
    expect(service).toBeTruthy();
  }));
});
