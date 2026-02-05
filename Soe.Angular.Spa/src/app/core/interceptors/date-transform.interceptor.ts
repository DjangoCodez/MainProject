import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpHandler,
  HttpRequest,
  HttpEvent,
  HttpResponse,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { cloneDeep } from 'lodash-es';

@Injectable()
export class DateTransformInterceptor implements HttpInterceptor {
  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    let reqClone: HttpRequest<any>;
    if (
      ['POST', 'PUT'].includes(req.method) &&
      !(req.body instanceof FormData)
    ) {
      // Need to use cloneDeep, otherwise the original object posted in the body will be mutated
      const newBody = this.recursivelyTransformDates(
        cloneDeep(req.body),
        req.method,
        false
      );
      reqClone = req.clone({
        body: newBody,
      });
    } else {
      reqClone = req.clone();
    }

    return next.handle(reqClone).pipe(
      map(event => {
        if (event instanceof HttpResponse) {
          return this.transformDates(event, req.method, true);
        }
        return event;
      })
    );
  }

  private transformDates(
    response: HttpResponse<any>,
    reqMethod: string,
    isResponse: boolean
  ): HttpResponse<any> {
    return new HttpResponse<any>({
      ...response,
      body: this.recursivelyTransformDates(
        response.body,
        reqMethod,
        isResponse
      ),
    } as any);
  }

  private recursivelyTransformDates(
    data: any,
    reqMethod: string,
    isResponse: boolean
  ): any {
    if (Array.isArray(data)) {
      return data.map(item =>
        this.recursivelyTransformDates(item, reqMethod, isResponse)
      );
    } else if (typeof data === 'object' && data !== null) {
      if (data instanceof Date) {
        data = this.convertToGmt0(data, reqMethod, isResponse);
      } else {
        for (const key in data) {
          if (
            (typeof data[key] === 'string' && this.isDateString(data[key])) ||
            data[key] instanceof Date
          ) {
            data[key] = this.convertToGmt0(data[key], reqMethod, isResponse);
          } else if (typeof data[key] === 'object') {
            data[key] = this.recursivelyTransformDates(
              data[key],
              reqMethod,
              isResponse
            );
          }
        }
      }
    }

    if (typeof data === 'string' && this.isDateString(data)) {
      data = this.convertToGmt0(data, reqMethod, isResponse);
    }

    return data;
  }

  private convertToGmt0(
    dateString: string | Date,
    reqMethod: string,
    isResponse: boolean
  ): Date | undefined {
    // Parse the input date string as a Date object
    const date = new Date(dateString);

    if (reqMethod === 'GET' || isResponse) {
      // When we fetch data from server, don't convert the date to GMT
      if (
        date.getFullYear() === 1 &&
        date.getMonth() === 0 &&
        date.getDate() === 1
      ) {
        // This is a special case for the date 0001-01-01T00:00:00.000Z
        // which is returned by the server when the date is null or undefined
        // In this case, we return undefined
        return undefined;
      }
      return date;
    }

    // When posting data to the server, convert the date to GMT
    // It will be converted back to local time by the server

    // Use Date.UTC with explicit components to avoid floating-point precision issues
    const gmt0Date = new Date(
      Date.UTC(
        date.getFullYear(),
        date.getMonth(),
        date.getDate(),
        date.getHours(),
        date.getMinutes(),
        date.getSeconds(),
        date.getMilliseconds()
      )
    );

    return gmt0Date;
  }

  private isDateString(value: string): boolean {
    // Not a string, so it can't be a date string.
    if (typeof value !== 'string') return false;

    // Try parsing the string as a Date.
    const parsedDate = new Date(value);

    // Remove milliseconds from the date string, before checking if it's a valid date with rexex
    if (value.includes('T') && value.includes(':') && value.includes('.')) {
      value = value.left(value.indexOf('.'));
    }

    // Check if the parsedDate is a valid date and the original string is not "Invalid Date".
    return parsedDate.toString() !== 'Invalid Date' && this.isDateRegex(value);
  }
  // (?:,\d{3})?
  private isDateRegex(dateString: string): boolean {
    const regex = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$/;
    const dateTimePattern = /^(\d{4}-\d{2}-\d{2})(?:\s(\d{2}:\d{2}:\d{2}))?$/;
    const isoDateTimePattern = /^(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2})$/;
    const isoMatch = dateString.match(isoDateTimePattern);
    const match = dateString.match(dateTimePattern);
    const regexMatch = dateString.match(regex);
    return !!match || !!isoMatch || !!regexMatch;
  }
}
