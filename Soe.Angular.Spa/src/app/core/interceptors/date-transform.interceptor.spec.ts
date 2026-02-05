import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { DateTransformInterceptor } from './date-transform.interceptor';

describe('DateTransformInterceptor', () => {
  beforeEach(() => TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
    providers: [
      DateTransformInterceptor
      ]
  }));

  it('should be created', () => {
    const interceptor: DateTransformInterceptor = TestBed.inject(DateTransformInterceptor);
    expect(interceptor).toBeTruthy();
  });
});
