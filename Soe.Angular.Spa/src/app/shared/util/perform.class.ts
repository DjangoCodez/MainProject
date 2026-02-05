import { inject } from '@angular/core';
import { CrudActionTypeEnum } from '@shared/enums';
import { ProgressOptions, ProgressService } from '@shared/services/progress';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, take, tap } from 'rxjs/operators';
import { ErrorUtil } from './error-util';
import { TranslateService } from '@ngx-translate/core';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from './response-util';

export class Perform<T> {
  data: T | undefined;
  dataSubject = new BehaviorSubject<T | undefined>(undefined);
  action$: Observable<T> | undefined;
  crudAction$: Observable<BackendResponse> | undefined;
  catchErrorCallback: (err: any) => Observable<never> =
    this.defaultCatchErrorCallback.bind(this);

  get inProgress() {
    return this.progressService.inProgress;
  }

  get hasError() {
    return this.progressService.hasError;
  }

  messageboxService = inject(MessageboxService);
  translateService = inject(TranslateService);

  constructor(private progressService: ProgressService) {}

  load(action$: Observable<T>, options?: ProgressOptions): void {
    this.action$ = action$;
    this.progressService.load(options);

    this.action$
      .pipe(
        take(1),
        catchError((err: any) => {
          this.progressService.resetLoadCounter();
          return this.catchErrorCallback(err);
        })
      )
      .subscribe((data: T) => {
        this.progressService.loadComplete(options);
        this.data = data;
        this.dataSubject.next(data);
      });
  }

  load$(action$: Observable<T>, options?: ProgressOptions): Observable<T> {
    this.action$ = action$;
    this.progressService.load(options);

    return this.action$.pipe(
      take(1),
      catchError((err: any) => {
        console.trace(err);
        this.progressService.resetLoadCounter();
        return this.catchErrorCallback(err);
      }),
      tap((data: T) => {
        this.progressService.loadComplete(options);
        this.data = data;
        this.dataSubject.next(data);
      })
    );
  }

  crud(
    actionType: CrudActionTypeEnum,
    crudAction$: Observable<BackendResponse>,
    callback?: (val: BackendResponse) => any,
    errorCallback?: (val: BackendResponse) => void,
    options?: ProgressOptions
  ): void {
    this.crudAction$ = crudAction$;

    if (actionType === CrudActionTypeEnum.Load) {
      this.progressService.load(options);
    } else if (actionType === CrudActionTypeEnum.Save) {
      this.progressService.save(options);
    } else if (actionType === CrudActionTypeEnum.Delete) {
      this.progressService.delete(options);
    } else if (actionType === CrudActionTypeEnum.Work) {
      this.progressService.work(options);
    }

    this.crudAction$
      .pipe(
        take(1),
        catchError((err: any) => {
          if (actionType === CrudActionTypeEnum.Load)
            this.progressService.resetLoadCounter();

          this.progressService.hideDialog(); // close any open progress dialog on error
          return this.catchErrorCallback(err);
        })
      )
      .subscribe((data: BackendResponse) => {
        if (data.success) {
          if (actionType === CrudActionTypeEnum.Load) {
            this.progressService.loadComplete(options);
          } else if (actionType === CrudActionTypeEnum.Save) {
            this.progressService.saveComplete(
              options,
              ResponseUtil.getObjectsAffected(data)
            );
          } else if (actionType === CrudActionTypeEnum.Delete) {
            this.progressService.deleteComplete(options);
          } else if (actionType === CrudActionTypeEnum.Work) {
            this.progressService.workComplete(options);
          }

          if (callback) callback(data);
        } else {
          if (!options) options = {};
          options.message = ResponseUtil.getErrorMessage(data);

          if (actionType === CrudActionTypeEnum.Load) {
            this.progressService.loadError(options);
          } else if (actionType === CrudActionTypeEnum.Save) {
            this.progressService.saveError(options);
          } else if (actionType === CrudActionTypeEnum.Delete) {
            this.progressService.deleteError(options);
          } else if (actionType === CrudActionTypeEnum.Work) {
            this.progressService.workError(options);
          }

          if (errorCallback) errorCallback(data);
        }
      });
  }

  // set your custom catch error callback function through this method
  setCatchErrorCallback(fnc: (err: any) => Observable<never>): void {
    this.catchErrorCallback = fnc;
  }

  // Default catch error callback - can be overridden by setCatchErrorCallback.
  // If no error should be thrown further when overriding, just return EMPTY in your custom callback function.
  private defaultCatchErrorCallback(err: any): Observable<never> {
    const status = err.status || 0;

    // currently only handle 400 and 401 errors here. Others are handled in HttpErrorInterceptor.
    if (status === 400 || status === 401) {
      const errorResponse = ErrorUtil.createErrorResponse(
        err,
        this.translateService
      );
      const message = errorResponse.getTitleAndMessage();

      this.messageboxService.error(message.title, message.message, {
        hiddenText: message.hidden,
      });
    }
    return throwError(() => new Error(err.message));
  }
}
