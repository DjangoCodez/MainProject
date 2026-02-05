import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { EdiService } from './edi.service';

describe('EdiService', () => {
  let service: EdiService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(EdiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
