import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { EmployeeGroupsService } from './employee-groups.service';

describe('EmployeeGroupsService', () => {
  let service: EmployeeGroupsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(EmployeeGroupsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
