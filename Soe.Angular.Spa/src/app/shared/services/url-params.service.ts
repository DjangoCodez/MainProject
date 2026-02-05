import { inject, Injectable } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SoeModule } from '@shared/models/generated-interfaces/Enumerations';
import { Location } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class UrlHelperService {
  location = inject(Location);
  route = inject(ActivatedRoute);

  private moduleMap = {
    billing: SoeModule.Billing,
    economy: SoeModule.Economy,
    time: SoeModule.Time,
    manage: SoeModule.Manage,
  };

  public get path() {
    return window.location.pathname;
  }

  public get module(): SoeModule {
    const mainModule = this.path.split('/')[1];
    return (
      this.moduleMap[mainModule as keyof typeof this.moduleMap] ??
      SoeModule.None
    );
  }

  public setQueryParam(key: string, value: string | number | boolean) {
    const currentParams = { ...this.route.snapshot.queryParams };

    if (value === null) {
      delete currentParams[key];
    } else {
      currentParams[key] = value;
    }

    const path = this.path;
    const newQuery = new URLSearchParams(currentParams).toString();

    this.location.replaceState(path + (newQuery ? '?' + newQuery : ''));
  }
}
