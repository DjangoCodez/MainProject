import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { StringTransformInterceptor } from './string-transform.interceptor';

describe('StringTransformInterceptor', () => {
  beforeEach(() =>
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [StringTransformInterceptor],
    })
  );

  it('should be created', () => {
    const interceptor: StringTransformInterceptor = TestBed.inject(
      StringTransformInterceptor
    );
    expect(interceptor).toBeTruthy();
  });
});
