import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SpEventService } from './sp-event.service';

describe('SpEventService', () => {
  let service: SpEventService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SpEventService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
