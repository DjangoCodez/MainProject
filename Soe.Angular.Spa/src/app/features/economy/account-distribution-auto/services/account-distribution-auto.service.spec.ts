import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { AccountDistributionAutoService } from './account-distribution-auto.service';

describe('AccountDistributionAutoService', () => {
  let service: AccountDistributionAutoService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(AccountDistributionAutoService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
