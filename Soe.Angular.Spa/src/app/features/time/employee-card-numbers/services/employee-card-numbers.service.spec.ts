import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { EmployeeCardNumbersService } from './employee-card-numbers.service';

describe('EmployeeCardNumbersService', () => {
  let service: EmployeeCardNumbersService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(EmployeeCardNumbersService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
