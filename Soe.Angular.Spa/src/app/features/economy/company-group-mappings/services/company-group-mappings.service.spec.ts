import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { CompanyGroupMappingsService } from './company-group-mappings.service';

describe('CompanyGroupMappingsService', () => {
  let service: CompanyGroupMappingsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(CompanyGroupMappingsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
