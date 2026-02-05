import { TestBed } from '@angular/core/testing';

import { SpShiftRequestService } from './sp-shift-request.service';

describe('SpShiftRequestService', () => {
  let service: SpShiftRequestService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SpShiftRequestService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
