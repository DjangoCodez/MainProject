import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SpEmployeeService } from './sp-employee.service';

describe('SpEmployeeService', () => {
  let service: SpEmployeeService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SpEmployeeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
