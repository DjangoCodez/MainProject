import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SpFilterService } from './sp-filter.service';

describe('SpFilterService', () => {
  let service: SpFilterService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SpFilterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
