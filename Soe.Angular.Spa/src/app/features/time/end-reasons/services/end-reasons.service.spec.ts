import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { EndReasonsService } from './end-reasons.service';

describe('EndReasonsService', () => {
  let service: EndReasonsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(EndReasonsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
