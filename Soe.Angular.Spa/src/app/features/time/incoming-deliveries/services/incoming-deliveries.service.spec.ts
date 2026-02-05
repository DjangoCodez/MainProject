import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { IncomingDeliveriesService } from './incoming-deliveries.service';

describe('IncomingDeliveriesService', () => {
  let service: IncomingDeliveriesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(IncomingDeliveriesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
