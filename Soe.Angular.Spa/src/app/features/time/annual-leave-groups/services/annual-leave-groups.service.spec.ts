import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { AnnualLeaveGroupsService } from './annual-leave-groups.service';

describe('AnnualLeaveGroupsService', () => {
  let service: AnnualLeaveGroupsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(AnnualLeaveGroupsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
