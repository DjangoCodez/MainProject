/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { ExternalCompanySearchService } from './external-company-search.service';

describe('Service: ExternalCompanySearch', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [ExternalCompanySearchService]
    });
  });

  it('should ...', inject([ExternalCompanySearchService], (service: ExternalCompanySearchService) => {
    expect(service).toBeTruthy();
  }));
});
