import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { DeliveryConditionService } from './delivery-condition.service';

describe('DeliveryConditionService', () => {
  let service: DeliveryConditionService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(DeliveryConditionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
