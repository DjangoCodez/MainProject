import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { LeisureCodeTypesService } from './leisure-code-types.service';

describe('LeisureCodeTypesService', () => {
  let service: LeisureCodeTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(LeisureCodeTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
