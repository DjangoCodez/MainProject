import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { PurchaseDeliveryService } from './purchase-delivery.service';

describe('PurchaseDeliveryService', () => {
  let service: PurchaseDeliveryService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(PurchaseDeliveryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
