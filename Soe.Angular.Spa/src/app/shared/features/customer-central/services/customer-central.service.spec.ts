import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { CustomerCentralService } from './customer-central.service';

describe('CustomerCentralService', () => {
  let service: CustomerCentralService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(CustomerCentralService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
