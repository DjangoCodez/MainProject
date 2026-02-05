import { TestBed } from '@angular/core/testing';

import { TimeCodeRanking } from './time-code-ranking';

describe('TimeCodeRanking', () => {
  let service: TimeCodeRanking;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimeCodeRanking);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
