import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { HttpHeaders } from '@angular/common/http';

export type ExtendedWindow = Window &
  typeof globalThis & {
    soe: any;
    soeConfig: any;
  };

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  config: { soeParameters: string };

  constructor() {
    const extendedWindow = window as ExtendedWindow;
    this.config = extendedWindow.soeConfig;
  }

  getHeaders(): Observable<HttpHeaders> {
    return of(
      new HttpHeaders({
        soeparameters: this.config.soeParameters,
      })
    );
  }
}
