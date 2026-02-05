import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { TimeCodeBreakGroupService } from './time-code-break-group.service';

describe('TimeCodeBreakGroupService', () => {
  let service: TimeCodeBreakGroupService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(TimeCodeBreakGroupService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
