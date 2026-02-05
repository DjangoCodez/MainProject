import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SpTranslateService } from './sp-translate.service';

describe('SpTranslateService', () => {
  let service: SpTranslateService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SpTranslateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
