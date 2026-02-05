import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { IncomingDeliveryTypesService } from './incoming-delivery-types.service';

describe('IncomingDeliveryTypesService', () => {
  let service: IncomingDeliveryTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(IncomingDeliveryTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
