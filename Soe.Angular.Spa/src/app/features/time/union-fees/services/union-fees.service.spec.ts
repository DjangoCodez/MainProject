import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { UnionFeesService } from './union-fees.service';

describe('UnionFeesService', () => {
  let service: UnionFeesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(UnionFeesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
