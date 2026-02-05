import { TestBed } from '@angular/core/testing';

import { AdjustTimeStampsService } from './adjust-time-stamps.service';

describe('AdjustTimeStampsService', () => {
  let service: AdjustTimeStampsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdjustTimeStampsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
