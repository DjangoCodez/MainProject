import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SaftService } from './saft.service';

describe('SaftService', () => {
  let service: SaftService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SaftService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
