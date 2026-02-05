import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { EdistributionService } from './edistribution.service';

describe('EdistributionService', () => {
  let service: EdistributionService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(EdistributionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
