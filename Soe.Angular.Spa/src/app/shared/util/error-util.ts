import { TranslateService } from '@ngx-translate/core';

// TODO: Should perhaps be ServiceResponse later?
export class SoeErrorResponse {
  status: number;
  trackId: string;
  message: string;
  data: any;
  innerExceptionMessage: string;
  innerExceptionData: any;
  success: boolean;
  error: string;
  details: string;
  isEvoBackend: boolean;

  constructor() {
    this.status = 200;
    this.trackId = '';
    this.message = '';
    this.data = {};
    this.innerExceptionMessage = '';
    this.innerExceptionData = {};
    this.success = false;
    this.error = '';
    this.details = '';
    this.isEvoBackend = false;
  }

  public getTitleAndMessage(): {
    title: string;
    message: string;
    hidden: string;
  } {
    const title = ErrorUtil.getErrorTitle(this.status);
    let messageTxt = this.message;
    if (this.innerExceptionMessage != '') {
      messageTxt += '\n' + 'Inner Exception: ' + this.innerExceptionMessage;
    }
    if (this.trackId != '') {
      messageTxt += '\n' + 'TrackId: ' + this.trackId;
    }

    return {
      title: `${title} (${this.status})`,
      message:
        this.status >= 500 ? this.error + '\n' + this.details : messageTxt,
      hidden: messageTxt,
    };
  }
}

export class ErrorUtil {
  static errorCodeTitles: { [key: number]: string } = {
    400: 'Bad Request',
    401: 'Unauthorized',
    403: 'Forbidden',
    404: 'Not Found',
    408: 'Request Timeout',
    500: 'Internal Server Error',
    502: 'Bad Gateway',
    503: 'Service Unavailable',
  };

  static getErrorTitle(status: number): string {
    return this.errorCodeTitles[status] || 'Error';
  }

  static getStandardErrorMessage(
    status: number,
    translate: TranslateService
  ): string {
    if (status >= 500) {
      return translate.instant('core.http.error.commonservererror');
    }
    return translate.instant('core.http.error.commonerror');
  }

  static createErrorResponse(
    error: any,
    translate: TranslateService
  ): SoeErrorResponse {
    const status = error.status;
    const standardErrorMessage = this.getStandardErrorMessage(
      status,
      translate
    );

    let success = error.message.success || error.success || false;
    let messageTxt =
      error.error?.message || error.message || standardErrorMessage;
    let errorTxt = messageTxt;
    let detailsTxt = '';

    // server errors
    if (status >= 500) errorTxt = standardErrorMessage;

    // create SoeErrorResponse
    const response = new SoeErrorResponse();
    response.status = status || 0;

    // check if EVO backend format
    if (
      error.hasOwnProperty('error') &&
      error.error.hasOwnProperty('response') &&
      error.error.response.hasOwnProperty('backend') &&
      error.error.response.backend.toLowerCase() == 'evo'
    ) {
      errorTxt = error.error.response.error || '';
      response.isEvoBackend = true;
      response.trackId = error.error.response.trackId || '';
      success = error.error.response.success || false;
      detailsTxt = error.error.response.details || detailsTxt;

      if (error.error.response.hasOwnProperty('exception')) {
        messageTxt = error.error.response.exception?.message || messageTxt;
        response.data = error.error.response.exception.data || {};
        if (error.error.response.exception.hasOwnProperty('innerException')) {
          response.innerExceptionMessage =
            error.error.response.exception.innerException?.message || '';
          response.innerExceptionData =
            error.error.response.exception.innerException?.data || {};
        }
      }
    }

    if (status >= 500) console.log('Server error:', messageTxt);

    response.message = messageTxt;
    response.error = errorTxt;
    response.details = detailsTxt;
    response.success = success;

    return response;
  }
}
