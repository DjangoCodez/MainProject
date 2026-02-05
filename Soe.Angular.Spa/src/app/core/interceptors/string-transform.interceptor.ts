import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpHandler,
  HttpRequest,
  HttpEvent,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { cloneDeep } from 'lodash-es';

@Injectable()
export class StringTransformInterceptor implements HttpInterceptor {
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
      if (req.body instanceof FormData) {
        reqClone = req;
      } else {
        const newBody = this.recursivelyTransformString(cloneDeep(req.body));
        reqClone = req.clone({
          body: newBody,
        });
      }
    } else {
      reqClone = req.clone();
    }

    return next.handle(reqClone);
  }

  private recursivelyTransformString(data: any): any {
    const removeLeadingSpaces = (value: string) => value.replace(/^\s+/, '');
    //const removeTrailingSpaces = (value: string) => value.replace(/\s+$/, '');

    if (Array.isArray(data)) {
      return data.map(item => this.recursivelyTransformString(item));
    } else if (typeof data === 'object' && data !== null) {
      for (const key in data) {
        if (typeof data[key] === 'string') {
          data[key] = removeLeadingSpaces(data[key]);
        } else if (typeof data[key] === 'object') {
          data[key] = this.recursivelyTransformString(data[key]);
        }
      }
    } else if (typeof data === 'string') {
      data = removeLeadingSpaces(data);
    }

    return data;
  }
}
