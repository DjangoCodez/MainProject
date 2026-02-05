import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { UiComponentsTestService } from './ui-components-test.service';

describe('UiComponentsTestService', () => {
  let service: UiComponentsTestService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(UiComponentsTestService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
