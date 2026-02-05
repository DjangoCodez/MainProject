import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { AccountProvisionTransactionsService } from './account-provision-transactions.service';

describe('AccountProvisionTransactionsService', () => {
  let service: AccountProvisionTransactionsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(AccountProvisionTransactionsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
