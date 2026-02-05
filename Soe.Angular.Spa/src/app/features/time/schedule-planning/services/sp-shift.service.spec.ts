import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SpShiftService } from './sp-shift.service';

describe('SpShiftService', () => {
  let service: SpShiftService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SpShiftService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
