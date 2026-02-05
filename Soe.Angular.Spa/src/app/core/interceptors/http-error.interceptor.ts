import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ErrorUtil } from '@shared/util/error-util';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { catchError, Observable, throwError, tap } from 'rxjs';

@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {
  constructor(
    private messageboxService: MessageboxService,
    private translateService: TranslateService
  ) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      tap({
        next: (event: HttpEvent<any>) => {
          // We can log successful responses here if needed
          //console.log('HTTP Request Successful:', event);
        },
        error: (error: any) => {
          // We can log any errors here if needed
          //console.error('HTTP Request Error:', error);
        },
      }),
      catchError((error: HttpErrorResponse) => {
        const status = error.status || 0;

        // currently don't show the error for 400 and 401 errors. Those are handled in Perform class
        // where it is possible to override the default behavior.
        if (status !== 400 && status !== 401) {
          const errorResponse = ErrorUtil.createErrorResponse(
            error,
            this.translateService
          );
          const message = errorResponse.getTitleAndMessage();
          this.messageboxService.error(message.title, message.message, {
            hiddenText: message.hidden,
          });
        }
        return throwError(() => error);
      })
    );
  }
}
