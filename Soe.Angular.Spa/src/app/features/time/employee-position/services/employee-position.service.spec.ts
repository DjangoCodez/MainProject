import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { EmployeePositionService } from './employee-position.service';

describe('EmployeePositionService', () => {
  let service: EmployeePositionService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(EmployeePositionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
