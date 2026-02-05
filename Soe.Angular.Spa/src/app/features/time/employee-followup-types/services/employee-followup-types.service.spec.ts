import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { EmployeeFollowupTypesService } from './employee-followup-types.service';

describe('EmployeeFollowupTypesService', () => {
  let service: EmployeeFollowupTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(EmployeeFollowupTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
