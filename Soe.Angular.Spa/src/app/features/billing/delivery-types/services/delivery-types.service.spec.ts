import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { DeliveryTypesService } from './delivery-types.service';

describe('DeliveryTypesService', () => {
  let service: DeliveryTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(DeliveryTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
