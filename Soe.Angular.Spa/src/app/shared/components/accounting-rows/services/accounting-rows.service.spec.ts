import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { AccountingRowsService } from './accounting-rows.service';

describe('AccountingRowsService', () => {
  let service: AccountingRowsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(AccountingRowsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
