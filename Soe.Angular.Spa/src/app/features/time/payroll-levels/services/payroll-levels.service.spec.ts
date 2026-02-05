import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { PayrollLevelsService } from './payroll-levels.service';

describe('PayrollLevelsService', () => {
  let service: PayrollLevelsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(PayrollLevelsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
