import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ChangeIntrastatCodeService } from './change-intrastat-code.service';

describe('ChangeIntrastatCodeService', () => {
  let service: ChangeIntrastatCodeService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ChangeIntrastatCodeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
