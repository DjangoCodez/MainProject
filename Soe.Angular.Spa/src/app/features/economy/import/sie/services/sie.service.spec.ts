import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SieService } from './sie.service';

describe('SieService', () => {
  let service: SieService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SieService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
