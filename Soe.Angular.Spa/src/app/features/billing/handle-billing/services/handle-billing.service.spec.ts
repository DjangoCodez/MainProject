import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { HandleBillingService } from './handle-billing.service';

describe('HandleBillingService', () => {
  let service: HandleBillingService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(HandleBillingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
