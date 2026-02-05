import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SysEdiMessageHeadService } from './sys-edi-message-head.service';

describe('SysEdiMessageHeadService', () => {
  let service: SysEdiMessageHeadService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SysEdiMessageHeadService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
