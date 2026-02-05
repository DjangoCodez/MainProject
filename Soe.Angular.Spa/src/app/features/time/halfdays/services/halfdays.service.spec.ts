import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { HalfdaysService } from './halfdays.service';

describe('HalfdaysService', () => {
  let service: HalfdaysService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(HalfdaysService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
