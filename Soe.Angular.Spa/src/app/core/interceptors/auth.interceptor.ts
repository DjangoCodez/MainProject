import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { mergeMap, Observable } from 'rxjs';
import { AuthenticationService } from '../services/authentication/authentication.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  // this should probably get injected instead.
  // TL220905: Should be enough with '/api' prefix. That's how it's handled in AngularJS.
  baseUrl = '/api';

  constructor(private auth: AuthenticationService) {}

  intercept(
    req: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    const requestUrl = req.url.startsWith('/')
      ? `${this.baseUrl}${req.url}`
      : `${this.baseUrl}/${req.url}`;

    return this.auth.getHeaders().pipe(
      mergeMap(headers => {
        const keys = headers?.keys();
        const headerValues: Record<string, string> = {};
        keys?.forEach(x => (headerValues[x] = headers?.get(x)!));

        const authReq = req.clone({
          setHeaders: headerValues,
          url: requestUrl,
        });

        return next.handle(authReq);
      })
    );
  }
}
