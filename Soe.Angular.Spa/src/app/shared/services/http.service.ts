import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, take } from 'rxjs/operators';
import { CacheService, ICacheOptions } from './cache.service';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { TranslateService } from '@ngx-translate/core';

export const CACHE_EXPIRE_VERY_SHORT = 5 * 60; // 5 minutes (used for data that might change often, but don't need fetching on every page load)
export const CACHE_EXPIRE_SHORT = 30 * 60; // 30 minutes (used for data that might change sometime every day, eg: Projects)
export const CACHE_EXPIRE_MEDIUM = 240 * 60; // 4 hours (used for data that change now and then, eg: Suppliers)
export const CACHE_EXPIRE_LONG = 480 * 60; // 8 hours (used for data that almost never change, eg: Vat codes)

export class CacheSettingsFactory {
  public static veryShort(): IGetRequestOptions {
    return {
      useCache: true,
      cacheOptions: { expires: CACHE_EXPIRE_VERY_SHORT },
    };
  }

  public static short(): IGetRequestOptions {
    return {
      useCache: true,
      cacheOptions: { expires: CACHE_EXPIRE_SHORT },
    };
  }

  public static medium(): IGetRequestOptions {
    return {
      useCache: true,
      cacheOptions: { expires: CACHE_EXPIRE_MEDIUM },
    };
  }

  public static long(): IGetRequestOptions {
    return {
      useCache: true,
      cacheOptions: { expires: CACHE_EXPIRE_LONG },
    };
  }
}

export interface IGetRequestOptions {
  acceptType?: string;
  useCache?: boolean;
  cacheOptions?: ICacheOptions;
}
// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface IPostRequestOptions {
  /** Add any post options */
}

// eslint-disable-next-line @typescript-eslint/no-empty-interface
export interface IDeleteRequestOptions {
  /** Add any delete options */
}

@Injectable({
  providedIn: 'root',
})
export class SoeHttpClient {
  private prefix = '';

  constructor(
    private http: HttpClient,
    private cache: CacheService
  ) {}

  public get<T>(endPoint: string, options?: IGetRequestOptions): Observable<T> {
    const completeEndPoint = this.prefix + endPoint;
    const useCache = options && options.useCache;

    if (!useCache) {
      return this.getFromServer(completeEndPoint, options);
    }

    const observable = new Observable<T>(observer => {
      this.getFromCache<T>(completeEndPoint, options.cacheOptions)
        .pipe(take(1))
        .subscribe(value => {
          if (value) {
            observer.next(value);
            observer.complete();
          } else {
            this.getFromServer<T>(completeEndPoint, options)
              //   .pipe(
              //     take(1),
              //     catchError(err => this.handleError(err, this.translate))
              //   )
              .subscribe(value => {
                this.cache.setCacheItem(
                  value,
                  completeEndPoint,
                  options.cacheOptions
                );
                observer.next(value);
                observer.complete();
              });
          }
        });
    });
    return observable;
  }

  // !!! Errors throught http is now handled in HttpErrorInterceptor (2025-11-20) !!!
  /*
  private handleError(error: HttpErrorResponse, translate: TranslateService) {
    if (error.status === 0) {
      // A client-side or network error occurred. Handle it accordingly.
      console.error('An error occurred:', error.error);
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong.
      console.error(
        `Backend returned code ${error.status}, body was: `,
        error.error
      );
    }
    // TODO: Does not seem to bubble up to calling component
    // It just stops here!

    // Return an observable with a user-facing error message.
    return throwError(
      () => new Error(translate.instant('error.default_error'))
    );
  }
    */

  public post<T>(
    endPoint: string,
    values: any,
    options?: IPostRequestOptions
  ): Observable<T> {
    const reqOptions = { headers: {} };
    const headersObject: any = {};
    // Set language in header according to configuration
    headersObject['Accept-Language'] = SoeConfigUtil.language;
    reqOptions.headers = headersObject;
    return this.http.post<T>(this.prefix + endPoint, values, reqOptions);
  }

  public delete<T>(
    endPoint: string,
    options?: IDeleteRequestOptions
  ): Observable<T> {
    const reqOptions = { headers: {} };
    const headersObject: any = {};
    // Set language in header according to configuration
    headersObject['Accept-Language'] = SoeConfigUtil.language;
    reqOptions.headers = headersObject;
    return this.http.delete<T>(this.prefix + endPoint, reqOptions);
  }

  private getFromServer<T>(endPoint: string, options: any): Observable<T> {
    const reqOptions = { headers: {} };
    const headersObject: any = {};
    // Set language in header according to configuration
    headersObject['Accept-Language'] = SoeConfigUtil.language;
    // Set accept in header according to options
    if (options?.acceptType) headersObject['Accept'] = options.acceptType;
    reqOptions.headers = headersObject;
    return this.http.get<T>(endPoint, reqOptions);
  }

  private getFromCache<T>(endpoint: string, options?: ICacheOptions) {
    return new Observable<T>(subscriber => {
      this.cache.getCacheItem<T>(subscriber, endpoint, options);
    });
  }
}
