import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { VoucherService } from './voucher.service';

describe('VoucherService', () => {
  let service: VoucherService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(VoucherService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
