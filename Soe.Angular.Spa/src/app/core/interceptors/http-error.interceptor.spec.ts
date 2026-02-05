import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { HttpErrorInterceptor } from './http-error.interceptor';

describe('HttpErrorInterceptor', () => {
  beforeEach(() =>
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [HttpErrorInterceptor],
    })
  );

  it('should be created', () => {
    const interceptor: HttpErrorInterceptor =
      TestBed.inject(HttpErrorInterceptor);
    expect(interceptor).toBeTruthy();
  });
});
