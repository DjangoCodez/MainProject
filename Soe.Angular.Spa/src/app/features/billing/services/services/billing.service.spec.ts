import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { BillingService } from './billing.service';

describe('ManageService', () => {
  let service: BillingService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(BillingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
