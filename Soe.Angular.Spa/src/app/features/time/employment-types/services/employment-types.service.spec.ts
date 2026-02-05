import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { EmploymentTypesService } from './employment-types.service';

describe('EmploymentTypesService', () => {
  let service: EmploymentTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(EmploymentTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
