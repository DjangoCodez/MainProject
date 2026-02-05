import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SelectCustomerService } from './select-customer.service';

describe('SelectCustomerService', () => {
  let service: SelectCustomerService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SelectCustomerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
