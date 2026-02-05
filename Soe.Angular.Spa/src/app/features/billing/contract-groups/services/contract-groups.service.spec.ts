import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ContractGroupsService } from './contract-groups.service';

describe('ContractGroupsService', () => {
  let service: ContractGroupsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ContractGroupsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
