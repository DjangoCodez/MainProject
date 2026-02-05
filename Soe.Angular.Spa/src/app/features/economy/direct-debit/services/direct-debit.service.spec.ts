import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { DirectDebitService } from './direct-debit.service';

describe('DirectDebitService', () => {
  let service: DirectDebitService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(DirectDebitService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
