import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { AccountingReconciliationService } from './accounting-reconciliation.service';

describe('AccountingReconciliationService', () => {
  let service: AccountingReconciliationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(AccountingReconciliationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
