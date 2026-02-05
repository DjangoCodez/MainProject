import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { CompanyGroupTransferService } from './company-group-transfer.service';

describe('CompanyGroupTransferService', () => {
  let service: CompanyGroupTransferService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(CompanyGroupTransferService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
