import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { InformationService } from './information.service';

describe('InformationService', () => {
  let service: InformationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(InformationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
