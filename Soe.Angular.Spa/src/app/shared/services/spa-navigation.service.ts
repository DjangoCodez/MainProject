import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { SoeHttpClient } from './http.service';
import { getAvailableSpaModules } from './generated-service-endpoints/manage/SysPageStatus.endpoints';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { BrowserUtil } from '@shared/util/browser-util';

type NavHelper = {
  migratedModules: Set<Feature>;
  spaRouter: Router;
  intercept: (
    event: MouseEvent | null,
    feature: Feature,
    url: string,
    label?: string
  ) => void;
};
declare const navHelper: NavHelper | undefined;

@Injectable({
  providedIn: 'root',
})
export class SpaNavigationService {
  private readonly httpClient = inject(SoeHttpClient);
  private readonly router = inject(Router);
  private migratedModules?: Set<Feature>;

  constructor() {
    this.setup();
  }

  private setup() {
    //Expose the router to the global window object for SPA navigation
    this.loadSpaFeatures().subscribe({
      next: modules => {
        this.migratedModules = new Set(modules);
        this.upadateNavHelper();
      },
      error: error => {
        console.error('Failed to load SPA features:', error);
      },
    });
  }

  public navigate(url: string, feature: Feature, label?: string) {
    if (navHelper) navHelper.intercept(null, feature, url, label);
    else BrowserUtil.openInSameTab(window, url);
  }
  public spaNavigate(url: string) {
    this.navigate(url, -1 as Feature, undefined);
  }

  private upadateNavHelper() {
    // Navhelper is our global object that's used to intercept navigation events
    // Can be found in Soe.Web/csssjs/navigationinterceptor.js
    if (navHelper) {
      navHelper.migratedModules = this.migratedModules || new Set();
      navHelper.spaRouter = this.router;
    }
  }

  private loadSpaFeatures() {
    return this.httpClient.get<number[]>(getAvailableSpaModules());
  }
}
