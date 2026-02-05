import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { AccountProvisionBaseService } from './account-provision-base.service';

describe('AccountProvisionBaseService', () => {
  let service: AccountProvisionBaseService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(AccountProvisionBaseService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
