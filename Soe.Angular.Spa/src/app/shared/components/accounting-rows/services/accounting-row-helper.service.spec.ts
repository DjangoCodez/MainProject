import { TestBed } from '@angular/core/testing';

import { AccountingRowHelperService } from './accounting-row-helper.service';

describe('AccountingRowHelperServiceService', () => {
  let service: AccountingRowHelperService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AccountingRowHelperService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
